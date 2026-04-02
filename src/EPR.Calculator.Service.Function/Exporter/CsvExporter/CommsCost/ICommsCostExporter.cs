using System.Text;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Exporter.CsvExporter.CommsCost
{
    public interface ICommsCostExporter
    {
        void Export(CalcResultCommsCost communicationCost, StringBuilder csvContent);
    }
}