using System.Linq;

namespace EPR.Calculator.Service.Function.Exporter.CsvExporter.ProjectedProducers
{
    using System;

    using System.Collections.Generic;
    using System.Linq;

    using System.Text;
    using EPR.Calculator.API.Utils;
    using EPR.Calculator.Service.Function.Constants;
    using EPR.Calculator.Service.Function.Enums;
    using EPR.Calculator.Service.Function.Models;

    public class CalcResultProjectedProducersExporter : ICalcResultProjectedProducersExporter
    {
        public void Export(CalcResultProjectedProducers calcResultProjectedProducers, StringBuilder stringBuilder)
        {
            // Add empty lines
            stringBuilder.AppendLine();
            stringBuilder.AppendLine();

            // Add headers
            PrepareH2ProjectedProducersHeaders(calcResultProjectedProducers.H2ProjectedProducersHeaders!, stringBuilder);

            // Add data
            if (calcResultProjectedProducers.H2ProjectedProducers?.Any() == true)
            {
                AppendH2ProjectedProducers(calcResultProjectedProducers.H2ProjectedProducers!, stringBuilder);
            }
            else
            {
                stringBuilder.AppendLine(CsvSanitiser.SanitiseData(CalcResultProjectedProducersHeaders.NoProjectedProducers));
            }
        }

        private static void AppendH2ProjectedProducers(IEnumerable<CalcResultH2ProjectedProducer> h2ProjectedProducers, StringBuilder csvContent)
        {
            foreach (var producer in h2ProjectedProducers)
            {
                csvContent.Append(CsvSanitiser.SanitiseData(producer.ProducerId));
                csvContent.Append(CsvSanitiser.SanitiseData(producer.SubsidiaryId));
                csvContent.Append(CsvSanitiser.SanitiseData(producer.Level));
                csvContent.Append(CsvSanitiser.SanitiseData(producer.SubmissionPeriodCode));

                AppendH2ProjectedProducerTonnageByMaterial(csvContent, producer.ProjectedTonnageByMaterial);

                csvContent.AppendLine();
            }
        }

        private static void AppendH2ProjectedProducerTonnageByMaterial(StringBuilder csvContent, Dictionary<string, CalcResultH2ProjectedProducerMaterialTonnage> h2ProjectedProducerTonnageByMaterial)
        {
            void appendRamTonnage(RAMTonnage tonnage)
            {
                csvContent.Append(CsvSanitiser.SanitiseData(tonnage.Tonnage, DecimalPlaces.Three, DecimalFormats.F3));
                csvContent.Append(CsvSanitiser.SanitiseData(tonnage.RedTonnage, DecimalPlaces.Three, DecimalFormats.F3));
                csvContent.Append(CsvSanitiser.SanitiseData(tonnage.AmberTonnage, DecimalPlaces.Three, DecimalFormats.F3));
                csvContent.Append(CsvSanitiser.SanitiseData(tonnage.GreenTonnage, DecimalPlaces.Three, DecimalFormats.F3));
                csvContent.Append(CsvSanitiser.SanitiseData(tonnage.RedMedicalTonnage, DecimalPlaces.Three, DecimalFormats.F3));
                csvContent.Append(CsvSanitiser.SanitiseData(tonnage.AmberMedicalTonnage, DecimalPlaces.Three, DecimalFormats.F3));
                csvContent.Append(CsvSanitiser.SanitiseData(tonnage.GreenMedicalTonnage, DecimalPlaces.Three, DecimalFormats.F3));
            }

            foreach (var producerTonnage in h2ProjectedProducerTonnageByMaterial)
            {
                var materialCode = producerTonnage.Key;
                var tonnage = producerTonnage.Value;

                appendRamTonnage(tonnage.HouseholdRAMTonnage);
                csvContent.Append(CsvSanitiser.SanitiseData(tonnage.HouseholdTonnageDefaultedRed, DecimalPlaces.Three, DecimalFormats.F3));

                appendRamTonnage(tonnage.PublicBinRAMTonnage);
                csvContent.Append(CsvSanitiser.SanitiseData(tonnage.PublicBinTonnageDefaultedRed, DecimalPlaces.Three, DecimalFormats.F3));

                if (materialCode == MaterialCodes.Glass)
                {
                    appendRamTonnage(tonnage.HouseholdDrinksContainerRAMTonnage!);
                    csvContent.Append(CsvSanitiser.SanitiseData(tonnage.HouseholdDrinksContainerDefaultedRed, DecimalPlaces.Three, DecimalFormats.F3));
                }

                csvContent.Append(CsvSanitiser.SanitiseData(tonnage.TotalTonnage, DecimalPlaces.Three, DecimalFormats.F3));
            }
        }

        private static void PrepareH2ProjectedProducersHeaders(ProjectedProducersHeaders headers, StringBuilder csvContent)
        {
            // Add H2 projected producers headers
            csvContent.AppendLine(CsvSanitiser.SanitiseData(headers.TitleHeader!.Name));
            csvContent.AppendLine();

            // Add material breakdown headers
            WriteH2ProjectedProducersSecondaryHeaders(headers.MaterialBreakdownHeaders!, csvContent);

            // Add column headers
            WriteH2ProjectedProducersColumnHeaders(headers.ColumnHeaders!, csvContent);
            csvContent.AppendLine();
        }

        private static void WriteH2ProjectedProducersSecondaryHeaders(IEnumerable<ProjectedProducersHeader> headers, StringBuilder csvContent)
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

        private static void WriteH2ProjectedProducersColumnHeaders(IEnumerable<ProjectedProducersHeader> columnHeaders, StringBuilder csvContent)
        {
            foreach (var item in columnHeaders)
            {
                csvContent.Append(CsvSanitiser.SanitiseData(item.Name));
            }
        }
    }
}