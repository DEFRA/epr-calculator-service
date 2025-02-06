using EPR.Calculator.Service.Function.Data.DataModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPR.Calculator.Service.Function.Builder.Summary.TonnageVsAllProducer.cs
{
    public static class TonnageVsAllProducerUtil
    {
        public static decimal GetPercentageofProducerReportedTonnagevsAllProducersTotal(IEnumerable<ProducerDetail> producers, IEnumerable<TotalPackagingTonnagePerRun> totalPackagingTonnage)
        {
            decimal totalPercentageofProducerReported = 0;

            totalPercentageofProducerReported = producers.Sum(producer => GetPercentageofProducerReportedTonnagevsAllProducers(producer, totalPackagingTonnage));

            return totalPercentageofProducerReported;
        }

        public static decimal GetPercentageofProducerReportedTonnagevsAllProducers(ProducerDetail producer, IEnumerable<TotalPackagingTonnagePerRun> totalPackagingTonnage)
        {
            var totalTonnage = totalPackagingTonnage.Sum(x => x.TotalPackagingTonnage);
            var producerData = totalPackagingTonnage.FirstOrDefault(r => r.ProducerId == producer.ProducerId && r.SubsidiaryId == producer.SubsidiaryId);
            var PercentageofTonnage = producerData != null && totalTonnage > 0
                ? producerData.TotalPackagingTonnage / totalTonnage * 100
                : 0;
            return PercentageofTonnage;
        }
    }
}
