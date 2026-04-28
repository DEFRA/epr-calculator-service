using Newtonsoft.Json;
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

    public record ProjectionData
    {

    }

    public interface ICalcResultProjectedProducersBuilder
    {
        Task<List<(L1, ProjectionData?)>> ConstructAsync(CalcResultsRequestDto resultsRequestDto, List<L1> producers);
    }

    public class CalcResultProjectedProducersBuilder : ICalcResultProjectedProducersBuilder
    {
        private readonly ApplicationDBContext context;

        public CalcResultProjectedProducersBuilder(ApplicationDBContext dbContext)
        {
            context = dbContext;
        }

        private decimal total(RamTonnage ram)
        {
            return ram.R + ram.A + ram.G + ram.RM + ram.AM + ram.GM;
        }

        private SingleL1 updateSingleL1(SingleL1 sl1)
        {
            Console.WriteLine($">> sl1 {JsonConvert.SerializeObject(sl1, Formatting.Indented)}");
            var materialSubmissons = sl1.MaterialSubmissions.Select(submission =>
            {
                if (submission.SubmissionPeriod.EndsWith("-H2"))
                {
                    var diff = submission.Total - total(submission.RAM);
                    if (diff != 0)
                    {
                        return submission with { RAM = submission.RAM with { R = submission.RAM.R + diff }};
                    }
                    else
                    {
                        return submission;
                    }
                }
                else
                {
                    return submission;
                }
            });
            return sl1 with { MaterialSubmissions = materialSubmissons.ToList() };
        }

        private L2 updateL2(L2 l2)
        {
            var materialSubmissons = l2.MaterialSubmissions.Select(submission =>
            {
                if (submission.SubmissionPeriod.EndsWith("-H2"))
                {
                    var diff = submission.Total - total(submission.RAM);
                    if (diff != 0)
                    {
                        return submission with { RAM = submission.RAM with { R = submission.RAM.R + diff }};
                    }
                    else
                    {
                        return submission;
                    }
                }
                else
                {
                    return submission;
                }
            });
            return l2 with { MaterialSubmissions = materialSubmissons.ToList() };
        }

        // TODO Unit Test should focus on this
        public (L1, ProjectionData?) project(L1 l1)
        {
            var updatedL1 = l1 switch
            {
                SingleL1 sl1 => (L1) updateSingleL1(sl1),
                HC hc => new HC(hc.OrgId, hc.L2s.Select(l2 => updateL2(l2)).ToList()),
                _ => throw new ArgumentException($"Invalid L1 {l1.GetType()}")
            };
            return (updatedL1, (ProjectionData?)null);
        }

        public async Task<List<(L1, ProjectionData?)>> ConstructAsync(CalcResultsRequestDto resultsRequestDto, List<L1> producers)
        {
            Console.WriteLine($">> Returning {JsonConvert.SerializeObject(producers, Formatting.Indented)}");
            return producers.Select(p => project(p)).ToList();
            /*var runId = resultsRequestDto.RunId;
            var materialsFromDb = await context.Material.ToListAsync();
            var materials = MaterialMapper.Map(materialsFromDb);

            var submissionPeriod = (string i) => $"{resultsRequestDto.RelativeYear.Value - 1}-{i}";
            var h2ReportedMaterials = producers.Where(r => r.SubmissionPeriod == submissionPeriod("H2")).ToList();
            var h1ReportedMaterials = producers.Where(r => r.SubmissionPeriod == submissionPeriod("H1")).ToList();

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

            return new CalcResultProjectedProducers
            {
                H2ProjectedProducersHeaders = H2ProjectedProducersBuilderUtils.GetProjectedProducerHeaders(materials),
                H1ProjectedProducersHeaders = H1ProjectedProducersBuilderUtils.GetProjectedProducerHeaders(materials),
                H2ProjectedProducers = h2ProjectedProducersWithSubtotals.OrderBy(p => p.ProducerId).ThenBy(p => p.Level).ThenBy(p => p.SubsidiaryId).ToList(),
                H1ProjectedProducers = h1ProjectedProducersWithSubtotals.OrderBy(p => p.ProducerId).ThenBy(p => p.Level).ThenBy(p => p.SubsidiaryId).ToList()
            };*/
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
