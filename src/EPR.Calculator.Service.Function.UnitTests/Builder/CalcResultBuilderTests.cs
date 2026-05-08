using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Builder;
using EPR.Calculator.Service.Function.Builder.Modulation;
using EPR.Calculator.Service.Function.Builder.ProjectedProducers;
using EPR.Calculator.Service.Function.Builder.RejectedProducers;
using EPR.Calculator.Service.Function.Builder.ScaledupProducers;
using EPR.Calculator.Service.Function.Features.Common;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Services;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.Data;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.Fixtures;

namespace EPR.Calculator.Service.Function.UnitTests.Builder;

[TestClass]
public class CalcResultBuilderTests
{
    private IFixture _fixture = null!;
    private CalcResultBuilder _sut = null!;
    private Mock<ICalcResultProjectedProducersBuilder> _projectedProducers = null!;
    private Mock<ICalcResultRejectedProducersBuilder> _rejectedProducers = null!;
    private Mock<ICalcResultScaledupProducersBuilder> _scaledUpProducers = null!;
    private Mock<ICalcResultModulationBuilder> _modulations = null!;
    private Mock<IProjectedProducersService> _projectedProducersSvc = null!;

    [TestInitialize]
    public void Init()
    {
        _fixture = TestFixtures.New();
        _projectedProducers = _fixture.Freeze<Mock<ICalcResultProjectedProducersBuilder>>();
        _rejectedProducers = _fixture.Freeze<Mock<ICalcResultRejectedProducersBuilder>>();
        _scaledUpProducers = _fixture.Freeze<Mock<ICalcResultScaledupProducersBuilder>>();
        _modulations = _fixture.Freeze<Mock<ICalcResultModulationBuilder>>();
        _projectedProducersSvc = _fixture.Freeze<Mock<IProjectedProducersService>>();

        _sut = _fixture.Create<CalcResultBuilder>();
    }

    private static IEnumerable<object[]> TestCases()
    {
        return
        [
            [DummyData.RunContexts.CalculatorRun2024, new Expected { BuildsProjectedProducers = false, BuildsRejectedProducers = false, BuildsScaledUpProducers = false, BuildsModulations = false, StoresProjectedProducers = true }],
            [DummyData.RunContexts.CalculatorRun2025, new Expected { BuildsProjectedProducers = false, BuildsRejectedProducers = false, BuildsScaledUpProducers = true,  BuildsModulations = false, StoresProjectedProducers = true }],
            [DummyData.RunContexts.CalculatorRun2026, new Expected { BuildsProjectedProducers = true,  BuildsRejectedProducers = false, BuildsScaledUpProducers = false, BuildsModulations = true,  StoresProjectedProducers = true }],

            [DummyData.RunContexts.BillingRun2024,    new Expected { BuildsProjectedProducers = false, BuildsRejectedProducers = true,  BuildsScaledUpProducers = false, BuildsModulations = false, StoresProjectedProducers = false }],
            [DummyData.RunContexts.BillingRun2025,    new Expected { BuildsProjectedProducers = false, BuildsRejectedProducers = true,  BuildsScaledUpProducers = true,  BuildsModulations = false, StoresProjectedProducers = false }],
            [DummyData.RunContexts.BillingRun2026,    new Expected { BuildsProjectedProducers = true,  BuildsRejectedProducers = true,  BuildsScaledUpProducers = false, BuildsModulations = true,  StoresProjectedProducers = false }]
        ];
    }

    [DynamicData(nameof(TestCases), DynamicDataSourceType.Method)]
    [TestMethod]
    public async Task Should_call_correct_services(RunContext runContext, Expected expected)
    {

        var result = await _sut.BuildAsync(runContext);

        result.ShouldNotBeNull();

        if (expected.BuildsProjectedProducers)
            _projectedProducers.Verify(m => m.Construct(runContext, It.IsAny<ImmutableList<MaterialDetail>>(), It.IsAny<List<ProducerDetail>>()), Times.Once);
        else
            _projectedProducers.Verify(m => m.Construct(It.IsAny<RunContext>(), It.IsAny<ImmutableList<MaterialDetail>>(), It.IsAny<List<ProducerDetail>>()), Times.Never);

        if (expected.BuildsRejectedProducers)
            _rejectedProducers.Verify(m => m.ConstructAsync(runContext), Times.Once);
        else
            _rejectedProducers.Verify(m => m.ConstructAsync(It.IsAny<RunContext>()), Times.Never);

        if (expected.BuildsScaledUpProducers)
            _scaledUpProducers.Verify(m => m.ConstructAsync(runContext, It.IsAny<ImmutableList<MaterialDetail>>(), It.IsAny<List<ProducerDetail>>()), Times.Once);
        else
            _scaledUpProducers.Verify(m => m.ConstructAsync(It.IsAny<RunContext>(), It.IsAny<ImmutableList<MaterialDetail>>(), It.IsAny<List<ProducerDetail>>()), Times.Never);

        if (expected.BuildsModulations)
            _modulations.Verify(m => m.ConstructAsync(It.IsAny<IReadOnlyDictionary<string,decimal>>(), It.IsAny<ImmutableList<MaterialDetail>>(), It.IsAny<CalcResultLaDisposalCostData>(), It.IsAny<SelfManagedConsumerWaste>()), Times.Once);
        else
            _modulations.Verify(m => m.ConstructAsync(It.IsAny<IReadOnlyDictionary<string,decimal>>(), It.IsAny<ImmutableList<MaterialDetail>>(), It.IsAny<CalcResultLaDisposalCostData>(), It.IsAny<SelfManagedConsumerWaste>()), Times.Never);

        if(expected.StoresProjectedProducers)
            _projectedProducersSvc.Verify(m => m.StoreProjectedProducers(runContext, It.IsAny<List<ProducerDetail>>()), Times.Once);
        else
            _projectedProducersSvc.Verify(m => m.StoreProjectedProducers(It.IsAny<RunContext>(), It.IsAny<List<ProducerDetail>>()), Times.Never);
    }

    public record Expected
    {
        public required bool BuildsProjectedProducers { get; init; }
        public required bool BuildsRejectedProducers { get; init; }
        public required bool BuildsScaledUpProducers { get; init; }
        public required bool BuildsModulations { get; init; }
        public required bool StoresProjectedProducers { get; init; }
    }
}
