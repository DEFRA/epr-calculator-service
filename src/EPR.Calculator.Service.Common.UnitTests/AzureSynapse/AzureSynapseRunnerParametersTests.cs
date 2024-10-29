namespace EPR.Calculator.Service.Common.UnitTests.AzureSynapse
{
    using System;
    using AutoFixture;
    using EPR.Calculator.Service.Common.AzureSynapse;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Unit tests for AzureSynapseRunnerParameters.
    /// </summary>
    [TestClass]
    public class AzureSynapseRunnerParametersTests
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AzureSynapseRunnerParametersTests"/> class.
        /// </summary>
        public AzureSynapseRunnerParametersTests()
        {
            var fixture = new Fixture();

            this.CalculatorRunId = fixture.Create<int>();
            this.FinancialYear = new FinancialYear(DateTime.UtcNow);
            this.CheckInterval = fixture.Create<int>();
            this.MaxChecks = fixture.Create<int>();
            this.PipelineName = fixture.Create<string>();
            this.PipelineUrl = fixture.Create<Uri>();
            this.StatusUpdateEndpoint = fixture.Create<Uri>();

            this.TestClass = new AzureSynapseRunnerParameters
            {
                CalculatorRunId = this.CalculatorRunId,
                FinancialYear = this.FinancialYear,
                CheckInterval = this.CheckInterval,
                MaxChecks = this.MaxChecks,
                PipelineName = this.PipelineName,
                PipelineUrl = this.PipelineUrl,
                StatusUpdateEndpoint = this.StatusUpdateEndpoint,
            };
        }

        private AzureSynapseRunnerParameters TestClass { get; }

        private int CalculatorRunId { get; }

        private FinancialYear FinancialYear { get; }

        private int CheckInterval { get; }

        private int MaxChecks { get; }

        private string PipelineName { get; }

        private Uri PipelineUrl { get; }

        private Uri StatusUpdateEndpoint { get; }

        /// <summary>
        /// Test that the class can be initialised successfully.
        /// </summary>
        [TestMethod]
        public void CanInitialize()
        {
            // Act
            var instance = new AzureSynapseRunnerParameters
            {
                CalculatorRunId = this.CalculatorRunId,
                FinancialYear = this.FinancialYear,
                CheckInterval = this.CheckInterval,
                MaxChecks = this.MaxChecks,
                PipelineName = this.PipelineName,
                PipelineUrl = this.PipelineUrl,
                StatusUpdateEndpoint = this.StatusUpdateEndpoint,
            };

            // Assert
            Assert.IsNotNull(instance);
            Assert.AreEqual(this.CalculatorRunId, this.TestClass.CalculatorRunId);
            Assert.AreEqual(this.FinancialYear, this.TestClass.FinancialYear);
            Assert.AreEqual(this.CheckInterval, this.TestClass.CheckInterval);
            Assert.AreEqual(this.MaxChecks, this.TestClass.MaxChecks);
            Assert.AreEqual(this.PipelineName, this.TestClass.PipelineName);
            Assert.AreEqual(this.PipelineUrl, this.TestClass.PipelineUrl);
            Assert.AreEqual(this.StatusUpdateEndpoint, this.TestClass.StatusUpdateEndpoint);
        }
    }
}