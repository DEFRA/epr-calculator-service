namespace EPR.Calculator.Service.Function.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using EPR.Calculator.Service.Common.Logging;
    using EPR.Calculator.API.Data;
    using EPR.Calculator.API.Data.DataModels;
    using EPR.Calculator.Service.Function.Dtos;
    using EPR.Calculator.Service.Function.Enums;
    using EPR.Calculator.Service.Function.Interface;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;

    /// <summary>
    /// Service for transposing POM and organization data.
    /// </summary>
    public class TransposePomAndOrgDataService : ITransposePomAndOrgDataService
    {
        private readonly ApplicationDBContext context;
        private readonly ICalculatorTelemetryLogger telemetryLogger;

        public class OrganisationDetails
        {
            public int? OrganisationId { get; set; }

            public required string OrganisationName { get; set; }

            public string? TradingName { get; set; }

            public string? SubmissionPeriod { get; set; }

            public string? SubmissionPeriodDescription { get; set; }

            public string? SubsidaryId { get; set; }
        }

        public TransposePomAndOrgDataService(
            ApplicationDBContext context,
            ICommandTimeoutService commandTimeoutService,
            IDbLoadingChunkerService<ProducerDetail> producerDetailChunker,
            IDbLoadingChunkerService<ProducerReportedMaterial> producerReportedMaterialChunker,
            IErrorReportService errorReportService,
            ICalculatorTelemetryLogger telemetryLogger)
        {
            this.context = context;
            this.CommandTimeoutService = commandTimeoutService;
            this.ProducerDetailChunker = producerDetailChunker;
            this.ProducerReportedMaterialChunker = producerReportedMaterialChunker;
            this.ErrorReportService = errorReportService;
            this.telemetryLogger = telemetryLogger;
        }

        public ICommandTimeoutService CommandTimeoutService { get; init; }

        private IDbLoadingChunkerService<ProducerDetail> ProducerDetailChunker { get; init; }

        private IDbLoadingChunkerService<ProducerReportedMaterial> ProducerReportedMaterialChunker { get; init; }

        private IErrorReportService ErrorReportService { get; init; }

        public async Task<bool> TransposeBeforeResultsFileAsync(
            [FromBody] CalcResultsRequestDto resultsRequestDto,
            string? runName,
            CancellationToken cancellationToken)
        {
            var startTime = DateTime.UtcNow;

            this.CommandTimeoutService.SetCommandTimeout(context.Database);

            CalculatorRun? calculatorRun = null;
            try
            {
                this.telemetryLogger.LogInformation(new TrackMessage
                {
                    RunId = resultsRequestDto.RunId,
                    RunName = runName,
                    Message = $"Transpose POM and ORG data for run: {resultsRequestDto.RunId}",
                });
                calculatorRun = await this.context.CalculatorRuns.SingleOrDefaultAsync(
                run => run.Id == resultsRequestDto.RunId,
                cancellationToken);
                if (calculatorRun == null)
                {
                    return false;
                }

                await this.Transpose(
                    resultsRequestDto,
                    cancellationToken);
                var endTime = DateTime.UtcNow;
                var timeDiff = startTime - endTime;
                this.telemetryLogger.LogInformation(new TrackMessage
                {
                    RunId = resultsRequestDto.RunId,
                    RunName = runName,
                    Message = $"Transpose POM and ORG data for run: {resultsRequestDto.RunId} completed in {timeDiff.TotalSeconds} seconds",
                });
                return true;
            }
            catch (OperationCanceledException exception)
            {
                this.telemetryLogger.LogError(new ErrorMessage
                {
                    RunId = resultsRequestDto.RunId,
                    RunName = runName,
                    Message = "Operation cancelled",
                    Exception = exception,
                });

                if (calculatorRun != null)
                {
                    calculatorRun.CalculatorRunClassificationId = (int)RunClassification.ERROR;
                    this.context.CalculatorRuns.Update(calculatorRun);
                    await this.context.SaveChangesAsync();
                    this.telemetryLogger.LogError(new ErrorMessage
                    {
                        RunId = resultsRequestDto.RunId,
                        RunName = runName,
                        Message = "RunId is updated with ClassificationId Error",
                        Exception = exception,
                    });
                }

                return false;
            }
            catch (Exception exception)
            {
                this.telemetryLogger.LogError(new ErrorMessage
                {
                    RunId = resultsRequestDto.RunId,
                    RunName = runName,
                    Message = "Error occurred while transposing POM and ORG data",
                    Exception = exception,
                });
                if (calculatorRun != null)
                {
                    calculatorRun.CalculatorRunClassificationId = (int)RunClassification.ERROR;
                    this.context.CalculatorRuns.Update(calculatorRun);
                    await this.context.SaveChangesAsync();
                    this.telemetryLogger.LogError(new ErrorMessage
                    {
                        RunId = resultsRequestDto.RunId,
                        RunName = runName,
                        Message = "RunId is updated with ClassificationId Error",
                        Exception = exception,
                    });
                }
            }

            return false;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Critical Code Smell",
            "S3776:Cognitive Complexity of methods should not be too high",
            Justification = "Temporaraly suppress - will refactor later.")]

        public async Task<bool> Transpose(CalcResultsRequestDto resultsRequestDto, CancellationToken cancellationToken)
        {
            this.context.ChangeTracker.AutoDetectChangesEnabled = false;
            var newProducerDetails = new List<ProducerDetail>();
            var newProducerReportedMaterials = new List<ProducerReportedMaterial>();

            var materials = await this.context.Material.ToListAsync(cancellationToken);

            var calculatorRun = await this.context.CalculatorRuns
                .Where(x => x.Id == resultsRequestDto.RunId)
                .SingleAsync(cancellationToken);
            var calculatorRunOrgDataDetails = await this.context.CalculatorRunOrganisationDataDetails
                .Where(x => x.CalculatorRunOrganisationDataMasterId == calculatorRun.CalculatorRunOrganisationDataMasterId)
                .ToListAsync(cancellationToken);
            var calculatorRunPomDataDetails = await this.context.CalculatorRunPomDataDetails
                .Where(x => x.CalculatorRunPomDataMasterId == calculatorRun.CalculatorRunPomDataMasterId)
                .ToListAsync(cancellationToken);

            var unmatchedSet = await ErrorReportService.HandleErrors(
                calculatorRunPomDataDetails,
                calculatorRunOrgDataDetails,
                resultsRequestDto.RunId,
                resultsRequestDto.CreatedBy,
                cancellationToken);

            calculatorRunPomDataDetails = calculatorRunPomDataDetails
                                            .Where(p =>
                                            {
                                                var orgId = p.OrganisationId.GetValueOrDefault();
                                                var subId = p.SubsidiaryId;
                                                return !unmatchedSet.Contains((orgId, subId));
                                            }).ToList();

            if (IsCalculatorRunPOMMasterIdExists(calculatorRun))
            {
                var organisationDataMaster = await this.context.CalculatorRunOrganisationDataMaster
                    .SingleAsync(x => x.Id == calculatorRun.CalculatorRunOrganisationDataMasterId, cancellationToken);

                var OrganisationsList = GetAllOrganisationsBasedonRunId(calculatorRunOrgDataDetails);


                var organisationDataDetails = calculatorRunOrgDataDetails
                    .Where(odd => odd.CalculatorRunOrganisationDataMasterId == organisationDataMaster.Id && odd.OrganisationName != null && odd.OrganisationName != "" && ObligationStates.IsObligated(odd.ObligationStatus))
                    .OrderBy(odd => odd.OrganisationName)
                    .GroupBy(odd => new { odd.OrganisationId, odd.SubsidiaryId, odd.SubmitterId })
                    .Select(odd => odd.First())
                    .ToList();

                // Get the calculator run pom data master record based on the CalculatorRunPomDataMasterId
                var pomDataMaster = await this.context.CalculatorRunPomDataMaster
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
                            pdd.SubsidiaryId == organisation.SubsidiaryId 
                        ).ToList();

                    // Proceed further only if there is any pom data based on the pom data master id and organisation id
                    // TO DO: We have to record if there is no pom data in a separate table post Dec 2024
                    if (IsRunPomDataDetailsExistsForSubsidaryId(runPomDataDetailsForSubsidaryId))
                    {
                        var organisations = organisationDataDetails.Where(odd => odd.OrganisationName == organisation.OrganisationName && odd.SubsidiaryId == organisation.SubsidiaryId);

                        // Get the producer based on the latest submission period
                        var producer = organisations.FirstOrDefault();

                        // Proceed further only if the organisation is not null and organisation id not null
                        // TO DO: We have to record if the organisation name is null in a separate table post Dec 2024
                        if (producer != null)
                        {
                            var producerDetail = new ProducerDetail
                            {
                                CalculatorRunId = resultsRequestDto.RunId,
                                ProducerId = producer.OrganisationId,
                                TradingName = organisation.TradingName,
                                SubsidiaryId = producer.SubsidiaryId,
                                ProducerName = GetLatestproducerName(producer.OrganisationId, producer.SubsidiaryId, OrganisationsList),
                                CalculatorRun = calculatorRun,
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
                                    var totalPackagingMaterialWeight = pom.Sum(x => x.PackagingMaterialWeight) ?? 0;

                                    // Proceed further only if the packaging type and packaging material weight is not null
                                    // TO DO: We have to record if the packaging type or packaging material weight is null in a separate table post Dec 2024
                                    if (IsPackagingTypeAndPackagingMaterialWeightExists(packagingType, totalPackagingMaterialWeight))
                                    {
                                        var producerReportedMaterial = new ProducerReportedMaterial
                                        {
                                            MaterialId = material.Id,
                                            Material = material,
                                            ProducerDetail = producerDetail,
                                            PackagingType = packagingType!,
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

        private static bool IsPackagingTypeAndPackagingMaterialWeightExists(string? packagingType, double? totalPackagingMaterialWeight)
        {
            return packagingType != null && totalPackagingMaterialWeight != null;
        }       

        private static bool IsRunPomDataDetailsExistsForSubsidaryId(List<CalculatorRunPomDataDetail> runPomDataDetailsForSubsidaryId)
        {
            return runPomDataDetailsForSubsidaryId.Count > 0;
        }

        private static bool IsCalculatorRunPOMMasterIdExists(CalculatorRun calculatorRun)
        {
            return calculatorRun.CalculatorRunPomDataMasterId != null;
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
                        TradingName = org.TradingName,
                        SubsidaryId = org.SubsidiaryId,
                    }).Distinct();
        }

        public string? GetLatestOrganisationName(int orgId, IEnumerable<OrganisationDetails> organisationsList)
        {
            var organisation = organisationsList.FirstOrDefault(t => t.OrganisationId == orgId && t.SubsidaryId == null);
            var orgName = organisation?.OrganisationName;
            return orgName;
        }

        public string? GetLatestproducerName(int orgId, string? subsidaryId, IEnumerable<OrganisationDetails> organisationsList)
        {

            var organisation = subsidaryId is null ? organisationsList.FirstOrDefault(t => t.OrganisationId == orgId && t.SubsidaryId == null) :
                organisationsList.FirstOrDefault(t => t.OrganisationId == orgId && t.SubsidaryId == subsidaryId);

            var subsidaryName = organisation?.OrganisationName;
            return subsidaryName;
        }
    }
}