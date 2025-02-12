namespace EPR.Calculator.Service.Function.UnitTests.Builder.ScaledupProducers
{
    using System;
    using AutoFixture;
    using EPR.Calculator.Service.Function.Builder.ScaledupProducers;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ScaleupProducerTests
    {
        private ScaleupProducer scaleupProducer;

        [TestInitialize]
        public void SetUp()
        {
            scaleupProducer = new ScaleupProducer();
        }

        [TestMethod]
        public void CanSetAndGetOrganisationId()
        {
            // Arrange
            var fixture = new Fixture();

            var testValue = fixture.Create<int>();

            // Act
            scaleupProducer.OrganisationId = testValue;

            // Assert
            Assert.AreEqual(testValue, scaleupProducer.OrganisationId);
        }

        [TestMethod]
        public void CanSetAndGetScaleupFactor()
        {
            // Arrange
            var fixture = new Fixture();

            var testValue = fixture.Create<decimal>();

            // Act
            scaleupProducer.ScaleupFactor = testValue;

            // Assert
            Assert.AreEqual(testValue, scaleupProducer.ScaleupFactor);
        }

        [TestMethod]
        public void CanSetAndGetSubmissionPeriod()
        {
            // Arrange
            var fixture = new Fixture();

            var testValue = fixture.Create<string>();

            // Act
            scaleupProducer.SubmissionPeriod = testValue;

            // Assert
            Assert.AreEqual(testValue, scaleupProducer.SubmissionPeriod);
        }

        [TestMethod]
        public void CanSetAndGetDaysInSubmissionPeriod()
        {
            // Arrange
            var fixture = new Fixture();

            var testValue = fixture.Create<int>();

            // Act
            scaleupProducer.DaysInSubmissionPeriod = testValue;

            // Assert
            Assert.AreEqual(testValue, scaleupProducer.DaysInSubmissionPeriod);
        }

        [TestMethod]
        public void CanSetAndGetDaysInWholePeriod()
        {
            // Arrange
            var fixture = new Fixture();

            var testValue = fixture.Create<int>();

            // Act
            scaleupProducer.DaysInWholePeriod = testValue;

            // Assert
            Assert.AreEqual(testValue, scaleupProducer.DaysInWholePeriod);
        }
    }
}