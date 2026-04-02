using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Mapper
{
    public interface IProducerInvoiceTonnageMapper
    {
        public ProducerInvoicedMaterialNetTonnage Map(ProducerInvoiceTonnage producerInvoiceTonnage);
    }
}
