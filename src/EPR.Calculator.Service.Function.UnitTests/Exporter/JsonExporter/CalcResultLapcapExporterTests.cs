namespace EPR.Calculator.Service.Function.UnitTests.Exporter.CsvExporter.JsonExporter
{
    using System;
    using AutoFixture;
    using EPR.Calculator.Service.Function.Exporter.JsonExporter.Lapcap;
    using EPR.Calculator.Service.Function.Models;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class CalcResultLapcapExporterTests
    {
        private CalcResultLapcapExporter TestClass;
        private IFixture Fixture;

        public CalcResultLapcapExporterTests()
        {
            Fixture = new Fixture();
            TestClass = new CalcResultLapcapExporter();
        }

        [TestMethod]
        public void CanCallExport()
        {
            // Arrange
            var data = Fixture.Create<CalcResultLapcapData>();

            // Act
            var result = TestClass.ConvertToJson(data);

            // Assert
            Assert.AreNotEqual(string.Empty, result);
        }
    }
}