using AutoFixture;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.Models;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.UnitTests.Models
{
    [TestClass]
    public class CalcResultProducerAndReportMaterialDetailTests
    {
        private readonly CalcResultProducerAndReportMaterialDetail calcResultProducerAndReportMaterialDetail;

        public CalcResultProducerAndReportMaterialDetailTests()
        {
            calcResultProducerAndReportMaterialDetail = new CalcResultProducerAndReportMaterialDetail
            {
                ProducerDetail = new ProducerDetail
                {
                    Id = 1,
                    ProducerName = "Allied Packaging",
                    CalculatorRunId = 1,
                    CalculatorRun = new CalculatorRun { RelativeYear = new RelativeYear(2024), Name = "Test Run 1" },
                },
                ProducerReportedMaterialProjected = new ProducerReportedMaterialProjected
                {
                    Material = new Material { Id = 1, Code = "AL", Name = "Aluminium", Description = "Aluminium" },
                    PackagingTonnage = 1000.00m,
                    PackagingType = "HH",
                    SubmissionPeriod = "2025-H1",
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
            calcResultProducerAndReportMaterialDetail.ProducerDetail = testValue;

            // Assert
            Assert.AreSame(testValue, calcResultProducerAndReportMaterialDetail.ProducerDetail);
        }

        [TestMethod]
        public void CanSetAndGetProducerReportedMaterial()
        {
            // Arrange
            var fixture = new Fixture();

            var testValue = fixture.Create<ProducerReportedMaterialProjected>();

            // Act
            calcResultProducerAndReportMaterialDetail.ProducerReportedMaterialProjected = testValue;

            // Assert
            Assert.AreSame(testValue, calcResultProducerAndReportMaterialDetail.ProducerReportedMaterialProjected);
        }
    }
}
