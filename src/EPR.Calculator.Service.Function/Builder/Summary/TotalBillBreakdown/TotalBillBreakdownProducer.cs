using EPR.Calculator.Service.Function.Builder.Summary.TotalProducerBillBreakdown;
using EPR.Calculator.Service.Function.Models;
using System.Collections.Generic;

namespace EPR.Calculator.Service.Function.Builder.Summary.TotalBillBreakdown
{
    public static class TotalBillBreakdownProducer
    {
        public static readonly int ColumnIndex = 285;

        public static IEnumerable<CalcResultSummaryHeader> GetHeaders()
        {
            return [
                new CalcResultSummaryHeader { Name = TotalBillBreakdownHeaders.TotalProducerBillWithoutBadDebtProvision, ColumnIndex = ColumnIndex },
                new CalcResultSummaryHeader { Name = TotalBillBreakdownHeaders.BadDebtProvision, ColumnIndex = ColumnIndex + 1 },
                new CalcResultSummaryHeader { Name = TotalBillBreakdownHeaders.TotalProducerBillWithBadDebtProvision, ColumnIndex = ColumnIndex + 2 },
                new CalcResultSummaryHeader { Name = TotalBillBreakdownHeaders.EnglandTotalWithBadDebtProvision, ColumnIndex = ColumnIndex + 3 },
                new CalcResultSummaryHeader { Name = TotalBillBreakdownHeaders.WalesTotalWithBadDebtProvision, ColumnIndex = ColumnIndex + 4 },
                new CalcResultSummaryHeader { Name = TotalBillBreakdownHeaders.ScotlandTotalWithBadDebtProvision, ColumnIndex = ColumnIndex + 5 },
                new CalcResultSummaryHeader { Name = TotalBillBreakdownHeaders.NorthernIrelandTotalWithBadDebtProvision, ColumnIndex = ColumnIndex + 6 }
            ];
        }

        public static IEnumerable<CalcResultSummaryHeader> GetSummaryHeaders()
        {
            return [
                new CalcResultSummaryHeader { Name = TotalBillBreakdownHeaders.TotalProducerBillBreakdown, ColumnIndex = ColumnIndex }
            ];
        }

        public static void SetValues(CalcResultSummary result)
        {
            foreach (var fee in result.ProducerDisposalFees)
            {
                fee.TotalProducerBillBreakdownSection = new CalcResultSummaryBadDebtProvision()
                {
                    TotalProducerFeeWithoutBadDebtProvision = GetTotalProducerBillWithoutBadDebtProvision(fee),
                    BadDebtProvision = GetBadDebtProvisionForTotalProducerBill(fee),
                    TotalProducerFeeWithBadDebtProvision = GetTotalProducerBillWithBadDebtProvision(fee),
                    EnglandTotalWithBadDebtProvision = GetEnglandTotalWithBadDebtProvision(fee),
                    WalesTotalWithBadDebtProvision = GetWalesTotalWithBadDebtProvision(fee),
                    ScotlandTotalWithBadDebtProvision = GetScotlandTotalWithBadDebtProvision(fee),
                    NorthernIrelandTotalWithBadDebtProvision = GetNorthernIrelandTotalWithBadDebtProvision(fee)
                };
            }
        }

        private static decimal GetTotalProducerBillWithoutBadDebtProvision(CalcResultSummaryProducerDisposalFees fee)
        {
            return fee.TotalProducerFeeforLADisposalCostswoBadDebtprovision +
                   fee.TotalProducerFeeforCommsCostsbyMaterialwoBadDebtprovision +
                   fee.TotalProducerFeeWithoutBadDebtFor2bComms +
                   fee.TwoCTotalProducerFeeForCommsCostsWithoutBadDebt +
                   fee.SchemeAdministratorOperatingCostsSection.TotalProducerFeeWithoutBadDebtProvision +
                   fee.LocalAuthorityDataPreparationCosts.TotalProducerFeeWithoutBadDebtProvision +
                   fee.TotalProducerFeeWithoutBadDebtProvisionSection5;
        }

