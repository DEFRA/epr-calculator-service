using EPR.Calculator.API.Data;
using EPR.Calculator.Service.Function.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EPR.Calculator.Service.Function.Services
{
    public class TransposePomAndOrgDataMYCService : ITransposePomAndOrgDataMYCService
    {
        private readonly ApplicationDBContext context;

        public TransposePomAndOrgDataMYCService(
            ApplicationDBContext context)
        {
            this.context = context;
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
            var scenarioService = new OrgAndPomDataMYCScenarioService(this.context);

            var calculatorRun = await this.context.CalculatorRuns
                .Where(x => x.Id == resultsRequestDto.RunId)
                .SingleAsync(cancellationToken);

            var calculatorRunOrgDataDetails = await this.context.CalculatorRunOrganisationDataDetails
                .Where(x => x.CalculatorRunOrganisationDataMasterId == calculatorRun.CalculatorRunOrganisationDataMasterId)
                .OrderBy(x => x.SubmissionPeriodDesc)
                .ToListAsync(cancellationToken);

            var calculatorRunPomDataDetails = await this.context.CalculatorRunPomDataDetails
                            .Where(x => x.CalculatorRunPomDataMasterId == calculatorRun.CalculatorRunPomDataMasterId)
                            .OrderBy(x => x.SubmissionPeriodDesc)
                            .ToListAsync(cancellationToken);

            await scenarioService.HandleMYCScenarios(resultsRequestDto.RunId, cancellationToken);


            return false;
        }
    }
}
