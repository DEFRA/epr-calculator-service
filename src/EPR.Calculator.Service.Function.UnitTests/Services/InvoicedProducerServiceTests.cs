using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.Enums;
using EPR.Calculator.API.Data.Models;
using EPR.Calculator.Service.Function.Features.Billing.Constants;
using EPR.Calculator.Service.Function.Services;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.Fixtures;

namespace EPR.Calculator.Service.Function.UnitTests.Services;

[TestClass]
public class InvoicedProducerServiceTests
{
    private ApplicationDBContext _dbContext = null!;
    private InvoicedProducerService _sut = null!;

    [TestInitialize]
    public void Init()
    {
        var fixture = TestFixtures.New();
        _dbContext = fixture.Freeze<ApplicationDBContext>();
        _sut = fixture.Freeze<InvoicedProducerService>();
    }

    [TestCleanup]
    public void TearDown()
    {
        _dbContext.Database.EnsureDeleted();
    }

    [TestMethod]
    public async Task GetProducerDetailsTest_Returns_CancelledProducers()
    {
        // Arrange
        const int alId = 2001; // Aluminium

        const int p1Id = 1001;
        const int p2Id = 1002;
        const int p3Id = 1003;

        _dbContext.Material.Add(
            new Material { Id = alId, Code = "AL", Name = "Aluminium", Description = "Aluminium" }
        );

        _dbContext.CalculatorRuns.AddRange(
            new CalculatorRun { Id = 1, Name = "R1", RelativeYear = 2025, Classification = RunClassification.FinalRunCompleted },
            new CalculatorRun { Id = 2, Name = "R2", RelativeYear = 2025, Classification = RunClassification.FinalRunCompleted }
        );

        // Org master + details so the orgDetail inner-join in the SUT yields rows for each producer.
        _dbContext.CalculatorRunOrganisationDataMaster.Add(new CalculatorRunOrganisationDataMaster
        {
            Id = 1,
            RelativeYear = 2025,
            EffectiveFrom = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "Test user"
        });

        _dbContext.CalculatorRunOrganisationDataDetails.AddRange(
            new CalculatorRunOrganisationDataDetail { Id = 1, OrganisationId = p1Id, OrganisationName = "Test1", LoadTimeStamp = DateTime.UtcNow, CalculatorRunOrganisationDataMasterId = 1 },
            new CalculatorRunOrganisationDataDetail { Id = 2, OrganisationId = p2Id, OrganisationName = "Test2", LoadTimeStamp = DateTime.UtcNow, CalculatorRunOrganisationDataMasterId = 1 },
            new CalculatorRunOrganisationDataDetail { Id = 3, OrganisationId = p3Id, OrganisationName = "Test3", LoadTimeStamp = DateTime.UtcNow, CalculatorRunOrganisationDataMasterId = 1 }
        );

        // Suggested billing instructions: P1 has Cancel/Accepted in BOTH runs (so it appears twice).
        _dbContext.ProducerResultFileSuggestedBillingInstruction.AddRange(
            new ProducerResultFileSuggestedBillingInstruction { Id = 1, CalculatorRunId = 1, ProducerId = p1Id, SuggestedBillingInstruction = BillingConstants.Suggestion.Cancel, BillingInstructionAcceptReject = BillingConstants.Action.Accepted },
            new ProducerResultFileSuggestedBillingInstruction { Id = 2, CalculatorRunId = 1, ProducerId = p2Id, SuggestedBillingInstruction = BillingConstants.Suggestion.Cancel, BillingInstructionAcceptReject = BillingConstants.Action.Accepted },
            new ProducerResultFileSuggestedBillingInstruction { Id = 3, CalculatorRunId = 2, ProducerId = p1Id, SuggestedBillingInstruction = BillingConstants.Suggestion.Cancel, BillingInstructionAcceptReject = BillingConstants.Action.Accepted },
            new ProducerResultFileSuggestedBillingInstruction { Id = 4, CalculatorRunId = 1, ProducerId = p3Id, SuggestedBillingInstruction = BillingConstants.Suggestion.Cancel, BillingInstructionAcceptReject = BillingConstants.Action.Accepted }
        );

        // Invoiced material net tonnage (joined on ProducerId + CalculatorRunId).
        _dbContext.ProducerInvoicedMaterialNetTonnage.AddRange(
            new ProducerInvoicedMaterialNetTonnage { Id = 101, CalculatorRunId = 1, ProducerId = p1Id, MaterialId = alId, InvoicedNetTonnage = 100m },
            new ProducerInvoicedMaterialNetTonnage { Id = 102, CalculatorRunId = 1, ProducerId = p2Id, MaterialId = alId, InvoicedNetTonnage = 100m },
            new ProducerInvoicedMaterialNetTonnage { Id = 103, CalculatorRunId = 2, ProducerId = p1Id, MaterialId = alId, InvoicedNetTonnage = 100m },
            new ProducerInvoicedMaterialNetTonnage { Id = 104, CalculatorRunId = 1, ProducerId = p3Id, MaterialId = alId, InvoicedNetTonnage = 100m }
        );

        // Designated run invoice instructions (joined on ProducerId + CalculatorRunId).
        _dbContext.ProducerDesignatedRunInvoiceInstruction.AddRange(
            new ProducerDesignatedRunInvoiceInstruction { Id = 11, ProducerId = p1Id, CalculatorRunId = 1, CurrentYearInvoicedTotalAfterThisRun = 100m, InvoiceAmount = 100m, BillingInstructionId = "1_1" },
            new ProducerDesignatedRunInvoiceInstruction { Id = 12, ProducerId = p2Id, CalculatorRunId = 1, CurrentYearInvoicedTotalAfterThisRun = 100m, InvoiceAmount = 100m, BillingInstructionId = "1_2" },
            new ProducerDesignatedRunInvoiceInstruction { Id = 13, ProducerId = p1Id, CalculatorRunId = 2, CurrentYearInvoicedTotalAfterThisRun = 200m, InvoiceAmount = 100m, BillingInstructionId = "2_1" },
            new ProducerDesignatedRunInvoiceInstruction { Id = 14, ProducerId = p3Id, CalculatorRunId = 1, CurrentYearInvoicedTotalAfterThisRun = 100m, InvoiceAmount = 100m, BillingInstructionId = "1_3" }
        );

        await _dbContext.SaveChangesAsync();

        // Act
        var producerIds = ImmutableHashSet.Create(p1Id);
        var result = await _sut.GetInvoicedProducerRecords(new RelativeYear(2025), producerIds);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(2);
    }

    [TestMethod]
    public async Task GetProducerDetailsTest_DoesNotReturns_CancelledProducers()
    {
        // Arrange: run classification is not in the SUT's ValidClassifications set,
        // so even with otherwise valid billing data the query should yield no records.
        _dbContext.CalculatorRuns.Add(
            new CalculatorRun { Id = 1, Name = "R1", RelativeYear = 2025, Classification = RunClassification.Running }
        );

        _dbContext.ProducerResultFileSuggestedBillingInstruction.Add(
            new ProducerResultFileSuggestedBillingInstruction { Id = 1, CalculatorRunId = 1, ProducerId = 1, SuggestedBillingInstruction = BillingConstants.Suggestion.Initial, BillingInstructionAcceptReject = BillingConstants.Action.Accepted }
        );

        await _dbContext.SaveChangesAsync();

        // Act
        var producerIds = ImmutableHashSet.Create(1);
        var result = await _sut.GetInvoicedProducerRecords(new RelativeYear(2025), producerIds);

        // Assert
        result.Count.ShouldBe(0);
    }

