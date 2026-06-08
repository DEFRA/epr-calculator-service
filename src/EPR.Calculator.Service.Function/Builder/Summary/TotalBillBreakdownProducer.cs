using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Builder.Summary;

public static class TotalBillBreakdownProducer
{
    public static void SetValues(CalcResultSummary summary)
    {
        foreach (var fee in summary.ProducerDisposalFees)
        {
            fee.TotalProducerBillBreakdownCosts = new CalcResultSummaryBadDebtProvision
            {
                TotalProducerFeeWithoutBadDebtProvision  = GetTotalProducerBillWithoutBadDebtProvision(fee) ?? 0,
                BadDebtProvision                         = GetBadDebtProvisionForTotalProducerBill(fee) ?? 0,
                TotalProducerFeeWithBadDebtProvision     = GetTotalProducerBillWithBadDebtProvision(fee)
            };
        }
    }

    private static decimal? GetTotalProducerBillWithoutBadDebtProvision(CalcResultSummaryProducerDisposalFees fee)
    {
        return fee.LocalAuthorityDisposalCostsSectionOne?.TotalProducerFeeWithoutBadDebtProvision
            + fee.CommunicationCostsSectionTwoA?.TotalProducerFeeWithoutBadDebtProvision
            + fee.CommunicationCostsSectionTwoB?.TotalProducerFeeWithoutBadDebtProvision
            + fee.TwoCTotalProducerFeeForCommsCostsWithoutBadDebt
            + fee.SchemeAdministratorOperatingCosts?.TotalProducerFeeWithoutBadDebtProvision
            + fee.LocalAuthorityDataPreparationCosts?.TotalProducerFeeWithoutBadDebtProvision
            + fee.OneOffSchemeAdministrationSetupCosts?.TotalProducerFeeWithoutBadDebtProvision;
    }

    private static decimal? GetBadDebtProvisionForTotalProducerBill(CalcResultSummaryProducerDisposalFees fee)
    {
        return fee.LocalAuthorityDisposalCostsSectionOne?.BadDebtProvision
            + fee.CommunicationCostsSectionTwoA?.BadDebtProvision
            + fee.CommunicationCostsSectionTwoB?.BadDebtProvision
            + fee.TwoCBadDebtProvision
            + fee.SchemeAdministratorOperatingCosts?.BadDebtProvision
            + fee.LocalAuthorityDataPreparationCosts?.BadDebtProvision
            + fee.OneOffSchemeAdministrationSetupCosts?.BadDebtProvision;
    }

    private static ByCountryCost GetTotalProducerBillWithBadDebtProvision(CalcResultSummaryProducerDisposalFees fee)
    {
        return (fee.LocalAuthorityDisposalCostsSectionOne?.TotalProducerFeeWithBadDebtProvision ?? ByCountryCost.Empty)
            + (fee.CommunicationCostsSectionTwoA?.TotalProducerFeeWithBadDebtProvision ?? ByCountryCost.Empty)
            + (fee.CommunicationCostsSectionTwoB?.TotalProducerFeeWithBadDebtProvision ?? ByCountryCost.Empty)
            + fee.TwoCTotalProducerFeeForCommsCostsWithBadDebt
            + (fee.SchemeAdministratorOperatingCosts?.TotalProducerFeeWithBadDebtProvision ?? ByCountryCost.Empty)
            + (fee.LocalAuthorityDataPreparationCosts?.TotalProducerFeeWithBadDebtProvision ?? ByCountryCost.Empty)
            + (fee.OneOffSchemeAdministrationSetupCosts?.TotalProducerFeeWithBadDebtProvision ?? ByCountryCost.Empty);
    }
}
