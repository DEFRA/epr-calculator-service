using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPR.Calculator.Service.Function.Interface
{
    public interface IPrepareBillingFileService
    {
        Task<bool> PrepareBillingFileAsync(int calculatorRunId, string runName);
    }
}
