using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPR.Calculator.Service.Function.Builder.Summary.Common
{
    internal static class TonnageChangeUtil
    {
        public static decimal? ComputePerMaterialChange(
            string level,
            decimal netReportedTonnage,
            decimal? previousInvoicedTonnage)
        {
            if (level != CommonConstants.LevelOne.ToString())
                return null;

            if (!previousInvoicedTonnage.HasValue)
                return null;

            if (previousInvoicedTonnage.Value == 0m)
                return 0m;

            return netReportedTonnage - previousInvoicedTonnage.Value;
        }

        public static decimal? GetOverallChangeTotal(
            IEnumerable<CalcResultSummaryProducerDisposalFees> producerDisposalFees,
            string materialCode)
        {
            var level1 = producerDisposalFees
                .Where(r => r.Level == CommonConstants.LevelOne.ToString());

            return level1.Sum(r => r.ProducerDisposalFeesByMaterial[materialCode].TonnageChange ?? 0m);
        }

        public static (string Count, string Advice) ComputeCountAndAdvice(
            string level,
            IDictionary<string, CalcResultSummaryProducerDisposalFeesByMaterial> byMaterial)
        {
            if (level != CommonConstants.LevelOne.ToString())
                return (null, null);

            int count = byMaterial.Values
                .Count(m => m.TonnageChange.HasValue && m.TonnageChange.Value != 0m);

            return (count.ToString(CultureInfo.InvariantCulture),
                    count > 0 ? "CHANGE" : string.Empty);
        }
    }
}