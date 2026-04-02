using System.Text;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Exporter.CsvExporter.LaDisposalCost
{
    public interface ICalcResultLaDisposalCostExporter
    {
        void Export(CalcResultLaDisposalCostData calcResultLaDisposalCostData, StringBuilder csvContent);
    }
}
