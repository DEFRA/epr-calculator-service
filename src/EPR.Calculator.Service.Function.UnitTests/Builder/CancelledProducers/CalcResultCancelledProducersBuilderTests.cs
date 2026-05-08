using EPR.Calculator.API.Data.Models;
using EPR.Calculator.Service.Function.Builder.CancelledProducers;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Features.Calculator.Contexts;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Services;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.Data;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.Fixtures;

namespace EPR.Calculator.Service.Function.UnitTests.Builder.CancelledProducers;

[TestClass]
public class CalcResultCancelledProducersBuilderTests
{
    private IFixture _fixture = null!;
    private Mock<IInvoicedProducerService> _invoicedProducersService = null!;
    private Mock<IMaterialService> _materialService = null!;
    private CalcResultCancelledProducersBuilder _sut = null!;

    [TestInitialize]
    public void Init()
    {
        _fixture = TestFixtures.New();

        _invoicedProducersService = _fixture.Freeze<Mock<IInvoicedProducerService>>();

        _materialService = _fixture.Freeze<Mock<IMaterialService>>();
        _materialService.Setup(t =>
                t.GetMaterialsByCode(It.IsAny<CancellationToken>()))
            .ReturnsAsync(DummyData.MaterialsByCode);

        _sut = _fixture.Freeze<CalcResultCancelledProducersBuilder>();
    }

    [TestMethod]
    public async Task ConstructAsync_ShouldSetTitleHeader()
    {
        // Arrange
        var runContext = DummyData.RunContexts.CalculatorRun2025;
        SetupEmptyInvoicedProducerService(runContext);

        // Act
        var result = await _sut.ConstructAsync(runContext);

        // Assert
        result.TitleHeader.ShouldBe(CommonConstants.CancelledProducers);
    }

    [TestMethod]
    public async Task ConstructAsync_WithNoProducers_ShouldReturnEmptyList()
    {
        // Arrange
        var runContext = DummyData.RunContexts.CalculatorRun2025;
        SetupEmptyInvoicedProducerService(runContext);

        // Act
        var result = await _sut.ConstructAsync(runContext);

        // Assert
        result.CancelledProducers.ShouldNotBeNull();
        result.CancelledProducers.Count.ShouldBe(0);
    }

