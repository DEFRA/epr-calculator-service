using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPR.Calculator.Service.Function
{
    public abstract class MessageBase
    {
        /// <summary>
        /// Gets or sets the identifier for the calculator run.
        /// </summary>
        public int CalculatorRunId { get; set; }

        public string MessageType { get; set; }
    }
}
