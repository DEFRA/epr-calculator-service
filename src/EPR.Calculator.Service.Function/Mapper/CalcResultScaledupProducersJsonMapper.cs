using System;
using System.Collections.Generic;
using System.Linq;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Models.JsonExporter;
using EPR.Calculator.Service.Function.Services;

namespace EPR.Calculator.Service.Function.Mapper
{
    public class CalcResultScaledupProducersJsonMapper : ICalcResultScaledupProducersJsonMapper
    {
        public CalcResultScaledupProducersJson Map(
            CalcResultScaledupProducers calcResultScaledupProducers,
            IEnumerable<int> acceptedProducerIds,
            List<MaterialDetail> materials)
        {
            if (calcResultScaledupProducers == null)
            {
                return new CalcResultScaledupProducersJson();
            }

            return new CalcResultScaledupProducersJson
            {
                Name = CalcResultScaledupProducerHeaders.ScaledupProducers,
                ProducerSubmissions = GetProducerSubmissions(calcResultScaledupProducers, acceptedProducerIds, materials)
            };
        }

        private IEnumerable<ProducerSubmission> GetProducerSubmissions(
            CalcResultScaledupProducers calcResultScaledupProducers,
            IEnumerable<int> acceptedProducerIds,
            List<MaterialDetail> materials)
        {
            var producerSubmissions = new List<ProducerSubmission>();

            var filteredProducers = calcResultScaledupProducers?.ScaledupProducers?.Where(producer => acceptedProducerIds.Contains(producer.ProducerId)) ?? [];

            foreach (var item in filteredProducers)
            {
                int? level = null;
                if (!string.IsNullOrWhiteSpace(item.Level) && int.TryParse(item.Level, out int result))
                {
                    level = result;
                }

                var producerSubmission = new ProducerSubmission
                {
                    ProducerId = item.ProducerId,
                    SubsidiaryId = string.IsNullOrWhiteSpace(item.SubsidiaryId) ? null : item.SubsidiaryId,
                    ProducerName = item.ProducerName,
                    TradingName = string.IsNullOrWhiteSpace(item.TradingName) ? null : item.TradingName,
                    Level = level,
                    SubmissionPeriodCode = item.SubmissionPeriodCode,
                    DaysInSubmissionPeriod = item.DaysInSubmissionPeriod,
                    DaysInWholePeriod = item.DaysInWholePeriod,
                    ScaleUpFactor = item.ScaleupFactor,
                    MaterialBreakdown = GetMaterialBreakdown(item.ScaledupProducerTonnageByMaterial, materials)
                };
                producerSubmissions.Add(producerSubmission);
            }

            return producerSubmissions;
        }

        private IEnumerable<MaterialBreakdown> GetMaterialBreakdown(
            Dictionary<string, CalcResultScaledupProducerTonnage> producerTonnageByMaterial,
            List<MaterialDetail> materials)
        {
            var materialBreakdown = new List<MaterialBreakdown>();

            foreach (var producerTonnage in producerTonnageByMaterial)
            {
                var material = materials.Single(m => m.Code == producerTonnage.Key);

                var breakdown = new MaterialBreakdown
                {
                    MaterialName = material.Name,
                    ReportedHouseholdPackagingWasteTonnage = producerTonnage.Value.ReportedHouseholdPackagingWasteTonnage,
                    ReportedPublicBinTonnage = producerTonnage.Value.ReportedPublicBinTonnage,
                    TotalReportedTonnage = producerTonnage.Value.TotalReportedTonnage,
                    ReportedSelfManagedConsumerWasteTonnage = producerTonnage.Value.ReportedSelfManagedConsumerWasteTonnage,
                    NetReportedTonnage = producerTonnage.Value.NetReportedTonnage,
                    ScaledUpReportedHouseholdPackagingWasteTonnage = producerTonnage.Value.ScaledupReportedHouseholdPackagingWasteTonnage,
                    ScaledUpReportedPublicBinTonnage = producerTonnage.Value.ScaledupReportedPublicBinTonnage,
                    ScaledUpTotalReportedTonnage = producerTonnage.Value.ScaledupTotalReportedTonnage,
                    ScaledUpReportedSelfManagedConsumerWasteTonnage = producerTonnage.Value.ScaledupReportedSelfManagedConsumerWasteTonnage,
                    ScaledUpNetReportedTonnage = producerTonnage.Value.ScaledupNetReportedTonnage,
                };

                if (producerTonnage.Key == MaterialCodes.Glass)
                {
                    breakdown.HouseholdDrinksContainersTonnageGlass = producerTonnage.Value.HouseholdDrinksContainersTonnageGlass;
                    breakdown.ScaledUpHouseholdDrinksContainersTonnageGlass = producerTonnage.Value.ScaledupHouseholdDrinksContainersTonnageGlass;
                }

                materialBreakdown.Add(breakdown);
            }

            return materialBreakdown;
        }
    }
}
