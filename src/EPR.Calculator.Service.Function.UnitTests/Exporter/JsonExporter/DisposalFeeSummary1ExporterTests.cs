namespace EPR.Calculator.Service.Function.UnitTests.Exporter.JsonExporter
{
    using AutoFixture;
    using EPR.Calculator.Service.Function.Exporter.JsonExporter;
    using EPR.Calculator.Service.Function.Models;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System.Globalization;
    using System.Text.Json;
    using System.Text.Json.Nodes;
    using static EPR.Calculator.Service.Common.UnitTests.Utils.JsonNodeComparer;


    [TestClass]
    public class DisposalFeeSummary1ExporterTests
    {
        private CalcResultSummaryProducerDisposalFeesExporter TestClass { get; init; }
        private IFixture Fixture { get; init; }

        public DisposalFeeSummary1ExporterTests()
        {
            Fixture = new Fixture();
            this.TestClass = new CalcResultSummaryProducerDisposalFeesExporter();
        }

        [TestMethod]
        public void CanCallExport()
        {
            // Arrange
            var summary = Fixture.Create<CalcResultSummaryProducerDisposalFees>();

            // Act
            var result = this.TestClass.Export(summary);

            // Assert
            Assert.IsNotNull(result);
        }

        /// <summary>
        /// Serialises a <see cref="CalcResultSummaryProducerDisposalFees"/>, then parses the resulting JSON
        /// and checks that the values still match up with the original.
        /// </summary>
        [TestMethod]
        public void Export_ValuesAreValid()
        {
            // Arrange
            var data = Fixture.Create<CalcResultSummaryProducerDisposalFees>();

            // Act
            var json = this.TestClass.Export(data);

            var roundTrippedData = JsonSerializer.Deserialize<JsonObject>(json)!
                ["disposalFeeSummary1"];


            // Assert
            Assert.IsNotNull(roundTrippedData);

            // Disposal Fee
            AssertAreEqual(data.TotalProducerDisposalFee,
                roundTrippedData["totalProducerDisposalFeeWithoutBadDebtProvision"]);
            AssertAreEqual(data.BadDebtProvision,
                roundTrippedData["badDebtProvision"]);
            AssertAreEqual(data.TotalProducerDisposalFeeWithBadDebtProvision,
                roundTrippedData["totalProducerDisposalFeeWithBadDebtProvision"]);

            // Countries
            AssertAreEqual(data.EnglandTotal,
                roundTrippedData["englandTotal"]);
            AssertAreEqual(data.WalesTotal,
                roundTrippedData["walesTotal"]);
            AssertAreEqual(data.ScotlandTotal,
                roundTrippedData["scotlandTotal"]);
            AssertAreEqual(data.NorthernIrelandTotal,
                roundTrippedData["northernIrelandTotal"]);

            // Tonnage Change
            AssertAreEqual(data.TonnageChangeCount,
                roundTrippedData["tonnageChangeCount"]);
            AssertAreEqual(data.TonnageChangeAdvice,
                roundTrippedData["tonnageChangeAdvice"]);

        }
    }
}