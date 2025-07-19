using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPR.Calculator.Service.Function.Mapper
{
    public interface IProducerInvoiceTonnageMapper
    {
        public ProducerInvoicedMaterialNetTonnage Map(ProducerInvoiceTonnage producerInvoiceTonnage);
    }
}
