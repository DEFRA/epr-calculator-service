using EPR.Calculator.API.Data;
using EPR.Calculator.API.Exporter;
using EPR.Calculator.Service.Common.Logging;
using EPR.Calculator.Service.Function.Builder;
using EPR.Calculator.Service.Function.Exporter.CsvExporter;
using EPR.Calculator.Service.Function.Interface;
using EPR.Calculator.Service.Function.Misc;
using EPR.Calculator.Service.Function.Services;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.Service.Function.Models
{
    public class PrepareCalcServiceDependencies
    {
        public required ApplicationDBContext Context { get; init; }
        public required ICalcResultBuilder Builder { get; init; }
        public required ICalcResultsExporter<CalcResult> Exporter { get; init; }
        public required IStorageService StorageService { get; init; }
        public required CalculatorRunValidator ValidationRules { get; init; }
        public required ICommandTimeoutService CommandTimeoutService { get; init; }
        public required ICalculatorTelemetryLogger TelemetryLogger { get; init; }
        public required IBillingInstructionService BillingInstructionService { get; init; }
        public required ICalcBillingJsonExporter<CalcResult> JsonExporter { get; init; }
        public required IConfigurationService ConfigService { get; init; }
        public required IBillingFileExporter<CalcResult> BillingFileExporter { get; init; }
        public required IProducerInvoiceNetTonnageService ProducerInvoiceNetTonnageService { get; init; }
    }
}