    [TestMethod]
    public async Task GetProducers_Returns_Producers()
    {
        // Arrange: 3 distinct producers exist for run 1 (producer 1 also exists for run 2,
        // but GetProducerIdsForRun should only consider run 1 and return distinct ids).
        _dbContext.ProducerDetail.AddRange(
            new ProducerDetail { Id = 1, CalculatorRunId = 1, ProducerName = "Test1", ProducerId = 1, TradingName = "TN1" },
            new ProducerDetail { Id = 2, CalculatorRunId = 1, ProducerName = "Test2", ProducerId = 2, TradingName = "TN2" },
            new ProducerDetail { Id = 3, CalculatorRunId = 2, ProducerName = "Test1", ProducerId = 1, TradingName = "TN3" },
            new ProducerDetail { Id = 4, CalculatorRunId = 1, ProducerName = "Test3", ProducerId = 3, TradingName = "TN4" }
        );

        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _sut.GetProducerIdsForRun(1);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(3);
    }

    [TestMethod]
    public async Task GetProducerDetails_Empty()
    {
        // Arrange
        var producerIds = ImmutableHashSet.Create(1);

        // Act
        var result = await _sut.GetInvoicedProducerRecords(new RelativeYear(2025), producerIds);

        // Assert
        result.ShouldNotBeNull();
    }



    [TestMethod]
    public async Task GetLatestAcceptedInvoicedProducerRecords_NoCancels_UsesLatestAcceptedRun() // US608960 happy path
    {
        // Arrange
        const int alId = 2001; // Aluminium
        const int fcId = 2002; // Fibre composite

        const int p1Id = 1001;
        const int p2Id = 1002;
        const int p3Id = 1003;

        // Materials
        _dbContext.Material.AddRange(
            new Material { Id = alId, Code = "AL", Name = "Aluminium", Description = "Aluminium" },
            new Material { Id = fcId, Code = "FC", Name = "Fibre composite", Description = "Fibre composite" }
        );

        _dbContext.CalculatorRuns.AddRange(
            new CalculatorRun { Id = 1, Name = "R1", RelativeYear = new RelativeYear(2024), Classification = RunClassification.FinalRunCompleted },
            new CalculatorRun { Id = 2, Name = "R2", RelativeYear = new RelativeYear(2024), Classification = RunClassification.FinalRunCompleted }
        );

        // Previous invoiced net tonnage
        _dbContext.ProducerInvoicedMaterialNetTonnage.AddRange(
            // R1
            new ProducerInvoicedMaterialNetTonnage { Id = 101, CalculatorRunId = 1, ProducerId = p1Id, MaterialId = alId, InvoicedNetTonnage = 6316.000m },
            new ProducerInvoicedMaterialNetTonnage { Id = 102, CalculatorRunId = 1, ProducerId = p1Id, MaterialId = fcId, InvoicedNetTonnage = 1983.000m },
            new ProducerInvoicedMaterialNetTonnage { Id = 103, CalculatorRunId = 1, ProducerId = p2Id, MaterialId = alId, InvoicedNetTonnage = 839.999m },
            new ProducerInvoicedMaterialNetTonnage { Id = 104, CalculatorRunId = 1, ProducerId = p2Id, MaterialId = fcId, InvoicedNetTonnage = 396.341m },
            new ProducerInvoicedMaterialNetTonnage { Id = 105, CalculatorRunId = 1, ProducerId = p3Id, MaterialId = alId, InvoicedNetTonnage = 2880.000m },
            new ProducerInvoicedMaterialNetTonnage { Id = 106, CalculatorRunId = 1, ProducerId = p3Id, MaterialId = fcId, InvoicedNetTonnage = 43.500m },

            // R2 (latest)
            new ProducerInvoicedMaterialNetTonnage { Id = 201, CalculatorRunId = 2, ProducerId = p1Id, MaterialId = alId, InvoicedNetTonnage = 6316.000m },
            new ProducerInvoicedMaterialNetTonnage { Id = 202, CalculatorRunId = 2, ProducerId = p1Id, MaterialId = fcId, InvoicedNetTonnage = 1983.000m },
            new ProducerInvoicedMaterialNetTonnage { Id = 203, CalculatorRunId = 2, ProducerId = p2Id, MaterialId = alId, InvoicedNetTonnage = 839.999m },
            new ProducerInvoicedMaterialNetTonnage { Id = 204, CalculatorRunId = 2, ProducerId = p2Id, MaterialId = fcId, InvoicedNetTonnage = 396.341m },
            new ProducerInvoicedMaterialNetTonnage { Id = 205, CalculatorRunId = 2, ProducerId = p3Id, MaterialId = alId, InvoicedNetTonnage = 2880.000m },
            new ProducerInvoicedMaterialNetTonnage { Id = 206, CalculatorRunId = 2, ProducerId = p3Id, MaterialId = fcId, InvoicedNetTonnage = 43.500m }
        );

        // Results file Suggested billing instructions – all Accepted, no Cancels
        _dbContext.ProducerResultFileSuggestedBillingInstruction.AddRange(
            // R1
            new ProducerResultFileSuggestedBillingInstruction { Id = 1, CalculatorRunId = 1, ProducerId = p1Id, SuggestedBillingInstruction = BillingConstants.Suggestion.Initial, BillingInstructionAcceptReject = BillingConstants.Action.Accepted },
            new ProducerResultFileSuggestedBillingInstruction { Id = 2, CalculatorRunId = 1, ProducerId = p2Id, SuggestedBillingInstruction = BillingConstants.Suggestion.Initial, BillingInstructionAcceptReject = BillingConstants.Action.Accepted },
            new ProducerResultFileSuggestedBillingInstruction { Id = 3, CalculatorRunId = 1, ProducerId = p3Id, SuggestedBillingInstruction = BillingConstants.Suggestion.Initial, BillingInstructionAcceptReject = BillingConstants.Action.Accepted },

            // R2
            new ProducerResultFileSuggestedBillingInstruction { Id = 4, CalculatorRunId = 2, ProducerId = p1Id, SuggestedBillingInstruction = BillingConstants.Suggestion.Delta, BillingInstructionAcceptReject = BillingConstants.Action.Accepted },
            new ProducerResultFileSuggestedBillingInstruction { Id = 5, CalculatorRunId = 2, ProducerId = p2Id, SuggestedBillingInstruction = BillingConstants.Suggestion.Delta, BillingInstructionAcceptReject = BillingConstants.Action.Accepted },
            new ProducerResultFileSuggestedBillingInstruction { Id = 6, CalculatorRunId = 2, ProducerId = p3Id, SuggestedBillingInstruction = BillingConstants.Suggestion.Delta, BillingInstructionAcceptReject = BillingConstants.Action.Accepted }
        );

        // Producer designated invoice instructions
        _dbContext.ProducerDesignatedRunInvoiceInstruction.AddRange(
            // R1
            new ProducerDesignatedRunInvoiceInstruction { Id = 11, ProducerId = p1Id, CalculatorRunId = 1, CurrentYearInvoicedTotalAfterThisRun = 126157.44m, InvoiceAmount = 126157.44m, BillingInstructionId = "1_1001" },
            new ProducerDesignatedRunInvoiceInstruction { Id = 12, ProducerId = p2Id, CalculatorRunId = 1, CurrentYearInvoicedTotalAfterThisRun = 103966.48m, InvoiceAmount = 103966.48m, BillingInstructionId = "1_1002" },
            new ProducerDesignatedRunInvoiceInstruction { Id = 13, ProducerId = p3Id, CalculatorRunId = 1, CurrentYearInvoicedTotalAfterThisRun = 23712.34m, InvoiceAmount = 23712.34m, BillingInstructionId = "1_1003" },

            // R2
            new ProducerDesignatedRunInvoiceInstruction { Id = 14, ProducerId = p1Id, CalculatorRunId = 2, CurrentYearInvoicedTotalAfterThisRun = 211377.83m, InvoiceAmount = 85220.39m, BillingInstructionId = "2_1001" },
            new ProducerDesignatedRunInvoiceInstruction { Id = 15, ProducerId = p2Id, CalculatorRunId = 2, CurrentYearInvoicedTotalAfterThisRun = 150000.00m, InvoiceAmount = 46033.52m, BillingInstructionId = "2_1002" },
            new ProducerDesignatedRunInvoiceInstruction { Id = 16, ProducerId = p3Id, CalculatorRunId = 2, CurrentYearInvoicedTotalAfterThisRun = 40367.29m, InvoiceAmount = 16654.95m, BillingInstructionId = "2_1003" }
        );

        SeedProducerOrgDetails();

        await _dbContext.SaveChangesAsync();

        // Act
        var invoicedProducerRecords = await _sut.GetLatestAcceptedInvoicedProducerRecords(new RelativeYear(2024));

        // Assert

        // From latest accepted run (R2)
        foreach (var mat in new[] { alId, fcId })
        {
            var p1 = invoicedProducerRecords.Single(r => r.ProducerId == p1Id && r.MaterialId == mat);
            p1.CalculatorRunId.ShouldBe(2);
            p1.CurrentYearInvoicedTotalAfterThisRun.ShouldBe(211377.83m);

            var p2 = invoicedProducerRecords.Single(r => r.ProducerId == p2Id && r.MaterialId == mat);
            p2.CalculatorRunId.ShouldBe(2);
            p2.CurrentYearInvoicedTotalAfterThisRun.ShouldBe(150000.00m);

            var p3 = invoicedProducerRecords.Single(r => r.ProducerId == p3Id && r.MaterialId == mat);
            p3.CalculatorRunId.ShouldBe(2);
            p3.CurrentYearInvoicedTotalAfterThisRun.ShouldBe(40367.29m);
        }

        //3 producers * 2 materials = 6
        invoicedProducerRecords.Count.ShouldBe(6);
    }

