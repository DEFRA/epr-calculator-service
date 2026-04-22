using System.Text;
using EPR.Calculator.Service.Function.Misc;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Exporter.CsvExporter.Lapcap
{
    public class LapcaptDetailExporter : ILapcaptDetailExporter
    {
        public void Export(CalcResultLapcapData calcResultLapcapData, StringBuilder csvContent)
        {
            csvContent.AppendLine();
            csvContent.AppendLine();

            csvContent.AppendLine(calcResultLapcapData.Name);
            var lapcapDataDetails = calcResultLapcapData.CalcResultLapcapDataDetail.OrderBy(x => x.OrderId);

            foreach (var lapcapData in lapcapDataDetails)
            {
                csvContent.Append($"{CsvSanitiser.SanitiseData(lapcapData.Name)}");
                csvContent.Append($"{CsvSanitiser.SanitiseData(lapcapData.EnglandDisposalCost)}");
                csvContent.Append($"{CsvSanitiser.SanitiseData(lapcapData.WalesDisposalCost)}");
                csvContent.Append($"{CsvSanitiser.SanitiseData(lapcapData.ScotlandDisposalCost)}");
                csvContent.Append($"{CsvSanitiser.SanitiseData(lapcapData.NorthernIrelandDisposalCost)}");
                csvContent.Append($"{CsvSanitiser.SanitiseData(lapcapData.TotalDisposalCost, false)}");
                csvContent.AppendLine();
            }
        }
    }
}
