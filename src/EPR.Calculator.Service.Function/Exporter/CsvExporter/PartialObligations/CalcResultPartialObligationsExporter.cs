namespace EPR.Calculator.Service.Function.Exporter.CsvExporter.PartialObligations
{
    using System.Collections.Generic;
    using System.Text;
    using EPR.Calculator.API.Utils;
    using EPR.Calculator.Service.Function.Constants;
    using EPR.Calculator.Service.Function.Enums;
    using EPR.Calculator.Service.Function.Models;
    using Microsoft.IdentityModel.Tokens;

    public class CalcResultPartialObligationsExporter : ICalcResultPartialObligationsExporter
    {
        public void Export(CalcResultPartialObligations calcResultPartialObligations, StringBuilder stringBuilder)
        {
            // Add empty lines
            stringBuilder.AppendLine();
            stringBuilder.AppendLine();

            // Add headers
            PreparePartialObligationsHeader(calcResultPartialObligations, stringBuilder);

            // Add data
            if (!calcResultPartialObligations.PartialObligations.IsNullOrEmpty())
            {
                AppendPartialObligations(calcResultPartialObligations.PartialObligations!, stringBuilder);
            }
            else
            {
                stringBuilder.AppendLine(CsvSanitiser.SanitiseData(CalcResultPartialObligationHeaders.NoPartialObligations));
            }
        }

        private static void AppendPartialObligations(IEnumerable<CalcResultPartialObligation> partialObligations, StringBuilder csvContent)
        {
            foreach (var producer in partialObligations)
            {
                csvContent.Append(CsvSanitiser.SanitiseData(producer.ProducerId));
                csvContent.Append(CsvSanitiser.SanitiseData(producer.SubsidiaryId));
                csvContent.Append(CsvSanitiser.SanitiseData(producer.ProducerName));
                csvContent.Append(CsvSanitiser.SanitiseData(producer.TradingName));
                csvContent.Append(CsvSanitiser.SanitiseData(producer.Level));
                csvContent.Append(CsvSanitiser.SanitiseData(producer.SubmissionYear));
                csvContent.Append(CsvSanitiser.SanitiseData(producer.DaysInSubmissionYear));
                csvContent.Append(CsvSanitiser.SanitiseData(producer.JoiningDate));
                csvContent.Append(CsvSanitiser.SanitiseData(producer.DaysObligated));
                csvContent.Append(CsvSanitiser.SanitiseData(producer.ObligatedPercentage));

                AppendPartialObligationTonnageByMaterial(csvContent, producer.PartialObligationTonnageByMaterial);

                csvContent.AppendLine();
            }
        }

        private static void AppendPartialObligationTonnageByMaterial(StringBuilder csvContent, Dictionary<string, CalcResultPartialObligationTonnage> partialObligationTonnageByMaterial)
        {
            foreach (var producerTonnage in partialObligationTonnageByMaterial)
            {
                var materialCode = producerTonnage.Key;
                var tonnage = producerTonnage.Value;

                csvContent.Append(CsvSanitiser.SanitiseData(tonnage.ReportedHouseholdPackagingWasteTonnage, DecimalPlaces.Three, DecimalFormats.F3));
                csvContent.Append(CsvSanitiser.SanitiseData(tonnage.ReportedPublicBinTonnage, DecimalPlaces.Three, DecimalFormats.F3));

                if (materialCode == MaterialCodes.Glass || materialCode == MaterialNames.Glass)
                {
                    csvContent.Append(CsvSanitiser.SanitiseData(tonnage.HouseholdDrinksContainersTonnageGlass, DecimalPlaces.Three, DecimalFormats.F3));
                }

                csvContent.Append(CsvSanitiser.SanitiseData(tonnage.TotalReportedTonnage, DecimalPlaces.Three, DecimalFormats.F3));
                csvContent.Append(CsvSanitiser.SanitiseData(tonnage.ReportedSelfManagedConsumerWasteTonnage, DecimalPlaces.Three, DecimalFormats.F3));
                csvContent.Append(CsvSanitiser.SanitiseData(tonnage.NetReportedTonnage, DecimalPlaces.Three, DecimalFormats.F3));
                csvContent.Append(CsvSanitiser.SanitiseData(tonnage.PartialReportedHouseholdPackagingWasteTonnage, DecimalPlaces.Three, DecimalFormats.F3));
                csvContent.Append(CsvSanitiser.SanitiseData(tonnage.PartialReportedPublicBinTonnage, DecimalPlaces.Three, DecimalFormats.F3));

                if (materialCode == MaterialCodes.Glass || materialCode == MaterialNames.Glass)
                {
                    csvContent.Append(CsvSanitiser.SanitiseData(tonnage.PartialHouseholdDrinksContainersTonnageGlass, DecimalPlaces.Three, DecimalFormats.F3));
                }

                csvContent.Append(CsvSanitiser.SanitiseData(tonnage.PartialTotalReportedTonnage, DecimalPlaces.Three, DecimalFormats.F3));
                csvContent.Append(CsvSanitiser.SanitiseData(tonnage.PartialReportedSelfManagedConsumerWasteTonnage, DecimalPlaces.Three, DecimalFormats.F3));
                csvContent.Append(CsvSanitiser.SanitiseData(tonnage.PartialNetReportedTonnage, DecimalPlaces.Three, DecimalFormats.F3));
            }
        }

        private static void PreparePartialObligationsHeader(CalcResultPartialObligations producers, StringBuilder csvContent)
        {
            // Add partial obligation producer header
            csvContent.AppendLine(CsvSanitiser.SanitiseData(producers.TitleHeader!.Name));
            csvContent.AppendLine();

            // Add material breakdown header
            WritePartialObligationsSecondaryHeaders(producers.MaterialBreakdownHeaders!, csvContent);

            // Add column header
            WritePartialObligationsColumnHeaders(producers, csvContent);
            csvContent.AppendLine();
        }

        private static void WritePartialObligationsSecondaryHeaders(IEnumerable<CalcResultPartialObligationHeader> headers, StringBuilder csvContent)
        {
            const int maxColumnSize = CommonConstants.SecondaryHeaderMaxColumnSize;
            var headerRows = new string[maxColumnSize];
            foreach (var item in headers)
            {
                if (item.ColumnIndex.HasValue)
                {
                    headerRows[item.ColumnIndex.Value - 1] = CsvSanitiser.SanitiseData(item.Name, false);
                }
            }

            var headerRow = string.Join(CommonConstants.CsvFileDelimiter, headerRows);
            csvContent.AppendLine(headerRow);
        }

        private static void WritePartialObligationsColumnHeaders(CalcResultPartialObligations producers, StringBuilder csvContent)
        {
            foreach (var item in producers.ColumnHeaders!)
            {
                csvContent.Append(CsvSanitiser.SanitiseData(item.Name));
            }
        }
    }
}