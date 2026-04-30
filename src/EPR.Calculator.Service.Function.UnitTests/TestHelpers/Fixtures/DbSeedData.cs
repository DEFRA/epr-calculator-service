using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.Enums;
using EPR.Calculator.API.Data.Models;
using EPR.Calculator.Service.Function.Features.Billing.Constants;
using EPR.Calculator.Service.Function.Features.Billing.Contexts;
using EPR.Calculator.Service.Function.Features.Calculator.Contexts;

namespace EPR.Calculator.Service.Function.UnitTests.TestHelpers.Fixtures;

public static class DbSeedData
{
    public static class Defaults
    {
        public static CalculatorRun ValidForCalculatorRun => CalculatorRuns[0];

        public static CalculatorRunContext ValidCalculatorRunContext =>
            new()
            {
                RunId = ValidForCalculatorRun.Id,
                RelativeYear = ValidForCalculatorRun.RelativeYear,
                RunName = ValidForCalculatorRun.Name,
                ProcessingStartedAt = new Fixture().Create<DateTimeOffset>(),
                User = "Test User"
            };

        public static CalculatorRun ValidForBillingRun => CalculatorRuns[1];

        public static BillingRunContext ValidBillingRunContext =>
            new()
            {
                RunId = ValidForBillingRun.Id,
                RelativeYear = ValidForBillingRun.RelativeYear,
                RunName = ValidForBillingRun.Name,
                ProcessingStartedAt = new Fixture().Create<DateTimeOffset>(),
                User = "Test User",
                AcceptedProducerIds = [1, 2]
            };
    }

    public static ImmutableArray<CalculatorRunRelativeYear> RelativeYears =>
    [
        new() { Value = 2023 },
        new() { Value = 2024 },
        new() { Value = 2025 },
        new() { Value = 2026 },
        new() { Value = 2027 },
        new() { Value = 2028 },
        new() { Value = 2029 },
        new() { Value = 2030 }
    ];

    public static ImmutableArray<CalculatorRun> CalculatorRuns =>
    [
        new() { Name = "Valid for new Calculator run", Id = 1, RelativeYear = new RelativeYear(2025), DefaultParameterSettingMasterId = 1, LapcapDataMasterId = 1, Classification = RunClassification.None, CreatedAt = new(2024, 6, 1, 0, 0, 0, DateTimeKind.Utc) },
        new() { Name = "Valid for new Billing run", Id = 2, RelativeYear = new RelativeYear(2025), DefaultParameterSettingMasterId = 1, LapcapDataMasterId = 1, Classification = RunClassification.FinalRunCompleted, CalculatorRunOrganisationDataMasterId = 1, CalculatorRunPomDataMasterId = 1, BillingRunStatus = BillingRunStatus.None, CreatedAt = new(2024, 9, 16, 0, 0, 0, DateTimeKind.Utc) }
    ];

    public static ImmutableArray<CalculatorRunOrganisationDataMaster> OrganisationMasters =>
    [
        new () { Id = 1, RelativeYear = new RelativeYear(2025), EffectiveFrom = Defaults.ValidForCalculatorRun.CreatedAt, CreatedAt = Defaults.ValidForCalculatorRun.CreatedAt, CreatedBy = "Test user" },
        new () { Id = 2, RelativeYear = new RelativeYear(2025), EffectiveFrom = Defaults.ValidForBillingRun.CreatedAt, CreatedAt = Defaults.ValidForBillingRun.CreatedAt, CreatedBy = "Test user" }
    ];

    public static ImmutableArray<CalculatorRunPomDataMaster> PomMasters =>
    [
        new () { Id = 1, RelativeYear = new RelativeYear(2025), EffectiveFrom = Defaults.ValidForCalculatorRun.CreatedAt, CreatedAt = Defaults.ValidForCalculatorRun.CreatedAt, CreatedBy = "Test user" },
        new () { Id = 2, RelativeYear = new RelativeYear(2025), EffectiveFrom = Defaults.ValidForBillingRun.CreatedAt, CreatedAt = Defaults.ValidForBillingRun.CreatedAt, CreatedBy = "Test user" }
    ];

    public static ImmutableArray<ProducerResultFileSuggestedBillingInstruction> SuggestedBillingInstruction =>
    [
        new() { Id = 1, CalculatorRunId = Defaults.ValidForBillingRun.Id, ProducerId = 1, BillingInstructionAcceptReject = BillingConstants.Action.Accepted, SuggestedBillingInstruction = "Debit" },
        new() { Id = 2, CalculatorRunId = Defaults.ValidForBillingRun.Id, ProducerId = 2, BillingInstructionAcceptReject = "Reject", SuggestedBillingInstruction = "Debit" },
        new() { Id = 3, CalculatorRunId = Defaults.ValidForBillingRun.Id, ProducerId = 3, BillingInstructionAcceptReject = BillingConstants.Action.Accepted, SuggestedBillingInstruction = BillingConstants.Suggestion.Cancel }
    ];

    public static ImmutableArray<Material> Materials =>
    [
        new() { Id = 1, Code = "AL", Name = "Aluminium", Description = "Aluminium" },
        new() { Id = 2, Code = "FC", Name = "Fibre composite", Description = "Fibre composite" },
        new() { Id = 3, Code = "GL", Name = "Glass", Description = "Glass" },
        new() { Id = 4, Code = "PC", Name = "Paper or card", Description = "Paper or card" },
        new() { Id = 5, Code = "PL", Name = "Plastic", Description = "Plastic" },
        new() { Id = 6, Code = "ST", Name = "Steel", Description = "Steel" },
        new() { Id = 7, Code = "WD", Name = "Wood", Description = "Wood" },
        new() { Id = 8, Code = "OT", Name = "Other materials", Description = "Other materials" }
    ];
}
