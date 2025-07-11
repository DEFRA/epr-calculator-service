using System.Collections.Generic;

namespace EPR.Calculator.Service.Function.Models
{
    public class CalcResultSummaryProducerDisposalFees
    {
        public required string ProducerId { get; set; }

        public required string SubsidiaryId { get; set; }

        public required string ProducerName { get; set; }

        public string? TradingName { get; set; }

        public string? Level { get; set; }

        public string IsProducerScaledup { get; set; } = "No";

        public bool isTotalRow { get; set; } = false;

        public decimal TotalProducerDisposalFee { get; set; }

        public decimal BadDebtProvision { get; set; }

        public decimal TotalProducerDisposalFeeWithBadDebtProvision { get; set; }

        public decimal EnglandTotal { get; set; }

        public decimal WalesTotal { get; set; }

        public decimal ScotlandTotal { get; set; }

        public decimal NorthernIrelandTotal { get; set; }

        public decimal TotalProducerCommsFee { get; set; }

        public decimal BadDebtProvisionComms { get; set; }

        public decimal TotalProducerCommsFeeWithBadDebtProvision { get; set; }

        public decimal EnglandTotalComms { get; set; }

        public decimal WalesTotalComms { get; set; }

        public decimal ScotlandTotalComms { get; set; }

        public decimal NorthernIrelandTotalComms { get; set; }

        public CalcResultSummaryBadDebtProvision? LocalAuthorityDisposalCostsSectionOne { get; set; }

        public CalcResultSummaryBadDebtProvision? CommunicationCostsSectionTwoA { get; set; }

        public CalcResultSummaryBadDebtProvision? CommunicationCostsSectionTwoB { get; set; }

        public CalcResultSummaryBadDebtProvision? CommunicationCostsSectionTwoC { get; set; }

        public CalcResultSummaryBadDebtProvision? SchemeAdministratorOperatingCostsSection { get; set; }

        public CalcResultSummaryBadDebtProvision? LocalAuthorityDataPreparationCosts { get; set; }

        public CalcResultSummaryBadDebtProvision? OneOffSchemeAdministrationSetupCosts { get; set; }

        public CalcResultSummaryBadDebtProvision? TotalProducerBillBreakdownSection { get; set; }

        public required CalcResultSummaryBillingInstruction BillingInstructionSection { get; set; }

        //Section-(1) & (2a) Start
        public decimal TotalProducerFeeforLADisposalCostswoBadDebtprovision { get; set; }

        public decimal BadDebtProvisionFor1 { get; set; }

        public decimal TotalProducerFeeforLADisposalCostswithBadDebtprovision { get; set; }

        public decimal EnglandTotalWithBadDebtProvision { get; set; }

        public decimal WalesTotalWithBadDebtProvision { get; set; }

        public decimal ScotlandTotalWithBadDebtProvision { get; set; }

        public decimal NorthernIrelandTotalWithBadDebtProvision { get; set; }

        public decimal TotalProducerFeeforCommsCostsbyMaterialwoBadDebtprovision { get; set; }

        public decimal BadDebtProvisionFor2A { get; set; }

        public decimal TotalProducerFeeforCommsCostsbyMaterialwithBadDebtprovision { get; set; }

        public decimal EnglandTotalWithBadDebtProvision2A { get; set; }

        public decimal WalesTotalWithBadDebtProvision2A { get; set; }

        public decimal ScotlandTotalWithBadDebtProvision2A { get; set; }

        public decimal NorthernIrelandTotalWithBadDebtProvision2A { get; set; }
        //Section-(1) & (2a) End

        public decimal TwoCTotalProducerFeeForCommsCostsWithoutBadDebt { get; set; }
        public decimal TwoCBadDebtProvision { get; set; }
        public decimal TwoCTotalProducerFeeForCommsCostsWithBadDebt { get; set; }
        public decimal TwoCEnglandTotalWithBadDebt { get; set; }
        public decimal TwoCWalesTotalWithBadDebt { get; set; }
        public decimal TwoCScotlandTotalWithBadDebt { get; set; }
        public decimal TwoCNorthernIrelandTotalWithBadDebt { get; set; }

        public decimal PercentageofProducerReportedTonnagevsAllProducers { get; set; }

        // Section Total bill (1 + 2a + 2b + 2c)
        public decimal ProducerTotalOnePlus2A2B2CWithBadDeptProvision { get; set; }

        public decimal ProducerOverallPercentageOfCostsForOnePlus2A2B2C { get; set; }
        // End Section Total bill (1 + 2a + 2b + 2c)

        public Dictionary<string, CalcResultSummaryProducerDisposalFeesByMaterial> ProducerDisposalFeesByMaterial { get; set; }

        public Dictionary<string, CalcResultSummaryProducerCommsFeesCostByMaterial>? ProducerCommsFeesByMaterial { get; set; }

        public decimal TotalProducerFeeWithoutBadDebtFor2bComms { get; set; }

        public decimal BadDebtProvisionFor2bComms { get; set; }

        public decimal TotalProducerFeeWithBadDebtFor2bComms { get; set; }

        public decimal EnglandTotalWithBadDebtFor2bComms { get; set; }

        public decimal WalesTotalWithBadDebtFor2bComms { get; set; }

        public decimal ScotlandTotalWithBadDebtFor2bComms { get; set; }

        public decimal NorthernIrelandTotalWithBadDebtFor2bComms { get; set; }

        public string TonnageChangeCount { get; set; }

        public string TonnageChangeAdvice { get; set; }

        public bool isOverallTotalRow { get; set; } = false;
        public int ProducerIdInt { get; set; }
    }
}