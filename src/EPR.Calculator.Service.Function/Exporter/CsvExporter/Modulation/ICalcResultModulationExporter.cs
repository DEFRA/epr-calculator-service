using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Builder.Modulation;
using EPR.Calculator.Service.Function.Services;
using System.Text;

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
