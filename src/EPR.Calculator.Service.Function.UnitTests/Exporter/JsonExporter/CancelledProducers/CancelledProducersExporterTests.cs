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
        private CancelledProducerExporter _testClass;
        private Mock<ICancelledProducerMapper> _testMapper;

        [TestInitialize]
        public void Setup()
        {
            _testMapper = new Mock<ICancelledProducerMapper>();
            _testClass = new CancelledProducerExporter(_testMapper.Object);
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
