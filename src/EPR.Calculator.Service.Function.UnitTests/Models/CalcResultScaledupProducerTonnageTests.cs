namespace EPR.Calculator.Service.Function.UnitTests.Models
{
    using AutoFixture;
    using EPR.Calculator.Service.Function.Models;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class CalcResultScaledupProducerTonnageTests
    {
        private CalcResultScaledupProducerTonnage calcResultScaledupProducerTonnage;

        [TestInitialize]
        public void SetUp()
        {
            calcResultScaledupProducerTonnage = new CalcResultScaledupProducerTonnage();
        }

        [TestMethod]
        public void CanSetAndGetReportedHouseholdPackagingWasteTonnage()
        {
            // Arrange
            var fixture = new Fixture();

            var testValue = fixture.Create<decimal>();

            // Act
            calcResultScaledupProducerTonnage.ReportedHouseholdPackagingWasteTonnage = testValue;

            // Assert
            Assert.AreEqual(testValue, calcResultScaledupProducerTonnage.ReportedHouseholdPackagingWasteTonnage);
        }

        [TestMethod]
        public void CanSetAndGetReportedPublicBinTonnage()
        {
            // Arrange
            var fixture = new Fixture();

            var testValue = fixture.Create<decimal>();

            // Act
            calcResultScaledupProducerTonnage.ReportedPublicBinTonnage = testValue;

            // Assert
            Assert.AreEqual(testValue, calcResultScaledupProducerTonnage.ReportedPublicBinTonnage);
        }

        [TestMethod]
        public void CanSetAndGetTotalReportedTonnage()
        {
            // Arrange
            var fixture = new Fixture();

            var testValue = fixture.Create<decimal>();

            // Act
            calcResultScaledupProducerTonnage.TotalReportedTonnage = testValue;

            // Assert
            Assert.AreEqual(testValue, calcResultScaledupProducerTonnage.TotalReportedTonnage);
        }

        [TestMethod]
        public void CanSetAndGetReportedSelfManagedConsumerWasteTonnage()
        {
            // Arrange
            var fixture = new Fixture();

            var testValue = fixture.Create<decimal>();

            // Act
            calcResultScaledupProducerTonnage.ReportedSelfManagedConsumerWasteTonnage = testValue;

            // Assert
            Assert.AreEqual(testValue, calcResultScaledupProducerTonnage.ReportedSelfManagedConsumerWasteTonnage);
        }

        [TestMethod]
        public void CanSetAndGetNetReportedTonnage()
        {
            // Arrange
            var fixture = new Fixture();

            var testValue = fixture.Create<decimal>();

            // Act
            calcResultScaledupProducerTonnage.NetReportedTonnage = testValue;

            // Assert
            Assert.AreEqual(testValue, calcResultScaledupProducerTonnage.NetReportedTonnage);
        }

        [TestMethod]
        public void CanSetAndGetScaledupReportedHouseholdPackagingWasteTonnage()
        {
            // Arrange
            var fixture = new Fixture();

            var testValue = fixture.Create<decimal>();

            // Act
            calcResultScaledupProducerTonnage.ScaledupReportedHouseholdPackagingWasteTonnage = testValue;

            // Assert
            Assert.AreEqual(testValue, calcResultScaledupProducerTonnage.ScaledupReportedHouseholdPackagingWasteTonnage);
        }

        [TestMethod]
        public void CanSetAndGetScaledupReportedPublicBinTonnage()
        {
            // Arrange
            var fixture = new Fixture();

            var testValue = fixture.Create<decimal>();

            // Act
            calcResultScaledupProducerTonnage.ScaledupReportedPublicBinTonnage = testValue;

            // Assert
            Assert.AreEqual(testValue, calcResultScaledupProducerTonnage.ScaledupReportedPublicBinTonnage);
        }

        [TestMethod]
        public void CanSetAndGetScaledupTotalReportedTonnage()
        {
            // Arrange
            var fixture = new Fixture();

            var testValue = fixture.Create<decimal>();

            // Act
            calcResultScaledupProducerTonnage.ScaledupTotalReportedTonnage = testValue;

            // Assert
            Assert.AreEqual(testValue, calcResultScaledupProducerTonnage.ScaledupTotalReportedTonnage);
        }

        [TestMethod]
        public void CanSetAndGetScaledupReportedSelfManagedConsumerWasteTonnage()
        {
            // Arrange
            var fixture = new Fixture();

            var testValue = fixture.Create<decimal>();

            // Act
            calcResultScaledupProducerTonnage.ScaledupReportedSelfManagedConsumerWasteTonnage = testValue;

            // Assert
            Assert.AreEqual(testValue, calcResultScaledupProducerTonnage.ScaledupReportedSelfManagedConsumerWasteTonnage);
        }

        [TestMethod]
        public void CanSetAndGetScaledupNetReportedTonnage()
        {
            // Arrange
            var fixture = new Fixture();

            var testValue = fixture.Create<decimal>();

            // Act
            calcResultScaledupProducerTonnage.ScaledupNetReportedTonnage = testValue;

            // Assert
            Assert.AreEqual(testValue, calcResultScaledupProducerTonnage.ScaledupNetReportedTonnage);
        }
    }
}