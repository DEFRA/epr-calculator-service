using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Models.JsonExporter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPR.Calculator.Service.Function.Mapper
{
    public interface ICalcResultCommsCostsWithBadDebtProvision2CMapper
    {
        CalcResultsCommsCostsWithBadDebtProvision2C Map(CalcResultSummaryProducerDisposalFees calcResultSummaryProducerDisposalFees);
    }
}
