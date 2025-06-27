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
                    SubsidiaryId = item.SubsidiaryId,
                    ProducerName = item.ProducerName,
                    TradingName = item.TradingName,
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

                if (producerTonnage.Key == MaterialCodes.Glass)
                {
                    breakdown.HouseholdDrinksContainersTonnageGlass = producerTonnage.Value.HouseholdDrinksContainersTonnageGlass;
                    breakdown.ScaledupHouseholdDrinksContainersTonnageGlass = producerTonnage.Value.ScaledupHouseholdDrinksContainersTonnageGlass;
                }

                materialBreakdown.Add(breakdown);
            }

            return materialBreakdown;
        }
    }
}
