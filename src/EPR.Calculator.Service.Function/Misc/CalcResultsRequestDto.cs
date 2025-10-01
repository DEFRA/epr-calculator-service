using System.Collections.Generic;

namespace EPR.Calculator.Service.Function.Dtos
{
    public class CalcResultsRequestDto
    {
        public string FinancialYear { get; set; } = string.Empty;

        public int RunId { get; set; }

        public IEnumerable<int> AcceptedProducerIds { get; set; } = new List<int>();

        public bool IsBillingFile { get; set; }

        public string ApprovedBy { get; set; } = string.Empty;
    }
}