    [TestMethod]
    public async Task GetLatestAcceptedInvoicedProducerRecords_CancelAcceptedInRun2_ExcludesPreviousInvoiceInRun1ForReappearingProducer() // US608960 AC1
    {
        //Arrange
        const int alId = 2001; // Aluminium
        const int fcId = 2002; // Fibre composite

        const int p1Id = 1001;
        const int p2Id = 1002; // cancel REJECTED in R2 reappears in R3
        const int p3Id = 1003;

        // Materials
        _dbContext.Material.AddRange(
            new Material { Id = alId, Code = "AL", Name = "Aluminium", Description = "Aluminium" },
            new Material { Id = fcId, Code = "FC", Name = "Fibre composite", Description = "Fibre composite" }
        );

        _dbContext.CalculatorRuns.AddRange(
            new CalculatorRun { Id = 1, Name = "R1", RelativeYear = new RelativeYear(2024), Classification = RunClassification.FinalRunCompleted },
            new CalculatorRun { Id = 2, Name = "R2", RelativeYear = new RelativeYear(2024), Classification = RunClassification.FinalRunCompleted },
            new CalculatorRun { Id = 3, Name = "R3", RelativeYear = new RelativeYear(2024), Classification = RunClassification.FinalRunCompleted }
        );

        // Previous invoiced net tonnage
        _dbContext.ProducerInvoicedMaterialNetTonnage.AddRange(
            // R1
            new ProducerInvoicedMaterialNetTonnage { Id = 101, CalculatorRunId = 1, ProducerId = p1Id, MaterialId = alId, InvoicedNetTonnage = 6316.000m },
            new ProducerInvoicedMaterialNetTonnage { Id = 102, CalculatorRunId = 1, ProducerId = p1Id, MaterialId = fcId, InvoicedNetTonnage = 1983.000m },
            new ProducerInvoicedMaterialNetTonnage { Id = 103, CalculatorRunId = 1, ProducerId = p2Id, MaterialId = alId, InvoicedNetTonnage = 839.999m },
            new ProducerInvoicedMaterialNetTonnage { Id = 104, CalculatorRunId = 1, ProducerId = p2Id, MaterialId = fcId, InvoicedNetTonnage = 396.341m },
            new ProducerInvoicedMaterialNetTonnage { Id = 105, CalculatorRunId = 1, ProducerId = p3Id, MaterialId = alId, InvoicedNetTonnage = 2880.000m },
            new ProducerInvoicedMaterialNetTonnage { Id = 106, CalculatorRunId = 1, ProducerId = p3Id, MaterialId = fcId, InvoicedNetTonnage = 43.500m },

            // R2 (No data for P2)
            new ProducerInvoicedMaterialNetTonnage { Id = 201, CalculatorRunId = 2, ProducerId = p1Id, MaterialId = alId, InvoicedNetTonnage = 6316.000m },
            new ProducerInvoicedMaterialNetTonnage { Id = 202, CalculatorRunId = 2, ProducerId = p1Id, MaterialId = fcId, InvoicedNetTonnage = 1983.000m },
            new ProducerInvoicedMaterialNetTonnage { Id = 203, CalculatorRunId = 2, ProducerId = p3Id, MaterialId = alId, InvoicedNetTonnage = 2880.000m },
            new ProducerInvoicedMaterialNetTonnage { Id = 204, CalculatorRunId = 2, ProducerId = p3Id, MaterialId = fcId, InvoicedNetTonnage = 43.500m }
        );

        // Results file Suggested billing instructions
        // R1: all producers Accepted
        _dbContext.ProducerResultFileSuggestedBillingInstruction.AddRange(
            new ProducerResultFileSuggestedBillingInstruction { Id = 1, CalculatorRunId = 1, ProducerId = p1Id, SuggestedBillingInstruction = BillingConstants.Suggestion.Initial, BillingInstructionAcceptReject = BillingConstants.Action.Accepted },
            new ProducerResultFileSuggestedBillingInstruction { Id = 2, CalculatorRunId = 1, ProducerId = p2Id, SuggestedBillingInstruction = BillingConstants.Suggestion.Initial, BillingInstructionAcceptReject = BillingConstants.Action.Accepted },
            new ProducerResultFileSuggestedBillingInstruction { Id = 3, CalculatorRunId = 1, ProducerId = p3Id, SuggestedBillingInstruction = BillingConstants.Suggestion.Initial, BillingInstructionAcceptReject = BillingConstants.Action.Accepted }
        );

        // R2: P1 Accepted, P2 Cancel Accepted, P3 Accepted
        _dbContext.ProducerResultFileSuggestedBillingInstruction.AddRange(
            new ProducerResultFileSuggestedBillingInstruction { Id = 4, CalculatorRunId = 2, ProducerId = p1Id, SuggestedBillingInstruction = BillingConstants.Suggestion.Delta, BillingInstructionAcceptReject = BillingConstants.Action.Accepted },
            new ProducerResultFileSuggestedBillingInstruction { Id = 5, CalculatorRunId = 2, ProducerId = p2Id, SuggestedBillingInstruction = BillingConstants.Suggestion.Cancel, BillingInstructionAcceptReject = BillingConstants.Action.Accepted },
            new ProducerResultFileSuggestedBillingInstruction { Id = 6, CalculatorRunId = 2, ProducerId = p3Id, SuggestedBillingInstruction = BillingConstants.Suggestion.Delta, BillingInstructionAcceptReject = BillingConstants.Action.Accepted }
        );

        // Producer designated invoice instructions
        _dbContext.ProducerDesignatedRunInvoiceInstruction.AddRange(
            //run 1
            new ProducerDesignatedRunInvoiceInstruction { Id = 11, ProducerId = p1Id, CalculatorRunId = 1, CurrentYearInvoicedTotalAfterThisRun = 126157.44m, InvoiceAmount = 126157.44m, BillingInstructionId = "1_1001" },
            new ProducerDesignatedRunInvoiceInstruction { Id = 12, ProducerId = p2Id, CalculatorRunId = 1, CurrentYearInvoicedTotalAfterThisRun = 103966.48m, InvoiceAmount = 103966.48m, BillingInstructionId = "1_1002" },
            new ProducerDesignatedRunInvoiceInstruction { Id = 13, ProducerId = p3Id, CalculatorRunId = 1, CurrentYearInvoicedTotalAfterThisRun = 23712.34m, InvoiceAmount = 23712.34m, BillingInstructionId = "1_1003" },

            // run 2 (P2 Cancelled null records)
            new ProducerDesignatedRunInvoiceInstruction { Id = 14, ProducerId = p1Id, CalculatorRunId = 2, CurrentYearInvoicedTotalAfterThisRun = 211377.83m, InvoiceAmount = 85220.39m, BillingInstructionId = "2_1001" },
            new ProducerDesignatedRunInvoiceInstruction { Id = 15, ProducerId = p2Id, CalculatorRunId = 2, CurrentYearInvoicedTotalAfterThisRun = null, InvoiceAmount = null, BillingInstructionId = "2_1002" },
            new ProducerDesignatedRunInvoiceInstruction { Id = 16, ProducerId = p3Id, CalculatorRunId = 2, CurrentYearInvoicedTotalAfterThisRun = 40367.29m, InvoiceAmount = 16654.95m, BillingInstructionId = "2_1003" }
        );

        SeedProducerOrgDetails();

        await _dbContext.SaveChangesAsync();

        // Act
        var invoicedProducerRecords = await _sut.GetLatestAcceptedInvoicedProducerRecords(new RelativeYear(2024));

        //Assert

        //PreviousInvoicedTonnage for P2 must not be present from R1 as R2 is Cancel (Accepted)
        invoicedProducerRecords.ShouldNotContain(r => r.ProducerId == p2Id);

        foreach (var mat in new[] { alId, fcId })
        {
            //PreviousInvoicedTonnage for P1 and P3 are from Run 2
            var p1 = invoicedProducerRecords.Single(r => r.ProducerId == p1Id && r.MaterialId == mat);
            p1.CalculatorRunId.ShouldBe(2);
            p1.CurrentYearInvoicedTotalAfterThisRun.ShouldBe(211377.83m);

            var p3 = invoicedProducerRecords.Single(r => r.ProducerId == p3Id && r.MaterialId == mat);
            p3.CalculatorRunId.ShouldBe(2);
            p3.CurrentYearInvoicedTotalAfterThisRun.ShouldBe(40367.29m);
        }

        // 2 producers * 2 materials = 4 rows
        invoicedProducerRecords.Count.ShouldBe(4);
    }

