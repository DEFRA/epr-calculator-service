using System.Collections.Generic;

namespace EPR.Calculator.Service.Function.Dtos
{
    public class CalcResultsRequestDto
    {
        public int RunId { get; set; }

        public IEnumerable<int> AcceptedProducerIds { get; set; } = new List<int>();

        public bool IsBillingFile { get; set; }
    }
}
