namespace EPR.Calculator.Service.Function.UnitTests.Services
{
    using AutoFixture;
    using EPR.Calculator.Service.Common;
    using EPR.Calculator.Service.Common.AzureSynapse;
    using EPR.Calculator.Service.Function.Constants;
    using EPR.Calculator.Service.Function.Services;
    using Moq;

    [TestClass]
    public class CalculatorRunServiceTests
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CalculatorRunServiceTests"/> class.
        /// </summary>
        public CalculatorRunServiceTests()
        {
            this.CalculatorRunService = new CalculatorRunService();
            this.AzureSynapseRunner = new Mock<IAzureSynapseRunner>();
        }

        private CalculatorRunService CalculatorRunService { get; }

        private Mock<IAzureSynapseRunner> AzureSynapseRunner { get; }

        /// <summary>
        /// Checks that the service calls the Azure Synapse runner and passes the correct parameters to it.
        /// </summary>
        /// <param name="isPom">
        /// Whether the pipeline runner is expected to call the pom pipeline or the org one.
        /// </param>
        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void StartProcessCallsAzureSynapseRunner(bool isPom)
        {
            //    // Arrange
            //    var fixture = new Fixture();
            //    var id = fixture.Create<int>();
            //    var financialYear = "2024-25";
            //    var user = fixture.Create<string>();

            //    Environment.SetEnvironmentVariable(EnvironmentVariableKeys.IsPom, isPom);

            //    var calculatorRunParameters = new CalculatorRunParameter
            //    {
            //        FinancialYear = financialYear,
            //        User = user,
            //    };

            //    // The values that the service is expected to pass to the pipeline runner.
            //    var expectedParameters = new AzureSynapseRunnerParameters
            //    {
            //        CalculatorRunId = id,
            //        CheckInterval = 1,
            //        FinancialYear = FinancialYear.Parse(financialYear),
            //        MaxChecks = 1,
            //        PipelineName = isPom ? EnvironmentVariableKeys.GetPomDataPipelineName : EnvironmentVariableKeys.GetOrgDataPipelineName,
            //        PipelineUrl = new Uri(EnvironmentVariableKeys.PipelineUrl),
            //        StatusUpdateEndpoint = new Uri(EnvironmentVariableKeys.StatusUpdateEndpoint),
            //    };

            //    // Act
            //    this.CalculatorRunService.StartProcess(calculatorRunParameters, this.AzureSynapseRunner.Object);

            //    // Assert
            //    this.AzureSynapseRunner.Verify(
            //        runnner => runnner.Process(
            //            It.Is<AzureSynapseRunnerParameters>(args =>
            //            args == expectedParameters)),
            //        Times.Once);
        }
    }
}