    [TestMethod]
    public async Task GetLatestAcceptedInvoicedProducerRecords_CancelRejectedInRun2_UsesInvoiceFromRun1ForReappearingProducer() // US608960 AC2
    {
        // Arrange
        const int alId = 2001; // Aluminium
        const int fcId = 2002; // Fibre composite

        const int p1Id = 1001;
        const int p2Id = 1002; // cancel REJECTED in R2 reappears in R3
        const int p3Id = 1003;

        // 2 materials only
        _dbContext.Material.AddRange(
            new Material { Id = alId, Code = "AL", Name = "Aluminium", Description = "Aluminium" },
            new Material { Id = fcId, Code = "FC", Name = "Fibre composite", Description = "Fibre composite" }
        );

        _dbContext.CalculatorRuns.AddRange(
            new CalculatorRun { Id = 1, Name = "R1", RelativeYear = new RelativeYear(2025), Classification = RunClassification.FinalRunCompleted },
            new CalculatorRun { Id = 2, Name = "R2", RelativeYear = new RelativeYear(2025), Classification = RunClassification.FinalRunCompleted }
        );

        // Previous invoiced net tonnage
        _dbContext.ProducerInvoicedMaterialNetTonnage.AddRange(
            // R1
            new ProducerInvoicedMaterialNetTonnage { Id = 101, CalculatorRunId = 1, ProducerId = p1Id, MaterialId = alId, InvoicedNetTonnage = 6316.000m },
            new ProducerInvoicedMaterialNetTonnage { Id = 102, CalculatorRunId = 1, ProducerId = p1Id, MaterialId = fcId, InvoicedNetTonnage = 1983.000m },
            new ProducerInvoicedMaterialNetTonnage { Id = 103, CalculatorRunId = 1, ProducerId = p2Id, MaterialId = alId, InvoicedNetTonnage = 839.999m },
            new ProducerInvoicedMaterialNetTonnage { Id = 104, CalculatorRunId = 1, ProducerId = p2Id, MaterialId = fcId, InvoicedNetTonnage = 396.341m },
            new ProducerInvoicedMaterialNetTonnage { Id = 105, CalculatorRunId = 1, ProducerId = p3Id, MaterialId = alId, InvoicedNetTonnage = 2880.000m },
            new ProducerInvoicedMaterialNetTonnage { Id = 106, CalculatorRunId = 1, ProducerId = p3Id, MaterialId = fcId, InvoicedNetTonnage = 43.500m },
            new ProducerInvoicedMaterialNetTonnage { Id = 201, CalculatorRunId = 2, ProducerId = p1Id, MaterialId = alId, InvoicedNetTonnage = 6316.000m },
            new ProducerInvoicedMaterialNetTonnage { Id = 202, CalculatorRunId = 2, ProducerId = p1Id, MaterialId = fcId, InvoicedNetTonnage = 1983.000m },
            new ProducerInvoicedMaterialNetTonnage { Id = 203, CalculatorRunId = 2, ProducerId = p3Id, MaterialId = alId, InvoicedNetTonnage = 2880.000m },
            new ProducerInvoicedMaterialNetTonnage { Id = 204, CalculatorRunId = 2, ProducerId = p3Id, MaterialId = fcId, InvoicedNetTonnage = 43.500m }
        );

        // Results file Suggested billing instructions
        // R1: all INITIAL Accepted
        _dbContext.ProducerResultFileSuggestedBillingInstruction.AddRange(
            new ProducerResultFileSuggestedBillingInstruction { Id = 1, CalculatorRunId = 1, ProducerId = p1Id, SuggestedBillingInstruction = BillingConstants.Suggestion.Initial, BillingInstructionAcceptReject = BillingConstants.Action.Accepted },
            new ProducerResultFileSuggestedBillingInstruction { Id = 2, CalculatorRunId = 1, ProducerId = p2Id, SuggestedBillingInstruction = BillingConstants.Suggestion.Initial, BillingInstructionAcceptReject = BillingConstants.Action.Accepted },
            new ProducerResultFileSuggestedBillingInstruction { Id = 3, CalculatorRunId = 1, ProducerId = p3Id, SuggestedBillingInstruction = BillingConstants.Suggestion.Initial, BillingInstructionAcceptReject = BillingConstants.Action.Accepted }
        );

        // R2: P1 DELTA Accepted, P2 Cancel REJECTED, P3 DELTA Accepted
        _dbContext.ProducerResultFileSuggestedBillingInstruction.AddRange(
            new ProducerResultFileSuggestedBillingInstruction { Id = 4, CalculatorRunId = 2, ProducerId = p1Id, SuggestedBillingInstruction = BillingConstants.Suggestion.Delta, BillingInstructionAcceptReject = BillingConstants.Action.Accepted },
            new ProducerResultFileSuggestedBillingInstruction { Id = 5, CalculatorRunId = 2, ProducerId = p2Id, SuggestedBillingInstruction = BillingConstants.Suggestion.Cancel, BillingInstructionAcceptReject = BillingConstants.Action.Rejected },
            new ProducerResultFileSuggestedBillingInstruction { Id = 6, CalculatorRunId = 2, ProducerId = p3Id, SuggestedBillingInstruction = BillingConstants.Suggestion.Delta, BillingInstructionAcceptReject = BillingConstants.Action.Accepted }
        );

        // Producer designated invoice instructions
        _dbContext.ProducerDesignatedRunInvoiceInstruction.AddRange(
            // R1
            new ProducerDesignatedRunInvoiceInstruction { Id = 11, ProducerId = p1Id, CalculatorRunId = 1, CurrentYearInvoicedTotalAfterThisRun = 126157.44m, InvoiceAmount = 126157.44m, BillingInstructionId = "1_1001" },
            new ProducerDesignatedRunInvoiceInstruction { Id = 12, ProducerId = p2Id, CalculatorRunId = 1, CurrentYearInvoicedTotalAfterThisRun = 103966.48m, InvoiceAmount = 103966.48m, BillingInstructionId = "1_1002" },
            new ProducerDesignatedRunInvoiceInstruction { Id = 13, ProducerId = p3Id, CalculatorRunId = 1, CurrentYearInvoicedTotalAfterThisRun = 23712.34m, InvoiceAmount = 23712.34m, BillingInstructionId = "1_1003" },

            // R2
            new ProducerDesignatedRunInvoiceInstruction { Id = 14, ProducerId = p1Id, CalculatorRunId = 2, CurrentYearInvoicedTotalAfterThisRun = 211377.83m, InvoiceAmount = 85220.39m, BillingInstructionId = "2_1001" },
            new ProducerDesignatedRunInvoiceInstruction { Id = 15, ProducerId = p2Id, CalculatorRunId = 2, CurrentYearInvoicedTotalAfterThisRun = null, InvoiceAmount = null, BillingInstructionId = "2_1002" },
            new ProducerDesignatedRunInvoiceInstruction { Id = 16, ProducerId = p3Id, CalculatorRunId = 2, CurrentYearInvoicedTotalAfterThisRun = 40367.29m, InvoiceAmount = 16654.95m, BillingInstructionId = "2_1003" }
        );

        SeedProducerOrgDetails();

        await _dbContext.SaveChangesAsync();

        // Act
        var invoicedProducerRecords = await _sut.GetLatestAcceptedInvoicedProducerRecords(new RelativeYear(2025));

        // Assert

        // P2 must be present and fetched from R1 as R2 cancel REJECTED
        foreach (var mat in new[] { alId, fcId })
        {
            var p2 = invoicedProducerRecords.Single(r => r.ProducerId == p2Id && r.MaterialId == mat);
            p2.CalculatorRunId.ShouldBe(1);
            p2.CurrentYearInvoicedTotalAfterThisRun.ShouldBe(103966.48m);
        }

        // P1 & P3 fetched from R2
        foreach (var mat in new[] { alId, fcId })
        {
            var p1 = invoicedProducerRecords.Single(r => r.ProducerId == p1Id && r.MaterialId == mat);
            p1.CalculatorRunId.ShouldBe(2);
            p1.CurrentYearInvoicedTotalAfterThisRun.ShouldBe(211377.83m);

            var p3 = invoicedProducerRecords.Single(r => r.ProducerId == p3Id && r.MaterialId == mat);
            p3.CalculatorRunId.ShouldBe(2);
            p3.CurrentYearInvoicedTotalAfterThisRun.ShouldBe(40367.29m);
        }

        // 3 producers * 2 materials = 6 rows
        invoicedProducerRecords.Count.ShouldBe(6);
    }

