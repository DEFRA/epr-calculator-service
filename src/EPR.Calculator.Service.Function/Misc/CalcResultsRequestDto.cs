using System.Collections.Generic;

namespace EPR.Calculator.Service.Function.Dtos
{
    public class CalcResultsRequestDto
    {
        public int RunId { get; set; }
        public IEnumerable<int> OrganisationIds { get; set; } = new List<int>();
        public bool IsBilling { get; set; }
    }
}
