using AutoFixture;
using EPR.Calculator.Service.Function.Exporter.JsonExporter.CancelledProducers;
using EPR.Calculator.Service.Function.Mapper;
using EPR.Calculator.Service.Function.Models;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPR.Calculator.Service.Function.UnitTests.Exporter.JsonExporter.CancelledProducers
{
    [TestClass]
    public class CancelledProducersExporterTests
    {
        private CancelledProducersExporter _testClass;
        private Mock<ICancelledProducersMapper> _testMapper;

        public CancelledProducersExporterTests()
        {
            _testMapper = new Mock<ICancelledProducersMapper>();
            _testClass = new CancelledProducersExporter(_testMapper.Object);
        }

        [TestMethod]
        public void CanCallExport()
        {
            // Arrange
            var fixture = new Fixture();
            var cancelledProducers = fixture.Create<CalcResultCancelledProducersResponse>();

            _testMapper.Setup(mock => mock.Map(It.IsAny<CalcResultCancelledProducersResponse>()));

            // Act
            var result = _testClass.Export(cancelledProducers);

            // Assert
            _testMapper.Verify(mock => mock.Map(It.IsAny<CalcResultCancelledProducersResponse>()));

            Assert.AreNotEqual(string.Empty, result);
        }
    }
}
