using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
