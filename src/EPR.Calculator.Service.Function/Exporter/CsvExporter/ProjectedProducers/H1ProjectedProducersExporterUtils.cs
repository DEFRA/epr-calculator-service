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

                AppendProjectedProducerTonnageByMaterial(csvContent, producer.H1ProjectedTonnageByMaterial);

                csvContent.AppendLine();
            }
        }

        private static void AppendProjectedProducerTonnageByMaterial(StringBuilder csvContent, IReadOnlyDictionary<string, CalcResultH1ProjectedProducerMaterialTonnage> h1ProjectedProducerTonnageByMaterial)
        {
            foreach (var producerTonnage in h1ProjectedProducerTonnageByMaterial)
            {
                AppendMaterialTonnage(csvContent, producerTonnage.Key, producerTonnage.Value);
            }
        }

        private static void AppendMaterialTonnage(StringBuilder csvContent, string materialCode, CalcResultH1ProjectedProducerMaterialTonnage tonnage)
        {
            string GetProportionPercentage(decimal proportion) {
                var showProportion = 
                    (tonnage.HouseholdTonnageWithoutRAM > 0 || tonnage.PublicBinTonnageWithoutRAM > 0 || (tonnage.HouseholdDrinksContainerTonnageWithoutRAM ?? 0) > 0)
                        && tonnage.H2TotalTonnage > 0;
                return showProportion ? CsvSanitiser.SanitiseData(proportion * 100, DecimalPlaces.Two, DecimalFormats.F2, isPercentage: true) : CsvSanitiser.SanitiseData(CommonConstants.Hyphen);
            }

            AppendRamTonnage(csvContent, tonnage.HouseholdRAMTonnage);
            csvContent.Append(CsvSanitiser.SanitiseData(tonnage.HouseholdTonnageWithoutRAM, DecimalPlaces.Three, DecimalFormats.F3));

            AppendRamTonnage(csvContent, tonnage.PublicBinRAMTonnage);
            csvContent.Append(CsvSanitiser.SanitiseData(tonnage.PublicBinTonnageWithoutRAM, DecimalPlaces.Three, DecimalFormats.F3));

            if (materialCode == MaterialCodes.Glass)
            {
                AppendRamTonnage(csvContent, tonnage.HouseholdDrinksContainerRAMTonnage!);
                csvContent.Append(CsvSanitiser.SanitiseData(tonnage.HouseholdDrinksContainerTonnageWithoutRAM, DecimalPlaces.Three, DecimalFormats.F3));
            }

            csvContent.Append(GetProportionPercentage(tonnage.H2RamProportions.Red));
            csvContent.Append(GetProportionPercentage(tonnage.H2RamProportions.Amber));
            csvContent.Append(GetProportionPercentage(tonnage.H2RamProportions.Green));
            csvContent.Append(GetProportionPercentage(tonnage.H2RamProportions.RedMedical));
            csvContent.Append(GetProportionPercentage(tonnage.H2RamProportions.AmberMedical));
            csvContent.Append(GetProportionPercentage(tonnage.H2RamProportions.GreenMedical));

            AppendRamTonnage(csvContent, tonnage.ProjectedHouseholdRAMTonnage);
            AppendRamTonnage(csvContent, tonnage.ProjectedPublicBinRAMTonnage);

            if (materialCode == MaterialCodes.Glass)
            {
                AppendRamTonnage(csvContent, tonnage.ProjectedHouseholdDrinksContainerRAMTonnage!);
            }
        }

        private static void AppendRamTonnage(StringBuilder csvContent, RAMTonnage tonnage)
        {
            csvContent.Append(CsvSanitiser.SanitiseData(tonnage.Tonnage, DecimalPlaces.Three, DecimalFormats.F3));
            csvContent.Append(CsvSanitiser.SanitiseData(tonnage.RedTonnage, DecimalPlaces.Three, DecimalFormats.F3));
            csvContent.Append(CsvSanitiser.SanitiseData(tonnage.AmberTonnage, DecimalPlaces.Three, DecimalFormats.F3));
            csvContent.Append(CsvSanitiser.SanitiseData(tonnage.GreenTonnage, DecimalPlaces.Three, DecimalFormats.F3));
            csvContent.Append(CsvSanitiser.SanitiseData(tonnage.RedMedicalTonnage, DecimalPlaces.Three, DecimalFormats.F3));
            csvContent.Append(CsvSanitiser.SanitiseData(tonnage.AmberMedicalTonnage, DecimalPlaces.Three, DecimalFormats.F3));
            csvContent.Append(CsvSanitiser.SanitiseData(tonnage.GreenMedicalTonnage, DecimalPlaces.Three, DecimalFormats.F3));
        }

        
    }
}