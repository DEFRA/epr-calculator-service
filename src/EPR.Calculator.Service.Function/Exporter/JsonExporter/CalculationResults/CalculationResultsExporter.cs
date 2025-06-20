using EPR.Calculator.Service.Function.Mapper;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Models.JsonExporter;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
        private readonly IProducerDisposalFeesWithBadDebtProvision1JsonMapper producerDisposalFeesWithBadDebtProvision1JsonMapper;
        private readonly ICommsCostsByMaterialFeesSummary2aMapper commsCostsByMaterialFeesSummary2AMapper;
        private readonly ICalcResultCommsCostByMaterial2AJsonMapper commsCostByMaterial2AJsonMapper;
        private readonly ISAOperatingCostsWithBadDebtProvisionMapper sAOperatingCostsWithBadDebtProvisionMapper;
        private readonly ICalcResultLADataPrepCostsWithBadDebtProvisionMapper laDataPrepCostsWithBadDebtProvisionMapper;
        private readonly IFeeForCommsCostsWithBadDebtProvision2aMapper feeForCommsCostsWithBadDebtProvision2aMapper;
        private readonly ITotalProducerFeeWithBadDebtProvisibadDebProvisionFor2con_1_2a_2b_2cMapper totalProducerFeeWithBadDebtProvisibadDebProvisionFor2con_1_2a_2b_2cMapper;
        private readonly IFeeForSASetUpCostsWithBadDebtProvision_5Mapper feeForSASetUpCostsWithBadDebtProvision_5Mapper;
        private readonly ICalcResultCommsCostsWithBadDebtProvision2cMapper calcResultCommsCostsWithBadDebtProvision2CMapper;

        [SuppressMessage("Constructor has 8 parameters, which is greater than the 7 authorized.", "S107", Justification = "This is suppressed for now and will be refactored later")]
        public CalculationResultsExporter(
            IProducerDisposalFeesWithBadDebtProvision1JsonMapper producerDisposalFeesWithBadDebtProvision1JsonMapper,
            ICommsCostsByMaterialFeesSummary2aMapper commsCostsByMaterialFeesSummary2AMapper,
            ICalcResultCommsCostByMaterial2AJsonMapper commsCostByMaterial2AJsonMapper,
            ISAOperatingCostsWithBadDebtProvisionMapper sAOperatingCostsWithBadDebtProvisionMapper,
            ICalcResultLADataPrepCostsWithBadDebtProvisionMapper laDataPrepCostsWithBadDebtProvisionMapper,
            IFeeForCommsCostsWithBadDebtProvision2aMapper feeForCommsCostsWithBadDebtProvision2aMapper,
            ITotalProducerFeeWithBadDebtProvisibadDebProvisionFor2con_1_2a_2b_2cMapper totalProducerFeeWithBadDebtProvisibadDebProvisionFor2con_1_2a_2b_2cMapper,
            IFeeForSASetUpCostsWithBadDebtProvision_5Mapper feeForSASetUpCostsWithBadDebtProvision_5Mapper,
            ICalcResultCommsCostsWithBadDebtProvision2cMapper calcResultCommsCostsWithBadDebtProvision2CMapper)
        {
            this.producerDisposalFeesWithBadDebtProvision1JsonMapper = producerDisposalFeesWithBadDebtProvision1JsonMapper;
            this.commsCostsByMaterialFeesSummary2AMapper = commsCostsByMaterialFeesSummary2AMapper;
            this.commsCostByMaterial2AJsonMapper = commsCostByMaterial2AJsonMapper;
            this.sAOperatingCostsWithBadDebtProvisionMapper = sAOperatingCostsWithBadDebtProvisionMapper;
            this.laDataPrepCostsWithBadDebtProvisionMapper = laDataPrepCostsWithBadDebtProvisionMapper;
            this.feeForCommsCostsWithBadDebtProvision2aMapper = feeForCommsCostsWithBadDebtProvision2aMapper;
            this.totalProducerFeeWithBadDebtProvisibadDebProvisionFor2con_1_2a_2b_2cMapper = totalProducerFeeWithBadDebtProvisibadDebProvisionFor2con_1_2a_2b_2cMapper;
            this.feeForSASetUpCostsWithBadDebtProvision_5Mapper = feeForSASetUpCostsWithBadDebtProvision_5Mapper;
            this.calcResultCommsCostsWithBadDebtProvision2CMapper = calcResultCommsCostsWithBadDebtProvision2CMapper;
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

            var filteredProducers = calcResultSummary.ProducerDisposalFees.Where(producer => int.TryParse(producer.ProducerId , out int parseId) &&  acceptedProducerIds.Contains(parseId)
            && !producer.isTotalRow && !string.IsNullOrWhiteSpace(producer.Level));

            foreach (var producer in filteredProducers)
            {
                results.Add(new CalcSummaryProducerCalculationResults {
                    ProducerDisposalFeesWithBadDebtProvision1 = this.producerDisposalFeesWithBadDebtProvision1JsonMapper.Map(producer.ProducerDisposalFeesByMaterial),
                    CalcResultCommsCostByMaterial2AJson = this.commsCostByMaterial2AJsonMapper.Map(producer.ProducerCommsFeesByMaterial!),
                    CalcResultSAOperatingCostsWithBadDebtProvision = this.sAOperatingCostsWithBadDebtProvisionMapper.Map(producer),
                    CalcResultLaDataPrepCostsWithBadDebtProvision = this.laDataPrepCostsWithBadDebtProvisionMapper.Map(producer),
                    FeeForCommsCostsWithBadDebtProvision2a = this.feeForCommsCostsWithBadDebtProvision2aMapper.Map(producer),
                    CommsCostsByMaterialFeesSummary2a = this.commsCostsByMaterialFeesSummary2AMapper.Map(producer),
                    TotalProducerFeeWithBadDebtProvisibadDebProvisionFor2con_1_2a_2b_2c = this.totalProducerFeeWithBadDebtProvisibadDebProvisionFor2con_1_2a_2b_2cMapper.Map(producer),
                    FeeForSASetUpCostsWithBadDebtProvision_5 = this.feeForSASetUpCostsWithBadDebtProvision_5Mapper.Map(producer),
                    FeeForCommsCostsWithBadDebtProvision2c = this.calcResultCommsCostsWithBadDebtProvision2CMapper.Map(producer)
                });
            }

            return results;
        }               
    }
}
