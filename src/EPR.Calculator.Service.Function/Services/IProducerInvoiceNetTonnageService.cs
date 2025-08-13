using EPR.Calculator.Service.Function.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPR.Calculator.Service.Function.Services
{
    public interface IProducerInvoiceNetTonnageService
    {
        public Task<bool> CreateProducerInvoiceNetTonnage(CalcResult calcResult);
    }
}
