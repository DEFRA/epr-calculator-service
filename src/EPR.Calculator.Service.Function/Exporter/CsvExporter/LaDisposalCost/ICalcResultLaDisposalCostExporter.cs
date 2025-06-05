namespace EPR.Calculator.Service.Function.Exporter.CsvExporter.LaDisposalCost
{
    using System.Text;
    using EPR.Calculator.Service.Function.Models;

    public interface ICalcResultLaDisposalCostExporter
    {
        void Export(CalcResultLaDisposalCostData calcResultLaDisposalCostData, StringBuilder csvContent);
    }
}
