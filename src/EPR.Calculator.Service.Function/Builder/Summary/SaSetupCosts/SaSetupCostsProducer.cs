﻿namespace EPR.Calculator.Service.Function.Builder.Summary.SaSetupCosts
{
    using System.Collections.Generic;
    using EPR.Calculator.Service.Function.Builder.Summary.Common;
    using EPR.Calculator.Service.Function.Enums;
    using EPR.Calculator.Service.Function.Models;

    public static class SaSetupCostsProducer
    {
        public static readonly int ColumnIndex = 224;

        public static IEnumerable<CalcResultSummaryHeader> GetHeaders()
        {
            return [
                new CalcResultSummaryHeader { Name = SaSetupCostsHeaders.ProducerOneOffFeeWithoutBadDebtProvision, ColumnIndex = ColumnIndex },
                new CalcResultSummaryHeader { Name = SaSetupCostsHeaders.BadDebtProvision, ColumnIndex = ColumnIndex + 1 },
                new CalcResultSummaryHeader { Name = SaSetupCostsHeaders.ProducerOneOffFeeWithBadDebtProvision, ColumnIndex = ColumnIndex + 2 },
                new CalcResultSummaryHeader { Name = SaSetupCostsHeaders.EnglandTotalWithBadDebtProvision, ColumnIndex = ColumnIndex + 3 },
                new CalcResultSummaryHeader { Name = SaSetupCostsHeaders.WalesTotalWithBadDebtProvision, ColumnIndex = ColumnIndex + 4 },
                new CalcResultSummaryHeader { Name = SaSetupCostsHeaders.ScotlandTotalWithBadDebtProvision, ColumnIndex = ColumnIndex + 5 },
                new CalcResultSummaryHeader { Name = SaSetupCostsHeaders.NorthernIrelandTotalWithBadDebtProvision, ColumnIndex = ColumnIndex + 6 }
            ];
        }


        public static void GetProducerSetUpCosts(CalcResult calcResult, CalcResultSummary summary)
        {
            summary.SaSetupCostsTitleSection5 = SaSetupCostsSummary.GetOneOffFeeSetupCostsWithoutBadDebtProvision(calcResult);
            summary.SaSetupCostsBadDebtProvisionTitleSection5 = (summary.SaSetupCostsTitleSection5 * SaSetupCostsSummary.GetSetUpBadDebtProvision(calcResult)) / 100;
            summary.SaSetupCostsWithBadDebtProvisionTitleSection5 = summary.SaSetupCostsBadDebtProvisionTitleSection5 + summary.SaSetupCostsTitleSection5;

            foreach (var item in summary.ProducerDisposalFees)
            {
                var totalProducerFeeWithoutBadDebtProvision = GetTotalProducerFeeWithoutBadDebtProvision(summary, item);
                var badDebtProvision = GetBadDebtProvision(calcResult, totalProducerFeeWithoutBadDebtProvision);

                item.OneOffSchemeAdministrationSetupCosts = new CalcResultSummaryBadDebtProvision
                {
                    TotalProducerFeeWithoutBadDebtProvision = totalProducerFeeWithoutBadDebtProvision,
                    BadDebtProvision = badDebtProvision,
                    TotalProducerFeeWithBadDebtProvision = totalProducerFeeWithoutBadDebtProvision + badDebtProvision,
                    EnglandTotalWithBadDebtProvision = GetCountryTotalWithBadDebtProvision(calcResult, summary.SaSetupCostsTitleSection5, item.ProducerOverallPercentageOfCostsForOnePlus2A2B2C, Countries.England),
                    WalesTotalWithBadDebtProvision = GetCountryTotalWithBadDebtProvision(calcResult, summary.SaSetupCostsTitleSection5, item.ProducerOverallPercentageOfCostsForOnePlus2A2B2C, Countries.Wales),
                    ScotlandTotalWithBadDebtProvision = GetCountryTotalWithBadDebtProvision(calcResult, summary.SaSetupCostsTitleSection5, item.ProducerOverallPercentageOfCostsForOnePlus2A2B2C, Countries.Scotland),
                    NorthernIrelandTotalWithBadDebtProvision = GetCountryTotalWithBadDebtProvision(calcResult, summary.SaSetupCostsTitleSection5, item.ProducerOverallPercentageOfCostsForOnePlus2A2B2C, Countries.NorthernIreland)
                };
            }
        }

        private static decimal GetBadDebtProvision(CalcResult calcResult, decimal totalProducerFeeWithoutBadDebtProvision)
        {
            return (totalProducerFeeWithoutBadDebtProvision * SaSetupCostsSummary.GetSetUpBadDebtProvision(calcResult)) / 100;
        }

        private static decimal GetTotalProducerFeeWithoutBadDebtProvision(CalcResultSummary summary, CalcResultSummaryProducerDisposalFees item)
        {
            return (item.ProducerOverallPercentageOfCostsForOnePlus2A2B2C * summary.SaSetupCostsTitleSection5) / 100;
        }

        public static decimal GetCountryTotalWithBadDebtProvision(CalcResult calcResult, decimal oneOffFeeSetupCostsWithoutBadDebtProvision, decimal ProducerOverallPercentageOfCostsForOnePlus2A2B2C, Countries country)
        {
            var countryTotal = (CalcResultSummaryUtil.GetCountryOnePlusFourApportionment(calcResult, country)) / 100;
            return oneOffFeeSetupCostsWithoutBadDebtProvision * (1 + (calcResult.CalcResultParameterOtherCost.BadDebtValue / 100)) * (ProducerOverallPercentageOfCostsForOnePlus2A2B2C / 100) * countryTotal;
        }

    }
}
