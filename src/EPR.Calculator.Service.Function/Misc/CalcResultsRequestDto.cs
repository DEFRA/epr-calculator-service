using System.Collections.Generic;
using EPR.Calculator.API.Data.Models;
using EPR.Calculator.Service.Common;

namespace EPR.Calculator.Service.Function.Dtos
{
    public class CalcResultsRequestDto
    {
        public required int RunId { get; set; }

        public required RelativeYear RelativeYear { get; set; }

        public IEnumerable<int> AcceptedProducerIds { get; set; } = new List<int>();

        public bool IsBillingFile { get; set; }

        public string ApprovedBy { get; set; } = string.Empty;

        public string CreatedBy { get; set; } = string.Empty;
    }
}
