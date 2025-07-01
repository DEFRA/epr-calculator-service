namespace EPR.Calculator.Service.Function.UnitTests.Exporter.JsonExporter
{
    using System;
    using AutoFixture;
    using EPR.Calculator.Service.Function.Exporter.JsonExporter.OnePlusFourApportionment;
    using EPR.Calculator.Service.Function.Mapper;
    using EPR.Calculator.Service.Function.Models;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class OnePlusFourApportionmentJsonExporterTests
    {
        private OnePlusFourApportionmentJsonExporter _testClass;
        private Mock<IOnePlusFourApportionmentMapper> _mapper;

        [TestInitialize]
        public void SetUp()
        {
            _mapper = new Mock<IOnePlusFourApportionmentMapper>();
            _testClass = new OnePlusFourApportionmentJsonExporter(_mapper.Object);
        }

        [TestMethod]
        public void CanConstruct()
        {
            // Act
            var instance = new OnePlusFourApportionmentJsonExporter(_mapper.Object);

            // Assert
            Assert.IsNotNull(instance);
        }

       

        [TestMethod]
        public void CanCallExport()
        {
            // Arrange
            var fixture = new Fixture();
            var calcResult1Plus4Apportionment = fixture.Create<CalcResultOnePlusFourApportionment>();
            _mapper.Setup(mock => mock.Map(It.IsAny<CalcResultOnePlusFourApportionment>())).Returns(fixture.Create<CalcResultOnePlusFourApportionmentJson>());

            // Act
            var result = _testClass.Export(calcResult1Plus4Apportionment);

            _mapper.Verify(mock => mock.Map(It.IsAny<CalcResultOnePlusFourApportionment>()));

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.OnePlusFourApportionment.OneFeeForLADisposalCosts);
            Assert.IsNotNull(result.OnePlusFourApportionment.FourLADataPrepCharge);
            Assert.IsNotNull(result.OnePlusFourApportionment.TotalOfonePlusFour);
            Assert.IsNotNull(result.OnePlusFourApportionment.OnePlusFourApportionmentPercentages);
        }
    }
}