namespace EPR.Calculator.Service.Function.UnitTests.Exporter
{
    using System;
    using System.Text;
    using AutoFixture;
    using EPR.Calculator.Service.Function.Exporter;
    using EPR.Calculator.Service.Function.Models;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class OnePlusFourApportionmentExporterTests
    {
        private OnePlusFourApportionmentExporter _testClass = new OnePlusFourApportionmentExporter();
        private CalcResultOnePlusFourApportionment calcResult1Plus4Apportionment;

        public OnePlusFourApportionmentExporterTests()
        {
            var fixture = new Fixture();
            this.calcResult1Plus4Apportionment = new CalcResultOnePlusFourApportionment
            {
                Name = "Apportionment",
                CalcResultOnePlusFourApportionmentDetails = new List<CalcResultOnePlusFourApportionmentDetail>()
                {
                    new CalcResultOnePlusFourApportionmentDetail
                    {
                        EnglandDisposalTotal = "210",
                        Name = "Name 1",
                        NorthernIrelandDisposalTotal = "110",
                        ScotlandDisposalTotal = "100",
                        WalesDisposalTotal = "110",
                        Total = "540",
                    },
                    new CalcResultOnePlusFourApportionmentDetail
                    {
                        EnglandDisposalTotal = "100",
                        Name = "Name 2",
                        NorthernIrelandDisposalTotal = "200",
                        ScotlandDisposalTotal = "300",
                        WalesDisposalTotal = "300",
                        Total = "900",
                    },
                },
            };
        }

        [TestMethod]
        public void CanCallExport() {
            // Arrange
            var csvContent = new StringBuilder();

            // Act
            this._testClass.Export(this.calcResult1Plus4Apportionment, csvContent);

            // Assert
            var result = csvContent.ToString();
            var rows = result.Split(Environment.NewLine);
            Assert.AreEqual(6, rows.Length);
            Assert.AreEqual("Apportionment", rows[2]);
        }
    }
}