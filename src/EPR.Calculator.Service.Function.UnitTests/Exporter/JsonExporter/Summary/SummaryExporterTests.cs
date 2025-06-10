namespace EPR.Calculator.Service.Function.UnitTests.Exporter.JsonExporter.CalcResult
{
    using System;
    using AutoFixture;
    using EPR.Calculator.Service.Function.Exporter.JsonExporter.CalcResult;
    using EPR.Calculator.Service.Function.Models;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class SummaryExporterTests
    {
        private SummaryExporter TestClass { get; init; }

        private IFixture Fixture { get; init; }

        public SummaryExporterTests()
        {
            Fixture = new Fixture();
            this.TestClass = new SummaryExporter();
        }

        [TestMethod]
        public void CanCallConvertToJson()
        {
            // Arrange
            var data = Fixture.Create<CalcResultSummary>();

            // Act
            var result = this.TestClass.ConvertToJson(data);

            // Assert
            Assert.IsNotNull(result);
        }
    }
}