using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.Models;
using EPR.Calculator.Service.Function.Features.Billing.Contexts;
using EPR.Calculator.Service.Function.Features.Calculator.Contexts;

namespace EPR.Calculator.Service.Function.UnitTests.TestHelpers.Fixtures;

public static class DbSeedData
{
    public static class Defaults
    {
        public static CalculatorRun ValidCalculatorRun => CalculatorRuns[0];

        public static CalculatorRunContext ValidCalculatorRunContext =>
            new()
            {
                RunId = ValidCalculatorRun.Id,
                RelativeYear = ValidCalculatorRun.RelativeYear,
                RunName = ValidCalculatorRun.Name,
                ProcessingStartedAt = new Fixture().Create<DateTimeOffset>(),
                User = "Test User"
            };

        public static CalculatorRun ValidBillingRun => CalculatorRuns[1];

        public static BillingRunContext ValidBillingRunContext =>
            new()
            {
                RunId = ValidBillingRun.Id,
                RelativeYear = ValidBillingRun.RelativeYear,
                RunName = ValidBillingRun.Name,
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
        new() { Name = "Valid Calculator run", Id = 1, RelativeYear = new RelativeYear(2025), DefaultParameterSettingMasterId = 1, LapcapDataMasterId = 1, CalculatorRunClassificationId = RunClassificationStatusIds.INTHEQUEUEID, CreatedAt = new(2024, 6, 1, 0, 0, 0, DateTimeKind.Utc) },
        new() { Name = "Valid Billing run", Id = 2, RelativeYear = new RelativeYear(2025), DefaultParameterSettingMasterId = 1, LapcapDataMasterId = 1, CalculatorRunClassificationId = RunClassificationStatusIds.FINALRUNCOMPLETEDID, CalculatorRunOrganisationDataMasterId = 1, CalculatorRunPomDataMasterId = 1, IsBillingFileGenerating = true, CreatedAt = new(2024, 9, 16, 0, 0, 0, DateTimeKind.Utc) }
    ];

    public static ImmutableArray<CalculatorRunOrganisationDataMaster> OrganisationMasters =>
    [
        new () { Id = 1, RelativeYear = new RelativeYear(2025), EffectiveFrom = Defaults.ValidCalculatorRun.CreatedAt, CreatedAt = Defaults.ValidCalculatorRun.CreatedAt, CreatedBy = "Test user" },
        new () { Id = 2, RelativeYear = new RelativeYear(2025), EffectiveFrom = Defaults.ValidBillingRun.CreatedAt, CreatedAt = Defaults.ValidBillingRun.CreatedAt, CreatedBy = "Test user" }
    ];

    public static ImmutableArray<CalculatorRunPomDataMaster> PomMasters =>
    [
        new () { Id = 1, RelativeYear = new RelativeYear(2025), EffectiveFrom = Defaults.ValidCalculatorRun.CreatedAt, CreatedAt = Defaults.ValidCalculatorRun.CreatedAt, CreatedBy = "Test user" },
        new () { Id = 2, RelativeYear = new RelativeYear(2025), EffectiveFrom = Defaults.ValidBillingRun.CreatedAt, CreatedAt = Defaults.ValidBillingRun.CreatedAt, CreatedBy = "Test user" }
    ];

    public static ImmutableArray<ProducerResultFileSuggestedBillingInstruction> SuggestedBillingInstruction =>
    [
        new() { Id = 1, CalculatorRunId = Defaults.ValidBillingRun.Id, ProducerId = 1, BillingInstructionAcceptReject = "Accepted", SuggestedBillingInstruction = "Debit" },
        new() { Id = 2, CalculatorRunId = Defaults.ValidBillingRun.Id, ProducerId = 2, BillingInstructionAcceptReject = "Reject", SuggestedBillingInstruction = "Debit" },
        new() { Id = 3, CalculatorRunId = Defaults.ValidBillingRun.Id, ProducerId = 3, BillingInstructionAcceptReject = "Accepted", SuggestedBillingInstruction = "Cancel" }
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