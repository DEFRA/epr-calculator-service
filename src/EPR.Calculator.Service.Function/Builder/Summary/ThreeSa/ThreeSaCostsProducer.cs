namespace EPR.Calculator.Service.Function.Builder.Summary.ThreeSA
{
    using System.Collections.Generic;
    using EPR.Calculator.Service.Function.Builder.Summary.Common;
    using EPR.Calculator.Service.Function.Builder.Summary.ThreeSa;
    using EPR.Calculator.Service.Function.Enums;
    using EPR.Calculator.Service.Function.Models;

    public static class ThreeSaCostsProducer
    {
        public static readonly int ColumnIndex = 210;

        public static IEnumerable<CalcResultSummaryHeader> GetHeaders()
        {
            return [
                new CalcResultSummaryHeader { Name = ThreeSaCostHeader.TotalSaOperatingCostsWoTitleSection3, ColumnIndex = ColumnIndex },
                new CalcResultSummaryHeader { Name = ThreeSaCostHeader.BadDebtProvisionSection3, ColumnIndex = ColumnIndex + 1 },
                new CalcResultSummaryHeader { Name = ThreeSaCostHeader.SaOperatingCostsWithTitleSection3, ColumnIndex = ColumnIndex + 2 },
                new CalcResultSummaryHeader { Name = ThreeSaCostHeader.EnglandTotalWithBadDebtProvisionSection3, ColumnIndex = ColumnIndex + 3 },
                new CalcResultSummaryHeader { Name = ThreeSaCostHeader.WalesTotalWithBadDebtProvisionSection3, ColumnIndex = ColumnIndex + 4 },
                new CalcResultSummaryHeader { Name = ThreeSaCostHeader.ScotlandTotalWithBadDebtProvisionSection3, ColumnIndex = ColumnIndex + 5 },
                new CalcResultSummaryHeader { Name = ThreeSaCostHeader.NorthernIrelandTotalWithBadDebtProvisionSection3, ColumnIndex = ColumnIndex + 6 }
            ];
        }


        public static void GetProducerSetUpCostsSection3(CalcResult calcResult, CalcResultSummary summary)
        {
            summary.SaOperatingCostsWoTitleSection3 = ThreeSaCostsSummary.GetThreeSaCostsWithoutBadDebtProvision(calcResult);
            summary.BadDebtProvisionTitleSection3 = (summary.SaOperatingCostsWoTitleSection3 * calcResult.CalcResultParameterOtherCost.BadDebtValue) / 100;
            summary.SaOperatingCostsWithTitleSection3 = summary.BadDebtProvisionTitleSection3 + summary.SaOperatingCostsWoTitleSection3;

            foreach (var item in summary.ProducerDisposalFees)
            {
                var totalProducerFeeWithoutBadDebtProvision = GetTotalProducerFeeWithoutBadDebtProvision(summary, item);
                var badDebtProvision = GetBadDebtProvision(calcResult, totalProducerFeeWithoutBadDebtProvision);

                item.SchemeAdministratorOperatingCostsSection = new CalcResultSummaryBadDebtProvision()
                {
                    TotalProducerFeeWithoutBadDebtProvision = totalProducerFeeWithoutBadDebtProvision,
                    BadDebtProvision = badDebtProvision,
                    TotalProducerFeeWithBadDebtProvision = totalProducerFeeWithoutBadDebtProvision + badDebtProvision,
                    EnglandTotalWithBadDebtProvision = GetCountryTotalWithBadDebtProvision(calcResult, summary.SaOperatingCostsWoTitleSection3, item.ProducerOverallPercentageOfCostsForOnePlus2A2B2C, Countries.England),
                    WalesTotalWithBadDebtProvision = GetCountryTotalWithBadDebtProvision(calcResult, summary.SaOperatingCostsWoTitleSection3, item.ProducerOverallPercentageOfCostsForOnePlus2A2B2C, Countries.Wales),
                    ScotlandTotalWithBadDebtProvision = GetCountryTotalWithBadDebtProvision(calcResult, summary.SaOperatingCostsWoTitleSection3, item.ProducerOverallPercentageOfCostsForOnePlus2A2B2C, Countries.Scotland),
                    NorthernIrelandTotalWithBadDebtProvision = GetCountryTotalWithBadDebtProvision(calcResult, summary.SaOperatingCostsWoTitleSection3, item.ProducerOverallPercentageOfCostsForOnePlus2A2B2C, Countries.NorthernIreland)
                };
            }
        }

        private static decimal GetBadDebtProvision(CalcResult calcResult, decimal totalProducerFeeWithoutBadDebtProvision)
        {
            return (totalProducerFeeWithoutBadDebtProvision * calcResult.CalcResultParameterOtherCost.BadDebtValue) / 100;
        }

        private static decimal GetTotalProducerFeeWithoutBadDebtProvision(CalcResultSummary summary, CalcResultSummaryProducerDisposalFees item)
        {
            return (item.ProducerOverallPercentageOfCostsForOnePlus2A2B2C * summary.SaOperatingCostsWoTitleSection3) / 100;
        }

        public static decimal GetCountryTotalWithBadDebtProvision(CalcResult calcResult, decimal threeSaCostsWithoutBadDebtProvision, decimal ProducerOverallPercentageOfCostsForOnePlus2A2B2C, Countries country)
        {
            var badDebt = calcResult.CalcResultParameterOtherCost.BadDebtValue;
            var countryTotal = (CalcResultSummaryUtil.GetCountryOnePlusFourApportionment(calcResult, country)) / 100;
            return threeSaCostsWithoutBadDebtProvision * (1 + (badDebt / 100)) * (ProducerOverallPercentageOfCostsForOnePlus2A2B2C / 100) * countryTotal;
        }
    }
}
