using System.Text;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Exporter.CsvExporter.Detail
{
    public interface ICalcResultDetailExporter
    {
        void Export(CalcResultDetail calcResultDetail, StringBuilder stringBuilder);
    }
}