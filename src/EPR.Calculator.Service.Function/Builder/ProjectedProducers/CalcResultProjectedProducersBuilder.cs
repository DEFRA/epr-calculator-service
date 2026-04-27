using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Builder.PartialObligations;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Mappers;
using EPR.Calculator.Service.Function.Misc;
using EPR.Calculator.Service.Function.Models;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.Service.Function.Builder.ProjectedProducers
{
    public interface ICalcResultProjectedProducersBuilder
    {
        Task<(List<ProducerDetail>, CalcResultProjectedProducers)> ConstructAsync(CalcResultsRequestDto resultsRequestDto, List<ProducerDetail> producerDetails);
    }

    public class CalcResultProjectedProducersBuilder : ICalcResultProjectedProducersBuilder
    {
        private readonly ApplicationDBContext dbContext;

        public CalcResultProjectedProducersBuilder(ApplicationDBContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<(List<ProducerDetail>, CalcResultProjectedProducers)> ConstructAsync(CalcResultsRequestDto resultsRequestDto, List<ProducerDetail> producerDetails)
        {
            var materialsFromDb = await dbContext.Material.ToListAsync();
            var materials = MaterialMapper.Map(materialsFromDb);
            var h2Period = $"{resultsRequestDto.RelativeYear.Value - 1}-H2";
            var h1Period = $"{resultsRequestDto.RelativeYear.Value - 1}-H1";

            var allH2Rows = new List<CalcResultH2ProjectedProducer>();
            var allH1Rows = new List<CalcResultH1ProjectedProducer>();
            var updatedProducers = new List<ProducerDetail>(producerDetails.Count);

            // H1 for a producer only depends on H2 from the same ProducerId group, so each group
            // can be processed fully in one pass.
            foreach (var producerGroup in producerDetails.GroupBy(pd => pd.ProducerId))
            {
                var groupList = producerGroup.ToList();

                var h2Rows = H2ProjectedProducersBuilderUtils.GetProjectedProducers(groupList, materials, h2Period);
                var h2WithGroupSubtotals = AddSubtotals<CalcResultH2ProjectedProducer>(
                    h2Rows,
                    createSubtotal: H2ProjectedProducersBuilderUtils.CreateParentProducer,
                    sumProducerGroupTonnages: H2ProjectedProducersBuilderUtils.SumProducerGroupTonnages
                );

                var h1Rows = H1ProjectedProducersBuilderUtils.GetProjectedProducers(groupList, h2WithGroupSubtotals, materials, h1Period);

                for (var i = 0; i < groupList.Count; i++)
                    updatedProducers.Add(ApplyProjectedMaterials(groupList[i], h1Rows[i], h2Rows[i], materials, h1Period, h2Period));

                allH2Rows.AddRange(h2WithGroupSubtotals);
                allH1Rows.AddRange(AddSubtotals<CalcResultH1ProjectedProducer>(
                    h1Rows,
                    createSubtotal: H1ProjectedProducersBuilderUtils.CreateParentProducer,
                    sumProducerGroupTonnages: H1ProjectedProducersBuilderUtils.SumProducerGroupTonnages
                ));
            }

            var result = new CalcResultProjectedProducers
            {
                H2ProjectedProducersHeaders = H2ProjectedProducersBuilderUtils.GetProjectedProducerHeaders(materials),
                H1ProjectedProducersHeaders = H1ProjectedProducersBuilderUtils.GetProjectedProducerHeaders(materials),
                H2ProjectedProducers = allH2Rows.OrderBy(p => p.ProducerId).ThenBy(p => p.Level).ThenBy(p => p.SubsidiaryId).ToList(),
                H1ProjectedProducers = allH1Rows.OrderBy(p => p.ProducerId).ThenBy(p => p.Level).ThenBy(p => p.SubsidiaryId).ToList()
            };

            return (updatedProducers, result);
        }

        private ProducerDetail ApplyProjectedMaterials(
            ProducerDetail pd,
            ICalcResultProjectedProducer h1Row,
            ICalcResultProjectedProducer h2Row,
            List<MaterialDetail> materials,
            string h1Period,
            string h2Period)
        {
            var h1ById = h1Row.ProjectedTonnageByMaterial
                .ToDictionary(kvp => materials.Find(m => m.Code == kvp.Key)?.Id ?? -1, kvp => kvp.Value);
            var h2ById = h2Row.ProjectedTonnageByMaterial
                .ToDictionary(kvp => materials.Find(m => m.Code == kvp.Key)?.Id ?? -1, kvp => kvp.Value);

            ProducerReportedMaterial Apply(ProducerReportedMaterial rm, Dictionary<int, CalcResultProjectedProducerMaterialTonnage> projectedById)
            {
                if (!projectedById.TryGetValue(rm.MaterialId, out var projected)) return rm;
                var projectedRam = rm.PackagingType switch
                {
                    PackagingTypes.Household => projected.ProjectedHouseholdRAMTonnage,
                    PackagingTypes.PublicBin => projected.ProjectedPublicBinRAMTonnage,
                    PackagingTypes.HouseholdDrinksContainers => projected.ProjectedHouseholdDrinksContainerRAMTonnage,
                    _ => null
                };
                if (projectedRam == null) return rm;
                return new ProducerReportedMaterial
                {
                    Id = rm.Id,
                    MaterialId = rm.MaterialId,
                    ProducerDetailId = rm.ProducerDetailId,
                    PackagingType = rm.PackagingType,
                    PackagingTonnage = rm.PackagingTonnage,
                    PackagingTonnageRed = projectedRam.RedTonnage,
                    PackagingTonnageAmber = projectedRam.AmberTonnage,
                    PackagingTonnageGreen = projectedRam.GreenTonnage,
                    PackagingTonnageRedMedical = projectedRam.RedMedicalTonnage,
                    PackagingTonnageAmberMedical = projectedRam.AmberMedicalTonnage,
                    PackagingTonnageGreenMedical = projectedRam.GreenMedicalTonnage,
                    SubmissionPeriod = rm.SubmissionPeriod,
                    ProducerDetail = rm.ProducerDetail,
                    Material = rm.Material
                };
            }

            return CalcResultPartialObligationBuilder.UpdateReportedMaterials(
                pd,
                reportedMaterials =>
                    reportedMaterials.Where(rm => rm.SubmissionPeriod == h1Period).Select(rm => Apply(rm, h1ById))
                    .Concat(reportedMaterials.Where(rm => rm.SubmissionPeriod == h2Period).Select(rm => Apply(rm, h2ById)))
                    .ToList()
            );
        }

        public static RAMTonnage GetRAMTonnage(string packagingType, List<ProducerReportedMaterial> reportedMaterials) {
            decimal GetReportedTonnage(string packagingType, Func<ProducerReportedMaterial, decimal?> tonnageFunc) {
                return reportedMaterials.Where(p => p.PackagingType == packagingType).Sum(t => tonnageFunc(t) ?? 0);
            }

            return new RAMTonnage {
                Tonnage = GetReportedTonnage(packagingType, t => t.PackagingTonnage),
                RedTonnage = GetReportedTonnage(packagingType, t => t.PackagingTonnageRed),
                RedMedicalTonnage = GetReportedTonnage(packagingType, t => t.PackagingTonnageRedMedical),
                AmberTonnage = GetReportedTonnage(packagingType, t => t.PackagingTonnageAmber),
                AmberMedicalTonnage = GetReportedTonnage(packagingType, t => t.PackagingTonnageAmberMedical),
                GreenTonnage = GetReportedTonnage(packagingType, t => t.PackagingTonnageGreen),
                GreenMedicalTonnage = GetReportedTonnage(packagingType, t => t.PackagingTonnageGreenMedical),
            };
        }

        public static decimal TonnageWithoutRAM(RAMTonnage tonnage)
        {
            return Math.Max(0, tonnage.Tonnage - tonnage.GetTotalRamTonnage());
        }

        public static RAMTonnage SumRAMTonnages(List<ICalcResultProjectedProducer> producers, string materialCode, Func<CalcResultProjectedProducerMaterialTonnage, RAMTonnage?> getRAMTonnage)
        {
            decimal tonnage = 0, red = 0, redMed = 0, amber = 0, amberMed = 0, green = 0, greenMed = 0;

            foreach (var p in producers)
            {
                CalcResultProjectedProducerMaterialTonnage? material = p.ProjectedTonnageByMaterial.FirstOrDefault(v => v.Key == materialCode).Value;
                if (material == null) continue;

                var ram = getRAMTonnage(material);
                if (ram == null) continue;

                tonnage += ram.Tonnage;
                red += ram.RedTonnage;
                redMed += ram.RedMedicalTonnage;
                amber += ram.AmberTonnage;
                amberMed += ram.AmberMedicalTonnage;
                green += ram.GreenTonnage;
                greenMed += ram.GreenMedicalTonnage;
            }

            return new RAMTonnage
            {
                Tonnage = tonnage,
                RedTonnage = red,
                RedMedicalTonnage = redMed,
                AmberTonnage = amber,
                AmberMedicalTonnage = amberMed,
                GreenTonnage = green,
                GreenMedicalTonnage = greenMed
            };
        }

        private static List<TSubmissionPeriodProducer> AddSubtotals<TSubmissionPeriodProducer>(
            List<TSubmissionPeriodProducer> projectedProducers,
            Func<TSubmissionPeriodProducer, TSubmissionPeriodProducer> createSubtotal,
            Func<List<TSubmissionPeriodProducer>, TSubmissionPeriodProducer> sumProducerGroupTonnages)
            where TSubmissionPeriodProducer : ICalcResultProjectedProducer
        {
            var result = new List<TSubmissionPeriodProducer>();
            var producerGroups = projectedProducers.GroupBy(p => p.ProducerId);

            foreach (var group in producerGroups)
            {
                if (group.Count() > 1)
                {
                    var updatedGroup = group.Select(p => p with { Level = CommonConstants.LevelTwo.ToString()});

                    result.AddRange(updatedGroup);
                    result.Add(
                        sumProducerGroupTonnages(group.ToList())
                    );
                }
                else
                {
                    var producer = group.First();

                    if (producer.SubsidiaryId != null)
                    {
                        var levelTwoProd = producer with { Level = CommonConstants.LevelTwo.ToString() };

                        var subtotal = createSubtotal(producer) with {
                            Level = CommonConstants.LevelOne.ToString(),
                            IsSubtotal = true,
                            SubsidiaryId = null
                        };

                        result.Add(subtotal);
                        result.Add(levelTwoProd);
                    }
                    else
                    {
                        var levelOneProd = producer with { Level = CommonConstants.LevelOne.ToString()};
                        result.Add(levelOneProd);
                    }
                }
            }

            return result;
        }
    }
}
