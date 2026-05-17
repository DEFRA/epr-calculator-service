using System.Text;
using EPR.Calculator.Service.Function.Enums;
using EPR.Calculator.Service.Function.Misc;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Constants;
using Microsoft.IdentityModel.Tokens;

namespace EPR.Calculator.Service.Function.Exporter.CsvExporter.LaDisposalCost
{
    public interface ICalcResultLaDisposalCostExporter
    {
        void Export(bool applyModulation, CalcResultLaDisposalCostData calcResultLaDisposalCostData, StringBuilder csvContent);
    }

    public class CalcResultLaDisposalCostExporter : ICalcResultLaDisposalCostExporter
    {
        public void Export(bool applyModulation, CalcResultLaDisposalCostData calcResultLaDisposalCostData, StringBuilder csvContent)
        {
            csvContent.AppendLine();
            csvContent.AppendLine();

            csvContent.AppendLine(CsvSanitiser.SanitiseData("LA Disposal Cost Data"));
            var laDisposalCostDataDetails = calcResultLaDisposalCostData.CalcResultLaDisposalCostDetails.OrderBy(x => x.OrderId);

            AppendHeader(applyModulation, csvContent);

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
                if (applyModulation)
                {
                    csvContent.Append(CsvSanitiser.SanitiseData(d.ActionedSelfManagedConsumerWasteTonnage, DecimalPlaces.Three, null));
                }
                csvContent.Append(CsvSanitiser.SanitiseData(d.ProducerReportedTotalTonnage, DecimalPlaces.Three, null));
                csvContent.Append(CsvSanitiser.SanitiseData(d.DisposalCostPricePerTonne));
                csvContent.AppendLine();
            }
        }

        private static void AppendHeader(bool applyModulation, StringBuilder csvContent)
        {
            csvContent.Append(CsvSanitiser.SanitiseData(CommonConstants.Material));
            csvContent.Append(CsvSanitiser.SanitiseData(CommonConstants.England));
            csvContent.Append(CsvSanitiser.SanitiseData(CommonConstants.Wales));
            csvContent.Append(CsvSanitiser.SanitiseData(CommonConstants.Scotland));
            csvContent.Append(CsvSanitiser.SanitiseData(CommonConstants.NorthernIreland));
            csvContent.Append(CsvSanitiser.SanitiseData(CommonConstants.Total));
            csvContent.Append(CsvSanitiser.SanitiseData(CommonConstants.ProducerReportedHouseholdPackagingWasteTonnage));
            csvContent.Append(CsvSanitiser.SanitiseData(CommonConstants.ReportedPublicBinTonnage));
            csvContent.Append(CsvSanitiser.SanitiseData(CommonConstants.HouseholdDrinkContainers));
            csvContent.Append(CsvSanitiser.SanitiseData(CommonConstants.LateReportingTonnage));
            if (applyModulation)
            {
                csvContent.Append(CsvSanitiser.SanitiseData(CommonConstants.ActionedSelfManagedConsumerWasteTonnage));
                csvContent.Append(CsvSanitiser.SanitiseData(CommonConstants.ModulatedProducerReportedTotalTonnage));
            }
            else
            {
                csvContent.Append(CsvSanitiser.SanitiseData(CommonConstants.ProducerReportedTotalTonnage));
            }
            csvContent.Append(CsvSanitiser.SanitiseData(CommonConstants.DisposalCostPricePerTonne));
            csvContent.AppendLine();
        }
    }
}
