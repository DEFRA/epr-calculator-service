using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Mapper;
using EPR.Calculator.Service.Function.Mappers;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Models.JsonExporter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

namespace EPR.Calculator.Service.Function.Exporter.JsonExporter.CalcResult
{
    /// <summary>
    /// Converts a <see cref="CalcResultSummary"/> to a JSON string representation.
    /// </summary>
    public class CalculationResultsExporter : ICalculationResultsExporter
    {
        private ICommsCostsByMaterialFeesSummary2aMapper commsCostsByMaterialFeesSummary2AMapper;
        private ICalcResultCommsCostByMaterial2aJsonMapper commsCostByMaterial2aJsonMapper;

        public CalculationResultsExporter(ICommsCostsByMaterialFeesSummary2aMapper commsCostsByMaterialFeesSummary2AMapper,
            ICalcResultCommsCostByMaterial2aJsonMapper commsCostByMaterial2aJsonMapper)
        {
            this.commsCostsByMaterialFeesSummary2AMapper = commsCostsByMaterialFeesSummary2AMapper;
            this.commsCostByMaterial2aJsonMapper = commsCostByMaterial2aJsonMapper;
        }


        /// <inheritdoc/>
        public string Export(CalcResultSummary summary, IEnumerable<object>? producerCalculations, IEnumerable<int> acceptedProducerIds)
            => JsonSerializer.Serialize(
                new
                {
                    calculationResults = new
                    {
                        producerCalculationResultsSummary = ArrangeSummary(summary),
                        producerCalculationResults = ArrangeProducerCalculationResult(summary, acceptedProducerIds),
                    },
                },
                new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = true,
                    Converters = { new Converter.CurrencyConverter() },

                    // This is required in order to output the £ symbol as-is rather than encoding it.
                    Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
                });

        /// <summary>
        /// Arrange the CalcResultSummary data using the property
        /// names and ordering required for serialisation.
        /// </summary>
        private object ArrangeSummary(CalcResultSummary data)
        {
            return new
            {
                FeeForLaDisposalCostsWithoutBadDebtprovision1 = data.TotalFeeforLADisposalCostswoBadDebtprovision1,
                BadDebtProvision1 = data.BadDebtProvisionFor1,
                FeeForLaDisposalCostsWithBadDebtprovision1 = data.TotalFeeforLADisposalCostswithBadDebtprovision1,

                FeeForCommsCostsByMaterialWithoutBadDebtprovision2a = data.TotalFeeforCommsCostsbyMaterialwoBadDebtProvision2A,
                BadDebtProvision2a = data.BadDebtProvisionFor2A,
                FeeForCommsCostsByMaterialWitBadDebtprovision2a = data.TotalFeeforCommsCostsbyMaterialwithBadDebtprovision2A,

                FeeForCommsCostsUkWideWithoutBadDebtprovision2b = data.CommsCostHeaderWithoutBadDebtFor2bTitle,
                BadDebtProvision2b = data.CommsCostHeaderBadDebtProvisionFor2bTitle,
                FeeForCommsCostsUkWideWithBadDebtprovision2b = data.CommsCostHeaderWithBadDebtFor2bTitle,

                FeeForCommsCostsByCountryWithoutBadDebtprovision2c = data.TwoCCommsCostsByCountryWithoutBadDebtProvision,
                BadDebtProvision2c = data.TwoCBadDebtProvision,
                FeeForCommsCostsByCountryWideWithBadDebtprovision2c = data.TwoCCommsCostsByCountryWithBadDebtProvision,

                Total12a2b2cWithBadDebt = data.TotalOnePlus2A2B2CFeeWithBadDebtProvision,

                SaOperatingCostsWithoutBadDebtProvision3 = data.SaOperatingCostsWoTitleSection3,
                BadDebtProvision3 = data.BadDebtProvisionTitleSection3,
                SaOperatingCostsWithBadDebtProvision3 = data.SaOperatingCostsWithTitleSection3,

                LaDataPrepCostsWithoutBadDebtProvision4 = data.LaDataPrepCostsTitleSection4,
                BadDebtProvision4 = data.LaDataPrepCostsBadDebtProvisionTitleSection4,
                LaDataPrepCostsWithbadDebtProvision4 = data.LaDataPrepCostsWithBadDebtProvisionTitleSection4,

                OneOffFeeSaSetuCostsWithbadDebtProvision5 = data.SaSetupCostsTitleSection5,
                BadDebtProvision5 = data.SaSetupCostsBadDebtProvisionTitleSection5,
                OneOffFeeSaSetuCostsWithoutbadDebtProvision5 = data.SaSetupCostsWithBadDebtProvisionTitleSection5,
            };
        }


        private List<CalcSummaryProducerCalculationResults> ArrangeProducerCalculationResult(CalcResultSummary calcResultSummary, IEnumerable<int> acceptedProducerIds)
        {
            var results = new List<CalcSummaryProducerCalculationResults>();

            var filteredProducers = calcResultSummary.ProducerDisposalFees.Where(producer => int.TryParse(producer.ProducerId, out int producerId) && acceptedProducerIds.Contains(producerId) && !string.IsNullOrEmpty(producer.Level)) ?? [];

            foreach (var producer in filteredProducers)
            {
                results.Add(new CalcSummaryProducerCalculationResults {
                    ProducerDisposalFeesWithBadDebtProvision1 = ProducerDisposalFeesWithBadDebtProvision1JsonMapper.Map(producer.ProducerDisposalFeesByMaterial),
                    CalcResultCommsCostByMaterial2aJson = this.commsCostByMaterial2aJsonMapper.Map(producer.ProducerCommsFeesByMaterial),
                    CommsCostsByMaterialFeesSummary2a = this.commsCostsByMaterialFeesSummary2AMapper.Map(producer)
                });
            }

            return results;
        }
    }
}
