using System.Net.Mime;
using System.Text;
using System.Text.Json;
using EPR.Calculator.Service.Common;
using EPR.Calculator.Service.Common.Logging;
using EPR.Calculator.Service.Function.Dtos;
using EPR.Calculator.Service.Function.Enums;
using EPR.Calculator.Service.Function.Interface;
using EPR.Calculator.Service.Function.Services.DataLoading;

namespace EPR.Calculator.Service.Function.Services
{
    public class CalculatorRunService(
        IConfigurationService configuration,
        IDataLoader dataLoader,
        ITransposePomAndOrgDataService transposePomAndOrgDataService,
        IPrepareCalcService prepareCalcService,
        IRpdStatusService statusService,
        ICalculatorTelemetryLogger telemetryLogger)
        : ICalculatorRunService
    {
        public async Task<bool> PrepareResultsFileAsync(CalculatorRunParameter calculatorRunParameter, string runName)
        {
            try
            {
                telemetryLogger.LogInformation(new TrackMessage
                {
                    RunId = calculatorRunParameter.Id,
                    RunName = runName,
                    Message = "Process started"
                });

                await dataLoader.LoadData(calculatorRunParameter, runName);
                return await RunResultsFileCalculationAsync(calculatorRunParameter, runName);
            }
            catch (TaskCanceledException ex)
            {
                LogError(calculatorRunParameter.Id, runName, "StartProcess - Task was canceled", ex);
                return false;
            }
            catch (Exception ex)
            {
                LogError(calculatorRunParameter.Id, runName, "StartProcess - An error occurred", ex);
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


        private async Task<bool> RunResultsFileCalculationAsync(CalculatorRunParameter calculatorRunParameter,
            string runName)
        {
            var isSuccess = false;

            var statusUpdateResponse = await LogAndUpdateStatus(calculatorRunParameter, runName);

            if (statusUpdateResponse == RunClassification.RUNNING)
            {
                var isTransposeSuccess = await transposePomAndOrgDataService.TransposeBeforeResultsFileAsync(
                    new CalcResultsRequestDto { RunId = calculatorRunParameter.Id, RelativeYear = calculatorRunParameter.RelativeYear, CreatedBy = calculatorRunParameter.User },
                    runName,
                    new CancellationTokenSource(configuration.TransposeTimeout).Token);

                LogInformation(calculatorRunParameter.Id, runName,
                    $"UpdateStatusAndPrepareResult - transposeResultResponse: {isTransposeSuccess}");

                if (!isTransposeSuccess)
                {
                    return false;
                }

                isSuccess = await prepareCalcService.PrepareCalcResultsAsync(
                    new CalcResultsRequestDto { RunId = calculatorRunParameter.Id, RelativeYear = calculatorRunParameter.RelativeYear },
                    runName,
                    new CancellationTokenSource(configuration.PrepareCalcResultsTimeout).Token);

                LogInformation(calculatorRunParameter.Id, runName,
                    $"UpdateStatusAndPrepareResult - prepareCalcResultResponse: {isSuccess}");
            }

            LogInformation(calculatorRunParameter.Id, runName,
                $"UpdateStatusAndPrepareResult - StatusEndPoint: {configuration.PrepareCalcResultEndPoint}");
            LogInformation(calculatorRunParameter.Id, runName,
                $"UpdateStatusAndPrepareResult - CalculatorRunParameter ID: {calculatorRunParameter.Id}");
            LogInformation(calculatorRunParameter.Id, runName,
                $"UpdateStatusAndPrepareResult - GetPrepareCalcResultMessage: {GetCalcResultMessage(calculatorRunParameter.Id)}");

            return isSuccess;
        }

        private async Task<RunClassification> LogAndUpdateStatus(CalculatorRunParameter calculatorRunParameter,
            string runName)
        {
            LogInformation(calculatorRunParameter.Id, runName,
                $"UpdateStatusAndPrepareResult - StatusEndPoint: {configuration.StatusEndpoint}");
            var statusUpdateResponse = await statusService.UpdateRpdStatus(
                calculatorRunParameter.Id,
                runName,
                calculatorRunParameter.User,
                new CancellationTokenSource(configuration.RpdStatusTimeout).Token);
            LogInformation(calculatorRunParameter.Id, runName,
                $"UpdateStatusAndPrepareResult - Status Response: {statusUpdateResponse}");
            return statusUpdateResponse;
        }

        private void LogInformation(int runId, string runName, string message)
        {
            telemetryLogger.LogInformation(new TrackMessage
            {
                RunId = runId,
                RunName = runName,
                Message = message
            });
        }

        private void LogError(int runId, string runName, string message, Exception ex)
        {
            telemetryLogger.LogError(new ErrorMessage
            {
                RunId = runId,
                RunName = runName,
                Message = message,
                Exception = ex
            });
        }
    }
}