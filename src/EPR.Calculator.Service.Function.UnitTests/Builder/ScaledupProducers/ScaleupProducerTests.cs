namespace EPR.Calculator.Service.Function.UnitTests.Builder.ScaledupProducers
{
    using System;
    using AutoFixture;
    using EPR.Calculator.Service.Function.Builder.ScaledupProducers;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ScaleupProducerTests
    {
        private ScaleupProducer _testClass;

        [TestInitialize]
        public void SetUp()
        {
            _testClass = new ScaleupProducer();
        }

        [TestMethod]
        public void CanSetAndGetOrganisationId()
        {
            // Arrange
            var fixture = new Fixture();

            var testValue = fixture.Create<int>();

            // Act
            _testClass.OrganisationId = testValue;

            // Assert
            Assert.AreEqual(testValue, _testClass.OrganisationId);
        }

        [TestMethod]
        public void CanSetAndGetScaleupFactor()
        {
            // Arrange
            var fixture = new Fixture();

            var testValue = fixture.Create<decimal>();

            // Act
            _testClass.ScaleupFactor = testValue;

            // Assert
            Assert.AreEqual(testValue, _testClass.ScaleupFactor);
        }

        [TestMethod]
        public void CanSetAndGetSubmissionPeriod()
        {
            // Arrange
            var fixture = new Fixture();

            var testValue = fixture.Create<string>();

            // Act
            _testClass.SubmissionPeriod = testValue;

            // Assert
            Assert.AreEqual(testValue, _testClass.SubmissionPeriod);
        }

        [TestMethod]
        public void CanSetAndGetDaysInSubmissionPeriod()
        {
            // Arrange
            var fixture = new Fixture();

            var testValue = fixture.Create<int>();

            // Act
            _testClass.DaysInSubmissionPeriod = testValue;

            // Assert
            Assert.AreEqual(testValue, _testClass.DaysInSubmissionPeriod);
        }

        [TestMethod]
        public void CanSetAndGetDaysInWholePeriod()
        {
            // Arrange
            var fixture = new Fixture();

            var testValue = fixture.Create<int>();

            // Act
            _testClass.DaysInWholePeriod = testValue;

            // Assert
            Assert.AreEqual(testValue, _testClass.DaysInWholePeriod);
        }
    }
}