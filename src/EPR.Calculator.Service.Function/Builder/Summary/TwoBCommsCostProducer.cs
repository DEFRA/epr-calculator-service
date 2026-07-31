using EPR.Calculator.API.Data.DataModels;

namespace EPR.Calculator.Service.Function.Builder.Summary;

public static class TwoBCommsCostProducer
{
    public static void SetValues(FeesState state, ProducerFees producerFees)
    {
        var withoutbadDebtProvision = state.CommsCost.CommsCostUkWide;
        var badDebtProvision = state.OtherCost.BadDebtValue / 100 * withoutbadDebtProvision;
        producerFees.Total.CommsCostsSection2b = new FeeWithBadDebt
        {
            FeeWithoutBadDebt = withoutbadDebtProvision.Total,
            BadDebt           = badDebtProvision.Total,
            ByCountry         = withoutbadDebtProvision + badDebtProvision
        };
    }
}
