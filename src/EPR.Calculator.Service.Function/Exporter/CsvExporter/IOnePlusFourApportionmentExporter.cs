using System.Text;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Exporter.CsvExporter
{
    public interface IOnePlusFourApportionmentExporter
    {
        void Export(CalcResultOnePlusFourApportionment calcResult1Plus4Apportionment, StringBuilder csvContent);
    }
}
