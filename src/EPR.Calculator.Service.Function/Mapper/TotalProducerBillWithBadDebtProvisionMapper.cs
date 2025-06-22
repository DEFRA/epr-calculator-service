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
                TotalProducerBillWithoutBadDebtProvision = calcResultSummaryProducerDisposalFees.TotalProducerBillWithoutBadDebtProvision,
                BadDebtProvisionForTotalProducerBill = calcResultSummaryProducerDisposalFees.BadDebtProvisionForTotalProducerBill,
                EnglandTotalForProducerBillWithBadDebtProvision = calcResultSummaryProducerDisposalFees.EnglandTotalWithBadDebtProvision,
                WalesTotalForProducerBillWithBadDebtProvision = calcResultSummaryProducerDisposalFees.WalesTotalWithBadDebtProvision,
                ScotlandTotalForProducerBillWithBadDebtProvision = calcResultSummaryProducerDisposalFees.ScotlandTotalWithBadDebtProvision,
                NorthernIrelandTotalForProducerBillWithBadDebtProvision = calcResultSummaryProducerDisposalFees.NorthernIrelandTotalWithBadDebtProvision,
                TotalProducerBillWithBadDebtProvisionAmount = calcResultSummaryProducerDisposalFees.TotalProducerBillWithBadDebtProvision,
            };
        }
    }
}
