using System;
using System.Collections.Generic;
using EPR.Calculator.Service.Function.Exporter.JsonExporter.Model;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Mappers
{
    public class CalcResultScaledupProducersJsonMapper : ICalcResultScaledupProducersJsonMapper
    {
        public CalcResultScaledupProducerJson Map(CalcResultScaledupProducers calcResultScaledupProducers)
        {
            return new CalcResultScaledupProducerJson
            {
                Name = "Scaled-up Producers",
                ProducerSubmissions = GetProducerSubmissions(calcResultScaledupProducers)
            };
        }

        private IEnumerable<ProducerSubmission> GetProducerSubmissions(CalcResultScaledupProducers calcResultScaledupProducers)
        {
            var producerSubmissions = new List<ProducerSubmission>();
            
            foreach (var item in calcResultScaledupProducers.ScaledupProducers)
            {
                int? level = null;
                if (!string.IsNullOrWhiteSpace(item.Level) && int.TryParse(item.Level, out int result))
                {
                    level = result;
                }

                var producerSubmission = new ProducerSubmission
                {
                    ProducerId = item.ProducerId,
                    SubsidiaryId = item.SubsidiaryId,
                    ProducerName = item.ProducerName,
                    TradingName = item.TradingName,
                    Level = level,
                    SubmissionPeriodCode = item.SubmissionPeriodCode,
                    DaysInSubmissionPeriod = item.DaysInSubmissionPeriod,
                    DaysInWholePeriod = item.DaysInWholePeriod,
                    ScaleUpFactor = item.ScaleupFactor,
                    MaterialBreakdown = GetMaterialBreakdown(item.ScaledupProducerTonnageByMaterial)
                };
                producerSubmissions.Add(producerSubmission);
            }

            return producerSubmissions;
        }

        private IEnumerable<MaterialBreakdown> GetMaterialBreakdown(Dictionary<string, CalcResultScaledupProducerTonnage> producerTonnageByMaterial)
        {
            var materialBreakdown = new List<MaterialBreakdown>();

            foreach (var producerTonnage in producerTonnageByMaterial)
            {
                var breakdown = new MaterialBreakdown
                {
                    MaterialName = producerTonnage.Key,
                    ReportedHouseholdPackagingWasteTonnage = Math.Round(producerTonnage.Value.ReportedHouseholdPackagingWasteTonnage, 3),
                    ReportedPublicBinTonnage = Math.Round(producerTonnage.Value.ReportedPublicBinTonnage, 3),
                    TotalReportedTonnage = Math.Round(producerTonnage.Value.TotalReportedTonnage, 3),
                    ReportedSelfManagedConsumerWasteTonnage = Math.Round(producerTonnage.Value.ReportedSelfManagedConsumerWasteTonnage, 3),
                    NetReportedTonnage = Math.Round(producerTonnage.Value.NetReportedTonnage, 3),
                    ScaledUpReportedHouseholdPackagingWasteTonnage = Math.Round(producerTonnage.Value.ScaledupReportedHouseholdPackagingWasteTonnage, 3),
                    ScaledUpReportedPublicBinTonnage = Math.Round(producerTonnage.Value.ScaledupReportedPublicBinTonnage, 3),
                    ScaledUpTotalReportedTonnage = Math.Round(producerTonnage.Value.ScaledupTotalReportedTonnage, 3),
                    ScaledUpReportedSelfManagedConsumerWasteTonnage = Math.Round(producerTonnage.Value.ScaledupReportedSelfManagedConsumerWasteTonnage, 3),
                    ScaledUpNetReportedTonnage = Math.Round(producerTonnage.Value.ScaledupNetReportedTonnage, 3)
                };
                materialBreakdown.Add(breakdown);
            }

            return materialBreakdown;
        }
    }
}
