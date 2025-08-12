namespace EPR.Calculator.Service.Function.Exporter.CsvExporter.ScaledupProducers
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using EPR.Calculator.API.Utils;
    using EPR.Calculator.Service.Function.Constants;
    using EPR.Calculator.Service.Function.Enums;
    using EPR.Calculator.Service.Function.Models;
    using Microsoft.IdentityModel.Tokens;

    public class CalcResultScaledupProducersExporter : ICalcResultScaledupProducersExporter
    {
        public void Export(CalcResultScaledupProducers producers, StringBuilder csvContent)
        {
            // Add empty lines
            csvContent.AppendLine();
            csvContent.AppendLine();

            // Add headers
            PrepareScaledupProducersHeader(producers, csvContent);

            // Add data
            if (producers.ScaledupProducers == null || !producers.ScaledupProducers.Any())
            {
                AppendScaledupProducers(producers, csvContent);
            }
            else
            {
                csvContent.AppendLine(CsvSanitiser.SanitiseData(CalcResultScaledupProducerHeaders.NoScaledupProducers));
            }
        }

        private static void AppendScaledupProducers(CalcResultScaledupProducers producers, StringBuilder csvContent)
        {
            foreach (var producer in producers.ScaledupProducers!)
            {
                if (producer.IsTotalRow)
                {
                    _ = csvContent.Append(new string(CommonConstants.CsvFileDelimiter[0], 8));
                    csvContent.Append(CsvSanitiser.SanitiseData(CommonConstants.Totals));
                }
                else
                {
                    csvContent.Append(CsvSanitiser.SanitiseData(producer.ProducerId));
                    csvContent.Append(CsvSanitiser.SanitiseData(producer.SubsidiaryId));
                    csvContent.Append(CsvSanitiser.SanitiseData(producer.ProducerName));
                    csvContent.Append(CsvSanitiser.SanitiseData(producer.TradingName));
                    csvContent.Append(CsvSanitiser.SanitiseData(producer.Level));
                    csvContent.Append(CsvSanitiser.SanitiseData(producer.SubmissionPeriodCode));
                    csvContent.Append(CsvSanitiser.SanitiseData(producer.DaysInSubmissionPeriod != -1 ? producer.DaysInSubmissionPeriod.ToString() : string.Empty));
                    csvContent.Append(CsvSanitiser.SanitiseData(producer.DaysInWholePeriod != -1 ? producer.DaysInWholePeriod.ToString() : string.Empty));
                    csvContent.Append(CsvSanitiser.SanitiseData(producer.ScaleupFactor == -1 ? CommonConstants.Totals : producer.ScaleupFactor.ToString()));
                }

                AppendScaledupProducerTonnageByMaterial(csvContent, producer);

                csvContent.AppendLine();
            }
        }

        private static void AppendScaledupProducerTonnageByMaterial(StringBuilder csvContent, CalcResultScaledupProducer producer)
        {
            foreach (var producerTonnage in producer.ScaledupProducerTonnageByMaterial)
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
                csvContent.Append(CsvSanitiser.SanitiseData(tonnage.ScaledupReportedHouseholdPackagingWasteTonnage, DecimalPlaces.Three, DecimalFormats.F3));
                csvContent.Append(CsvSanitiser.SanitiseData(tonnage.ScaledupReportedPublicBinTonnage, DecimalPlaces.Three, DecimalFormats.F3));

                if (materialCode == MaterialCodes.Glass || materialCode == MaterialNames.Glass)
                {
                    csvContent.Append(CsvSanitiser.SanitiseData(tonnage.ScaledupHouseholdDrinksContainersTonnageGlass, DecimalPlaces.Three, DecimalFormats.F3));
                }

                csvContent.Append(CsvSanitiser.SanitiseData(tonnage.ScaledupTotalReportedTonnage, DecimalPlaces.Three, DecimalFormats.F3));
                csvContent.Append(CsvSanitiser.SanitiseData(tonnage.ScaledupReportedSelfManagedConsumerWasteTonnage, DecimalPlaces.Three, DecimalFormats.F3));
                csvContent.Append(CsvSanitiser.SanitiseData(tonnage.ScaledupNetReportedTonnage, DecimalPlaces.Three, DecimalFormats.F3));
            }
        }

        private static void PrepareScaledupProducersHeader(CalcResultScaledupProducers producers, StringBuilder csvContent)
        {
            // Add scaledup producer header
            csvContent.AppendLine(CsvSanitiser.SanitiseData(producers.TitleHeader!.Name));
            csvContent.AppendLine();

            // Add material breakdown header
            WriteScaledupProducersSecondaryHeaders(producers.MaterialBreakdownHeaders!, csvContent);

            // Add column header
            WriteScaledupProducersColumnHeaders(producers, csvContent);
            csvContent.AppendLine();
        }

        private static void WriteScaledupProducersSecondaryHeaders(IEnumerable<CalcResultScaledupProducerHeader> headers, StringBuilder csvContent)
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

        private static void WriteScaledupProducersColumnHeaders(CalcResultScaledupProducers producers, StringBuilder csvContent)
        {
            foreach (var item in producers.ColumnHeaders!)
            {
                csvContent.Append(CsvSanitiser.SanitiseData(item.Name));
            }
        }
    }
}