    [TestMethod]
    public async Task GetLatestAcceptedInvoicedProducerRecords_ReappearAcceptedInRun3_UsesRun3_IgnoringEarlierCancelAccepted() // US608960 AC 1
    {
        // Arrange
        const int alId = 2001; // Aluminium
        const int fcId = 2002; // Fibre composite

        const int p1Id = 1001;
        const int p2Id = 1002; // (R2 Cancel Accepted, R3 Reappear Accepted)
        const int p3Id = 1003;

        _dbContext.Material.AddRange(
            new Material { Id = alId, Code = "AL", Name = "Aluminium", Description = "Aluminium" },
            new Material { Id = fcId, Code = "FC", Name = "Fibre composite", Description = "Fibre composite" }
        );

        _dbContext.CalculatorRuns.AddRange(
            new CalculatorRun { Id = 1, Name = "R1", RelativeYear = new RelativeYear(2024), Classification = RunClassification.FinalRunCompleted },
            new CalculatorRun { Id = 2, Name = "R2", RelativeYear = new RelativeYear(2024), Classification = RunClassification.FinalRunCompleted },
            new CalculatorRun { Id = 3, Name = "R3", RelativeYear = new RelativeYear(2024), Classification = RunClassification.FinalRunCompleted }
        );

        // Previous invoiced net tonnage
        _dbContext.ProducerInvoicedMaterialNetTonnage.AddRange(
            // R1
            new ProducerInvoicedMaterialNetTonnage { Id = 101, CalculatorRunId = 1, ProducerId = p1Id, MaterialId = alId, InvoicedNetTonnage = 6316.000m },
            new ProducerInvoicedMaterialNetTonnage { Id = 102, CalculatorRunId = 1, ProducerId = p1Id, MaterialId = fcId, InvoicedNetTonnage = 1983.000m },
            new ProducerInvoicedMaterialNetTonnage { Id = 103, CalculatorRunId = 1, ProducerId = p2Id, MaterialId = alId, InvoicedNetTonnage = 839.999m },
            new ProducerInvoicedMaterialNetTonnage { Id = 104, CalculatorRunId = 1, ProducerId = p2Id, MaterialId = fcId, InvoicedNetTonnage = 396.341m },
            new ProducerInvoicedMaterialNetTonnage { Id = 105, CalculatorRunId = 1, ProducerId = p3Id, MaterialId = alId, InvoicedNetTonnage = 2880.000m },
            new ProducerInvoicedMaterialNetTonnage { Id = 106, CalculatorRunId = 1, ProducerId = p3Id, MaterialId = fcId, InvoicedNetTonnage = 43.500m },

            // R2 (no tonnage for P2 because Cancel Accepted)
            new ProducerInvoicedMaterialNetTonnage { Id = 201, CalculatorRunId = 2, ProducerId = p1Id, MaterialId = alId, InvoicedNetTonnage = 6316.000m },
            new ProducerInvoicedMaterialNetTonnage { Id = 202, CalculatorRunId = 2, ProducerId = p1Id, MaterialId = fcId, InvoicedNetTonnage = 1983.000m },
            new ProducerInvoicedMaterialNetTonnage { Id = 203, CalculatorRunId = 2, ProducerId = p3Id, MaterialId = alId, InvoicedNetTonnage = 2880.000m },
            new ProducerInvoicedMaterialNetTonnage { Id = 204, CalculatorRunId = 2, ProducerId = p3Id, MaterialId = fcId, InvoicedNetTonnage = 43.500m },

            // R3 (P2 reappears Accepted)
            new ProducerInvoicedMaterialNetTonnage { Id = 301, CalculatorRunId = 3, ProducerId = p1Id, MaterialId = alId, InvoicedNetTonnage = 6316.000m },
            new ProducerInvoicedMaterialNetTonnage { Id = 302, CalculatorRunId = 3, ProducerId = p1Id, MaterialId = fcId, InvoicedNetTonnage = 1983.000m },
            new ProducerInvoicedMaterialNetTonnage { Id = 303, CalculatorRunId = 3, ProducerId = p2Id, MaterialId = alId, InvoicedNetTonnage = 839.999m },
            new ProducerInvoicedMaterialNetTonnage { Id = 304, CalculatorRunId = 3, ProducerId = p2Id, MaterialId = fcId, InvoicedNetTonnage = 396.341m },
            new ProducerInvoicedMaterialNetTonnage { Id = 305, CalculatorRunId = 3, ProducerId = p3Id, MaterialId = alId, InvoicedNetTonnage = 2880.000m },
            new ProducerInvoicedMaterialNetTonnage { Id = 306, CalculatorRunId = 3, ProducerId = p3Id, MaterialId = fcId, InvoicedNetTonnage = 43.500m }
        );

        // Results file Suggested billing instructions
        // R1: All Accepted
        _dbContext.ProducerResultFileSuggestedBillingInstruction.AddRange(
            new ProducerResultFileSuggestedBillingInstruction { Id = 1, CalculatorRunId = 1, ProducerId = p1Id, SuggestedBillingInstruction = BillingConstants.Suggestion.Initial, BillingInstructionAcceptReject = BillingConstants.Action.Accepted },
            new ProducerResultFileSuggestedBillingInstruction { Id = 2, CalculatorRunId = 1, ProducerId = p2Id, SuggestedBillingInstruction = BillingConstants.Suggestion.Initial, BillingInstructionAcceptReject = BillingConstants.Action.Accepted },
            new ProducerResultFileSuggestedBillingInstruction { Id = 3, CalculatorRunId = 1, ProducerId = p3Id, SuggestedBillingInstruction = BillingConstants.Suggestion.Initial, BillingInstructionAcceptReject = BillingConstants.Action.Accepted }
        );

        // R2: P2 Cancel Accepted
        _dbContext.ProducerResultFileSuggestedBillingInstruction.AddRange(
            new ProducerResultFileSuggestedBillingInstruction { Id = 4, CalculatorRunId = 2, ProducerId = p1Id, SuggestedBillingInstruction = BillingConstants.Suggestion.Delta, BillingInstructionAcceptReject = BillingConstants.Action.Accepted },
            new ProducerResultFileSuggestedBillingInstruction { Id = 5, CalculatorRunId = 2, ProducerId = p2Id, SuggestedBillingInstruction = BillingConstants.Suggestion.Cancel, BillingInstructionAcceptReject = BillingConstants.Action.Accepted },
            new ProducerResultFileSuggestedBillingInstruction { Id = 6, CalculatorRunId = 2, ProducerId = p3Id, SuggestedBillingInstruction = BillingConstants.Suggestion.Delta, BillingInstructionAcceptReject = BillingConstants.Action.Accepted }
        );

        // R3: P2 Reappear Accepted (non-cancel)
        _dbContext.ProducerResultFileSuggestedBillingInstruction.AddRange(
            new ProducerResultFileSuggestedBillingInstruction { Id = 7, CalculatorRunId = 3, ProducerId = p1Id, SuggestedBillingInstruction = BillingConstants.Suggestion.Rebill, BillingInstructionAcceptReject = BillingConstants.Action.Accepted },
            new ProducerResultFileSuggestedBillingInstruction { Id = 8, CalculatorRunId = 3, ProducerId = p2Id, SuggestedBillingInstruction = BillingConstants.Suggestion.Initial, BillingInstructionAcceptReject = BillingConstants.Action.Accepted },
            new ProducerResultFileSuggestedBillingInstruction { Id = 9, CalculatorRunId = 3, ProducerId = p3Id, SuggestedBillingInstruction = BillingConstants.Suggestion.Rebill, BillingInstructionAcceptReject = BillingConstants.Action.Accepted }
        );

        // Designated invoice instructions
        _dbContext.ProducerDesignatedRunInvoiceInstruction.AddRange(
            // R1
            new ProducerDesignatedRunInvoiceInstruction { Id = 11, ProducerId = p1Id, CalculatorRunId = 1, CurrentYearInvoicedTotalAfterThisRun = 126157.44m, InvoiceAmount = 126157.44m, BillingInstructionId = "1_1001" },
            new ProducerDesignatedRunInvoiceInstruction { Id = 12, ProducerId = p2Id, CalculatorRunId = 1, CurrentYearInvoicedTotalAfterThisRun = 103966.48m, InvoiceAmount = 103966.48m, BillingInstructionId = "1_1002" },
            new ProducerDesignatedRunInvoiceInstruction { Id = 13, ProducerId = p3Id, CalculatorRunId = 1, CurrentYearInvoicedTotalAfterThisRun = 23712.34m, InvoiceAmount = 23712.34m, BillingInstructionId = "1_1003" },

            // R2 (P2 cancelled)
            new ProducerDesignatedRunInvoiceInstruction { Id = 14, ProducerId = p1Id, CalculatorRunId = 2, CurrentYearInvoicedTotalAfterThisRun = 211377.83m, InvoiceAmount = 85220.39m, BillingInstructionId = "2_1001" },
            new ProducerDesignatedRunInvoiceInstruction { Id = 15, ProducerId = p2Id, CalculatorRunId = 2, CurrentYearInvoicedTotalAfterThisRun = null, InvoiceAmount = null, BillingInstructionId = "2_1002" },
            new ProducerDesignatedRunInvoiceInstruction { Id = 16, ProducerId = p3Id, CalculatorRunId = 2, CurrentYearInvoicedTotalAfterThisRun = 40367.29m, InvoiceAmount = 16654.95m, BillingInstructionId = "2_1003" },

            // R3 (latest accepted for all three)
            new ProducerDesignatedRunInvoiceInstruction { Id = 17, ProducerId = p1Id, CalculatorRunId = 3, CurrentYearInvoicedTotalAfterThisRun = 338317.32m, InvoiceAmount = 126242.54m, BillingInstructionId = "3_1001" },
            new ProducerDesignatedRunInvoiceInstruction { Id = 18, ProducerId = p2Id, CalculatorRunId = 3, CurrentYearInvoicedTotalAfterThisRun = 104368.07m, InvoiceAmount = 401.59m, BillingInstructionId = "3_1002" },
            new ProducerDesignatedRunInvoiceInstruction { Id = 19, ProducerId = p3Id, CalculatorRunId = 3, CurrentYearInvoicedTotalAfterThisRun = 62909.94m, InvoiceAmount = 23239.82m, BillingInstructionId = "3_1003" }
        );

        SeedProducerOrgDetails();

        await _dbContext.SaveChangesAsync();

        // Act
        var invoicedProducerRecords = await _sut.GetLatestAcceptedInvoicedProducerRecords(new RelativeYear(2024));

        // Assert – all producers now come from R3
        foreach (var mat in new[] { alId, fcId })
        {
            var p1 = invoicedProducerRecords.Single(r => r.ProducerId == p1Id && r.MaterialId == mat);
            p1.CalculatorRunId.ShouldBe(3);
            p1.CurrentYearInvoicedTotalAfterThisRun.ShouldBe(338317.32m);

            var p2 = invoicedProducerRecords.Single(r => r.ProducerId == p2Id && r.MaterialId == mat);
            p2.CalculatorRunId.ShouldBe(3);
            p2.CurrentYearInvoicedTotalAfterThisRun.ShouldBe(104368.07m);

            var p3 = invoicedProducerRecords.Single(r => r.ProducerId == p3Id && r.MaterialId == mat);
            p3.CalculatorRunId.ShouldBe(3);
            p3.CurrentYearInvoicedTotalAfterThisRun.ShouldBe(62909.94m);
        }

        // 3 producers * 2 materials = 6
        invoicedProducerRecords.Count.ShouldBe(6);
    }

