using EPR.Calculator.Service.Function.Models;
using System.Text;

namespace EPR.Calculator.Service.Function.Exporter.OtherCosts
{
    public interface ICalcResultParameterOtherCostExporter
    {
        void OtherCostExporter(CalcResultParameterOtherCost otherCost, StringBuilder csvContent);

        void SchemeSetupCost(CalcResultParameterOtherCost otherCost, StringBuilder csvContent);

        void Materiality(CalcResultParameterOtherCost otherCost, StringBuilder csvContent);

        void LaDataPrepCosts(CalcResultParameterOtherCost otherCost, StringBuilder csvContent);

        void SaOpertingCosts(CalcResultParameterOtherCost otherCost, StringBuilder csvContent);
    }
}
