using EPR.Calculator.API.Data.DataModels;

namespace EPR.Calculator.Service.Function.Builder.Summary.TonnageVsAllProducer
{
    public static class TonnageVsAllProducerUtil
    {
        public static decimal GetPercentageofProducerReportedTonnagevsAllProducersTotal(IEnumerable<ProducerDetail> producers, IEnumerable<TotalPackagingTonnagePerRun> totalPackagingTonnage)
        {
            var totalPercentageofProducerReported = producers.Sum(producer => GetPercentageofProducerReportedTonnagevsAllProducers(producer, totalPackagingTonnage));

            return totalPercentageofProducerReported;
        }

        public static decimal GetPercentageofProducerReportedTonnagevsAllProducers(ProducerDetail producer, IEnumerable<TotalPackagingTonnagePerRun> totalPackagingTonnage)
        {
            var totalTonnage = totalPackagingTonnage.Sum(x => x.TotalPackagingTonnage);
            var producerData = totalPackagingTonnage.FirstOrDefault(r => r.ProducerId == producer.ProducerId && r.SubsidiaryId == producer.SubsidiaryId);
            var percentageofTonnage = producerData != null && totalTonnage > 0
                ? producerData.TotalPackagingTonnage / totalTonnage * 100
                : 0;
            return percentageofTonnage;
        }
    }
}