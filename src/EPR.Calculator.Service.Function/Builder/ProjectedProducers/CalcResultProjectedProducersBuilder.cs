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
        Task<CalcResultProjectedProducers> ConstructAsync(CalcResultsRequestDto resultsRequestDto);
    }

    public class CalcResultProjectedProducersBuilder : ICalcResultProjectedProducersBuilder
    {
        private readonly ApplicationDBContext context;

        public CalcResultProjectedProducersBuilder(ApplicationDBContext dbContext)
        {
            context = dbContext;
        }

        public async Task<CalcResultProjectedProducers> ConstructAsync(CalcResultsRequestDto resultsRequestDto)
        {
            var runId = resultsRequestDto.RunId;
            var materialsFromDb = await context.Material.ToListAsync();
            var materials = MaterialMapper.Map(materialsFromDb);

            var reportedMaterialsForRun = await GetReportedMaterialsForRun(runId);
            var submissionPeriod = (string i) => $"{resultsRequestDto.RelativeYear.Value - 1}-{i}";
            var h2ReportedMaterials = reportedMaterialsForRun.Where(r => r.SubmissionPeriod == submissionPeriod("H2"));
            var h1ReportedMaterials = reportedMaterialsForRun.Where(r => r.SubmissionPeriod == submissionPeriod("H1"));

            var h2ProjectedProduers = await H2ProjectedProducersBuilderUtils.GetProjectedProducers(h2ReportedMaterials.ToList(), materials);
            var h2ProjectedProducersWithSubtotals = AddSubtotals<CalcResultH2ProjectedProducer, CalcResultH2ProjectedProducerMaterialTonnage>(
                h2ProjectedProduers,
                createSubtotal: H2ProjectedProducersBuilderUtils.CreateParentProducer,
                sumProducerGroupTonnages: H2ProjectedProducersBuilderUtils.SumProducerGroupTonnages
            );
            var h1ProjectedProduers = await H1ProjectedProducersBuilderUtils.GetProjectedProducers(h1ReportedMaterials.ToList(), h2ProjectedProduers, materials);
            var h1ProjectedProducersWithSubtotals = AddSubtotals<CalcResultH1ProjectedProducer, CalcResultH1ProjectedProducerMaterialTonnage>(
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
            };
        }

        private async Task<List<ProducerReportedMaterialsForSubmissionPeriod>> GetReportedMaterialsForRun(int runId)
        {
            return await (from run in context.CalculatorRuns.AsNoTracking()
                    join pd in context.ProducerDetail.AsNoTracking() on run.Id equals pd.CalculatorRunId
                    join prm in context.ProducerReportedMaterial.AsNoTracking() on pd.Id equals prm.ProducerDetailId
                    where pd.CalculatorRunId == runId
                    group prm by new { pd.ProducerId, pd.SubsidiaryId, prm.SubmissionPeriod } into prms 
                    select new ProducerReportedMaterialsForSubmissionPeriod(prms.Key.ProducerId, prms.Key.SubsidiaryId, prms.Key.SubmissionPeriod, prms.ToList())).ToListAsync();
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
            var ramTonnage = tonnage.RedTonnage + tonnage.RedMedicalTonnage + tonnage.AmberTonnage + tonnage.AmberMedicalTonnage + tonnage.GreenTonnage + tonnage.GreenMedicalTonnage;
            var diffTonnage = tonnage.Tonnage - ramTonnage;
            return diffTonnage > 0 ? diffTonnage : 0;
        }

        public static RAMTonnage SumRAMTonnages<TProducer, TTonnage>(List<TProducer> producers, string materialCode, Func<TTonnage, RAMTonnage?> getRAMTonnage) 
            where TProducer : CalcResultProjectedProducer<TTonnage>
            where TTonnage : CalcResultProjectedProducerMaterialTonnage
        {
            return new RAMTonnage {
                Tonnage = producers.Sum(p => getRAMTonnage(p.ProjectedTonnageByMaterial[materialCode])?.Tonnage ?? 0),
                RedTonnage = producers.Sum(p => getRAMTonnage(p.ProjectedTonnageByMaterial[materialCode])?.RedTonnage ?? 0),
                RedMedicalTonnage = producers.Sum(p => getRAMTonnage(p.ProjectedTonnageByMaterial[materialCode])?.RedMedicalTonnage ?? 0),
                AmberTonnage = producers.Sum(p => getRAMTonnage(p.ProjectedTonnageByMaterial[materialCode])?.AmberTonnage ?? 0),
                AmberMedicalTonnage = producers.Sum(p => getRAMTonnage(p.ProjectedTonnageByMaterial[materialCode])?.AmberMedicalTonnage ?? 0),
                GreenTonnage = producers.Sum(p => getRAMTonnage(p.ProjectedTonnageByMaterial[materialCode])?.GreenTonnage ?? 0),
                GreenMedicalTonnage = producers.Sum(p => getRAMTonnage(p.ProjectedTonnageByMaterial[materialCode])?.GreenMedicalTonnage ?? 0),
            };
        }

        private static List<TProducer> AddSubtotals<TProducer, TTonnage>(
            List<TProducer> projectedProducers,
            Func<TProducer, TProducer> createSubtotal,
            Func<IEnumerable<TProducer>, TProducer> sumProducerGroupTonnages)
            where TProducer : CalcResultProjectedProducer<TTonnage>
            where TTonnage : CalcResultProjectedProducerMaterialTonnage
        {
            var result = new List<TProducer>();
            var producerGroups = projectedProducers.GroupBy(p => p.ProducerId);

            foreach (var group in producerGroups)
            {
                if (group.Count() > 1)
                {
                    foreach (var p in group)
                    {
                        p.Level = CommonConstants.LevelTwo.ToString();
                    }

                    result.AddRange(group);
                    result.Add(
                        sumProducerGroupTonnages(group)
                    );
                }
                else
                {
                    var producer = group.First();

                    if (producer.SubsidiaryId != null)
                    {
                        producer.Level = CommonConstants.LevelTwo.ToString();

                        var subtotal = createSubtotal(producer);
                        subtotal.Level = CommonConstants.LevelOne.ToString();
                        subtotal.IsSubtotal = true;
                        subtotal.SubsidiaryId = null;

                        result.Add(subtotal);
                        result.Add(producer);
                    }
                    else
                    {
                        producer.Level = CommonConstants.LevelOne.ToString();
                        result.Add(producer);
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