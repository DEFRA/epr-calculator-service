using System.Text;
using EPR.Calculator.Service.Function.Enums;
using EPR.Calculator.Service.Function.Misc;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Exporter.CsvExporter.LaDisposalCost
{
    public class CalcResultLaDisposalCostExporter : ICalcResultLaDisposalCostExporter
    {
        public void Export(CalcResultLaDisposalCostData calcResultLaDisposalCostData, StringBuilder csvContent)
        {
            csvContent.AppendLine();
            csvContent.AppendLine();

            csvContent.AppendLine(calcResultLaDisposalCostData.Name);
            var laDisposalCostDataDetails = calcResultLaDisposalCostData.CalcResultLaDisposalCostDetails.OrderBy(x => x.OrderId);

            foreach (var d in laDisposalCostDataDetails)
            {
                csvContent.Append(CsvSanitiser.SanitiseData(d.Name));
                csvContent.Append(CsvSanitiser.SanitiseData(d.England));
                csvContent.Append(CsvSanitiser.SanitiseData(d.Wales));
                csvContent.Append(CsvSanitiser.SanitiseData(d.Scotland));
                csvContent.Append(CsvSanitiser.SanitiseData(d.NorthernIreland));
                csvContent.Append(CsvSanitiser.SanitiseData(d.Total));
                csvContent.Append(CsvSanitiser.SanitiseData(d.ProducerReportedHouseholdPackagingWasteTonnage));
                csvContent.Append(CsvSanitiser.SanitiseData(d.ReportedPublicBinTonnage));
                csvContent.Append(CsvSanitiser.SanitiseData(d.HouseholdDrinkContainers, DecimalPlaces.Three, null));
                csvContent.Append(CsvSanitiser.SanitiseData(d.LateReportingTonnage));
                if (!string.IsNullOrEmpty(d.ActionedSelfManagedConsumerWasteTonnage)) {
                    csvContent.Append(CsvSanitiser.SanitiseData(d.ActionedSelfManagedConsumerWasteTonnage));
                }
                csvContent.Append(CsvSanitiser.SanitiseData(d.ProducerReportedTotalTonnage, DecimalPlaces.Three, null));
                csvContent.Append(CsvSanitiser.SanitiseData(d.DisposalCostPricePerTonne));
                csvContent.AppendLine();
            }
        }
    }
}
