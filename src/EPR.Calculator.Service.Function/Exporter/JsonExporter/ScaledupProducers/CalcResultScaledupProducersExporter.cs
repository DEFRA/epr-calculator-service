namespace EPR.Calculator.Service.Function.Exporter.JsonExporter.ScaledupProducers
{
    using System.Collections.Generic;
    using System.Text;
    using EPR.Calculator.API.Utils;
    using EPR.Calculator.Service.Function.Constants;
    using EPR.Calculator.Service.Function.Enums;
    using EPR.Calculator.Service.Function.Models;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;

    public class CalcResultScaledupProducersExporter : ICalcResultScaledupProducersExporter
    {
        public string Export(CalcResultScaledupProducers producers, StringBuilder csvContent)
        {
            var settings = new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new CamelCaseNamingStrategy()
                }
            };

            return JsonConvert.SerializeObject(GetScaledupProducers(producers), settings);
        }

        public static ScaledupProducers GetScaledupProducers(CalcResultScaledupProducers producers)
        {
            var producerSubmissions = new List<ProducerSubmission>();

            foreach (var producer in producers.ScaledupProducers!)
            {
                var producerSubmission = new ProducerSubmission()
                {
                    producerId = producer.ProducerId,
                    subsidiaryId = producer.SubsidiaryId,
                    producerName = producer.ProducerName,
                    tradingName = producer.TradingName,
                    level = producer.Level,
                    submissionPeriodCode = producer.SubmissionPeriodCode,
                    daysInSubmissionPeriod = producer.DaysInSubmissionPeriod,
                    daysInWholePeriod = producer.DaysInWholePeriod,
                    scaleUpFactor = producer.ScaleupFactor,
                    materialBreakdown = GetMaterialBreakdowns(producer)
                };

                producerSubmissions.Add(producerSubmission);
            }

            var scaledupProducers = new ScaledupProducers()
            {
                name = "Scaled-up Producers",
                producerSubmissions = producerSubmissions
            };

            return scaledupProducers;
        }
        private static IEnumerable<MaterialBreakdown> GetMaterialBreakdowns(CalcResultScaledupProducer producer)
        {
            var materialBreakdowns = new List<MaterialBreakdown>();

            foreach (var material in producer.ScaledupProducerTonnageByMaterial)
            {
                materialBreakdowns.Add(new MaterialBreakdown()
                {
                    materialName = material.Key,
                    reportedHouseholdPackagingWasteTonnage = material.Value.ReportedHouseholdPackagingWasteTonnage,
                    reportedPublicBinTonnage = material.Value.ReportedPublicBinTonnage,
                    totalReportedTonnage = material.Value.TotalReportedTonnage,
                    reportedSelfManagedConsumerWasteTonnage = material.Value.ReportedSelfManagedConsumerWasteTonnage,
                    netReportedTonnage = material.Value.NetReportedTonnage,
                    scaledUpReportedHouseholdPackagingWasteTonnage = material.Value.ScaledupReportedHouseholdPackagingWasteTonnage,
                    scaledUpReportedPublicBinTonnage = material.Value.ScaledupReportedPublicBinTonnage,
                    scaledUpTotalReportedTonnage = material.Value.ScaledupTotalReportedTonnage,
                    scaledUpReportedSelfManagedConsumerWasteTonnage = material.Value.ScaledupReportedSelfManagedConsumerWasteTonnage,
                    scaledUpNetReportedTonnage = material.Value.ScaledupNetReportedTonnage
                });
            }

            return materialBreakdowns;
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
    }
}