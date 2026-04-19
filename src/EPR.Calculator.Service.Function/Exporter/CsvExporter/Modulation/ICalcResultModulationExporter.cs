using System.Text;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Builder.Modulation;

namespace EPR.Calculator.Service.Function.Exporter.CsvExporter.LaDisposalCost
{
    public interface ICalcResultModulationExporter
    {
        void Export(CalcResultLaDisposalCostData laDisposalCostData, ModulationResult modulationResult, StringBuilder csvContent);
    }
}
