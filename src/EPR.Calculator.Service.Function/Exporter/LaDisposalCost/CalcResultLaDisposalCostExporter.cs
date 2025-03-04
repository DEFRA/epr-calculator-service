namespace EPR.Calculator.Service.Function.Exporter.LaDisposalCost
{
    using System.Linq;
    using System.Text;
    using EPR.Calculator.API.Utils;
    using EPR.Calculator.Service.Function.Models;

    public class CalcResultLaDisposalCostExporter : ICalcResultLaDisposalCostExporter
    {
        public void Export(CalcResultLaDisposalCostData calcResultLaDisposalCostData, StringBuilder csvContent)
        {
            csvContent.AppendLine();
            csvContent.AppendLine();

            csvContent.AppendLine(calcResultLaDisposalCostData.Name);
            var lapcapDataDetails = calcResultLaDisposalCostData.CalcResultLaDisposalCostDetails.OrderBy(x => x.OrderId);

            foreach (var lapcapData in lapcapDataDetails)
            {
                csvContent.Append(CsvSanitiser.SanitiseData(lapcapData.Name));
                csvContent.Append(CsvSanitiser.SanitiseData(lapcapData.England));
                csvContent.Append(CsvSanitiser.SanitiseData(lapcapData.Wales));
                csvContent.Append(CsvSanitiser.SanitiseData(lapcapData.Scotland));
                csvContent.Append(CsvSanitiser.SanitiseData(lapcapData.NorthernIreland));
                csvContent.Append(CsvSanitiser.SanitiseData(lapcapData.Total));
                csvContent.Append(CsvSanitiser.SanitiseData(lapcapData.ProducerReportedHouseholdPackagingWasteTonnage));
                csvContent.Append(CsvSanitiser.SanitiseData(lapcapData.ReportedPublicBinTonnage));
                csvContent.Append(CsvSanitiser.SanitiseData(lapcapData.HouseholdDrinkContainers));
                csvContent.Append(CsvSanitiser.SanitiseData(lapcapData.LateReportingTonnage));
                csvContent.Append(CsvSanitiser.SanitiseData(lapcapData.ProducerReportedTotalTonnage));
                csvContent.Append(CsvSanitiser.SanitiseData(lapcapData.DisposalCostPricePerTonne));
                csvContent.AppendLine();
            }
        }
    }
}
