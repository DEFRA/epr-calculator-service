namespace EPR.Calculator.Service.Function.UnitTests.Exporter.JsonExporter
{
    using System;
    using AutoFixture;
    using EPR.Calculator.Service.Function.Exporter.JsonExporter;
    using EPR.Calculator.Service.Function.Mapper;
    using EPR.Calculator.Service.Function.Models;
    using EPR.Calculator.Service.Function.Models.JsonExporter;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class LateReportingTonnageTests
    {
        private LateReportingTonnage _testClass;
        private Mock<ILateReportingTonnageMapper> _lateReportingMapper;

        [TestInitialize]
        public void SetUp()
        {
            _lateReportingMapper = new Mock<ILateReportingTonnageMapper>();
            _testClass = new LateReportingTonnage(_lateReportingMapper.Object);
        }

        [TestMethod]
        public void CanCallExport()
        {
            // Arrange
            var fixture = new Fixture();
            var calcResultLateReportingData = fixture.Create<CalcResultLateReportingTonnage>();

            _lateReportingMapper.Setup(mock => mock.Map(It.IsAny<CalcResultLateReportingTonnage>())).Returns(fixture.Create<CalcResultLateReportingTonnageJson>());

            // Act
            var result = _testClass.Export(calcResultLateReportingData);

            // Assert
            _lateReportingMapper.Verify(mock => mock.Map(It.IsAny<CalcResultLateReportingTonnage>()));

            Assert.AreNotEqual(string.Empty, result);
        }
    }
}