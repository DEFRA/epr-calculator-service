using System.Text;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Exporter.CsvExporter.OtherCosts
{
    public interface ICalcResultParameterOtherCostExporter
    {
        void Export(CalcResultParameterOtherCost otherCost, StringBuilder csvContent);
    }
}
