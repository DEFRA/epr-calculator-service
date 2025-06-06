namespace EPR.Calculator.Service.Function.UnitTests.Models
{
    using AutoFixture;
    using EPR.Calculator.API.Data.DataModels;
    using EPR.Calculator.Service.Function.Models;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class CalcResultProducerAndReportMaterialDetailTests
    {
        private readonly CalcResultProducerAndReportMaterialDetail calcResultProducerAndReportMaterialDetail;

        public CalcResultProducerAndReportMaterialDetailTests()
        {
            var calculatorRunFinancialYear = new CalculatorRunFinancialYear { Name = "2024-25" };
            this.calcResultProducerAndReportMaterialDetail = new CalcResultProducerAndReportMaterialDetail
            {
                ProducerDetail = new ProducerDetail
                {
                    Id = 1,
                    ProducerName = "Allied Packaging",
                    CalculatorRunId = 1,
                    CalculatorRun = new CalculatorRun { Financial_Year = calculatorRunFinancialYear, Name = "Test Run 1" },
                },
                ProducerReportedMaterial = new ProducerReportedMaterial
                {
                    Material = new Material { Id = 1, Code = "AL", Name = "Aluminium", Description = "Aluminium" },
                    PackagingTonnage = 1000.00m,
                    PackagingType = "HH",
                    MaterialId = 1,
                    ProducerDetail = null,
                },
            };
        }

        [TestMethod]
        public void CanSetAndGetProducerDetail()
        {
            // Arrange
            var fixture = new Fixture();

            var testValue = fixture.Create<ProducerDetail>();

            // Act
            this.calcResultProducerAndReportMaterialDetail.ProducerDetail = testValue;

            // Assert
            Assert.AreSame(testValue, this.calcResultProducerAndReportMaterialDetail.ProducerDetail);
        }

        [TestMethod]
        public void CanSetAndGetProducerReportedMaterial()
        {
            // Arrange
            var fixture = new Fixture();

            var testValue = fixture.Create<ProducerReportedMaterial>();

            // Act
            this.calcResultProducerAndReportMaterialDetail.ProducerReportedMaterial = testValue;

            // Assert
            Assert.AreSame(testValue, this.calcResultProducerAndReportMaterialDetail.ProducerReportedMaterial);
        }
    }
}