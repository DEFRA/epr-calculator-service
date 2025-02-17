namespace EPR.Calculator.Service.Function.UnitTests.Models
{
    using AutoFixture;
    using EPR.Calculator.Service.Function.Models;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class CalcResultScaledupProducerTonnageTests
    {
        private CalcResultScaledupProducerTonnage calcResultScaledupProducerTonnage = null!;

        [TestInitialize]
        public void SetUp()
        {
            this.calcResultScaledupProducerTonnage = new CalcResultScaledupProducerTonnage();
        }

        [TestMethod]
        public void CanSetAndGetReportedHouseholdPackagingWasteTonnage()
        {
            // Arrange
            var fixture = new Fixture();

            var testValue = fixture.Create<decimal>();

            // Act
            this.calcResultScaledupProducerTonnage.ReportedHouseholdPackagingWasteTonnage = testValue!;

            // Assert
            Assert.AreEqual(testValue, this.calcResultScaledupProducerTonnage.ReportedHouseholdPackagingWasteTonnage);
        }

        [TestMethod]
        public void CanSetAndGetReportedPublicBinTonnage()
        {
            // Arrange
            var fixture = new Fixture();

            var testValue = fixture.Create<decimal>();

            // Act
            this.calcResultScaledupProducerTonnage.ReportedPublicBinTonnage = testValue;

            // Assert
            Assert.AreEqual(testValue, this.calcResultScaledupProducerTonnage.ReportedPublicBinTonnage);
        }

        [TestMethod]
        public void CanSetAndGetTotalReportedTonnage()
        {
            // Arrange
            var fixture = new Fixture();

            var testValue = fixture.Create<decimal>();

            // Act
            this.calcResultScaledupProducerTonnage.TotalReportedTonnage = testValue;

            // Assert
            Assert.AreEqual(testValue, this.calcResultScaledupProducerTonnage.TotalReportedTonnage);
        }

        [TestMethod]
        public void CanSetAndGetReportedSelfManagedConsumerWasteTonnage()
        {
            // Arrange
            var fixture = new Fixture();

            var testValue = fixture.Create<decimal>();

            // Act
            this.calcResultScaledupProducerTonnage.ReportedSelfManagedConsumerWasteTonnage = testValue;

            // Assert
            Assert.AreEqual(testValue, this.calcResultScaledupProducerTonnage.ReportedSelfManagedConsumerWasteTonnage);
        }

        [TestMethod]
        public void CanSetAndGetNetReportedTonnage()
        {
            // Arrange
            var fixture = new Fixture();

            var testValue = fixture.Create<decimal>();

            // Act
            this.calcResultScaledupProducerTonnage.NetReportedTonnage = testValue;

            // Assert
            Assert.AreEqual(testValue, this.calcResultScaledupProducerTonnage.NetReportedTonnage);
        }

        [TestMethod]
        public void CanSetAndGetScaledupReportedHouseholdPackagingWasteTonnage()
        {
            // Arrange
            var fixture = new Fixture();

            var testValue = fixture.Create<decimal>();

            // Act
            this.calcResultScaledupProducerTonnage.ScaledupReportedHouseholdPackagingWasteTonnage = testValue;

            // Assert
            Assert.AreEqual(testValue, this.calcResultScaledupProducerTonnage.ScaledupReportedHouseholdPackagingWasteTonnage);
        }

        [TestMethod]
        public void CanSetAndGetScaledupReportedPublicBinTonnage()
        {
            // Arrange
            var fixture = new Fixture();

            var testValue = fixture.Create<decimal>();

            // Act
            this.calcResultScaledupProducerTonnage.ScaledupReportedPublicBinTonnage = testValue;

            // Assert
            Assert.AreEqual(testValue, this.calcResultScaledupProducerTonnage.ScaledupReportedPublicBinTonnage);
        }

        [TestMethod]
        public void CanSetAndGetScaledupTotalReportedTonnage()
        {
            // Arrange
            var fixture = new Fixture();

            var testValue = fixture.Create<decimal>();

            // Act
            this.calcResultScaledupProducerTonnage.ScaledupTotalReportedTonnage = testValue;

            // Assert
            Assert.AreEqual(testValue, this.calcResultScaledupProducerTonnage.ScaledupTotalReportedTonnage);
        }

        [TestMethod]
        public void CanSetAndGetScaledupReportedSelfManagedConsumerWasteTonnage()
        {
            // Arrange
            var fixture = new Fixture();

            var testValue = fixture.Create<decimal>();

            // Act
            this.calcResultScaledupProducerTonnage.ScaledupReportedSelfManagedConsumerWasteTonnage = testValue;

            // Assert
            Assert.AreEqual(testValue, this.calcResultScaledupProducerTonnage.ScaledupReportedSelfManagedConsumerWasteTonnage);
        }

        [TestMethod]
        public void CanSetAndGetScaledupNetReportedTonnage()
        {
            // Arrange
            var fixture = new Fixture();

            var testValue = fixture.Create<decimal>();

            // Act
            this.calcResultScaledupProducerTonnage.ScaledupNetReportedTonnage = testValue;

            // Assert
            Assert.AreEqual(testValue, this.calcResultScaledupProducerTonnage.ScaledupNetReportedTonnage);
        }
    }
}