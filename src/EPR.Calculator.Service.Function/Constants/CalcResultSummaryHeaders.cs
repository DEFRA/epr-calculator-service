﻿using Microsoft.Extensions.Hosting;
using static Azure.Core.HttpHeader;

namespace EPR.Calculator.Service.Function.Constants
{
    public static class CalcResultSummaryHeaders
    {
        public static readonly string CalculationResult = "Calculation Result";
        public static readonly string Notes = "NOTE: Rows with 'Scaled-up tonnages?' = " +
            "Yes include reported tonnages for a period that have been scaled-up to a full 6 month equivalent period. " +
            "See 'Scaled-up Producers' table for details.";

        public static readonly string OneProducerDisposalFeesWithBadDebtProvision =
            "1 Producer Disposal Fees with Bad Debt Provision";

        public static readonly string CommsCostHeader = "2a Fees for Comms Costs - by Material with Bad Debt provision";
        public static readonly string CommsCostSummaryHeader = "Summary of Fee for Comms Costs - by Material";


        public static readonly string DisposalFeeSummary = "Disposal Fee Summary";
        public static readonly string ProducerId = "Producer ID";
        public static readonly string SubsidiaryId = "Subsidiary ID";
        public static readonly string ProducerOrSubsidiaryName = "Producer / Subsidiary Name";
        public static readonly string Level = "Level";
        public static readonly string ScaledupTonnages = "Scaled-up tonnages?";

        public static readonly string HouseholdPackagingWasteTonnage = "Household Packaging Waste Tonnage";
        public static readonly string PublicBinTonnage = "Public Bin Tonnage";
        public static readonly string HouseholdDrinksContainersTonnage = "Household Drinks Containers Tonnage - Glass";
        public static readonly string TotalTonnage = "Total Tonnage";
        public static readonly string SelfManagedConsumerWasteTonnage = "Self Managed Consumer Waste Tonnage";
        public static readonly string NetTonnage = "Net Tonnage";

        public static readonly string PricePerTonne = "Price per Tonne";
        public static readonly string ProducerDisposalFee = "Producer Disposal Fee w/o Bad Debt Provision";
        public static readonly string BadDebtProvision = "Bad Debt Provision";

        public static readonly string ProducerDisposalFeeWithBadDebtProvision =
            "Producer Disposal Fee with Bad Debt Provision";

        public static readonly string EnglandWithBadDebtProvision = "England with Bad Debt Provision";
        public static readonly string WalesWithBadDebtProvision = "Wales with Bad Debt Provision";
        public static readonly string ScotlandWithBadDebtProvision = "Scotland with Bad Debt Provision";
        public static readonly string NorthernIrelandWithBadDebtProvision = "Northern Ireland with Bad Debt Provision";

        public static readonly string ProducerTotalCostWithoutBadDebtProvision =
            "Producer Total Cost w/o Bad Debt Provision";

        public static readonly string ProducerTotalCostwithBadDebtProvision =
            "Producer Total Cost with Bad Debt Provision";

        public static readonly string TotalProducerFeeforCommsCostsbyMaterialwoBadDebtprovision =
            "2a Total Producer Fee for Comms Costs - by Material w/o Bad Debt provision";

        public static readonly string TotalBadDebtProvision = "Total Bad Debt Provision";

        public static readonly string TotalProducerFeeforCommsCostsbyMaterialwithBadDebtprovision =
            "2a Total Producer Fee for Comms Costs - by Material with Bad Debt provision";

        public static readonly string EnglandTotalwithBadDebtprovision = "England Total with Bad Debt provision";
        public static readonly string WalesTotalwithBadDebtprovision = "Wales Total with Bad Debt provision";
        public static readonly string ScotlandTotalwithBadDebtprovision = "Scotland Total with Bad Debt provision";

        public static readonly string NorthernIrelandTotalwithBadDebtprovision =
            "Northern Ireland Total with Bad Debt provision";

        public static readonly string TotalProducerDisposalFee = "1 Total Producer Disposal Fee w/o Bad Debt Provision";
        public static readonly string TotalProducerDisposalFeeWithBadDebtProvision = "1 Total Producer Disposal Fee with Bad Debt Provision";
        public static readonly string EnglandTotal = "England Total";
        public static readonly string WalesTotal = "Wales Total";
        public static readonly string ScotlandTotal = "Scotland Total";
        public static readonly string NorthernIrelandTotal = "Northern Ireland Total";
        public static readonly string OneCountryApportionment = "1 Country Apportionment %s";

        //section 7
        public static readonly string TotalProducerFeeforLADisposalCostswoBadDebtprovision =
            "1 Total Producer Fee for LA Disposal Costs w/o Bad Debt provision";

        public static readonly string BadDebtProvisionFor1 = "Bad Debt Provision for 1";

        public static readonly string TotalProducerFeeforLADisposalCostswithBadDebtprovision =
            "1 Total Producer Fee for LA Disposal Costs with Bad Debt provision";

        public static readonly string TotalProducerFeeforCommsCostsbyMaterialwoBadDebtprovision2A =
            "2a Total Producer Fee for Comms Costs - by Material w/o Bad Debt provision";

        public static readonly string BadDebtProvisionfor2A = "Bad Debt Provision for 2a";

        public static readonly string TotalProducerFeeforCommsCostsbyMaterialwithBadDebtprovision2A =
            "2a Total Producer Fee for Comms Costs - by Material with Bad Debt provision";

        public static readonly string FeeforLADisposalCostswoBadDebtprovision1 =
            "1 Fee for LA Disposal Costs w/o Bad Debt provision";

        public static readonly string FeeforLADisposalCostswithBadDebtprovision1 =
            "1 Fee for LA Disposal Costs with Bad Debt provision";

        public static readonly string FeeforCommsCostsbyMaterialwoBadDebtprovision2A =
            "2a Fee for Comms Costs - by Material w/o Bad Debt provision";

        public static readonly string FeeforCommsCostsbyMaterialwithBadDebtprovision2A =
            "2a Fee for Comms Costs - by Material with Bad Debt provision";

        // Percentage of Producer Tonnage vs All Producers
        public static readonly string PercentageofProducerTonnagevsAllProducers = "Percentage of Producer Tonnage vs All Producers";

        // 2b comms total
        public static readonly string CommsCostHeaderWithoutBadDebtFor2bTitle = "2b Comms Costs - UK wide w/o Bad Debt provision";
        public static readonly string CommsCostHeaderWithBadDebtFor2bTitle = "2b Comms Costs - UK wide with Bad Debt provision";
        public static readonly string CommsCostHeaderBadDebtProvisionFor2bTitle = "Bad Debt provision";
        public static readonly string ProducerFeeWithoutBadDebtForComms2b = "2b Total Producer Fee for Comms Costs - UK wide In proportion to Producer Tonnage w/o Bad Debt provision";
        public static readonly string BadDebtProvisionForComms2b = "Bad Debt Provision for 2b";
        public static readonly string ProducerFeeForCommsCostsWithBadDebtForComms2b = "2b Total Producer Fee for Comms Costs - UK wide In proportion to Producer Tonnage with Bad Debt provision";
        public static readonly string EnglandTotalWithBadDebtProvisionForComms2b = "England Total with Bad Debt provision";
        public static readonly string WalesTotalWithBadDebtProvisionForComms2b = "Wales Total with Bad Debt provision";
        public static readonly string ScotlandTotalWithBadDebtProvisionForComms2b = "Scotland Total with Bad Debt provision";
        public static readonly string NorthernIrelandTotalWithBadDebtProvisionForComms2b = "Northern Ireland Total with Bad Debt provision";
    }
}