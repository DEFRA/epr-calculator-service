﻿using EPR.Calculator.Service.Function.Builder.Summary.Common;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Data;
using EPR.Calculator.Service.Function.Data.DataModels;
using EPR.Calculator.Service.Function.Dtos;
using EPR.Calculator.Service.Function.Mappers;
using EPR.Calculator.Service.Function.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EPR.Calculator.Service.Function.Builder.ScaledupProducers
{
    public class CalcResultScaledupProducersBuilder : ICalcResultScaledupProducersBuilder
    {
        private const decimal NormalScaleup = 1.0M;
        private const int MaterialsBreakdownHeaderInitialColumnIndex = 9;
        private const int MaterialsBreakdownHeaderIncrementalColumnIndex = 10;

        private readonly ApplicationDBContext context;
        private readonly object Mappers;

        public CalcResultScaledupProducersBuilder(ApplicationDBContext context)
        {
            this.context = context;
        }

        public async Task<CalcResultScaledupProducers> Construct(CalcResultsRequestDto resultsRequestDto,
            CalcResult calcResult)
        {
            var runId = resultsRequestDto.RunId;
            var materialsFromDb = await context.Material.ToListAsync();
            var materials = MaterialMapper.Map(materialsFromDb);

            var scaledupProducersSummary = new CalcResultScaledupProducers();

            var scaleupOrganisationIds = await GetScaledUpOrganisationIds(resultsRequestDto.RunId);
            if (scaleupOrganisationIds.Any() && scaleupOrganisationIds != null)
            {
                List<int> organisationIds = scaleupOrganisationIds.Select(x => x.OrganisationId).ToList();
                var runProducerMaterialDetails = await (from run in context.CalculatorRuns
                                                        join crpdm in context.CalculatorRunPomDataMaster on run.CalculatorRunPomDataMasterId equals crpdm.Id
                                                        join crpdd in context.CalculatorRunPomDataDetails on crpdm.Id equals crpdd.CalculatorRunPomDataMasterId
                                                        join spl in context.SubmissionPeriodLookup on crpdd.SubmissionPeriod equals spl.SubmissionPeriod
                                                        join pd in context.ProducerDetail.Include(x => x.ProducerReportedMaterials) on crpdd.OrganisationId equals pd.ProducerId
                                                        where run.Id == runId && organisationIds.Contains(crpdd.OrganisationId.GetValueOrDefault())
                                                        select new CalcResultScaledupProducer
                                                        {
                                                            ProducerId = pd.ProducerId.ToString(),
                                                            SubsidiaryId = pd.SubsidiaryId,
                                                            ProducerName = pd.ProducerName,
                                                            ScaleupFactor = spl.ScaleupFactor,
                                                            SubmissonPeriodCode = spl.SubmissionPeriod,
                                                            DaysInSubmissionPeriod = spl.DaysInSubmissionPeriod,
                                                            DaysInWholePeriod = spl.DaysInSubmissionPeriod,
                                                            Level = string.Empty,
                                                        }).Distinct().ToListAsync();

                var groupByResult = runProducerMaterialDetails.Where(x => x.SubsidiaryId != null).GroupBy(x => new { x.ProducerId, x.SubsidiaryId }).ToList();

                var allOrganisationPomDetails = await (from run in context.CalculatorRuns
                                        join crpdm in context.CalculatorRunPomDataMaster on run.CalculatorRunPomDataMasterId equals crpdm.Id
                                        join crpdd in context.CalculatorRunPomDataDetails on crpdm.Id equals crpdd.CalculatorRunPomDataMasterId
                                        where run.Id == runId && organisationIds.Contains(crpdd.OrganisationId.GetValueOrDefault())
                                        select crpdd).Distinct().ToListAsync();

                foreach (var pair in groupByResult)
                {
                    var first = pair.ToList().First();
                    // Create a new Something
                    var extraRow = new CalcResultScaledupProducer
                    {
                        ProducerId = pair.Key.ProducerId,
                        SubsidiaryId = pair.Key.SubsidiaryId,
                        ProducerName = first.ProducerName,
                        ScaleupFactor = first.ScaleupFactor,
                        SubmissonPeriodCode = first.SubmissonPeriodCode,
                        DaysInSubmissionPeriod = first.DaysInSubmissionPeriod,
                        DaysInWholePeriod = first.DaysInSubmissionPeriod,
                        Level = string.Empty,
                        ScaledupProducerTonnageByMaterial = new Dictionary<string, CalcResultScaledupProducerTonnage>()
                    };
                    runProducerMaterialDetails.Add(extraRow);
                }
            }

            SetHeaders(scaledupProducersSummary, materials);
            return scaledupProducersSummary;
        }

        public async Task<IEnumerable<ScaleupProducer>> GetScaledUpOrganisationIds(int runId)
        {
            var scaleupProducerIds = await (from run in context.CalculatorRuns
                                            join crpdm in context.CalculatorRunPomDataMaster on run.CalculatorRunPomDataMasterId equals crpdm.Id
                                            join crpdd in context.CalculatorRunPomDataDetails on crpdm.Id equals crpdd.CalculatorRunPomDataMasterId
                                            join spl in context.SubmissionPeriodLookup on crpdd.SubmissionPeriod equals spl.SubmissionPeriod
                                            where run.Id == runId && crpdd.OrganisationId != null && spl.ScaleupFactor > NormalScaleup
                                            select new ScaleupProducer
                                            {
                                                OrganisationId = crpdd.OrganisationId.GetValueOrDefault(),
                                                ScaleupFactor = spl.ScaleupFactor,
                                                SubmissionPeriod = spl.SubmissionPeriod,
                                                DaysInSubmissionPeriod = spl.DaysInSubmissionPeriod,
                                                DaysInWholePeriod = spl.DaysInWholePeriod,
                                            })
                               .Distinct().ToListAsync();
            return scaleupProducerIds == null ? new List<ScaleupProducer>() : scaleupProducerIds;
        }

        private static CalcResultScaledupProducers GetCalcResultScaledupProducers(IEnumerable<ProducerDetail> producers,
            IEnumerable<MaterialDetail> materials,
            IEnumerable<ScaleupProducer> scaleupProducers)
        {
            var scaledupProducersSummary = new CalcResultScaledupProducers();
            var scaledupProducers = new List<CalcResultScaledupProducer>();

            foreach (var producer in producers)
            {
                // We have to write an additional row if a producer have at least one subsidiary
                // This additional row will be the total of this producer and its subsidiaries
                var producersAndSubsidiaries = producers.Where(pd => pd.ProducerId == producer.ProducerId);

                var subsidiaries = producersAndSubsidiaries.Where(p => p.SubsidiaryId != null);


                var submissionPeriods = scaleupProducers
                    .Where(p => p.OrganisationId == producersAndSubsidiaries.First().ProducerId)
                    .Select(p => p.SubmissionPeriod);

                // Make sure the total row is written only once
                if (subsidiaries.Any() &&
                    scaledupProducers.FirstOrDefault(p => p.ProducerId == producer.ProducerId.ToString() && p.ScaleupFactor > NormalScaleup) == null)
                {
                    foreach (var submissionPeriod in submissionPeriods)
                    {
                        var totalRow = GetProducerTotalRow(producersAndSubsidiaries, materials, scaleupProducers, submissionPeriod, false);
                        scaledupProducers.Add(totalRow);
                    }
                }

                if (IsValidScaledupProducer(scaleupProducers, producer))
                {
                    var abc = scaledupProducers.FirstOrDefault(p => p.ProducerId == producer.ProducerId.ToString() && p.SubsidiaryId == producer.SubsidiaryId);
                    if (abc == null)
                    {
                        foreach (var submissionPeriod in submissionPeriods)
                        {
                            var scaledupProducer = GetProducerRow(producer, materials, scaleupProducers, scaledupProducers, submissionPeriod);
                            scaledupProducers.Add(scaledupProducer);
                        }
                    }
                }
            }

            // Calculate the total for all the producers
            var allTotalRow = GetProducerTotalRow(producers, materials, scaleupProducers, string.Empty, true);
            scaledupProducers.Add(allTotalRow);

            scaledupProducersSummary.ScaledupProducers = scaledupProducers;

            return scaledupProducersSummary;
        }

        private static bool IsValidScaledupProducer(IEnumerable<ScaleupProducer> scaledupProducers, ProducerDetail producer)
        {
            var scaledupProducer = scaledupProducers.FirstOrDefault(p => p.OrganisationId == producer.ProducerId && p.ScaleupFactor > NormalScaleup);
            return scaledupProducer != null;

        }

        private static CalcResultScaledupProducer GetProducerRow(ProducerDetail producer,
            IEnumerable<MaterialDetail> materials,
            IEnumerable<ScaleupProducer> scaleupProducers,
            IEnumerable<CalcResultScaledupProducer> scaledupProducers,
            string submissionPeriod)
        {
            var scaleupProducer = scaleupProducers.FirstOrDefault(p => p.OrganisationId == producer.ProducerId && p.SubmissionPeriod == submissionPeriod);
            return new CalcResultScaledupProducer
            {
                ProducerId = producer.ProducerId.ToString(),
                ProducerName = producer.ProducerName ?? string.Empty,
                SubsidiaryId = producer.SubsidiaryId ?? string.Empty,
                Level = GetLevelIndex(scaledupProducers, producer).ToString(),
                SubmissonPeriodCode = scaleupProducer != null ? scaleupProducer.SubmissionPeriod : string.Empty,
                DaysInSubmissionPeriod = scaleupProducer != null ? scaleupProducer.DaysInSubmissionPeriod : 0,
                DaysInWholePeriod = scaleupProducer != null ? scaleupProducer.DaysInWholePeriod : 0,
                ScaleupFactor = scaleupProducer != null ? scaleupProducer.ScaleupFactor : 0,
                ScaledupProducerTonnageByMaterial = GetScaledupProducerTonnages([producer], materials, scaleupProducer.ScaleupFactor, submissionPeriod)
            };
        }

        private static CalcResultScaledupProducer GetProducerTotalRow(IEnumerable<ProducerDetail> producersAndSubsidiaries,
            IEnumerable<MaterialDetail> materials,
            IEnumerable<ScaleupProducer> scaleupProducers,
            string submissionPeriod,
            bool isOverAllTotalRow)
        {
            var producers = producersAndSubsidiaries.ToList();

            var scaleupProducer = isOverAllTotalRow
                ? scaleupProducers.FirstOrDefault(p => p.OrganisationId == producers[0].ProducerId)
                : scaleupProducers.FirstOrDefault(p => p.OrganisationId == producers[0].ProducerId && p.SubmissionPeriod == submissionPeriod);

            return new CalcResultScaledupProducer
            {
                ProducerId = isOverAllTotalRow ? string.Empty : producers[0].ProducerId.ToString(),
                ProducerName = isOverAllTotalRow ? string.Empty : producers[0].ProducerName ?? string.Empty,
                SubsidiaryId = string.Empty,
                isTotalRow = true,
                Level = isOverAllTotalRow ? CommonConstants.Totals : CommonConstants.LevelOne,
                SubmissonPeriodCode = isOverAllTotalRow || scaleupProducer == null ? string.Empty : submissionPeriod,
                DaysInSubmissionPeriod = isOverAllTotalRow || scaleupProducer == null ? -1 : scaleupProducer.DaysInSubmissionPeriod,
                DaysInWholePeriod = isOverAllTotalRow || scaleupProducer == null ? -1 : scaleupProducer.DaysInWholePeriod,
                ScaleupFactor = isOverAllTotalRow || scaleupProducer == null ? -1 : scaleupProducer.ScaleupFactor,
                ScaledupProducerTonnageByMaterial = GetScaledupProducerTonnages(producers, materials, scaleupProducer.ScaleupFactor, submissionPeriod)
            };
        }

        private static Dictionary<string, CalcResultScaledupProducerTonnage> GetScaledupProducerTonnages(
            IEnumerable<ProducerDetail> producers,
            IEnumerable<MaterialDetail> materials,
            decimal scaleUpFactor,
            string submissionPeriod)
        {
            var scaledupProducerTonnages = new Dictionary<string, CalcResultScaledupProducerTonnage>();

            foreach (var material in materials)
            {
                var scaledupProducerTonnage = new CalcResultScaledupProducerTonnage();
                if (producers.Count() > 1)
                {
                    scaledupProducerTonnage.ReportedHouseholdPackagingWasteTonnage = CalcResultSummaryUtil.GetHouseholdPackagingWasteTonnageProducerTotal(producers, material);
                    scaledupProducerTonnage.ReportedPublicBinTonnage = CalcResultSummaryUtil.GetReportedPublicBinTonnageTotal(producers, material);
                    scaledupProducerTonnage.TotalReportedTonnage = scaledupProducerTonnage.ReportedHouseholdPackagingWasteTonnage +
                        scaledupProducerTonnage.ReportedPublicBinTonnage;
                    scaledupProducerTonnage.ReportedSelfManagedConsumerWasteTonnage = CalcResultSummaryUtil.GetManagedConsumerWasteTonnageProducerTotal(producers, material);
                    scaledupProducerTonnage.NetReportedTonnage = CalcResultSummaryUtil.GetNetReportedTonnageProducerTotal(producers, material);
                    scaledupProducerTonnage.ScaledupReportedHouseholdPackagingWasteTonnage = scaledupProducerTonnage.ReportedHouseholdPackagingWasteTonnage * scaleUpFactor;
                    scaledupProducerTonnage.ScaledupReportedPublicBinTonnage = scaledupProducerTonnage.ReportedPublicBinTonnage * scaleUpFactor;
                    scaledupProducerTonnage.ScaledupTotalReportedTonnage = scaledupProducerTonnage.TotalReportedTonnage * scaleUpFactor;
                    scaledupProducerTonnage.ScaledupReportedSelfManagedConsumerWasteTonnage = scaledupProducerTonnage.ReportedSelfManagedConsumerWasteTonnage * scaleUpFactor;
                    scaledupProducerTonnage.ScaledupNetReportedTonnage = scaledupProducerTonnage.NetReportedTonnage * scaleUpFactor;
                }
                else
                {
                    var producer = producers.First();
                    scaledupProducerTonnage.ReportedHouseholdPackagingWasteTonnage = CalcResultSummaryUtil.GetHouseholdPackagingWasteTonnage(producer, material);
                    scaledupProducerTonnage.ReportedPublicBinTonnage = CalcResultSummaryUtil.GetReportedPublicBinTonnage(producer, material);
                    scaledupProducerTonnage.TotalReportedTonnage = scaledupProducerTonnage.ReportedHouseholdPackagingWasteTonnage +
                        scaledupProducerTonnage.ReportedPublicBinTonnage;

                    scaledupProducerTonnage.ReportedSelfManagedConsumerWasteTonnage = CalcResultSummaryUtil.GetManagedConsumerWasteTonnage(producer, material);
                    scaledupProducerTonnage.NetReportedTonnage = CalcResultSummaryUtil.GetNetReportedTonnage(producer, material);
                    scaledupProducerTonnage.ScaledupReportedHouseholdPackagingWasteTonnage = scaledupProducerTonnage.ReportedHouseholdPackagingWasteTonnage * scaleUpFactor;
                    scaledupProducerTonnage.ScaledupReportedPublicBinTonnage = scaledupProducerTonnage.ReportedPublicBinTonnage * scaleUpFactor;
                    scaledupProducerTonnage.ScaledupTotalReportedTonnage = scaledupProducerTonnage.TotalReportedTonnage * scaleUpFactor;
                    scaledupProducerTonnage.ScaledupReportedSelfManagedConsumerWasteTonnage = scaledupProducerTonnage.ReportedSelfManagedConsumerWasteTonnage * scaleUpFactor;
                    scaledupProducerTonnage.ScaledupNetReportedTonnage = scaledupProducerTonnage.NetReportedTonnage * scaleUpFactor;
                }
                scaledupProducerTonnages.Add(material.Name, scaledupProducerTonnage);
            }

            return scaledupProducerTonnages;
        }

        private static void SetHeaders(CalcResultScaledupProducers producers, IEnumerable<MaterialDetail> materials)
        {
            producers.TitleHeader = new CalcResultScaledupProducerHeader
            {
                Name = CalcResultScaledupProducerHeaders.ScaledupProducers,
                ColumnIndex = 1
            };

            producers.MaterialBreakdownHeaders = GetMaterialsBreakdownHeader(materials);

            producers.ColumnHeaders = GetColumnHeaders(materials);
        }

        private static List<CalcResultScaledupProducerHeader> GetMaterialsBreakdownHeader(IEnumerable<MaterialDetail> materials)
        {
            var materialsBreakdownHeaders = new List<CalcResultScaledupProducerHeader>();
            var columnIndex = MaterialsBreakdownHeaderInitialColumnIndex;

            materialsBreakdownHeaders.Add(new CalcResultScaledupProducerHeader
            {
                Name = CalcResultScaledupProducerHeaders.EachSubmissionForTheYear,
                ColumnIndex = 1
            });

            foreach (var material in materials)
            {
                materialsBreakdownHeaders.Add(new CalcResultScaledupProducerHeader
                {
                    Name = $"{material.Name} Breakdown",
                    ColumnIndex = columnIndex
                });

                columnIndex = material.Code == MaterialCodes.Glass
                    ? columnIndex + MaterialsBreakdownHeaderIncrementalColumnIndex + 1
                    : columnIndex + MaterialsBreakdownHeaderIncrementalColumnIndex;
            }

            return materialsBreakdownHeaders;
        }

        private static List<CalcResultScaledupProducerHeader> GetColumnHeaders(IEnumerable<MaterialDetail> materials)
        {
            var columnHeaders = new List<CalcResultScaledupProducerHeader>();

            columnHeaders.AddRange([
                new CalcResultScaledupProducerHeader { Name = CalcResultScaledupProducerHeaders.ProducerId },
                new CalcResultScaledupProducerHeader { Name = CalcResultScaledupProducerHeaders.SubsidiaryId },
                new CalcResultScaledupProducerHeader { Name = CalcResultScaledupProducerHeaders.ProducerOrSubsidiaryName },
                new CalcResultScaledupProducerHeader { Name = CalcResultScaledupProducerHeaders.Level },
                new CalcResultScaledupProducerHeader { Name = CalcResultScaledupProducerHeaders.SubmissionPeriodCode },
                new CalcResultScaledupProducerHeader { Name = CalcResultScaledupProducerHeaders.DaysInSubmissionPeriod },
                new CalcResultScaledupProducerHeader { Name = CalcResultScaledupProducerHeaders.DaysInWholePeriod },
                new CalcResultScaledupProducerHeader { Name = CalcResultScaledupProducerHeaders.ScaleupFactor }
            ]);

            foreach (var material in materials)
            {
                var columnHeadersList = new List<CalcResultScaledupProducerHeader>
                {
                    new CalcResultScaledupProducerHeader { Name = CalcResultScaledupProducerHeaders.ReportedHouseholdPackagingWasteTonnage },
                    new CalcResultScaledupProducerHeader { Name = CalcResultScaledupProducerHeaders.ReportedPublicBinTonnage },
                    new CalcResultScaledupProducerHeader { Name = CalcResultScaledupProducerHeaders.TotalReportedTonnage },
                    new CalcResultScaledupProducerHeader { Name = CalcResultScaledupProducerHeaders.ReportedSelfManagedConsumerWasteTonnage },
                    new CalcResultScaledupProducerHeader { Name = CalcResultScaledupProducerHeaders.NetReportedTonnage },
                    new CalcResultScaledupProducerHeader { Name = CalcResultScaledupProducerHeaders.ScaledupReportedHouseholdPackagingWasteTonnage },
                    new CalcResultScaledupProducerHeader { Name = CalcResultScaledupProducerHeaders.ScaledupReportedPublicBinTonnage },
                    new CalcResultScaledupProducerHeader { Name = CalcResultScaledupProducerHeaders.ScaledupTotalReportedTonnage },
                    new CalcResultScaledupProducerHeader { Name = CalcResultScaledupProducerHeaders.ScaledupReportedSelfManagedConsumerWasteTonnage },
                    new CalcResultScaledupProducerHeader { Name = CalcResultScaledupProducerHeaders.ScaledupNetReportedTonnage },
                };

                columnHeaders.AddRange(columnHeadersList);
            }

            return columnHeaders;
        }

        private static int GetLevelIndex(IEnumerable<CalcResultScaledupProducer> scaledupProducers, ProducerDetail producer)
        {
            var totalRow = scaledupProducers.FirstOrDefault(p => p.ProducerId == producer.ProducerId.ToString() && p.isTotalRow);

            return totalRow == null ? (int)CalcResultSummaryLevelIndex.One : (int)CalcResultSummaryLevelIndex.Two;
        }
    }
}
