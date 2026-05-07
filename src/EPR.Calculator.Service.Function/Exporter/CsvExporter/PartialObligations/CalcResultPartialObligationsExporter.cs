using System.Text;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Enums;
using EPR.Calculator.Service.Function.Misc;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Exporter.CsvExporter.PartialObligations
{
    public interface ICalcResultPartialObligationsExporter
    {
        public void Export(CalcResultPartialObligations calcResultPartialObligations, StringBuilder stringBuilder, bool showModulation);
    }

    public class CalcResultPartialObligationsExporter : ICalcResultPartialObligationsExporter
    {
        public void Export(CalcResultPartialObligations calcResultPartialObligations, StringBuilder stringBuilder, bool showModulation)
        {
            // Add empty lines
            stringBuilder.AppendLine();
            stringBuilder.AppendLine();

            // Add headers
            PreparePartialObligationsHeader(calcResultPartialObligations, stringBuilder);

            // Add data
            if (calcResultPartialObligations.PartialObligations?.Any() == true)
            {
                AppendPartialObligations(calcResultPartialObligations.PartialObligations!, stringBuilder, showModulation);
            }
            else
            {
                stringBuilder.AppendLine(CsvSanitiser.SanitiseData(CalcResultPartialObligationHeaders.NoPartialObligations));
            }
        }

        private static void AppendPartialObligations(IEnumerable<CalcResultPartialObligation> partialObligations, StringBuilder csvContent, bool showModulation)
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

                AppendPartialObligationTonnageByMaterial(csvContent, producer.PartialObligationTonnageByMaterial, showModulation);

                csvContent.AppendLine();
            }
        }

        private static void AppendPartialObligationTonnageByMaterial(StringBuilder csvContent, Dictionary<string, CalcResultPartialObligationTonnage> partialObligationTonnageByMaterial, bool showModulation)
        {
            void AppendRam(RAMTonnage? ram)
            {
                if (showModulation && ram != null)
                { 
                    csvContent.Append(CsvSanitiser.SanitiseData(ram.RedTonnage, DecimalPlaces.Three, DecimalFormats.F3));
                    csvContent.Append(CsvSanitiser.SanitiseData(ram.AmberTonnage, DecimalPlaces.Three, DecimalFormats.F3));
                    csvContent.Append(CsvSanitiser.SanitiseData(ram.GreenTonnage, DecimalPlaces.Three, DecimalFormats.F3));
                    csvContent.Append(CsvSanitiser.SanitiseData(ram.RedMedicalTonnage, DecimalPlaces.Three, DecimalFormats.F3));
                    csvContent.Append(CsvSanitiser.SanitiseData(ram.AmberMedicalTonnage, DecimalPlaces.Three, DecimalFormats.F3));
                    csvContent.Append(CsvSanitiser.SanitiseData(ram.GreenMedicalTonnage, DecimalPlaces.Three, DecimalFormats.F3));
                }
            } 

            foreach (var producerTonnage in partialObligationTonnageByMaterial)
            {
                var materialCode = producerTonnage.Key;
                var tonnage = producerTonnage.Value;

                csvContent.Append(CsvSanitiser.SanitiseData(tonnage.HouseholdTonnage, DecimalPlaces.Three, DecimalFormats.F3));
                AppendRam(tonnage.HouseholdRAMTonnage);

                csvContent.Append(CsvSanitiser.SanitiseData(tonnage.PublicBinTonnage, DecimalPlaces.Three, DecimalFormats.F3));
                AppendRam(tonnage.PublicBinRAMTonnage);

                if (materialCode == MaterialCodes.Glass)
                {
                    csvContent.Append(CsvSanitiser.SanitiseData(tonnage.HouseholdDrinksContainersTonnage, DecimalPlaces.Three, DecimalFormats.F3));
                    AppendRam(tonnage.HouseholdDrinksContainersRAMTonnage);
                }

                csvContent.Append(CsvSanitiser.SanitiseData(tonnage.TotalTonnage, DecimalPlaces.Three, DecimalFormats.F3));
                csvContent.Append(CsvSanitiser.SanitiseData(tonnage.SelfManagedConsumerWasteTonnage, DecimalPlaces.Three, DecimalFormats.F3));
                
                csvContent.Append(CsvSanitiser.SanitiseData(tonnage.PartialHouseholdTonnage, DecimalPlaces.Three, DecimalFormats.F3));
                AppendRam(tonnage.PartialHouseholdRAMTonnage);

                csvContent.Append(CsvSanitiser.SanitiseData(tonnage.PartialPublicBinTonnage, DecimalPlaces.Three, DecimalFormats.F3));
                AppendRam(tonnage.PartialPublicBinRAMTonnage);

                if (materialCode == MaterialCodes.Glass)
                {
                    csvContent.Append(CsvSanitiser.SanitiseData(tonnage.PartialHouseholdDrinksContainersTonnage, DecimalPlaces.Three, DecimalFormats.F3));
                    AppendRam(tonnage.PartialHouseholdDrinksContainersRAMTonnage);
                }

                csvContent.Append(CsvSanitiser.SanitiseData(tonnage.PartialTotalTonnage, DecimalPlaces.Three, DecimalFormats.F3));
                csvContent.Append(CsvSanitiser.SanitiseData(tonnage.PartialSelfManagedConsumerWasteTonnage, DecimalPlaces.Three, DecimalFormats.F3));
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
            var maxColumnSize = headers.MaxBy(h => h.ColumnIndex ?? 0)?.ColumnIndex ?? throw new ArgumentException("No headers specified");

            var headerRows = new string[maxColumnSize];
            foreach (var item in headers.Where(h => h.ColumnIndex.HasValue))
            {
                headerRows[item.ColumnIndex!.Value - 1] = CsvSanitiser.SanitiseData(item.Name, false);
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
