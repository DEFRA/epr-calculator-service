using System.Text;
using EPR.Calculator.Service.Function.Builder.Modulation;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Services;

namespace EPR.Calculator.Service.Function.Exporter.CsvExporter.Modulation
{
    public interface ICalcResultModulationExporter
    {
        void Export(
            CalcResultLaDisposalCostData laDisposalCostData,
            SelfManagedConsumerWaste smcw,
            ModulationResult modulationResult,
            StringBuilder csvContent
        );
    }
}
