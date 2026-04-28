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


    public interface ICalcResultProjectedProducersBuilder
    {
        Task<(List<L1>, CalcResultProjectedProducers)> ConstructAsync(CalcResultsRequestDto resultsRequestDto, List<L1> producers);
    }

    public class CalcResultProjectedProducersBuilder : ICalcResultProjectedProducersBuilder
    {
        private readonly ApplicationDBContext context;

        public CalcResultProjectedProducersBuilder(ApplicationDBContext dbContext)
        {
            context = dbContext;
        }

        private List<ProducerReportedMaterialsForSubmissionPeriod> transformSl1ToOldModel(List<MaterialDetail> materials, SingleL1 sl1)
        {
            return sl1.MaterialSubmissions.Select(submission => {
                Console.WriteLine($">> Loking for {submission.MaterialCode} in {JsonConvert.SerializeObject(materials, Formatting.Indented)}");
                return new ProducerReportedMaterial
                {
                    MaterialId = materials.First(m => m.Code == submission.MaterialCode).Id,
                    PackagingType = submission.PackagingType,
                    SubmissionPeriod = submission.SubmissionPeriod,
                    PackagingTonnage = submission.Total,
                    PackagingTonnageRed = submission.RAM?.R ?? 0m,
                    PackagingTonnageAmber = submission.RAM?.A ?? 0m,
                    PackagingTonnageGreen = submission.RAM?.G ?? 0m,
                    PackagingTonnageRedMedical = submission.RAM?.RM ?? 0m,
                    PackagingTonnageAmberMedical = submission.RAM?.AM ?? 0m,
                    PackagingTonnageGreenMedical = submission.RAM?.GM ?? 0m,
                };
            }
            ).GroupBy(e => e.SubmissionPeriod)
              .Select(reportedMaterials =>
                new ProducerReportedMaterialsForSubmissionPeriod(
                    producerId: sl1.OrgId,
                    subsidiaryId: null,
                    submissionPeriod: reportedMaterials.Key,
                    reportedMaterials : reportedMaterials.ToList()
                )
              ).ToList();
        }

        private List<ProducerReportedMaterialsForSubmissionPeriod> transformL2ToOldModel(List<MaterialDetail> materials, L2 l2)
        {
            return l2.MaterialSubmissions.Select(submission =>
                new ProducerReportedMaterial
                {
                    MaterialId = materials.First(m => m.Code == submission.MaterialCode).Id,
                    PackagingType = submission.PackagingType,
                    SubmissionPeriod = submission.SubmissionPeriod,
                    PackagingTonnage = submission.Total,
                    PackagingTonnageRed = submission.RAM?.R ?? 0m,
                    PackagingTonnageAmber = submission.RAM?.A ?? 0m,
                    PackagingTonnageGreen = submission.RAM?.G ?? 0m,
                    PackagingTonnageRedMedical = submission.RAM?.RM ?? 0m,
                    PackagingTonnageAmberMedical = submission.RAM?.AM ?? 0m,
                    PackagingTonnageGreenMedical = submission.RAM?.GM ?? 0m,
                }
            ).GroupBy(e => e.SubmissionPeriod)
              .Select(reportedMaterials =>
                new ProducerReportedMaterialsForSubmissionPeriod(
                    producerId: l2.OrgId,
                    subsidiaryId: l2.SubsidiaryId,
                    submissionPeriod: reportedMaterials.Key,
                    reportedMaterials : reportedMaterials.ToList()
                )
              ).ToList();
        }

        List<ProducerReportedMaterialsForSubmissionPeriod> transformHcToOldModel(List<MaterialDetail> materials, HC hc)
        {
            return hc.L2s.SelectMany(l2 => transformL2ToOldModel(materials, l2)).ToList();
        }

        private List<ProducerReportedMaterialsForSubmissionPeriod> transformToOldModel(List<MaterialDetail> materials, List<L1> producers)
        {
            return producers.SelectMany(producer =>
               producer switch
               {
                   SingleL1 sl1 => transformSl1ToOldModel(materials, sl1),
                   HC hc => transformHcToOldModel(materials, hc)
               }
            ).ToList();
        }

        private List<L1> toNewModel(CalcResultProjectedProducers result)
        {/*
            result.H2ProjectedProducers.Select(p =>
            {
               return p.ProducerId;
               return p.SubsidiaryId;
            })
        public IEnumerable<CalcResultH2ProjectedProducer>? H2ProjectedProducers { get; init; }
        public IEnumerable<CalcResultH1ProjectedProducer>? H1ProjectedProducers { get; init; }*/

            return new List<L1>();

  /*                  public required int ProducerId { get; init; }
        public string? SubsidiaryId { get; init; }
        public required string Level { get; init; }
        public required string SubmissionPeriodCode { get; init; }
        public bool IsSubtotal { get; init; }
        public abstract IEnumerable<KeyValuePair<string, CalcResultProjectedProducerMaterialTonnage>> ProjectedTonnageByMaterial { get; }
    }

    public record CalcResultH2ProjectedProducer : ICalcResultProjectedProducer
    {
        public IReadOnlyDictionary<string, CalcResultH2ProjectedProducerMaterialTonnage> H2ProjectedTonnageByMaterial { get; init; }
            = new Dictionary<string, CalcResultH2ProjectedProducerMaterialTonnage>();

        public override IEnumerable<KeyValuePair<string, CalcResultProjectedProducerMaterialTonnage>> ProjectedTonnageByMaterial =>
            H2ProjectedTonnageByMaterial.Select(kv => new KeyValuePair<string, CalcResultProjectedProducerMaterialTonnage>(kv.Key, kv.Value));
*/
        }


        public async Task<(List<L1>, CalcResultProjectedProducers)> ConstructAsync(CalcResultsRequestDto resultsRequestDto, List<L1> producers)
        {
            //var result = producers.Select(p => project(p)).ToList();
            //Console.WriteLine($">> Returning {JsonConvert.SerializeObject(result, Formatting.Indented)}");
            var runId = resultsRequestDto.RunId;
            var materialsFromDb = await context.Material.ToListAsync();
            var materials = MaterialMapper.Map(materialsFromDb);

            var submissionPeriod = (string i) => $"{resultsRequestDto.RelativeYear.Value - 1}-{i}";
            var producersAsOldModel = transformToOldModel(materials, producers);
            List<ProducerReportedMaterialsForSubmissionPeriod> h2ReportedMaterials = producersAsOldModel.Where(r => r.SubmissionPeriod == submissionPeriod("H2")).ToList();
            List<ProducerReportedMaterialsForSubmissionPeriod> h1ReportedMaterials = producersAsOldModel.Where(r => r.SubmissionPeriod == submissionPeriod("H1")).ToList();

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
            return (toNewModel(x), x);
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
