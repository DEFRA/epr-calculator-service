using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Models.JsonExporter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPR.Calculator.Service.Function.Mapper
{
    public class CommsCostsByMaterialFeesSummary2aMapper : ICommsCostsByMaterialFeesSummary2aMapper
    {
        public CalcResultSummaryCommsCostsByMaterialFeesSummary2a Map(CalcResultSummaryProducerDisposalFees calcResultSummaryProducerDisposalFees)
        {
            return new CalcResultSummaryCommsCostsByMaterialFeesSummary2a
            {
                EnglandTotalWithBadDebtProvision = calcResultSummaryProducerDisposalFees.EnglandTotalWithBadDebtProvision2A,
                NorthernIrelandTotalWithBadDebtProvision = calcResultSummaryProducerDisposalFees.NorthernIrelandTotalWithBadDebtProvision2A,
                ScotlandTotalWithBadDebtProvision = calcResultSummaryProducerDisposalFees.ScotlandTotalWithBadDebtProvision2A,
                TotalProducerFeeForCommsCostsWithBadDebtProvision2a = calcResultSummaryProducerDisposalFees.TotalProducerFeeforCommsCostsbyMaterialwithBadDebtprovision,
                TotalBadDebtProvision = calcResultSummaryProducerDisposalFees.BadDebtProvisionFor2A,
                WalesTotalWithBadDebtProvision = calcResultSummaryProducerDisposalFees.WalesTotalWithBadDebtProvision2A,
                TotalProducerFeeForCommsCostsWithoutBadDebtProvision2a = calcResultSummaryProducerDisposalFees.TotalProducerFeeforCommsCostsbyMaterialwoBadDebtprovision
            };

        }
    }
}
