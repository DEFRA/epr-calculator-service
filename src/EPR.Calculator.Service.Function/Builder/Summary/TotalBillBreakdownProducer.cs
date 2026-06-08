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
                FeeWithoutBadDebtProvision = GetTotalProducerBillWithoutBadDebtProvision(fee) ?? 0,
                BadDebtProvision           = GetBadDebtProvisionForTotalProducerBill(fee) ?? 0,
                FeeWithBadDebtProvision    = GetTotalProducerBillWithBadDebtProvision(fee)
            };
        }
    }

    // TODO add these up once, then separate out the Without/With/BadDebt
    private static decimal? GetTotalProducerBillWithoutBadDebtProvision(CalcResultSummaryProducerDisposalFees fee)
    {
        return fee.LocalAuthorityDisposalCostsSectionOne?.FeeWithoutBadDebtProvision
            + fee.CommunicationCostsSectionTwoA?.FeeWithoutBadDebtProvision
            + fee.CommunicationCostsSectionTwoB?.FeeWithoutBadDebtProvision
            + fee.TwoCTotalProducerFeeForCommsCostsWithoutBadDebt
            + fee.SchemeAdministratorOperatingCosts?.FeeWithoutBadDebtProvision
            + fee.LocalAuthorityDataPreparationCosts?.FeeWithoutBadDebtProvision
            + fee.OneOffSchemeAdministrationSetupCosts?.FeeWithoutBadDebtProvision;
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
        return (fee.LocalAuthorityDisposalCostsSectionOne?.FeeWithBadDebtProvision ?? ByCountryCost.Empty)
            + (fee.CommunicationCostsSectionTwoA?.FeeWithBadDebtProvision ?? ByCountryCost.Empty)
            + (fee.CommunicationCostsSectionTwoB?.FeeWithBadDebtProvision ?? ByCountryCost.Empty)
            + fee.TwoCTotalProducerFeeForCommsCostsWithBadDebt
            + (fee.SchemeAdministratorOperatingCosts?.FeeWithBadDebtProvision ?? ByCountryCost.Empty)
            + (fee.LocalAuthorityDataPreparationCosts?.FeeWithBadDebtProvision ?? ByCountryCost.Empty)
            + (fee.OneOffSchemeAdministrationSetupCosts?.FeeWithBadDebtProvision ?? ByCountryCost.Empty);
    }
}
