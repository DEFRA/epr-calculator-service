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
            return new TotalProducerBillWithBadDebtProvision
            {
                TotalProducerBillWithoutBadDebtProvision = CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees.TotalProducerBillWithoutBadDebtProvision),
                BadDebtProvisionForTotalProducerBill = CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees.BadDebtProvisionForTotalProducerBill),
                EnglandTotalForProducerBillWithBadDebtProvision = CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees.EnglandTotalWithBadDebtProvisionTotalBill),
                WalesTotalForProducerBillWithBadDebtProvision = CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees.WalesTotalWithBadDebtProvisionTotalBill),
                ScotlandTotalForProducerBillWithBadDebtProvision = CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees.ScotlandTotalWithBadDebtProvisionTotalBill),
                NorthernIrelandTotalForProducerBillWithBadDebtProvision = CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees.NorthernIrelandTotalWithBadDebtProvisionTotalBill),
                TotalProducerBillWithBadDebtProvisionAmount = CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees.TotalProducerBillWithBadDebtProvision),
            };
        }
    }
}
