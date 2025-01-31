using System.Collections.Generic;

namespace EPR.Calculator.Service.Function.Models
{
    public class CalcResult1Plus4Apportionment
    {
        public IEnumerable<CalcResultParameterCostDetail> CalcResultParameterCommunicationCostDetails { get; set; } =
            new List<CalcResultParameterCostDetail>();
    }
}
