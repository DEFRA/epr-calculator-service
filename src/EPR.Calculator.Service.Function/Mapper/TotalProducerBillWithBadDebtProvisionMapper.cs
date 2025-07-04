using EPR.Calculator.Service.Common.Utils;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Models.JsonExporter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace EPR.Calculator.Service.Function.Mapper
{
    public class TotalProducerBillWithBadDebtProvisionMapper : ITotalProducerBillWithBadDebtProvisionMapper
    {
        public TotalProducerBillWithBadDebtProvision Map(CalcResultSummaryProducerDisposalFees calcResultSummaryProducerDisposalFees)
        {
            var costs = calcResultSummaryProducerDisposalFees.TotalProducerBillBreakdownSection;

            return new TotalProducerBillWithBadDebtProvision
            {
                TotalProducerBillWithoutBadDebtProvision = CurrencyConverter.ConvertToCurrency(costs.TotalProducerFeeWithoutBadDebtProvision),
                BadDebtProvisionForTotalProducerBill = CurrencyConverter.ConvertToCurrency(costs.BadDebtProvision),
                TotalProducerBillWithBadDebtProvisionAmount = CurrencyConverter.ConvertToCurrency(costs.TotalProducerFeeWithBadDebtProvision),
                EnglandTotalForProducerBillWithBadDebtProvision = CurrencyConverter.ConvertToCurrency(costs.EnglandTotalWithBadDebtProvision),
                WalesTotalForProducerBillWithBadDebtProvision = CurrencyConverter.ConvertToCurrency(costs.WalesTotalWithBadDebtProvision),
                ScotlandTotalForProducerBillWithBadDebtProvision = CurrencyConverter.ConvertToCurrency(costs.ScotlandTotalWithBadDebtProvision),
                NorthernIrelandTotalForProducerBillWithBadDebtProvision = CurrencyConverter.ConvertToCurrency(costs.NorthernIrelandTotalWithBadDebtProvision)
            };
        }
    }
}
