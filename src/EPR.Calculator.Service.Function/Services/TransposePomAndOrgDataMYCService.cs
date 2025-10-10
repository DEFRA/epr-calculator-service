using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Common.Logging;
using EPR.Calculator.Service.Function.Dtos;
using EPR.Calculator.Service.Function.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EPR.Calculator.Service.Function.Services
{
    public class TransposePomAndOrgDataMYCService : ITransposePomAndOrgDataMYCService
    {
        private readonly ApplicationDBContext context;

        private readonly ICalculatorTelemetryLogger telemetryLogger;

        public ICommandTimeoutService CommandTimeoutService { get; init; }

        private IDbLoadingChunkerService<ProducerDetail> ProducerDetailChunker { get; init; }

        private IDbLoadingChunkerService<ProducerReportedMaterial> ProducerReportedMaterialChunker { get; init; }

        public TransposePomAndOrgDataMYCService(
            ApplicationDBContext context,
            ICommandTimeoutService commandTimeoutService,
            IDbLoadingChunkerService<ProducerDetail> producerDetailChunker,
            IDbLoadingChunkerService<ProducerReportedMaterial> producerReportedMaterialChunker,
            ICalculatorTelemetryLogger telemetryLogger)
        {
            this.context = context;
            this.CommandTimeoutService = commandTimeoutService;
            this.ProducerDetailChunker = producerDetailChunker;
            this.ProducerReportedMaterialChunker = producerReportedMaterialChunker;
            this.telemetryLogger = telemetryLogger;
        }

        public async Task<bool> TransposeBeforeResultsFileAsync(
            [FromBody] CalcResultsRequestDto resultsRequestDto,
            string? runName,
            CancellationToken cancellationToken)
        {
            try
            {
                var status = await this.Transpose(
                    resultsRequestDto,
                    cancellationToken);

                return status;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private async Task<bool> Transpose(CalcResultsRequestDto resultsRequestDto, CancellationToken cancellationToken)
        {
            var producers = new List<ProducerDetail>();
            var producerReportedMaterials = new List<ProducerReportedMaterial>();

            try
            {

                var calculatorRun = await this.context.CalculatorRuns
                    .Where(x => x.Id == resultsRequestDto.RunId)
                    .SingleAsync(cancellationToken);

                // TODO: Include the new column filter "isValid = true"
                var calculatorRunOrgDataDetails = await this.context.CalculatorRunOrganisationDataDetails
                    .Where(x => x.CalculatorRunOrganisationDataMasterId == calculatorRun.CalculatorRunOrganisationDataMasterId)
                    .OrderBy(x => x.SubmissionPeriodDesc)
                    .ToListAsync(cancellationToken);

                // TODO: Include the new column filter "isValid = true"
                var calculatorRunPomDataDetails = await this.context.CalculatorRunPomDataDetails
                                .Where(x => x.CalculatorRunPomDataMasterId == calculatorRun.CalculatorRunPomDataMasterId)
                                .OrderBy(x => x.SubmissionPeriodDesc)
                                .ToListAsync(cancellationToken);

                // Populate producers
                producers.AddRange(
                    calculatorRunOrgDataDetails.Select(orgData => new ProducerDetail
                    {
                        Id = orgData.Id,
                        ProducerId = orgData.OrganisationId ?? 0,
                        TradingName = orgData.TradingName,
                        SubsidiaryId = orgData.SubsidaryId,
                        ProducerName = orgData.OrganisationName,
                        CalculatorRunId = resultsRequestDto.RunId,

                        // TODO: New column values to be added here
                        // ObligatedDays = scenarioService.GetObligatedDays(),
                        // ObligatedPercentage = scenarioService.GetObligatedPercentage(),
                        // ADR: https://eaflood.atlassian.net/wiki/spaces/MWR/pages/5861376146/ADR-120.C+MYC+Changes
                    })
                );

                var scenarioService = new OrgAndPomDataMYCScenarioService(producers, calculatorRunPomDataDetails);

                producerReportedMaterials.AddRange(scenarioService.GetProducerReportedMaterials());

                await this.ProducerDetailChunker.InsertRecords(producers);
                await this.ProducerReportedMaterialChunker.InsertRecords(producerReportedMaterials);

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
