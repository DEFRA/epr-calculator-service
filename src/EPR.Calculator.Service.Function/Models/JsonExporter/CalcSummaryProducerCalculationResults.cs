using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPR.Calculator.Service.Function.Models.JsonExporter
{
    public class CalcSummaryProducerCalculationResults
    {
        public required ProducerDisposalFeesWithBadDebtProvision1 ProducerDisposalFeesWithBadDebtProvision1 { get; set; }

        public CalcResultCommsCostByMaterial2aJson CalcResultCommsCostByMaterial2aJson { get; set; }

        public CalcResultSummaryCommsCostsByMaterialFeesSummary2a? CommsCostsByMaterialFeesSummary2a { get; set; }
    }
}
