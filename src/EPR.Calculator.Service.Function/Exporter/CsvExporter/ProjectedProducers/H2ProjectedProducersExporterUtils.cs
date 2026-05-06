using System.Collections.Generic;
using System.Linq;
using System.Text;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Enums;
using EPR.Calculator.Service.Function.Misc;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Exporter.CsvExporter.ProjectedProducers
{
    public static class H2ProjectedProducersExporterUtils
    {
        public static void AppendProjectedProducers(IEnumerable<CalcResultH2ProjectedProducer> h2ProjectedProducers, StringBuilder csvContent)
        {
            foreach (var producer in h2ProjectedProducers)
            {
                csvContent.Append(CsvSanitiser.SanitiseData(producer.ProducerId));
                csvContent.Append(CsvSanitiser.SanitiseData(producer.SubsidiaryId));
                csvContent.Append(CsvSanitiser.SanitiseData(producer.Level));
                csvContent.Append(CsvSanitiser.SanitiseData(producer.SubmissionPeriodCode));

                AppendProjectedProducerTonnageByMaterial(csvContent, producer.H2ProjectedTonnageByMaterial);

                csvContent.AppendLine();
            }
        }

        private static void AppendProjectedProducerTonnageByMaterial(StringBuilder csvContent, IReadOnlyDictionary<string, CalcResultH2ProjectedProducerMaterialTonnage> h2ProjectedProducerTonnageByMaterial)
        {
            void appendRamTonnage(RAMTonnage tonnage)
            {
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

                csvContent.Append(CsvSanitiser.SanitiseData(tonnage.HouseholdTonnage, DecimalPlaces.Three, DecimalFormats.F3));
                appendRamTonnage(tonnage.HouseholdRAMTonnage);
                csvContent.Append(CsvSanitiser.SanitiseData(tonnage.HouseholdTonnageWithoutRAM, DecimalPlaces.Three, DecimalFormats.F3));

                csvContent.Append(CsvSanitiser.SanitiseData(tonnage.PublicBinTonnage, DecimalPlaces.Three, DecimalFormats.F3));
                appendRamTonnage(tonnage.PublicBinRAMTonnage);
                csvContent.Append(CsvSanitiser.SanitiseData(tonnage.PublicBinTonnageWithoutRAM, DecimalPlaces.Three, DecimalFormats.F3));

                if (materialCode == MaterialCodes.Glass)
                {
                    csvContent.Append(CsvSanitiser.SanitiseData(tonnage.HouseholdDrinksContainerTonnage, DecimalPlaces.Three, DecimalFormats.F3));
                    appendRamTonnage(tonnage.HouseholdDrinksContainerRAMTonnage!);
                    csvContent.Append(CsvSanitiser.SanitiseData(tonnage.HouseholdDrinksContainerTonnageWithoutRAM, DecimalPlaces.Three, DecimalFormats.F3));
                }

                csvContent.Append(CsvSanitiser.SanitiseData(tonnage.TotalTonnage, DecimalPlaces.Three, DecimalFormats.F3));
            }
        }
    }
}