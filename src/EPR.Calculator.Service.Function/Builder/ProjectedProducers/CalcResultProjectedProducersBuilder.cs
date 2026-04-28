using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Builder.ProjectedProducers;
using Newtonsoft.Json;
using static EPR.Calculator.Service.Function.Constants.CommonConstants;
namespace EPR.Calculator.Service.Function.Builder.ProjectedProducers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using EPR.Calculator.API.Data;
    using EPR.Calculator.API.Data.DataModels;
    using EPR.Calculator.Service.Function.Constants;
    using EPR.Calculator.Service.Function.Mappers;
    using EPR.Calculator.Service.Function.Misc;
    using EPR.Calculator.Service.Function.Models;
    using EPR.Calculator.Service.Function.Services;
    using EPR.Calculator.Service.Function.Builder.Summary.Common;

    using Microsoft.EntityFrameworkCore;
    using Azure.Analytics.Synapse.Artifacts.Models;
    using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal;


    public interface ICalcResultProjectedProducersBuilder
    {
        Task<(List<ProducerReportedMaterialsForSubmissionPeriod>, CalcResultProjectedProducers)> ConstructAsync(CalcResultsRequestDto resultsRequestDto, List<ProducerReportedMaterialsForSubmissionPeriod> producers);
    }

    public class CalcResultProjectedProducersBuilder : ICalcResultProjectedProducersBuilder
    {
        private readonly ApplicationDBContext context;

        public CalcResultProjectedProducersBuilder(ApplicationDBContext dbContext)
        {
            context = dbContext;
        }

        private List<ProducerReportedMaterial> asd(List<MaterialDetail> materials, ProducerReportedMaterialsForSubmissionPeriod p, IEnumerable<ICalcResultProjectedProducer> projectedProducers)
        {
            return projectedProducers
                .Where(q => !q.IsSubtotal)
                .Where(q => q.ProducerId == p.ProducerId && q.SubsidiaryId == p.SubsidiaryId && q.SubmissionPeriodCode == p.SubmissionPeriod)
                .SelectMany(q => {
                    //Console.WriteLine($">> q For {p.ProducerId} {p.SubsidiaryId} {p.SubmissionPeriod}: {JsonConvert.SerializeObject(q, Formatting.Indented)}");
                    return p.ReportedMaterials.Select(rm =>
                    {
                        var projected = q.ProjectedTonnageByMaterial.First(pt => pt.Key == materials.Find(m => m.Id == rm.MaterialId).Code).Value;
                        var projectedRam = rm.PackagingType switch
                        {
                            "HH" => projected.ProjectedHouseholdRAMTonnage,
                            "PB" => projected.ProjectedPublicBinRAMTonnage,
                            "HDC" => projected.ProjectedHouseholdDrinksContainerRAMTonnage,
                        };
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
                    }).ToList();
                }
                ).ToList();
        }

        private List<ProducerReportedMaterialsForSubmissionPeriod> toNewModel(List<MaterialDetail> materials, List<ProducerReportedMaterialsForSubmissionPeriod> producers, CalcResultProjectedProducers result)
        {
            return producers.Select( p =>
            {
                List<ProducerReportedMaterial> h1reportedMaterials = asd(materials, p, result.H1ProjectedProducers);
                List<ProducerReportedMaterial> h2reportedMaterials = asd(materials, p, result.H2ProjectedProducers);
                return new ProducerReportedMaterialsForSubmissionPeriod(
                    producerId : p.ProducerId,
                    subsidiaryId : p.SubsidiaryId,
                    submissionPeriod : p.SubmissionPeriod,
                    reportedMaterials : h1reportedMaterials.Concat(h2reportedMaterials).ToList()
                );
            }).ToList();
        }

        public async Task<(List<ProducerReportedMaterialsForSubmissionPeriod>, CalcResultProjectedProducers)> ConstructAsync(CalcResultsRequestDto resultsRequestDto, List<ProducerReportedMaterialsForSubmissionPeriod> producers)
        {
            Console.WriteLine($">> producers: {JsonConvert.SerializeObject(producers, Formatting.Indented)}");

            var runId = resultsRequestDto.RunId;
            var materialsFromDb = await context.Material.ToListAsync();
            var materials = MaterialMapper.Map(materialsFromDb);

            var submissionPeriod = (string i) => $"{resultsRequestDto.RelativeYear.Value - 1}-{i}";
            List<ProducerReportedMaterialsForSubmissionPeriod> h2ReportedMaterials = producers.Where(r => r.SubmissionPeriod == submissionPeriod("H2")).ToList();
            List<ProducerReportedMaterialsForSubmissionPeriod> h1ReportedMaterials = producers.Where(r => r.SubmissionPeriod == submissionPeriod("H1")).ToList();

            var h2ProjectedProduers = H2ProjectedProducersBuilderUtils.GetProjectedProducers(h2ReportedMaterials, materials);
            var h2ProjectedProducersWithSubtotals = AddSubtotals<CalcResultH2ProjectedProducer>(
                h2ProjectedProduers,
                createSubtotal: H2ProjectedProducersBuilderUtils.CreateParentProducer,
                sumProducerGroupTonnages: H2ProjectedProducersBuilderUtils.SumProducerGroupTonnages
            );
            var h1ProjectedProduers = H1ProjectedProducersBuilderUtils.GetProjectedProducers(h1ReportedMaterials, h2ProjectedProducersWithSubtotals, materials);
            var h1ProjectedProducersWithSubtotals = AddSubtotals<CalcResultH1ProjectedProducer>(
                h1ProjectedProduers,
                createSubtotal: H1ProjectedProducersBuilderUtils.CreateParentProducer,
                sumProducerGroupTonnages: H1ProjectedProducersBuilderUtils.SumProducerGroupTonnages
            );

            var x = new CalcResultProjectedProducers
            {
                H2ProjectedProducersHeaders = H2ProjectedProducersBuilderUtils.GetProjectedProducerHeaders(materials),
                H1ProjectedProducersHeaders = H1ProjectedProducersBuilderUtils.GetProjectedProducerHeaders(materials),
                H2ProjectedProducers = h2ProjectedProducersWithSubtotals.OrderBy(p => p.ProducerId).ThenBy(p => p.Level).ThenBy(p => p.SubsidiaryId).ToList(),
                H1ProjectedProducers = h1ProjectedProducersWithSubtotals.OrderBy(p => p.ProducerId).ThenBy(p => p.Level).ThenBy(p => p.SubsidiaryId).ToList()
            };
            //Console.WriteLine($">> x: {JsonConvert.SerializeObject(x, Formatting.Indented)}");

            return (toNewModel(materials, producers, x), x);
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

    public class ProducerReportedMaterialsForSubmissionPeriod
    {
        public int ProducerId { get; }
        public string? SubsidiaryId { get; }
        public string SubmissionPeriod { get; }
        public List<ProducerReportedMaterial> ReportedMaterials { get; }

        public ProducerReportedMaterialsForSubmissionPeriod(int producerId, string? subsidiaryId, string submissionPeriod, List<ProducerReportedMaterial> reportedMaterials)
        {
            ProducerId = producerId;
            SubsidiaryId = subsidiaryId;
            SubmissionPeriod = submissionPeriod;
            ReportedMaterials = reportedMaterials;
        }
    }
}