        private static decimal GetBadDebtProvisionForTotalProducerBill(CalcResultSummaryProducerDisposalFees fee)
        {
            return fee.BadDebtProvisionFor1 +
                   fee.BadDebtProvisionFor2A +
                   fee.BadDebtProvisionFor2bComms +
                   fee.TwoCBadDebtProvision +
                   fee.SchemeAdministratorOperatingCostsSection.BadDebtProvision +
                   fee.LocalAuthorityDataPreparationCosts.BadDebtProvision +
                   fee.BadDebtProvisionSection5;
        }

        private static decimal GetTotalProducerBillWithBadDebtProvision(CalcResultSummaryProducerDisposalFees fee)
        {
            return fee.TotalProducerFeeforLADisposalCostswithBadDebtprovision +
                   fee.TotalProducerFeeforCommsCostsbyMaterialwithBadDebtprovision +
                   fee.TotalProducerFeeWithBadDebtFor2bComms +
                   fee.TwoCTotalProducerFeeForCommsCostsWithBadDebt +
                   fee.SchemeAdministratorOperatingCostsSection.TotalProducerFeeWithBadDebtProvision +
                   fee.LocalAuthorityDataPreparationCosts.TotalProducerFeeWithBadDebtProvision +
                   fee.TotalProducerFeeWithBadDebtProvisionSection5;
        }

        private static decimal GetEnglandTotalWithBadDebtProvision(CalcResultSummaryProducerDisposalFees fee)
        {
            return fee.EnglandTotalWithBadDebtProvision +
                   fee.EnglandTotalWithBadDebtProvision2A +
                   fee.EnglandTotalWithBadDebtFor2bComms +
                   fee.TwoCEnglandTotalWithBadDebt +
                   fee.SchemeAdministratorOperatingCostsSection.EnglandTotalWithBadDebtProvision +
                   fee.LocalAuthorityDataPreparationCosts.EnglandTotalWithBadDebtProvision +
                   fee.EnglandTotalWithBadDebtProvisionSection5;
        }

        private static decimal GetWalesTotalWithBadDebtProvision(CalcResultSummaryProducerDisposalFees fee)
        {
            return fee.WalesTotalWithBadDebtProvision +
                   fee.WalesTotalWithBadDebtProvision2A +
                   fee.WalesTotalWithBadDebtFor2bComms +
                   fee.TwoCWalesTotalWithBadDebt +
                   fee.SchemeAdministratorOperatingCostsSection.WalesTotalWithBadDebtProvision +
                   fee.LocalAuthorityDataPreparationCosts.WalesTotalWithBadDebtProvision +
                   fee.WalesTotalWithBadDebtProvisionSection5;
        }

        private static decimal GetScotlandTotalWithBadDebtProvision(CalcResultSummaryProducerDisposalFees fee)
        {
            return fee.ScotlandTotalWithBadDebtProvision +
                   fee.ScotlandTotalWithBadDebtProvision2A +
                   fee.ScotlandTotalWithBadDebtFor2bComms +
                   fee.TwoCScotlandTotalWithBadDebt +
                   fee.SchemeAdministratorOperatingCostsSection.ScotlandTotalWithBadDebtProvision +
                   fee.LocalAuthorityDataPreparationCosts.ScotlandTotalWithBadDebtProvision +
                   fee.ScotlandTotalWithBadDebtProvisionSection5;
        }

        private static decimal GetNorthernIrelandTotalWithBadDebtProvision(CalcResultSummaryProducerDisposalFees fee)
        {
            return fee.NorthernIrelandTotalWithBadDebtProvision +
                   fee.NorthernIrelandTotalWithBadDebtProvision2A +
                   fee.NorthernIrelandTotalWithBadDebtFor2bComms +
                   fee.TwoCNorthernIrelandTotalWithBadDebt +
                   fee.SchemeAdministratorOperatingCostsSection.NorthernIrelandTotalWithBadDebtProvision +
                   fee.LocalAuthorityDataPreparationCosts.NorthernIrelandTotalWithBadDebtProvision +
                   fee.NorthernIrelandTotalWithBadDebtProvisionSection5;
        }
    }
}