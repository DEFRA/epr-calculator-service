﻿using System.Collections.Generic;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Models.JsonExporter;

namespace EPR.Calculator.Service.Function.Mapper
{
    public interface ICalcResultLaDisposalCostDataMapper
    {
        public CalcResultLaDisposalCostDataJson Map(IEnumerable<CalcResultLaDisposalCostDataDetail> laDisposalCostDataDetail);
    }
}
