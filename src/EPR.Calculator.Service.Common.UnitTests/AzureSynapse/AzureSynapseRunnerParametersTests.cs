namespace EPR.Calculator.Service.Common.UnitTests.AzureSynapse
{
    using System;
    using AutoFixture;
    using EPR.Calculator.Service.Common.AzureSynapse;
    using EPR.Calculator.Service.Common.UnitTests.AutoFixtureCustomisations;
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
            fixture.Customizations.Add(new RelativeYearCustomisation());

            this.CalculatorRunId = fixture.Create<int>();
            this.RelativeYear = fixture.Create<RelativeYear>();
            this.CheckInterval = fixture.Create<int>();
            this.MaxCheckCount = fixture.Create<int>();
            this.PipelineName = fixture.Create<string>();
            this.PipelineUrl = fixture.Create<Uri>();
            this.StatusUpdateEndpoint = fixture.Create<Uri>();
            this.PrepareCalcResultEndPoint = fixture.Create<Uri>();

            this.TestClass = new AzureSynapseRunnerParameters
            {
                CalculatorRunId = this.CalculatorRunId,
                RelativeYear = this.RelativeYear,
                CheckInterval = this.CheckInterval,
                MaxCheckCount = this.MaxCheckCount,
                PipelineName = this.PipelineName,
                PipelineUrl = this.PipelineUrl,
            };
        }

        private AzureSynapseRunnerParameters TestClass { get; }

        private int CalculatorRunId { get; }

        private RelativeYear RelativeYear { get; }

        private int CheckInterval { get; }

        private int MaxCheckCount { get; }

        private string PipelineName { get; }

        private Uri PipelineUrl { get; }

        private Uri StatusUpdateEndpoint { get; }

        private Uri PrepareCalcResultEndPoint { get; }

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
                RelativeYear = this.RelativeYear,
                CheckInterval = this.CheckInterval,
                MaxCheckCount = this.MaxCheckCount,
                PipelineName = this.PipelineName,
                PipelineUrl = this.PipelineUrl,
            };

            // Assert
            Assert.IsNotNull(instance);
            Assert.AreEqual(this.CalculatorRunId, this.TestClass.CalculatorRunId);
            Assert.AreEqual(this.RelativeYear, this.TestClass.RelativeYear);
            Assert.AreEqual(this.CheckInterval, this.TestClass.CheckInterval);
            Assert.AreEqual(this.MaxCheckCount, this.TestClass.MaxCheckCount);
            Assert.AreEqual(this.PipelineName, this.TestClass.PipelineName);
            Assert.AreEqual(this.PipelineUrl, this.TestClass.PipelineUrl);
        }
    }
}