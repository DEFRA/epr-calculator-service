using System.Net.Mime;
using System.Text;
using System.Text.Json;
using EPR.Calculator.Service.Function.Enums;
using EPR.Calculator.Service.Function.Misc;
using EPR.Calculator.Service.Function.Services.DataLoading;

namespace EPR.Calculator.Service.Function.Services
{
    /// <summary>
    /// Interface for starting the calculator process.
    /// </summary>
    public interface ICalculatorRunService
    {
        /// <summary>
        /// Interface method to start the calculator process.
        /// </summary>
        /// <param name="calculatorRunParameter">The parameters required to run the calculator.</param>
        /// <param name="runName">The name of the calculator run.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a boolean indicating success or failure.</returns>
        Task<bool> PrepareResultsFileAsync(CalculatorRunParameter calculatorRunParameter, string runName);
    }

    public class CalculatorRunService(
        IDataLoader dataLoader,
        IProducerDataTransposer producerDataTransposer,
        IPrepareCalcService prepareCalcService,
        IRpdStatusService statusService,
        ILogger<CalculatorRunService> logger)
        : ICalculatorRunService
    {
        public async Task<bool> PrepareResultsFileAsync(CalculatorRunParameter calculatorRunParameter, string runName)
        {
            try
            {
                await dataLoader.LoadData(calculatorRunParameter, runName);
                return await RunResultsFileCalculationAsync(calculatorRunParameter, runName);
            }
            catch (OperationCanceledException ex)
            {
                logger.LogError(ex, "Prepare results file cancelled");
                return false;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Prepare results file failed");
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

            var statusUpdateResponse = await statusService.UpdateRpdStatus(
                calculatorRunParameter.Id,
                runName,
                calculatorRunParameter.User,
                CancellationToken.None);

            if (statusUpdateResponse == RunClassification.RUNNING)
            {
                await producerDataTransposer.Transpose(calculatorRunParameter.Id, CancellationToken.None);

                isSuccess = await prepareCalcService.PrepareCalcResultsAsync(
                    new CalcResultsRequestDto { RunId = calculatorRunParameter.Id, RelativeYear = calculatorRunParameter.RelativeYear },
                    runName,
                    CancellationToken.None);
            }

            return isSuccess;
        }
    }
}
