using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPR.Calculator.Service.Function.Exceptions
{
    public class NullValueException : Exception
    {
        public NullValueException(string message): base(message) {
        }
    }
}
