namespace EPR.Calculator.Service.Function.Exporter.Lapcap
{
    using System.Linq;
    using System.Text;
    using EPR.Calculator.API.Utils;
    using EPR.Calculator.Service.Function.Models;

    public class LapcaptDetailExporter: ILapcaptDetailExporter
    {
        public void Export(CalcResultLapcapData calcResultLapcapData, StringBuilder csvContent)
        {
            csvContent.AppendLine();
            csvContent.AppendLine();

            csvContent.AppendLine(calcResultLapcapData.Name);
            var lapcapDataDetails = calcResultLapcapData.CalcResultLapcapDataDetails.OrderBy(x => x.OrderId);

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
