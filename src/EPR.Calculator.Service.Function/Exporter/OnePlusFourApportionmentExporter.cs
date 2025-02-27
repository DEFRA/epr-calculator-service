using EPR.Calculator.API.Utils;
using EPR.Calculator.Service.Function.Models;
using System.Linq;
using System.Text;

namespace EPR.Calculator.Service.Function.Exporter
{

    public class OnePlusFourApportionmentExporter : IOnePlusFourApportionmentExporter
    {
        public void Export(CalcResultOnePlusFourApportionment calcResult1Plus4Apportionment, StringBuilder csvContent)
        {
            csvContent.AppendLine();
            csvContent.AppendLine();

            csvContent.AppendLine(calcResult1Plus4Apportionment.Name);
            var lapcapDataDetails = calcResult1Plus4Apportionment.CalcResultOnePlusFourApportionmentDetails.OrderBy(x => x.OrderId);

            foreach (var lapcapData in lapcapDataDetails)
            {
                csvContent.Append($"{CsvSanitiser.SanitiseData(lapcapData.Name)},");
                csvContent.Append($"\"{CsvSanitiser.SanitiseData(lapcapData.EnglandDisposalTotal)}\",");
                csvContent.Append($"\"{CsvSanitiser.SanitiseData(lapcapData.WalesDisposalTotal)}\",");
                csvContent.Append($"\"{CsvSanitiser.SanitiseData(lapcapData.ScotlandDisposalTotal)}\",");
                csvContent.Append($"\"{CsvSanitiser.SanitiseData(lapcapData.NorthernIrelandDisposalTotal)}\",");
                csvContent.Append($"\"{CsvSanitiser.SanitiseData(lapcapData.Total)}\",");
                csvContent.AppendLine();
            }
        }
    }
}
