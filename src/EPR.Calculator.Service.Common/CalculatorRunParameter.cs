﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPR.Calculator.Service.Common
{
    public class CalculatorRunParameter
    {
        public int Id { get; set; }

        public required string User { get; set; }

        public required string FinancialYear { get; set; }

    }
}