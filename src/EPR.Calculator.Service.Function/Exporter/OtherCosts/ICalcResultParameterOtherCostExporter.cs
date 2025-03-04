using EPR.Calculator.Service.Function.Models;
using System.Text;

namespace EPR.Calculator.Service.Function.Exporter.OtherCosts
{
    public interface ICalcResultParameterOtherCostExporter
    {
        void Export(CalcResultParameterOtherCost otherCost, StringBuilder csvContent);
    }
}
