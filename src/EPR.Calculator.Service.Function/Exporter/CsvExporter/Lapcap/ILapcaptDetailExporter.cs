using System.Text;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Exporter.CsvExporter.Lapcap
{
    public interface ILapcaptDetailExporter
    {
        void Export(CalcResultLapcapData calcResultLapcapData, StringBuilder csvContent);
    }
}
