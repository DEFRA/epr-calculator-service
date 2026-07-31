using EPR.Calculator.API.Data.DataModels;

namespace EPR.Calculator.Service.Function.Builder.Summary;

public static class TwoCCommsCostProducer
{
    public static void SetValues(FeesState state, ProducerFees producerFees)
    {
        var commsCostByCountry = state.CommsCost.CommsCostByCountry;
        var badDebtProvision = state.OtherCost.BadDebtValue / 100 * commsCostByCountry;
        producerFees.Total.CommsCostsSection2c = new FeeWithBadDebt
        {
            FeeWithoutBadDebt = commsCostByCountry.Total,
            BadDebt           = badDebtProvision.Total,
            ByCountry    = commsCostByCountry + badDebtProvision
        };
    }

    public static void UpdateTwoCRows(FeesState state, FeeDetail result)
    {
        var commsCost = state.CommsCost.CommsCostByCountry;

        var badDebtProvisionValue =
            state.OtherCost.BadDebtValue / 100
            * state.CommsCost.CommsCostByCountry;

        result.CommsCostsSection2c = new FeeWithBadDebt
        {
            FeeWithoutBadDebt =
                commsCost.Total
                * result.ReportedTonnagePercentage
                / 100,
            BadDebt = badDebtProvisionValue.Total
                * result.ReportedTonnagePercentage
                / 100,
            ByCountry = (commsCost + badDebtProvisionValue)
                * (result.ReportedTonnagePercentage
                / 100)
        };
    }
}
