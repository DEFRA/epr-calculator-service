using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Mapper
{
    public class ProducerInvoiceTonnageMapper : IProducerInvoiceTonnageMapper
    {
        public ProducerInvoicedMaterialNetTonnage Map(ProducerInvoiceTonnage producerInvoiceTonnage)
        {
            return new ProducerInvoicedMaterialNetTonnage()
            {
                CalculatorRunId = producerInvoiceTonnage.RunId,
                ProducerId = producerInvoiceTonnage.ProducerId,
                InvoicedNetTonnage = producerInvoiceTonnage.NetTonnage,
                MaterialId = producerInvoiceTonnage.MaterialId
            };
        }
    }
}
