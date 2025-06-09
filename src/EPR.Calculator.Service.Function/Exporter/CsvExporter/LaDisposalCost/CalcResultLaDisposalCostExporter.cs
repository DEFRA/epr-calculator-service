namespace EPR.Calculator.Service.Function.Exporter.CsvExporter.LaDisposalCost
{
    using System.Linq;
    using System.Text;
    using EPR.Calculator.API.Utils;
    using EPR.Calculator.Service.Function.Enums;
    using EPR.Calculator.Service.Function.Models;

    public class CalcResultLaDisposalCostExporter : ICalcResultLaDisposalCostExporter
    {
        public void Export(CalcResultLaDisposalCostData calcResultLaDisposalCostData, StringBuilder csvContent)
        {
            csvContent.AppendLine();
            csvContent.AppendLine();

            csvContent.AppendLine(calcResultLaDisposalCostData.Name);
            var laDisposalCostDataDetails = calcResultLaDisposalCostData.CalcResultLaDisposalCostDetails.OrderBy(x => x.OrderId);

            foreach (var laDisposalCostData in laDisposalCostDataDetails)
            {
                csvContent.Append(CsvSanitiser.SanitiseData(laDisposalCostData.Name));
                csvContent.Append(CsvSanitiser.SanitiseData(laDisposalCostData.England));
                csvContent.Append(CsvSanitiser.SanitiseData(laDisposalCostData.Wales));
                csvContent.Append(CsvSanitiser.SanitiseData(laDisposalCostData.Scotland));
                csvContent.Append(CsvSanitiser.SanitiseData(laDisposalCostData.NorthernIreland));
                csvContent.Append(CsvSanitiser.SanitiseData(laDisposalCostData.Total));
                csvContent.Append(CsvSanitiser.SanitiseData(laDisposalCostData.ProducerReportedHouseholdPackagingWasteTonnage));
                csvContent.Append(CsvSanitiser.SanitiseData(laDisposalCostData.ReportedPublicBinTonnage));
                csvContent.Append(CsvSanitiser.SanitiseData(laDisposalCostData.HouseholdDrinkContainers, DecimalPlaces.Three, null));
                csvContent.Append(CsvSanitiser.SanitiseData(laDisposalCostData.LateReportingTonnage));
                csvContent.Append(CsvSanitiser.SanitiseData(laDisposalCostData.ProducerReportedTotalTonnage, DecimalPlaces.Three, null));
                csvContent.Append(CsvSanitiser.SanitiseData(laDisposalCostData.DisposalCostPricePerTonne));
                csvContent.AppendLine();
            }
        }
    }
}