    [TestMethod]
    public async Task GetLatestAcceptedInvoicedProducerRecords_CancelRejectedInRun4_AfterReappearInRun3_StillUsesRun3() // US608960 AC 2
    {
        // Arrange
        const int alId = 2001; // Aluminium
        const int fcId = 2002; // Fibre composite

        const int p1Id = 1001;
        const int p2Id = 1002; // (R2 Cancel Accepted, R3 Reappear Accepted, R4 Cancel Rejected)
        const int p3Id = 1003;

        _dbContext.Material.AddRange(
            new Material { Id = alId, Code = "AL", Name = "Aluminium", Description = "Aluminium" },
            new Material { Id = fcId, Code = "FC", Name = "Fibre composite", Description = "Fibre composite" }
        );

        _dbContext.CalculatorRuns.AddRange(
            new CalculatorRun { Id = 1, Name = "R1", RelativeYear = new RelativeYear(2024), Classification = RunClassification.FinalRunCompleted },
            new CalculatorRun { Id = 2, Name = "R2", RelativeYear = new RelativeYear(2024), Classification = RunClassification.FinalRunCompleted },
            new CalculatorRun { Id = 3, Name = "R3", RelativeYear = new RelativeYear(2024), Classification = RunClassification.FinalRunCompleted },
            new CalculatorRun { Id = 4, Name = "R4", RelativeYear = new RelativeYear(2024), Classification = RunClassification.FinalRunCompleted }
        );

        // Invoiced net tonnage
        _dbContext.ProducerInvoicedMaterialNetTonnage.AddRange(
            // R1
            new ProducerInvoicedMaterialNetTonnage { Id = 101, CalculatorRunId = 1, ProducerId = p1Id, MaterialId = alId, InvoicedNetTonnage = 6316.000m },
            new ProducerInvoicedMaterialNetTonnage { Id = 102, CalculatorRunId = 1, ProducerId = p1Id, MaterialId = fcId, InvoicedNetTonnage = 1983.000m },
            new ProducerInvoicedMaterialNetTonnage { Id = 103, CalculatorRunId = 1, ProducerId = p2Id, MaterialId = alId, InvoicedNetTonnage = 839.999m },
            new ProducerInvoicedMaterialNetTonnage { Id = 104, CalculatorRunId = 1, ProducerId = p2Id, MaterialId = fcId, InvoicedNetTonnage = 396.341m },
            new ProducerInvoicedMaterialNetTonnage { Id = 105, CalculatorRunId = 1, ProducerId = p3Id, MaterialId = alId, InvoicedNetTonnage = 2880.000m },
            new ProducerInvoicedMaterialNetTonnage { Id = 106, CalculatorRunId = 1, ProducerId = p3Id, MaterialId = fcId, InvoicedNetTonnage = 43.500m },

            // R2 (no P2)
            new ProducerInvoicedMaterialNetTonnage { Id = 201, CalculatorRunId = 2, ProducerId = p1Id, MaterialId = alId, InvoicedNetTonnage = 6316.000m },
            new ProducerInvoicedMaterialNetTonnage { Id = 202, CalculatorRunId = 2, ProducerId = p1Id, MaterialId = fcId, InvoicedNetTonnage = 1983.000m },
            new ProducerInvoicedMaterialNetTonnage { Id = 203, CalculatorRunId = 2, ProducerId = p3Id, MaterialId = alId, InvoicedNetTonnage = 2880.000m },
            new ProducerInvoicedMaterialNetTonnage { Id = 204, CalculatorRunId = 2, ProducerId = p3Id, MaterialId = fcId, InvoicedNetTonnage = 43.500m },

            // R3 (all present again)
            new ProducerInvoicedMaterialNetTonnage { Id = 301, CalculatorRunId = 3, ProducerId = p1Id, MaterialId = alId, InvoicedNetTonnage = 6316.000m },
            new ProducerInvoicedMaterialNetTonnage { Id = 302, CalculatorRunId = 3, ProducerId = p1Id, MaterialId = fcId, InvoicedNetTonnage = 1983.000m },
            new ProducerInvoicedMaterialNetTonnage { Id = 303, CalculatorRunId = 3, ProducerId = p2Id, MaterialId = alId, InvoicedNetTonnage = 839.999m },
            new ProducerInvoicedMaterialNetTonnage { Id = 304, CalculatorRunId = 3, ProducerId = p2Id, MaterialId = fcId, InvoicedNetTonnage = 396.341m },
            new ProducerInvoicedMaterialNetTonnage { Id = 305, CalculatorRunId = 3, ProducerId = p3Id, MaterialId = alId, InvoicedNetTonnage = 2880.000m },
            new ProducerInvoicedMaterialNetTonnage { Id = 306, CalculatorRunId = 3, ProducerId = p3Id, MaterialId = fcId, InvoicedNetTonnage = 43.500m },

            // R4 (no P2)
            new ProducerInvoicedMaterialNetTonnage { Id = 307, CalculatorRunId = 4, ProducerId = p1Id, MaterialId = alId, InvoicedNetTonnage = 6316.000m },
            new ProducerInvoicedMaterialNetTonnage { Id = 308, CalculatorRunId = 4, ProducerId = p1Id, MaterialId = fcId, InvoicedNetTonnage = 1983.000m },
            new ProducerInvoicedMaterialNetTonnage { Id = 309, CalculatorRunId = 4, ProducerId = p3Id, MaterialId = alId, InvoicedNetTonnage = 2880.000m },
            new ProducerInvoicedMaterialNetTonnage { Id = 310, CalculatorRunId = 4, ProducerId = p3Id, MaterialId = fcId, InvoicedNetTonnage = 43.500m }
        );

        // Suggested billing instructions:
        // R1: all Accepted
        _dbContext.ProducerResultFileSuggestedBillingInstruction.AddRange(
            new ProducerResultFileSuggestedBillingInstruction { Id = 1, CalculatorRunId = 1, ProducerId = p1Id, SuggestedBillingInstruction = BillingConstants.Suggestion.Initial, BillingInstructionAcceptReject = BillingConstants.Action.Accepted },
            new ProducerResultFileSuggestedBillingInstruction { Id = 2, CalculatorRunId = 1, ProducerId = p2Id, SuggestedBillingInstruction = BillingConstants.Suggestion.Initial, BillingInstructionAcceptReject = BillingConstants.Action.Accepted },
            new ProducerResultFileSuggestedBillingInstruction { Id = 3, CalculatorRunId = 1, ProducerId = p3Id, SuggestedBillingInstruction = BillingConstants.Suggestion.Initial, BillingInstructionAcceptReject = BillingConstants.Action.Accepted }
        );

        // R2: P2 Cancel Accepted
        _dbContext.ProducerResultFileSuggestedBillingInstruction.AddRange(
            new ProducerResultFileSuggestedBillingInstruction { Id = 4, CalculatorRunId = 2, ProducerId = p1Id, SuggestedBillingInstruction = BillingConstants.Suggestion.Delta, BillingInstructionAcceptReject = BillingConstants.Action.Accepted },
            new ProducerResultFileSuggestedBillingInstruction { Id = 5, CalculatorRunId = 2, ProducerId = p2Id, SuggestedBillingInstruction = BillingConstants.Suggestion.Cancel, BillingInstructionAcceptReject = BillingConstants.Action.Accepted },
            new ProducerResultFileSuggestedBillingInstruction { Id = 6, CalculatorRunId = 2, ProducerId = p3Id, SuggestedBillingInstruction = BillingConstants.Suggestion.Delta, BillingInstructionAcceptReject = BillingConstants.Action.Accepted }
        );

        // R3: P2 Reappear Accepted (non-cancel)
        _dbContext.ProducerResultFileSuggestedBillingInstruction.AddRange(
            new ProducerResultFileSuggestedBillingInstruction { Id = 7, CalculatorRunId = 3, ProducerId = p1Id, SuggestedBillingInstruction = BillingConstants.Suggestion.Rebill, BillingInstructionAcceptReject = BillingConstants.Action.Accepted },
            new ProducerResultFileSuggestedBillingInstruction { Id = 8, CalculatorRunId = 3, ProducerId = p2Id, SuggestedBillingInstruction = BillingConstants.Suggestion.Initial, BillingInstructionAcceptReject = BillingConstants.Action.Accepted },
            new ProducerResultFileSuggestedBillingInstruction { Id = 9, CalculatorRunId = 3, ProducerId = p3Id, SuggestedBillingInstruction = BillingConstants.Suggestion.Rebill, BillingInstructionAcceptReject = BillingConstants.Action.Accepted }
        );

        // R4: P2 Cancel REJECTED
        _dbContext.ProducerResultFileSuggestedBillingInstruction.AddRange(
            new ProducerResultFileSuggestedBillingInstruction { Id = 10, CalculatorRunId = 4, ProducerId = p1Id, SuggestedBillingInstruction = BillingConstants.Suggestion.Rebill, BillingInstructionAcceptReject = BillingConstants.Action.Accepted },
            new ProducerResultFileSuggestedBillingInstruction { Id = 11, CalculatorRunId = 4, ProducerId = p2Id, SuggestedBillingInstruction = BillingConstants.Suggestion.Cancel, BillingInstructionAcceptReject = BillingConstants.Action.Rejected },
            new ProducerResultFileSuggestedBillingInstruction { Id = 12, CalculatorRunId = 4, ProducerId = p3Id, SuggestedBillingInstruction = BillingConstants.Suggestion.Rebill, BillingInstructionAcceptReject = BillingConstants.Action.Accepted }
        );

        // Designated invoice instructions
        _dbContext.ProducerDesignatedRunInvoiceInstruction.AddRange(
            // R1
            new ProducerDesignatedRunInvoiceInstruction { Id = 11, ProducerId = p1Id, CalculatorRunId = 1, CurrentYearInvoicedTotalAfterThisRun = 126157.44m, InvoiceAmount = 126157.44m, BillingInstructionId = "1_1001" },
            new ProducerDesignatedRunInvoiceInstruction { Id = 12, ProducerId = p2Id, CalculatorRunId = 1, CurrentYearInvoicedTotalAfterThisRun = 103966.48m, InvoiceAmount = 103966.48m, BillingInstructionId = "1_1002" },
            new ProducerDesignatedRunInvoiceInstruction { Id = 13, ProducerId = p3Id, CalculatorRunId = 1, CurrentYearInvoicedTotalAfterThisRun = 23712.34m, InvoiceAmount = 23712.34m, BillingInstructionId = "1_1003" },

            // R2 (P2 cancel accepted)
            new ProducerDesignatedRunInvoiceInstruction { Id = 14, ProducerId = p1Id, CalculatorRunId = 2, CurrentYearInvoicedTotalAfterThisRun = 211377.83m, InvoiceAmount = 85220.39m, BillingInstructionId = "2_1001" },
            new ProducerDesignatedRunInvoiceInstruction { Id = 15, ProducerId = p2Id, CalculatorRunId = 2, CurrentYearInvoicedTotalAfterThisRun = null, InvoiceAmount = null, BillingInstructionId = "2_1002" },
            new ProducerDesignatedRunInvoiceInstruction { Id = 16, ProducerId = p3Id, CalculatorRunId = 2, CurrentYearInvoicedTotalAfterThisRun = 40367.29m, InvoiceAmount = 16654.95m, BillingInstructionId = "2_1003" },

            // R3 (P2 reappear accepted)
            new ProducerDesignatedRunInvoiceInstruction { Id = 17, ProducerId = p1Id, CalculatorRunId = 3, CurrentYearInvoicedTotalAfterThisRun = 338317.32m, InvoiceAmount = 126242.54m, BillingInstructionId = "3_1001" },
            new ProducerDesignatedRunInvoiceInstruction { Id = 18, ProducerId = p2Id, CalculatorRunId = 3, CurrentYearInvoicedTotalAfterThisRun = 104368.07m, InvoiceAmount = 401.59m, BillingInstructionId = "3_1002" },
            new ProducerDesignatedRunInvoiceInstruction { Id = 19, ProducerId = p3Id, CalculatorRunId = 3, CurrentYearInvoicedTotalAfterThisRun = 62909.94m, InvoiceAmount = 23239.82m, BillingInstructionId = "3_1003" },

            // R4 P2 cancel rejected
            new ProducerDesignatedRunInvoiceInstruction { Id = 20, ProducerId = p1Id, CalculatorRunId = 4, CurrentYearInvoicedTotalAfterThisRun = 350000.00m, InvoiceAmount = 11682.68m, BillingInstructionId = "4_1001" },
            new ProducerDesignatedRunInvoiceInstruction { Id = 21, ProducerId = p2Id, CalculatorRunId = 4, CurrentYearInvoicedTotalAfterThisRun = null, InvoiceAmount = null, BillingInstructionId = "4_1002" },
            new ProducerDesignatedRunInvoiceInstruction { Id = 22, ProducerId = p3Id, CalculatorRunId = 4, CurrentYearInvoicedTotalAfterThisRun = 65000.00m, InvoiceAmount = 208.71m, BillingInstructionId = "4_1003" }
        );

        SeedProducerOrgDetails();

        await _dbContext.SaveChangesAsync();

        // Act
        var invoicedProducerRecords = await _sut.GetLatestAcceptedInvoicedProducerRecords(new RelativeYear(2024));

        // P2 comes from R3 as rejected in R4; P1 & P3 are from R4
        foreach (var mat in new[] { alId, fcId })
        {
            var p2 = invoicedProducerRecords.Single(r => r.ProducerId == p2Id && r.MaterialId == mat);
            p2.CalculatorRunId.ShouldBe(3);
            p2.CurrentYearInvoicedTotalAfterThisRun.ShouldBe(104368.07m);

            var p1 = invoicedProducerRecords.Single(r => r.ProducerId == p1Id && r.MaterialId == mat);
            p1.CalculatorRunId.ShouldBe(4);
            p1.CurrentYearInvoicedTotalAfterThisRun.ShouldBe(350000.00m);

            var p3 = invoicedProducerRecords.Single(r => r.ProducerId == p3Id && r.MaterialId == mat);
            p3.CalculatorRunId.ShouldBe(4);
            p3.CurrentYearInvoicedTotalAfterThisRun.ShouldBe(65000.00m);
        }

        // 3 producers * 2 materials = 6
        invoicedProducerRecords.Count.ShouldBe(6);
    }

