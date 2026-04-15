using System.Collections.Generic;
using System.Linq;
using System.Text;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Enums;
using EPR.Calculator.Service.Function.Misc;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Exporter.CsvExporter.ProjectedProducers
{
    public static class H1ProjectedProducersExporterUtils
    {
        public static void AppendProjectedProducers(IEnumerable<CalcResultH1ProjectedProducer> h1ProjectedProducers, StringBuilder csvContent)
        {
            foreach (var producer in h1ProjectedProducers)
            {
                csvContent.Append(CsvSanitiser.SanitiseData(producer.ProducerId));
                csvContent.Append(CsvSanitiser.SanitiseData(producer.SubsidiaryId));
                csvContent.Append(CsvSanitiser.SanitiseData(producer.Level));
                csvContent.Append(CsvSanitiser.SanitiseData(producer.SubmissionPeriodCode));

                AppendProjectedProducerTonnageByMaterial(csvContent, producer.ProjectedTonnageByMaterial);

                csvContent.AppendLine();
            }
        }

        private static void AppendProjectedProducerTonnageByMaterial(StringBuilder csvContent, Dictionary<string, CalcResultH1ProjectedProducerMaterialTonnage> h1ProjectedProducerTonnageByMaterial)
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

            foreach (var producerTonnage in h1ProjectedProducerTonnageByMaterial)
            {
                var materialCode = producerTonnage.Key;
                var tonnage = producerTonnage.Value;

                appendRamTonnage(tonnage.HouseholdRAMTonnage);
                csvContent.Append(CsvSanitiser.SanitiseData(tonnage.HouseholdTonnageWithoutRAM, DecimalPlaces.Three, DecimalFormats.F3));

                appendRamTonnage(tonnage.PublicBinRAMTonnage);
                csvContent.Append(CsvSanitiser.SanitiseData(tonnage.PublicBinTonnageWithoutRAM, DecimalPlaces.Three, DecimalFormats.F3));

                if (materialCode == MaterialCodes.Glass)
                {
                    appendRamTonnage(tonnage.HouseholdDrinksContainerRAMTonnage!);
                    csvContent.Append(CsvSanitiser.SanitiseData(tonnage.HouseholdDrinksContainerTonnageWithoutRAM, DecimalPlaces.Three, DecimalFormats.F3));
                }

                var anyTonnageWithoutRam = tonnage.HouseholdTonnageWithoutRAM > 0 || tonnage.PublicBinTonnageWithoutRAM > 0 || (tonnage.HouseholdDrinksContainerTonnageWithoutRAM ?? 0) > 0;

                csvContent.Append(anyTonnageWithoutRam ? CsvSanitiser.SanitiseData(tonnage.RedH2Proportion * 100, DecimalPlaces.Two, DecimalFormats.F2, isPercentage: true) : CsvSanitiser.SanitiseData(CommonConstants.Hyphen));
                csvContent.Append(anyTonnageWithoutRam ? CsvSanitiser.SanitiseData(tonnage.AmberH2Proportion * 100, DecimalPlaces.Two, DecimalFormats.F2, isPercentage: true) : CsvSanitiser.SanitiseData(CommonConstants.Hyphen));
                csvContent.Append(anyTonnageWithoutRam ? CsvSanitiser.SanitiseData(tonnage.GreenH2Proportion * 100, DecimalPlaces.Two, DecimalFormats.F2, isPercentage: true) : CsvSanitiser.SanitiseData(CommonConstants.Hyphen));
                csvContent.Append(anyTonnageWithoutRam ? CsvSanitiser.SanitiseData(tonnage.RedMedicalH2Proportion * 100, DecimalPlaces.Two, DecimalFormats.F2, isPercentage: true) : CsvSanitiser.SanitiseData(CommonConstants.Hyphen));
                csvContent.Append(anyTonnageWithoutRam ? CsvSanitiser.SanitiseData(tonnage.AmberMedicalH2Proportion * 100, DecimalPlaces.Two, DecimalFormats.F2, isPercentage: true) : CsvSanitiser.SanitiseData(CommonConstants.Hyphen));
                csvContent.Append(anyTonnageWithoutRam ? CsvSanitiser.SanitiseData(tonnage.GreenMedicalH2Proportion * 100, DecimalPlaces.Two, DecimalFormats.F2, isPercentage: true) : CsvSanitiser.SanitiseData(CommonConstants.Hyphen));

                appendRamTonnage(tonnage.ProjectedHouseholdRAMTonnage);
                appendRamTonnage(tonnage.ProjectedPublicBinRAMTonnage);

                if (materialCode == MaterialCodes.Glass)
                {
                    appendRamTonnage(tonnage.ProjectedHouseholdDrinksContainerRAMTonnage!);
                }
            }
        }

        
    }
}