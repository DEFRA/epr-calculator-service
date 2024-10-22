using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPR.Calculator.Service.Common
{
    public class CalculatorRunConfiguration
    {
        public required string PipelineUrl { get; set; }

        public required  string PipelineName { get; set; }

        public required string MaxCheckCount { get; set; }

        public required string CheckInterval { get; set; }

    }
}
