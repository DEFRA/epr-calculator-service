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
                fee.TotalProducerBillBreakdownCosts = new CalcResultSummaryBadDebtProvision()
                {
                    TotalProducerFeeWithoutBadDebtProvision = GetTotalProducerBillWithoutBadDebtProvision(fee) ?? 0,
                    BadDebtProvision = GetBadDebtProvisionForTotalProducerBill(fee) ?? 0,
                    TotalProducerFeeWithBadDebtProvision = GetTotalProducerBillWithBadDebtProvision(fee) ?? 0,
                    EnglandTotalWithBadDebtProvision = GetEnglandTotalWithBadDebtProvision(fee) ?? 0,
                    WalesTotalWithBadDebtProvision = GetWalesTotalWithBadDebtProvision(fee) ?? 0,
                    ScotlandTotalWithBadDebtProvision = GetScotlandTotalWithBadDebtProvision(fee) ?? 0,
                    NorthernIrelandTotalWithBadDebtProvision = GetNorthernIrelandTotalWithBadDebtProvision(fee) ?? 0
                };
            }
        }

        private static decimal? GetTotalProducerBillWithoutBadDebtProvision(CalcResultSummaryProducerDisposalFees fee)
        {
            return fee.LocalAuthorityDisposalCostsSectionOne?.TotalProducerFeeWithoutBadDebtProvision +
                   fee.CommunicationCostsSectionTwoA?.TotalProducerFeeWithoutBadDebtProvision +
                   fee.CommunicationCostsSectionTwoB?.TotalProducerFeeWithoutBadDebtProvision +
                   fee.TwoCTotalProducerFeeForCommsCostsWithoutBadDebt +
                   fee.SchemeAdministratorOperatingCosts?.TotalProducerFeeWithoutBadDebtProvision +
                   fee.LocalAuthorityDataPreparationCosts?.TotalProducerFeeWithoutBadDebtProvision +
                   fee.OneOffSchemeAdministrationSetupCosts?.TotalProducerFeeWithoutBadDebtProvision;
        }

        private static decimal? GetBadDebtProvisionForTotalProducerBill(CalcResultSummaryProducerDisposalFees fee)
        {
            return fee.LocalAuthorityDisposalCostsSectionOne?.BadDebtProvision +
                   fee.CommunicationCostsSectionTwoA?.BadDebtProvision +
                   fee.CommunicationCostsSectionTwoB?.BadDebtProvision +
                   fee.TwoCBadDebtProvision +
                   fee.SchemeAdministratorOperatingCosts?.BadDebtProvision +
                   fee.LocalAuthorityDataPreparationCosts?.BadDebtProvision +
                   fee.OneOffSchemeAdministrationSetupCosts?.BadDebtProvision;
        }

        private static decimal? GetTotalProducerBillWithBadDebtProvision(CalcResultSummaryProducerDisposalFees fee)
        {
            return fee.LocalAuthorityDisposalCostsSectionOne?.TotalProducerFeeWithBadDebtProvision +
                   fee.CommunicationCostsSectionTwoA?.TotalProducerFeeWithBadDebtProvision +
                   fee.CommunicationCostsSectionTwoB?.TotalProducerFeeWithBadDebtProvision +
                   fee.TwoCTotalProducerFeeForCommsCostsWithBadDebt +
                   fee.SchemeAdministratorOperatingCosts?.TotalProducerFeeWithBadDebtProvision +
                   fee.LocalAuthorityDataPreparationCosts?.TotalProducerFeeWithBadDebtProvision +
                   fee.OneOffSchemeAdministrationSetupCosts?.TotalProducerFeeWithBadDebtProvision;
        }

        private static decimal? GetEnglandTotalWithBadDebtProvision(CalcResultSummaryProducerDisposalFees fee)
        {
            return fee.LocalAuthorityDisposalCostsSectionOne?.EnglandTotalWithBadDebtProvision +
                   fee.CommunicationCostsSectionTwoA?.EnglandTotalWithBadDebtProvision +
                   fee.CommunicationCostsSectionTwoB?.EnglandTotalWithBadDebtProvision +
                   fee.TwoCEnglandTotalWithBadDebt +
                   fee.SchemeAdministratorOperatingCosts?.EnglandTotalWithBadDebtProvision +
                   fee.LocalAuthorityDataPreparationCosts?.EnglandTotalWithBadDebtProvision +
                   fee.OneOffSchemeAdministrationSetupCosts?.EnglandTotalWithBadDebtProvision;
        }

        private static decimal? GetWalesTotalWithBadDebtProvision(CalcResultSummaryProducerDisposalFees fee)
        {
            return fee.LocalAuthorityDisposalCostsSectionOne?.WalesTotalWithBadDebtProvision +
                   fee.CommunicationCostsSectionTwoA?.WalesTotalWithBadDebtProvision +
                   fee.CommunicationCostsSectionTwoB?.WalesTotalWithBadDebtProvision +
                   fee.TwoCWalesTotalWithBadDebt +
                   fee.SchemeAdministratorOperatingCosts?.WalesTotalWithBadDebtProvision +
                   fee.LocalAuthorityDataPreparationCosts?.WalesTotalWithBadDebtProvision +
                   fee.OneOffSchemeAdministrationSetupCosts?.WalesTotalWithBadDebtProvision;
        }

        private static decimal? GetScotlandTotalWithBadDebtProvision(CalcResultSummaryProducerDisposalFees fee)
        {
            return fee.LocalAuthorityDisposalCostsSectionOne?.ScotlandTotalWithBadDebtProvision +
                   fee.CommunicationCostsSectionTwoA?.ScotlandTotalWithBadDebtProvision +
                   fee.CommunicationCostsSectionTwoB?.ScotlandTotalWithBadDebtProvision +
                   fee.TwoCScotlandTotalWithBadDebt +
                   fee.SchemeAdministratorOperatingCosts?.ScotlandTotalWithBadDebtProvision +
                   fee.LocalAuthorityDataPreparationCosts?.ScotlandTotalWithBadDebtProvision +
                   fee.OneOffSchemeAdministrationSetupCosts?.ScotlandTotalWithBadDebtProvision;
        }

        private static decimal? GetNorthernIrelandTotalWithBadDebtProvision(CalcResultSummaryProducerDisposalFees fee)
        {
            return fee.LocalAuthorityDisposalCostsSectionOne?.NorthernIrelandTotalWithBadDebtProvision +
                   fee.CommunicationCostsSectionTwoA?.NorthernIrelandTotalWithBadDebtProvision +
                   fee.CommunicationCostsSectionTwoB?.NorthernIrelandTotalWithBadDebtProvision +
                   fee.TwoCNorthernIrelandTotalWithBadDebt +
                   fee.SchemeAdministratorOperatingCosts?.NorthernIrelandTotalWithBadDebtProvision +
                   fee.LocalAuthorityDataPreparationCosts?.NorthernIrelandTotalWithBadDebtProvision +
                   fee.OneOffSchemeAdministrationSetupCosts?.NorthernIrelandTotalWithBadDebtProvision;
        }
    }
}