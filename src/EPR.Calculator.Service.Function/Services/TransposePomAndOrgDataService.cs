﻿namespace EPR.Calculator.Service.Function.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using EPR.Calculator.Service.Function.Data;
    using EPR.Calculator.Service.Function.Data.DataModels;
    using EPR.Calculator.Service.Function.Dtos;
    using EPR.Calculator.Service.Function.Enums;
    using EPR.Calculator.Service.Function.Interface;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;

    public class TransposePomAndOrgDataService : ITransposePomAndOrgDataService
    {
        private readonly ApplicationDBContext context;
        private const string PeriodSeparator = "-P";
        public class OrganisationDetails
        {
            public int? OrganisationId { get; set; }
            public required string OrganisationName { get; set; }
            public string? SubmissionPeriod { get; set; }
            public string? SubmissionPeriodDescription { get; set; }
            public string? SubsidaryId { get; set; }
        }

        internal class SubmissionDetails
        {
            public string? SubmissionPeriod { get; set; }

            public string? SubmissionPeriodDesc { get; set; }
        }

        public TransposePomAndOrgDataService(
            ApplicationDBContext context,
            ICommandTimeoutService commandTimeoutService,
            IDbLoadingChunkerService<ProducerDetail> producerDetailChunker,
            IDbLoadingChunkerService<ProducerReportedMaterial> producerReportedMaterialChunker)
        {
            this.context = context;
            this.CommandTimeoutService = commandTimeoutService;
            this.ProducerDetailChunker = producerDetailChunker;
            this.ProducerReportedMaterialChunker = producerReportedMaterialChunker;
        }

        public ICommandTimeoutService CommandTimeoutService { get; init; }

        private IDbLoadingChunkerService<ProducerDetail> ProducerDetailChunker { get; init; }

        private IDbLoadingChunkerService<ProducerReportedMaterial> ProducerReportedMaterialChunker { get; init; }

        public async Task<bool> TransposeBeforeCalcResults(
            [FromBody] CalcResultsRequestDto resultsRequestDto,
            CancellationToken cancellationToken)
        {
            var startTime = DateTime.Now;

            this.CommandTimeoutService.SetCommandTimeout(context.Database);

            CalculatorRun? calculatorRun = null;
            try
            {
                calculatorRun = await this.context.CalculatorRuns.SingleOrDefaultAsync(
                run => run.Id == resultsRequestDto.RunId,
                cancellationToken);
                if (calculatorRun == null)
                {
                    return false;
                }

                var isTransposeSuccessful = await this.Transpose(
                    resultsRequestDto,
                    cancellationToken);
                var endTime = DateTime.Now;
                var timeDiff = startTime - endTime;
                return true;
            }
            catch (OperationCanceledException exception)
            {
                if (calculatorRun != null)
                {
                    calculatorRun.CalculatorRunClassificationId = (int)RunClassification.ERROR;
                    this.context.CalculatorRuns.Update(calculatorRun);
                    await this.context.SaveChangesAsync();
                }

                return false;
            }
            catch (Exception exception)
            {
                if (calculatorRun != null)
                {
                    calculatorRun.CalculatorRunClassificationId = (int)RunClassification.ERROR;
                    this.context.CalculatorRuns.Update(calculatorRun);
                    await this.context.SaveChangesAsync();
                }

                return false;
            }
        }

        public async Task<bool> Transpose(CalcResultsRequestDto resultsRequestDto, CancellationToken cancellationToken)
        {
            context.ChangeTracker.AutoDetectChangesEnabled = false;
            var newProducerDetails = new List<ProducerDetail>();
            var newProducerReportedMaterials = new List<ProducerReportedMaterial>();

            var materials = await this.context.Material.ToListAsync(cancellationToken);

            var calculatorRun = await context.CalculatorRuns
                .Where(x => x.Id == resultsRequestDto.RunId)
                .SingleAsync(cancellationToken);
            var calculatorRunPomDataDetails = await context.CalculatorRunPomDataDetails
                .Where(x => x.CalculatorRunPomDataMasterId == calculatorRun.CalculatorRunPomDataMasterId)
                .OrderBy(x => x.SubmissionPeriodDesc)
                .ToListAsync(cancellationToken);
            var calculatorRunOrgDataDetails = await context.CalculatorRunOrganisationDataDetails
                .Where(x => x.CalculatorRunOrganisationDataMasterId == calculatorRun.CalculatorRunOrganisationDataMasterId)
                .OrderBy(x => x.SubmissionPeriodDesc)
                .ToListAsync(cancellationToken);

            if (calculatorRun.CalculatorRunPomDataMasterId != null)
            {
                var organisationDataMaster = await context.CalculatorRunOrganisationDataMaster
                    .SingleAsync(x => x.Id == calculatorRun.CalculatorRunOrganisationDataMasterId, cancellationToken);

                var SubmissionPeriodDetails = (from s in calculatorRunPomDataDetails
                                               where s.CalculatorRunPomDataMasterId == calculatorRun.CalculatorRunPomDataMasterId
                                               select new SubmissionDetails
                                               {
                                                   SubmissionPeriod = s.SubmissionPeriod,
                                                   SubmissionPeriodDesc = s.SubmissionPeriodDesc
                                               }
                                        ).Distinct().ToList();

                var OrganisationsList = GetAllOrganisationsBasedonRunId(calculatorRunOrgDataDetails);

                var OrganisationsBySubmissionPeriod = GetOrganisationDetailsBySubmissionPeriod(OrganisationsList, SubmissionPeriodDetails).ToList();

                var organisationDataDetails = calculatorRunOrgDataDetails
                    .Where(odd => odd.CalculatorRunOrganisationDataMasterId == organisationDataMaster.Id && odd.OrganisationName != null && odd.OrganisationName != "")
                    .OrderBy(odd => odd.OrganisationName)
                    .GroupBy(odd => new { odd.OrganisationId, odd.SubsidaryId })
                    .Select(odd => odd.First())
                    .ToList();

                // Get the calculator run pom data master record based on the CalculatorRunPomDataMasterId
                var pomDataMaster = await context.CalculatorRunPomDataMaster
                    .SingleAsync(x => x.Id == calculatorRun.CalculatorRunPomDataMasterId, cancellationToken);


                foreach (var organisation in organisationDataDetails.Where(t => !string.IsNullOrWhiteSpace(t.OrganisationName)))
                {
                    // Initialise the producerReportedMaterials
                    var producerReportedMaterials = new List<ProducerReportedMaterial>();

                    // Get the calculator run pom data details related to the calculator run pom data master
                    var runPomDataDetailsForSubsidaryId = calculatorRunPomDataDetails.Where
                        (
                            pdd => pdd.CalculatorRunPomDataMasterId == pomDataMaster.Id &&
                            pdd.OrganisationId == organisation.OrganisationId &&
                            pdd.SubsidaryId == organisation.SubsidaryId
                        ).ToList();

                    // Proceed further only if there is any pom data based on the pom data master id and organisation id
                    // TO DO: We have to record if there is no pom data in a separate table post Dec 2024
                    if (runPomDataDetailsForSubsidaryId.Count > 0)
                    {
                        var organisations = organisationDataDetails.Where(odd => odd.OrganisationName == organisation.OrganisationName && odd.SubsidaryId == organisation.SubsidaryId).OrderByDescending(odd => odd.SubmissionPeriodDesc);

                        // Get the producer based on the latest submission period
                        var producer = organisations.FirstOrDefault();

                        // Proceed further only if the organisation is not null and organisation id not null
                        // TO DO: We have to record if the organisation name is null in a separate table post Dec 2024
                        if (producer != null && producer.OrganisationId != null)
                        {
                            var producerDetail = new ProducerDetail
                            {
                                CalculatorRunId = resultsRequestDto.RunId,
                                ProducerId = producer.OrganisationId.Value,
                                SubsidiaryId = producer.SubsidaryId,
                                ProducerName = string.IsNullOrWhiteSpace(producer.SubsidaryId) ? GetLatestOrganisationName(producer.OrganisationId.Value, OrganisationsBySubmissionPeriod, OrganisationsList) : GetLatestSubsidaryName(producer.OrganisationId.Value, producer.SubsidaryId, OrganisationsBySubmissionPeriod, OrganisationsList),
                                CalculatorRun = calculatorRun
                            };

                            // Add producer detail record to the database context
                            newProducerDetails.Add(producerDetail);

                            foreach (var material in materials)
                            {
                                var pomDataDetailsByMaterial = runPomDataDetailsForSubsidaryId.Where(pdd => pdd.PackagingMaterial == material.Code).GroupBy(pdd => pdd.PackagingType);

                                foreach (var pomData in pomDataDetailsByMaterial)
                                {
                                    var pom = pomData.AsEnumerable();
                                    var packagingType = pom.FirstOrDefault()?.PackagingType;
                                    var totalPackagingMaterialWeight = pom.Sum(x => x.PackagingMaterialWeight);

                                    // Proceed further only if the packaging type and packaging material weight is not null
                                    // TO DO: We have to record if the packaging type or packaging material weight is null in a separate table post Dec 2024
                                    if (packagingType != null && totalPackagingMaterialWeight != null)
                                    {
                                        var producerReportedMaterial = new ProducerReportedMaterial
                                        {
                                            MaterialId = material.Id,
                                            Material = material,
                                            ProducerDetail = producerDetail,
                                            PackagingType = packagingType,
                                            PackagingTonnage = Math.Round((decimal)(totalPackagingMaterialWeight) / 1000, 3),
                                        };

                                        // Populate the producer reported material list
                                        producerReportedMaterials.Add(producerReportedMaterial);
                                    }
                                }
                            }

                            // Add the list of producer reported materials to the database context
                            newProducerReportedMaterials.AddRange(producerReportedMaterials);
                        }
                    }
                }

                await this.ProducerDetailChunker.InsertRecords(newProducerDetails);
                await this.ProducerReportedMaterialChunker.InsertRecords(newProducerReportedMaterials);
            }

            return true;
        }

        private static List<OrganisationDetails> GetOrganisationDetailsBySubmissionPeriod(
            IEnumerable<OrganisationDetails> organisationsList,
            IEnumerable<SubmissionDetails> submissionPeriodDetails)
        {
            var list = new List<OrganisationDetails>();
            foreach (var org in organisationsList)
            {
                if (submissionPeriodDetails.Any(x => x.SubmissionPeriodDesc == org.SubmissionPeriodDescription))
                {
                    var sub = submissionPeriodDetails.First(x => x.SubmissionPeriodDesc == org.SubmissionPeriodDescription);
                    list.Add(new OrganisationDetails
                    {
                        OrganisationId = org.OrganisationId,
                        OrganisationName = org.OrganisationName,
                        SubmissionPeriodDescription = org.SubmissionPeriodDescription,
                        SubmissionPeriod = sub.SubmissionPeriod,
                        SubsidaryId = org.SubsidaryId
                    });
                }
            }
            return list;
        }

        public IEnumerable<OrganisationDetails> GetAllOrganisationsBasedonRunId(
            IEnumerable<CalculatorRunOrganisationDataDetail> calculatorRunOrganisationDataDetails)
        {
            return calculatorRunOrganisationDataDetails.Where(org => org.OrganisationName != null)
                .Select(org =>
                    new OrganisationDetails
                    {
                        OrganisationId = org.OrganisationId,
                        OrganisationName = org.OrganisationName,
                        SubmissionPeriodDescription = org.SubmissionPeriodDesc,
                        SubsidaryId = org.SubsidaryId
                    }).Distinct();
        }

        public string? GetLatestOrganisationName(int orgId, List<OrganisationDetails> organisationsBySubmissionPeriod, IEnumerable<OrganisationDetails> organisationsList)
        {
            if (organisationsBySubmissionPeriod is null) return string.Empty;

            var organisations = organisationsBySubmissionPeriod.Where(t => t.OrganisationId == orgId && t.SubsidaryId == null).OrderByDescending(t => t.SubmissionPeriod?.Replace(PeriodSeparator, string.Empty)).ToList();

            var orgName = organisations?.FirstOrDefault(t => t.OrganisationId == orgId)?.OrganisationName;
            return string.IsNullOrWhiteSpace(orgName) ? organisationsList.FirstOrDefault(t => t.OrganisationId == orgId)?.OrganisationName : orgName;
        }

        public string? GetLatestSubsidaryName(int orgId, string? subsidaryId, List<OrganisationDetails> organisationsBySubmissionPeriod, IEnumerable<OrganisationDetails> organisationsList)
        {
            if (organisationsBySubmissionPeriod is null) return string.Empty;
            var subsidaries = organisationsBySubmissionPeriod.Where(t => t.OrganisationId == orgId && t.SubsidaryId == subsidaryId).OrderByDescending(t => t.SubmissionPeriod?.Replace(PeriodSeparator, string.Empty)).ToList();
            var subsidaryName = subsidaries?.FirstOrDefault(t => t.OrganisationId == orgId && t.SubsidaryId == subsidaryId)?.OrganisationName;
            return string.IsNullOrWhiteSpace(subsidaryName) ? organisationsList.FirstOrDefault(t => t.OrganisationId == orgId && t.SubsidaryId == subsidaryId)?.OrganisationName : subsidaryName;
        }
    }
}