    // GetLatestAcceptedInvoicedProducerRecords inner-joins on CalculatorRunOrganisationDataDetails to populate
    // ProducerName/TradingName. Without a master + a detail row per producer (with SubsidiaryId null),
    // the join yields zero rows and every assertion below fails with "Sequence contains no elements".
    private void SeedProducerOrgDetails()
    {
        _dbContext.CalculatorRunOrganisationDataMaster.Add(new CalculatorRunOrganisationDataMaster
        {
            Id = 1,
            RelativeYear = new RelativeYear(2024),
            EffectiveFrom = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "Test user"
        });

        _dbContext.CalculatorRunOrganisationDataDetails.AddRange(
            new CalculatorRunOrganisationDataDetail
            {
                Id = 1,
                OrganisationId = 1001,
                SubsidiaryId = null,
                OrganisationName = "Allied Packaging",
                TradingName = "Allied Trading",
                LoadTimeStamp = DateTime.UtcNow,
                CalculatorRunOrganisationDataMasterId = 1
            },
            new CalculatorRunOrganisationDataDetail
            {
                Id = 2,
                OrganisationId = 1002,
                SubsidiaryId = null,
                OrganisationName = "Beeline Materials",
                TradingName = "Beeline Trading",
                LoadTimeStamp = DateTime.UtcNow,
                CalculatorRunOrganisationDataMasterId = 1
            },
            new CalculatorRunOrganisationDataDetail
            {
                Id = 3,
                OrganisationId = 1003,
                SubsidiaryId = null,
                OrganisationName = "Cloud Boxes",
                TradingName = "Cloud Trading",
                LoadTimeStamp = DateTime.UtcNow,
                CalculatorRunOrganisationDataMasterId = 1
            });
    }
}
