using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Models.JsonExporter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPR.Calculator.Service.Function.Mapper
{
    public class CalcResultCommsCostsWithBadDebtProvision2cMapper : ICalcResultCommsCostsWithBadDebtProvision2cMapper
    {
        public CalcResultsCommsCostsWithBadDebtProvision2c Map(CalcResultSummaryProducerDisposalFees calcResultSummaryProducerDisposalFees)
        {
            return new CalcResultsCommsCostsWithBadDebtProvision2c
            {
                TotalProducerFeeForCommsCostsByCountryWithoutBadDebtProvision = calcResultSummaryProducerDisposalFees.TwoCTotalProducerFeeForCommsCostsWithoutBadDebt,
                EnglandTotalWithBadDebtProvision = calcResultSummaryProducerDisposalFees.TwoCEnglandTotalWithBadDebt,
                NorthernIrelandTotalWithBadDebtProvision = calcResultSummaryProducerDisposalFees.TwoCNorthernIrelandTotalWithBadDebt,
                ScotlandTotalWithBadDebtProvision = calcResultSummaryProducerDisposalFees.TwoCScotlandTotalWithBadDebt,               
                BadDebProvisionFor2c = calcResultSummaryProducerDisposalFees.TwoCBadDebtProvision,
                WalesTotalWithBadDebtProvision = calcResultSummaryProducerDisposalFees.TwoCWalesTotalWithBadDebt,
                TotalProducerFeeForCommsCostsByCountryWithBadDebtProvision = calcResultSummaryProducerDisposalFees.TwoCTotalProducerFeeForCommsCostsWithBadDebt
            };
        }
    }
}
