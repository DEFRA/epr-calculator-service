using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using EPR.Calculator.Service.Common.Utils;
using EPR.Calculator.Service.Function.Mapper;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Models.JsonExporter;
using Microsoft.AspNetCore.JsonPatch.Internal;

namespace EPR.Calculator.Service.Function.Exporter.JsonExporter.CalculationResults
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
        private readonly ICalcResultLADataPrepCostsWithBadDebtProvision4Mapper laDataPrepCostsWithBadDebtProvision4Mapper;
        private readonly IFeeForCommsCostsWithBadDebtProvision2aMapper feeForCommsCostsWithBadDebtProvision2aMapper;
        private readonly IFeeForCommsCostsWithBadDebtProvision2bMapper feeForCommsCostsWithBadDebtProvision2bMapper;
        private readonly ITotalProducerFeeWithBadDebtProvisibadDebProvisionFor2con_1_2a_2b_2cMapper totalProducerFeeWithBadDebtProvisibadDebProvisionFor2con_1_2a_2b_2cMapper;
        private readonly IFeeForSASetUpCostsWithBadDebtProvision_5Mapper feeForSASetUpCostsWithBadDebtProvision_5Mapper;
        private readonly ICalcResultCommsCostsWithBadDebtProvision2cMapper calcResultCommsCostsWithBadDebtProvision2CMapper;
        private readonly ICalculationOfSuggestedBillingInstructionsAndInvoiceAmountsMapper calculationOfSuggestedBillingInstructionsAndInvoiceAmountsMapper;
        private readonly ITotalProducerBillWithBadDebtProvisionMapper totalProducerBillWithBadDebtProvisionMapper;
        private readonly ICalculationResultsProducerCalculationResultsFeeForLADisposalCosts1Mapper calculationResultsProducerCalculationResultsFeeForLADisposalCosts1Mapper;
        private readonly ICalcResultProducerCalculationResultsTotalMapper calcResultProducerCalculationResultsTotalMapper;
        private readonly IDisposalFeeSummary1Mapper disposalFeeSummary1Mapper;

        [SuppressMessage("Constructor has 8 parameters, which is greater than the 7 authorized.", "S107", Justification = "This is suppressed for now and will be refactored later")]
        public CalculationResultsExporter(
            IProducerDisposalFeesWithBadDebtProvision1JsonMapper producerDisposalFeesWithBadDebtProvision1JsonMapper,
            ICommsCostsByMaterialFeesSummary2aMapper commsCostsByMaterialFeesSummary2AMapper,
            ICalcResultCommsCostByMaterial2AJsonMapper commsCostByMaterial2AJsonMapper,
            ISAOperatingCostsWithBadDebtProvisionMapper sAOperatingCostsWithBadDebtProvisionMapper,
            ICalcResultLADataPrepCostsWithBadDebtProvision4Mapper laDataPrepCostsWithBadDebtProvision4Mapper,
            IFeeForCommsCostsWithBadDebtProvision2aMapper feeForCommsCostsWithBadDebtProvision2aMapper,
            IFeeForCommsCostsWithBadDebtProvision2bMapper feeForCommsCostsWithBadDebtProvision2bMapper,
            ITotalProducerFeeWithBadDebtProvisibadDebProvisionFor2con_1_2a_2b_2cMapper totalProducerFeeWithBadDebtProvisibadDebProvisionFor2con_1_2a_2b_2cMapper,
            IFeeForSASetUpCostsWithBadDebtProvision_5Mapper feeForSASetUpCostsWithBadDebtProvision_5Mapper,
            ICalcResultCommsCostsWithBadDebtProvision2cMapper calcResultCommsCostsWithBadDebtProvision2CMapper,
            ICalculationOfSuggestedBillingInstructionsAndInvoiceAmountsMapper calculationOfSuggestedBillingInstructionsAndInvoiceAmountsMapper,
            ITotalProducerBillWithBadDebtProvisionMapper totalProducerBillWithBadDebtProvisionMapper,
            ICalculationResultsProducerCalculationResultsFeeForLADisposalCosts1Mapper calculationResultsProducerCalculationResultsFeeForLADisposalCosts1Mapper,
            ICalcResultProducerCalculationResultsTotalMapper calcResultProducerCalculationResultsTotalMapper,
            IDisposalFeeSummary1Mapper disposalFeeSummary1Mapper
            )
        {
            this.producerDisposalFeesWithBadDebtProvision1JsonMapper = producerDisposalFeesWithBadDebtProvision1JsonMapper;
            this.commsCostsByMaterialFeesSummary2AMapper = commsCostsByMaterialFeesSummary2AMapper;
            this.commsCostByMaterial2AJsonMapper = commsCostByMaterial2AJsonMapper;
            this.sAOperatingCostsWithBadDebtProvisionMapper = sAOperatingCostsWithBadDebtProvisionMapper;
            this.laDataPrepCostsWithBadDebtProvision4Mapper = laDataPrepCostsWithBadDebtProvision4Mapper;
            this.feeForCommsCostsWithBadDebtProvision2aMapper = feeForCommsCostsWithBadDebtProvision2aMapper;
            this.feeForCommsCostsWithBadDebtProvision2bMapper = feeForCommsCostsWithBadDebtProvision2bMapper;
            this.totalProducerFeeWithBadDebtProvisibadDebProvisionFor2con_1_2a_2b_2cMapper = totalProducerFeeWithBadDebtProvisibadDebProvisionFor2con_1_2a_2b_2cMapper;
            this.feeForSASetUpCostsWithBadDebtProvision_5Mapper = feeForSASetUpCostsWithBadDebtProvision_5Mapper;
            this.calcResultCommsCostsWithBadDebtProvision2CMapper = calcResultCommsCostsWithBadDebtProvision2CMapper;
            this.calculationOfSuggestedBillingInstructionsAndInvoiceAmountsMapper = calculationOfSuggestedBillingInstructionsAndInvoiceAmountsMapper;
            this.totalProducerBillWithBadDebtProvisionMapper = totalProducerBillWithBadDebtProvisionMapper;
            this.calculationResultsProducerCalculationResultsFeeForLADisposalCosts1Mapper = calculationResultsProducerCalculationResultsFeeForLADisposalCosts1Mapper;
            this.calcResultProducerCalculationResultsTotalMapper = calcResultProducerCalculationResultsTotalMapper;
            this.disposalFeeSummary1Mapper = disposalFeeSummary1Mapper;
        }

        /// <inheritdoc/>
        public object Export(CalcResultSummary summary, IEnumerable<int> acceptedProducerIds, List<MaterialDetail> materials)
            =>
                new
                {
                    producerCalculationResultsSummary = ArrangeSummary(summary),
                    producerCalculationResults = ArrangeProducerCalculationResult(summary, acceptedProducerIds, materials),
                    producerCalculationResultsTotal = ArrangeProducerCalculationResultsTotal(summary),
                };

        /// <summary>
        /// Arrange the CalcResultSummary data using the property
        /// names and ordering required for serialisation.
        /// </summary>
        private object ArrangeSummary(CalcResultSummary data)
        {
            return new ProducerCalculationResultsSummary
            {
                FeeForLaDisposalCostsWithoutBadDebtprovision1 = CurrencyConverter.ConvertToCurrency(data.TotalFeeforLADisposalCostswoBadDebtprovision1),
                BadDebtProvision1 = CurrencyConverter.ConvertToCurrency(data.BadDebtProvisionFor1),
                FeeForLaDisposalCostsWithBadDebtprovision1 = CurrencyConverter.ConvertToCurrency(data.TotalFeeforLADisposalCostswithBadDebtprovision1),

                FeeForCommsCostsByMaterialWithoutBadDebtprovision2a = CurrencyConverter.ConvertToCurrency(data.TotalFeeforCommsCostsbyMaterialwoBadDebtProvision2A),
                BadDebtProvision2a = CurrencyConverter.ConvertToCurrency(data.BadDebtProvisionFor2A),
                FeeForCommsCostsByMaterialWitBadDebtprovision2a = CurrencyConverter.ConvertToCurrency(data.TotalFeeforCommsCostsbyMaterialwithBadDebtprovision2A),

                FeeForCommsCostsUkWideWithoutBadDebtprovision2b = CurrencyConverter.ConvertToCurrency(data.CommsCostHeaderWithoutBadDebtFor2bTitle),
                BadDebtProvision2b = CurrencyConverter.ConvertToCurrency(data.CommsCostHeaderBadDebtProvisionFor2bTitle),
                FeeForCommsCostsUkWideWithBadDebtprovision2b = CurrencyConverter.ConvertToCurrency(data.CommsCostHeaderWithBadDebtFor2bTitle),

                FeeForCommsCostsByCountryWithoutBadDebtprovision2c = CurrencyConverter.ConvertToCurrency(data.TwoCCommsCostsByCountryWithoutBadDebtProvision),
                BadDebtProvision2c = CurrencyConverter.ConvertToCurrency(data.TwoCBadDebtProvision),
                FeeForCommsCostsByCountryWideWithBadDebtprovision2c = CurrencyConverter.ConvertToCurrency(data.TwoCCommsCostsByCountryWithBadDebtProvision),

                Total12a2b2cWithBadDebt = CurrencyConverter.ConvertToCurrency(data.TotalOnePlus2A2B2CFeeWithBadDebtProvision),

                SaOperatingCostsWithoutBadDebtProvision3 = CurrencyConverter.ConvertToCurrency(data.SaOperatingCostsWoTitleSection3),
                BadDebtProvision3 = CurrencyConverter.ConvertToCurrency(data.BadDebtProvisionTitleSection3),
                SaOperatingCostsWithBadDebtProvision3 = CurrencyConverter.ConvertToCurrency(data.SaOperatingCostsWithTitleSection3),

                LaDataPrepCostsWithoutBadDebtProvision4 = CurrencyConverter.ConvertToCurrency(data.LaDataPrepCostsTitleSection4),
                BadDebtProvision4 = CurrencyConverter.ConvertToCurrency(data.LaDataPrepCostsBadDebtProvisionTitleSection4),
                LaDataPrepCostsWithbadDebtProvision4 = CurrencyConverter.ConvertToCurrency(data.LaDataPrepCostsWithBadDebtProvisionTitleSection4),

                OneOffFeeSaSetuCostsWithbadDebtProvision5 = CurrencyConverter.ConvertToCurrency(data.SaSetupCostsTitleSection5),
                BadDebtProvision5 = CurrencyConverter.ConvertToCurrency(data.SaSetupCostsBadDebtProvisionTitleSection5),
                OneOffFeeSaSetuCostsWithoutbadDebtProvision5 = CurrencyConverter.ConvertToCurrency(data.SaSetupCostsWithBadDebtProvisionTitleSection5),
            };
        }


        private List<CalcSummaryProducerCalculationResults> ArrangeProducerCalculationResult(
            CalcResultSummary calcResultSummary,
            IEnumerable<int> acceptedProducerIds,
            List<MaterialDetail> materials)
        {
            var results = new List<CalcSummaryProducerCalculationResults>();

            var filteredProducers = calcResultSummary.ProducerDisposalFees.Where(producer => acceptedProducerIds.Contains(producer.ProducerIdInt) && !string.IsNullOrWhiteSpace(producer.Level));

            foreach (var producer in filteredProducers)
            {
                results.Add(new CalcSummaryProducerCalculationResults
                {
                    ProducerID = producer.ProducerId,
                    SubsidiaryID = producer.SubsidiaryId,
                    ProducerName = producer.ProducerName,
                    TradingName = producer.TradingName,
                    Level = string.IsNullOrWhiteSpace(producer.Level) ? null : int.Parse(producer.Level),
                    ScaledUpTonnages = producer.IsProducerScaledup,
                    ProducerDisposalFeesWithBadDebtProvision1 = this.producerDisposalFeesWithBadDebtProvision1JsonMapper.Map(producer.ProducerDisposalFeesByMaterial, materials, producer.Level!),
                    FeesForCommsCostsWithBadDebtProvision2a = this.commsCostByMaterial2AJsonMapper.Map(producer.ProducerCommsFeesByMaterial!, materials),
                    FeeForSAOperatingCostsWithBadDebtProvision_3 = this.sAOperatingCostsWithBadDebtProvisionMapper.Map(producer),
                    FeeForLADataPrepCostsWithBadDebtProvision_4 = this.laDataPrepCostsWithBadDebtProvision4Mapper.Map(producer),
                    FeeForCommsCostsWithBadDebtProvision_2a = this.feeForCommsCostsWithBadDebtProvision2aMapper.Map(producer),
                    FeeForCommsCostsWithBadDebtProvision_2b = this.feeForCommsCostsWithBadDebtProvision2bMapper.Map(producer),
                    CommsCostsByMaterialFeesSummary2a = this.commsCostsByMaterialFeesSummary2AMapper.Map(producer),
                    TotalProducerFeeWithBadDebtProvisibadDebProvisionFor2con_1_2a_2b_2c = this.totalProducerFeeWithBadDebtProvisibadDebProvisionFor2con_1_2a_2b_2cMapper.Map(producer),
                    FeeForSASetUpCostsWithBadDebtProvision_5 = this.feeForSASetUpCostsWithBadDebtProvision_5Mapper.Map(producer),
                    FeeForCommsCostsWithBadDebtProvision_2c = this.calcResultCommsCostsWithBadDebtProvision2CMapper.Map(producer),
                    CalculationOfSuggestedBillingInstructionsAndInvoiceAmounts = this.calculationOfSuggestedBillingInstructionsAndInvoiceAmountsMapper.Map(producer),
                    TotalProducerBillWithBadDebtProvision = this.totalProducerBillWithBadDebtProvisionMapper.Map(producer),
                    FeeForLADisposalCosts1 = this.calculationResultsProducerCalculationResultsFeeForLADisposalCosts1Mapper.Map(producer),
                    DisposalFeeSummary1 = this.disposalFeeSummary1Mapper.Map(producer),
                });
            }

            return results;
        }

        private CalcResultProducerCalculationResultsTotal? ArrangeProducerCalculationResultsTotal(CalcResultSummary calcResultSummary)
        {
            return calcResultProducerCalculationResultsTotalMapper.Map(calcResultSummary);
        }
    }
}
