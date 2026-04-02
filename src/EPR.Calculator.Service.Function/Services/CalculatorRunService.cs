using System.Net.Mime;
using System.Text;
using System.Text.Json;
using EPR.Calculator.Service.Function.Enums;
using EPR.Calculator.Service.Function.Interface;
using EPR.Calculator.Service.Function.Misc;
using EPR.Calculator.Service.Function.Services.DataLoading;
using Microsoft.Extensions.Logging;

namespace EPR.Calculator.Service.Function.Services
{
    public class CalculatorRunService(
        IConfigurationService configuration,
        IDataLoader dataLoader,
        ITransposePomAndOrgDataService transposePomAndOrgDataService,
        IPrepareCalcService prepareCalcService,
        IRpdStatusService statusService,
        ILogger<CalculatorRunService> logger)
        : ICalculatorRunService
    {
        public async Task<bool> PrepareResultsFileAsync(CalculatorRunParams calculatorRunParams)
        {
            try
            {
                logger.LogInformation("Process started");
                await dataLoader.LoadData(calculatorRunParams);
                return await RunResultsFileCalculationAsync(calculatorRunParams);
            }
            catch (TaskCanceledException ex)
            {
                logger.LogError(ex, "StartProcess - Task was canceled");
                return false;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "StartProcess - An error occurred");
                return false;
            }
        }

        /// <summary>
        ///     Creates a JSON message containing the calculator run ID for preparing calculation results.
        /// </summary>
        /// <param name="calculatorRunId">The ID of the calculator run.</param>
        /// <returns>A <see cref="StringContent" /> object containing the JSON message.</returns>
        public static StringContent GetCalcResultMessage(int calculatorRunId)
        {
            var calcResultsRequest = new
            {
                runId = calculatorRunId
            };

            return new StringContent(
                JsonSerializer.Serialize(calcResultsRequest),
                Encoding.UTF8,
                MediaTypeNames.Application.Json);
        }


        private async Task<bool> RunResultsFileCalculationAsync(CalculatorRunParams calculatorRunParams)
        {
            var isSuccess = false;

            var statusUpdateResponse = await LogAndUpdateStatus(calculatorRunParams);

            if (statusUpdateResponse == RunClassification.RUNNING)
            {
                var isTransposeSuccess = await transposePomAndOrgDataService.TransposeBeforeResultsFileAsync(
                    new CalcResultsRequestDto { RunId = calculatorRunParams.Id, RelativeYear = calculatorRunParams.RelativeYear, CreatedBy = calculatorRunParams.User },
                    calculatorRunParams.Name,
                    new CancellationTokenSource(configuration.TransposeTimeout).Token);

                logger.LogInformation("Transpose completed. IsSuccess: {IsTransposeSuccess}", isTransposeSuccess);

                if (!isTransposeSuccess)
                {
                    return false;
                }

                isSuccess = await prepareCalcService.PrepareCalcResultsAsync(
                    new CalcResultsRequestDto { RunId = calculatorRunParams.Id, RelativeYear = calculatorRunParams.RelativeYear },
                    calculatorRunParams.Name,
                    new CancellationTokenSource(configuration.PrepareCalcResultsTimeout).Token);

                logger.LogInformation("PrepareCalcResults completed. IsSuccess: {IsSuccess}", isSuccess);
            }

            return isSuccess;
        }

        private async Task<RunClassification> LogAndUpdateStatus(CalculatorRunParams calculatorRunParams)
        {
            var statusUpdateResponse = await statusService.UpdateRpdStatus(
                calculatorRunParams.Id,
                calculatorRunParams.Name,
                calculatorRunParams.User,
                new CancellationTokenSource(configuration.RpdStatusTimeout).Token);
            logger.LogInformation("RPD status update response: {StatusResponse}", statusUpdateResponse);
            return statusUpdateResponse;
        }
    }
}