using EPR.Calculator.Service.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPR.Calculator.Service.Function.Interface
{
    public interface ICalculatorRunService
    {
        public void startProcess(CalculatorRunParameter calculatorRunParameter);
    }
}
