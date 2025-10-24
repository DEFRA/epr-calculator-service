using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPR.Calculator.Service.Function.Models
{
    public class CalcResultErrorReport
    {
        public int ProducerId { get; set; }

        public required string SubsidiaryId { get; set; }

        public required string ProducerName { get; set; }

        public required string TradingName { get; set; }

        public required string LeaverCode { get; set; }

        public required string ErrorCodeText { get; set; }
    }
}
