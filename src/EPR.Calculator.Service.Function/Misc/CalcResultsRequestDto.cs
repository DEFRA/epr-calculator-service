using System.Collections.Immutable;
using EPR.Calculator.API.Data.Models;

namespace EPR.Calculator.Service.Function.Misc
{
    public record CalcResultsRequestDto
    {
        public required int RunId { get; init; }

        public required RelativeYear RelativeYear { get; init; }

        public ImmutableHashSet<int> AcceptedProducerIds { get; init; } = [];

        public bool IsBillingFile { get; init; }

        public string ApprovedBy { get; init; } = string.Empty;

        public string CreatedBy { get; init; } = string.Empty;
    }
}