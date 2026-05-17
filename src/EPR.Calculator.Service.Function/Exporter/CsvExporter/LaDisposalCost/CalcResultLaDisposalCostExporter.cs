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

            AppendHeader(applyModulation, csvContent);
            foreach (var d in calcResultLaDisposalCostData.ByMaterial)
            {
                AppendRow(applyModulation, d.Key.Name, d.Value, csvContent);
            }
            AppendRow(applyModulation, "Total", calcResultLaDisposalCostData.Total, csvContent);
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

        private static void AppendRow(bool applyModulation, string name, CalcResultLaDisposalCostDataDetail data, StringBuilder csvContent)
        {
            csvContent.Append(CsvSanitiser.SanitiseData(name));
            csvContent.Append(CsvSanitiser.SanitiseData(data.England));
            csvContent.Append(CsvSanitiser.SanitiseData(data.Wales));
            csvContent.Append(CsvSanitiser.SanitiseData(data.Scotland));
            csvContent.Append(CsvSanitiser.SanitiseData(data.NorthernIreland));
            csvContent.Append(CsvSanitiser.SanitiseData(data.Total));
            csvContent.Append(CsvSanitiser.SanitiseData(data.ProducerReportedHouseholdPackagingWasteTonnage));
            csvContent.Append(CsvSanitiser.SanitiseData(data.ReportedPublicBinTonnage));
            csvContent.Append(CsvSanitiser.SanitiseData(data.HouseholdDrinkContainers, DecimalPlaces.Three, null));
            csvContent.Append(CsvSanitiser.SanitiseData(data.LateReportingTonnage));
            if (applyModulation)
            {
                csvContent.Append(CsvSanitiser.SanitiseData(data.ActionedSelfManagedConsumerWasteTonnage, DecimalPlaces.Three, null));
            }
            csvContent.Append(CsvSanitiser.SanitiseData(data.ProducerReportedTotalTonnage, DecimalPlaces.Three, null));
            csvContent.Append(CsvSanitiser.SanitiseData(data.DisposalCostPricePerTonne));
            csvContent.AppendLine();
        }
    }
}
