namespace EPR.Calculator.Service.Function.UnitTests.Builder.ScaledupProducers
{
    using System;
    using AutoFixture;
    using EPR.Calculator.Service.Function.Builder.ScaledupProducers;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Unit tests for the <see cref="ScaleupProducer"/> class.
    /// </summary>
    [TestClass]
    public class ScaleupProducerTests
    {
        private ScaleupProducer scaleupProducer = null!;

        /// <summary>
        /// Initializes a new instance of the <see cref="ScaleupProducerTests"/> class.
        /// </summary>
        public ScaleupProducerTests()
        {
            this.scaleupProducer = new ScaleupProducer();
        }

        [TestMethod]
        public void CanSetAndGetOrganisationId()
        {
            // Arrange
            var fixture = new Fixture();

            var testValue = fixture.Create<int>();

            // Act
            this.scaleupProducer.OrganisationId = testValue;

            // Assert
            Assert.AreEqual(testValue, this.scaleupProducer.OrganisationId);
        }

        [TestMethod]
        public void CanSetAndGetScaleupFactor()
        {
            // Arrange
            var fixture = new Fixture();

            var testValue = fixture.Create<decimal>();

            // Act
            this.scaleupProducer.ScaleupFactor = testValue;

            // Assert
            Assert.AreEqual(testValue, this.scaleupProducer.ScaleupFactor);
        }

        [TestMethod]
        public void CanSetAndGetSubmissionPeriod()
        {
            // Arrange
            var fixture = new Fixture();

            var testValue = fixture.Create<string>();

            // Act
            this.scaleupProducer.SubmissionPeriod = testValue;

            // Assert
            Assert.AreEqual(testValue, this.scaleupProducer.SubmissionPeriod);
        }

        [TestMethod]
        public void CanSetAndGetDaysInSubmissionPeriod()
        {
            // Arrange
            var fixture = new Fixture();

            var testValue = fixture.Create<int>();

            // Act
            this.scaleupProducer.DaysInSubmissionPeriod = testValue;

            // Assert
            Assert.AreEqual(testValue, this.scaleupProducer.DaysInSubmissionPeriod);
        }

        [TestMethod]
        public void CanSetAndGetDaysInWholePeriod()
        {
            // Arrange
            var fixture = new Fixture();

            var testValue = fixture.Create<int>();

            // Act
            this.scaleupProducer.DaysInWholePeriod = testValue;

            // Assert
            Assert.AreEqual(testValue, this.scaleupProducer.DaysInWholePeriod);
        }
    }
}