    [TestMethod]
    public async Task ConstructAsync_WithCalculatorRunContext_ShouldExcludeInvoicedThenCancelledProducers()
    {
        // Arrange
        var runContext = DummyData.RunContexts.CalculatorRun2025;
        var producersForRun = ImmutableHashSet.Create(1, 2);
        var invoicedProducersForYear = ImmutableHashSet.Create(1, 2, 3, 4);
        var invoicedThenCancelledProducers = ImmutableHashSet.Create(3); // Producer 3 was invoiced then cancelled
        // Expected: missing producers = {3, 4}, minus invoiced-then-cancelled = {4}

        _invoicedProducersService
            .Setup(x => x.GetProducerIdsForRun(runContext.RunId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(producersForRun);

        _invoicedProducersService
            .Setup(x => x.GetInvoicedProducerIdsForYear(runContext.RelativeYear, It.IsAny<CancellationToken>()))
            .ReturnsAsync(invoicedProducersForYear);

        _invoicedProducersService
            .Setup(x => x.GetInvoicedThenCancelledProducerIdsForYear(runContext.RelativeYear, It.IsAny<CancellationToken>()))
            .ReturnsAsync(invoicedThenCancelledProducers);

        var expectedProducerIds = ImmutableHashSet.Create(4);
        _invoicedProducersService
            .Setup(x => x.GetInvoicedProducerRecords(runContext.RelativeYear, expectedProducerIds, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateInvoicedProducerRecords(4));

        // Act
        var result = await _sut.ConstructAsync(runContext);

        // Assert
        result.CancelledProducers.Count.ShouldBe(1);
        result.CancelledProducers[0].ProducerId.ShouldBe(4);
        _invoicedProducersService.Verify(x => x.GetInvoicedThenCancelledProducerIdsForYear(runContext.RelativeYear, It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task ConstructAsync_WithBillingRunContext_ShouldIncludeAcceptedCancelledProducers()
    {
        // Arrange
        var runContext = DummyData.RunContexts.BillingRun2025;
        var producersForRun = ImmutableHashSet.Create(1, 2);
        var invoicedProducersForYear = ImmutableHashSet.Create(1, 2, 3, 4);
        var acceptedCancelledProducers = ImmutableHashSet.Create(3); // Producer 3 is accepted-cancelled in this run
        // Expected: missing producers = {3, 4}, intersected with accepted-cancelled = {3}

        _invoicedProducersService
            .Setup(x => x.GetProducerIdsForRun(runContext.RunId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(producersForRun);

        _invoicedProducersService
            .Setup(x => x.GetInvoicedProducerIdsForYear(runContext.RelativeYear, It.IsAny<CancellationToken>()))
            .ReturnsAsync(invoicedProducersForYear);

        _invoicedProducersService
            .Setup(x => x.GetAcceptedCancelledProducerIdsForRun(runContext.RunId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(acceptedCancelledProducers);

        var expectedProducerIds = ImmutableHashSet.Create(3);
        _invoicedProducersService
            .Setup(x => x.GetInvoicedProducerRecords(runContext.RelativeYear, expectedProducerIds, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateInvoicedProducerRecords(3));

        // Act
        var result = await _sut.ConstructAsync(runContext);

        // Assert
        result.CancelledProducers.Count.ShouldBe(1);
        result.CancelledProducers[0].ProducerId.ShouldBe(3);
        _invoicedProducersService.Verify(x => x.GetAcceptedCancelledProducerIdsForRun(runContext.RunId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task ConstructAsync_ShouldPopulateProducerDetails()
    {
        // Arrange
        var runContext = DummyData.RunContexts.CalculatorRun2025;
        var producerId = 100;
        var producerName = "Test Producer Ltd";
        var tradingName = "Test Trading Name";

        SetupServiceForSingleProducer(runContext, producerId, producerName, tradingName);

        // Act
        var result = await _sut.ConstructAsync(runContext);

        // Assert
        var producer = result.CancelledProducers[0];
        producer.ProducerId.ShouldBe(producerId);
        producer.ProducerOrSubsidiaryNameValue.ShouldBe(producerName);
        producer.TradingNameValue.ShouldBe(tradingName);
    }

    [TestMethod]
    public async Task ConstructAsync_ShouldPopulateLastTonnageForAllMaterials()
    {
        // Arrange
        var runContext = DummyData.RunContexts.CalculatorRun2025;
        var producerId = 100;
        var records = CreateInvoicedProducerRecordsWithAllMaterials(producerId);

        SetupServiceForRecords(runContext, records);

        // Act
        var result = await _sut.ConstructAsync(runContext);

        // Assert
        var lastTonnage = result.CancelledProducers[0].LastTonnage;
        lastTonnage.ShouldNotBeNull();
        lastTonnage.AluminiumValue.ShouldBe(10.5m);
        lastTonnage.FibreCompositeValue.ShouldBe(20.3m);
        lastTonnage.GlassValue.ShouldBe(30.7m);
        lastTonnage.PaperOrCardValue.ShouldBe(40.2m);
        lastTonnage.PlasticValue.ShouldBe(50.9m);
        lastTonnage.SteelValue.ShouldBe(15.5m);
        lastTonnage.WoodValue.ShouldBe(25.1m);
        lastTonnage.OtherMaterialsValue.ShouldBe(12.3m);
    }

    [TestMethod]
    public async Task ConstructAsync_ShouldHandleNullTonnageValues()
    {
        // Arrange
        var runContext = DummyData.RunContexts.CalculatorRun2025;
        var producerId = 100;
        var records = CreateInvoicedProducerRecordsWithPartialMaterials(producerId);

        SetupServiceForRecords(runContext, records);

        // Act
        var result = await _sut.ConstructAsync(runContext);

        // Assert
        var lastTonnage = result.CancelledProducers[0].LastTonnage;
        lastTonnage.ShouldNotBeNull();
        lastTonnage.AluminiumValue.ShouldBe(10.5m);
        lastTonnage.FibreCompositeValue.ShouldBeNull();
        lastTonnage.GlassValue.ShouldBe(30.7m);
        lastTonnage.PaperOrCardValue.ShouldBeNull();
    }

    [TestMethod]
    public async Task ConstructAsync_ShouldPopulateLatestInvoiceDetails()
    {
        // Arrange
        var runContext = DummyData.RunContexts.CalculatorRun2025;
        var billingInstructionId = "BILL-12345";
        var calculatorRunId = 20251;
        var calculatorName = "2025 Calculator Run";
        var invoiceTotal = 5432.10m;

        var records = CreateInvoicedProducerRecordsWithInvoiceDetails(
            100, billingInstructionId, calculatorRunId, calculatorName, invoiceTotal);

        SetupServiceForRecords(runContext, records);

        // Act
        var result = await _sut.ConstructAsync(runContext);

        // Assert
        var latestInvoice = result.CancelledProducers[0].LatestInvoice;
        latestInvoice.ShouldNotBeNull();
        latestInvoice.BillingInstructionIdValue.ShouldBe(billingInstructionId);
        latestInvoice.RunNumberValue.ShouldBe(calculatorRunId.ToString());
        latestInvoice.RunNameValue.ShouldBe(calculatorName);
        latestInvoice.CurrentYearInvoicedTotalToDateValue.ShouldBe(invoiceTotal);
    }

    [TestMethod]
    public async Task ConstructAsync_WithMultipleRunsForProducer_ShouldUseLatestRun()
    {
        // Arrange
        var runContext = DummyData.RunContexts.CalculatorRun2025;
        var producerId = 100;
        
        var records = ImmutableList.Create(
            CreateInvoicedProducerRecord(producerId, "Producer", null, 
                DummyData.MaterialsByCode["AL"].Id, 10m, 20240, "Old Run", "BILL-001", 100m),
            CreateInvoicedProducerRecord(producerId, "Producer", null,
                DummyData.MaterialsByCode["AL"].Id, 20m, 20251, "Latest Run", "BILL-002", 200m)
        );

        SetupServiceForRecords(runContext, records);

        // Act
        var result = await _sut.ConstructAsync(runContext);

        // Assert
        var producer = result.CancelledProducers[0];
        producer.LatestInvoice?.RunNameValue.ShouldBe("Latest Run");
        producer.LatestInvoice?.BillingInstructionIdValue.ShouldBe("BILL-002");
        producer.LatestInvoice?.CurrentYearInvoicedTotalToDateValue.ShouldBe(200m);
    }

    [TestMethod]
    public async Task ConstructAsync_WithMultipleProducers_ShouldReturnAllProducers()
    {
        // Arrange
        var runContext = DummyData.RunContexts.CalculatorRun2025;
        var records = ImmutableList.Create(
            CreateInvoicedProducerRecord(100, "Producer A", "Trading A", 
                DummyData.MaterialsByCode["AL"].Id, 10m, 20251, "Run", "BILL-001", 100m),
            CreateInvoicedProducerRecord(200, "Producer B", "Trading B",
                DummyData.MaterialsByCode["PL"].Id, 20m, 20251, "Run", "BILL-002", 200m),
            CreateInvoicedProducerRecord(300, "Producer C", null,
                DummyData.MaterialsByCode["GL"].Id, 30m, 20251, "Run", "BILL-003", 300m)
        );

        SetupServiceForRecords(runContext, records);

        // Act
        var result = await _sut.ConstructAsync(runContext);

        // Assert
        result.CancelledProducers.Count.ShouldBe(3);
        result.CancelledProducers.ShouldContain(p => p.ProducerId == 100);
        result.CancelledProducers.ShouldContain(p => p.ProducerId == 200);
        result.CancelledProducers.ShouldContain(p => p.ProducerId == 300);
    }

    [TestMethod]
    public async Task ConstructAsync_WithProducerHavingMultipleMaterials_ShouldGroupByProducer()
    {
        // Arrange
        var runContext = DummyData.RunContexts.CalculatorRun2025;
        var producerId = 100;
        var records = ImmutableList.Create(
            CreateInvoicedProducerRecord(producerId, "Producer", "Trading",
                DummyData.MaterialsByCode["AL"].Id, 10m, 20251, "Run", "BILL-001", 100m),
            CreateInvoicedProducerRecord(producerId, "Producer", "Trading",
                DummyData.MaterialsByCode["PL"].Id, 20m, 20251, "Run", "BILL-001", 100m),
            CreateInvoicedProducerRecord(producerId, "Producer", "Trading",
                DummyData.MaterialsByCode["GL"].Id, 30m, 20251, "Run", "BILL-001", 100m)
        );

        SetupServiceForRecords(runContext, records);

        // Act
        var result = await _sut.ConstructAsync(runContext);

        // Assert
        result.CancelledProducers.Count.ShouldBe(1);
        var producer = result.CancelledProducers[0];
        producer.LastTonnage?.AluminiumValue.ShouldBe(10m);
        producer.LastTonnage?.PlasticValue.ShouldBe(20m);
        producer.LastTonnage?.GlassValue.ShouldBe(30m);
    }

    private void SetupEmptyInvoicedProducerService(CalculatorRunContext runContext)
    {
        _invoicedProducersService
            .Setup(x => x.GetProducerIdsForRun(runContext.RunId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ImmutableHashSet<int>.Empty);

        _invoicedProducersService
            .Setup(x => x.GetInvoicedProducerIdsForYear(runContext.RelativeYear, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ImmutableHashSet<int>.Empty);

        _invoicedProducersService
            .Setup(x => x.GetInvoicedThenCancelledProducerIdsForYear(runContext.RelativeYear, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ImmutableHashSet<int>.Empty);

        _invoicedProducersService
            .Setup(x => x.GetInvoicedProducerRecords(It.IsAny<RelativeYear>(), It.IsAny<ImmutableHashSet<int>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ImmutableList<InvoicedProducerRecord>.Empty);
    }

    private void SetupServiceForSingleProducer(CalculatorRunContext runContext, int producerId, string producerName, string? tradingName)
    {
        var producerIds = ImmutableHashSet.Create(producerId);
        var records = CreateInvoicedProducerRecords(producerId, producerName, tradingName);

        _invoicedProducersService
            .Setup(x => x.GetProducerIdsForRun(runContext.RunId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ImmutableHashSet<int>.Empty);

        _invoicedProducersService
            .Setup(x => x.GetInvoicedProducerIdsForYear(runContext.RelativeYear, It.IsAny<CancellationToken>()))
            .ReturnsAsync(producerIds);

        _invoicedProducersService
            .Setup(x => x.GetInvoicedThenCancelledProducerIdsForYear(runContext.RelativeYear, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ImmutableHashSet<int>.Empty);

        _invoicedProducersService
            .Setup(x => x.GetInvoicedProducerRecords(runContext.RelativeYear, producerIds, It.IsAny<CancellationToken>()))
            .ReturnsAsync(records);
    }

    private void SetupServiceForRecords(CalculatorRunContext runContext, ImmutableList<InvoicedProducerRecord> records)
    {
        var producerIds = records.Select(r => r.ProducerId).ToImmutableHashSet();

        _invoicedProducersService
            .Setup(x => x.GetProducerIdsForRun(runContext.RunId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ImmutableHashSet<int>.Empty);

        _invoicedProducersService
            .Setup(x => x.GetInvoicedProducerIdsForYear(runContext.RelativeYear, It.IsAny<CancellationToken>()))
            .ReturnsAsync(producerIds);

        _invoicedProducersService
            .Setup(x => x.GetInvoicedThenCancelledProducerIdsForYear(runContext.RelativeYear, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ImmutableHashSet<int>.Empty);

        _invoicedProducersService
            .Setup(x => x.GetInvoicedProducerRecords(runContext.RelativeYear, producerIds, It.IsAny<CancellationToken>()))
            .ReturnsAsync(records);
    }

    private ImmutableList<InvoicedProducerRecord> CreateInvoicedProducerRecords(int producerId, string? producerName = null, string? tradingName = null)
    {
        return ImmutableList.Create(
            CreateInvoicedProducerRecord(
                producerId,
                producerName ?? $"Producer {producerId}",
                tradingName,
                DummyData.MaterialsByCode["AL"].Id,
                10.5m,
                20251,
                "Test Run",
                "BILL-001",
                1000m)
        );
    }

    private ImmutableList<InvoicedProducerRecord> CreateInvoicedProducerRecordsWithAllMaterials(int producerId)
    {
        return ImmutableList.Create(
            CreateInvoicedProducerRecord(producerId, "Producer", null, DummyData.MaterialsByCode["AL"].Id, 10.5m, 20251, "Run", "BILL-001", 100m),
            CreateInvoicedProducerRecord(producerId, "Producer", null, DummyData.MaterialsByCode["FC"].Id, 20.3m, 20251, "Run", "BILL-001", 100m),
            CreateInvoicedProducerRecord(producerId, "Producer", null, DummyData.MaterialsByCode["GL"].Id, 30.7m, 20251, "Run", "BILL-001", 100m),
            CreateInvoicedProducerRecord(producerId, "Producer", null, DummyData.MaterialsByCode["PC"].Id, 40.2m, 20251, "Run", "BILL-001", 100m),
            CreateInvoicedProducerRecord(producerId, "Producer", null, DummyData.MaterialsByCode["PL"].Id, 50.9m, 20251, "Run", "BILL-001", 100m),
            CreateInvoicedProducerRecord(producerId, "Producer", null, DummyData.MaterialsByCode["ST"].Id, 15.5m, 20251, "Run", "BILL-001", 100m),
            CreateInvoicedProducerRecord(producerId, "Producer", null, DummyData.MaterialsByCode["WD"].Id, 25.1m, 20251, "Run", "BILL-001", 100m),
            CreateInvoicedProducerRecord(producerId, "Producer", null, DummyData.MaterialsByCode["OT"].Id, 12.3m, 20251, "Run", "BILL-001", 100m)
        );
    }

    private ImmutableList<InvoicedProducerRecord> CreateInvoicedProducerRecordsWithPartialMaterials(int producerId)
    {
        return ImmutableList.Create(
            CreateInvoicedProducerRecord(producerId, "Producer", null, DummyData.MaterialsByCode["AL"].Id, 10.5m, 20251, "Run", "BILL-001", 100m),
            CreateInvoicedProducerRecord(producerId, "Producer", null, DummyData.MaterialsByCode["GL"].Id, 30.7m, 20251, "Run", "BILL-001", 100m)
        );
    }

    private ImmutableList<InvoicedProducerRecord> CreateInvoicedProducerRecordsWithInvoiceDetails(
        int producerId, string billingInstructionId, int calculatorRunId, string calculatorName, decimal invoiceTotal)
    {
        return ImmutableList.Create(
            CreateInvoicedProducerRecord(
                producerId,
                "Producer",
                "Trading",
                DummyData.MaterialsByCode["AL"].Id,
                10m,
                calculatorRunId,
                calculatorName,
                billingInstructionId,
                invoiceTotal)
        );
    }

    private InvoicedProducerRecord CreateInvoicedProducerRecord(
        int producerId,
        string producerName,
        string? tradingName,
        int materialId,
        decimal tonnage,
        int calculatorRunId,
        string calculatorName,
        string billingInstructionId,
        decimal invoiceTotal)
    {
        return new InvoicedProducerRecord
        {
            ProducerId = producerId,
            ProducerName = producerName,
            TradingName = tradingName,
            MaterialId = materialId,
            InvoicedNetTonnage = tonnage,
            CalculatorRunId = calculatorRunId,
            CalculatorName = calculatorName,
            BillingInstructionId = billingInstructionId,
            CurrentYearInvoicedTotalAfterThisRun = invoiceTotal
        };
    }
}
