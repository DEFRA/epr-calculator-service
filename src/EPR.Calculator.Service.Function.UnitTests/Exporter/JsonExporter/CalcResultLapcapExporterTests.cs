namespace EPR.Calculator.Service.Function.UnitTests.Exporter.JsonExporter
{
    using AutoFixture;
    using EPR.Calculator.Service.Function.Builder.Lapcap;
    using EPR.Calculator.Service.Function.Exporter.JsonExporter.Lapcap;
    using EPR.Calculator.Service.Function.Models;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;

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
            var records = Fixture.CreateMany<CalcResultLapcapDataDetails>().ToList();
            
            var totalRecord = Fixture.Create<CalcResultLapcapDataDetails>();
            totalRecord.Name = CalcResultLapcapDataBuilder.Total;
            records.Add(totalRecord);

            var apportionmentRecord = Fixture.Create<CalcResultLapcapDataDetails>();
            apportionmentRecord.Name = CalcResultLapcapDataBuilder.CountryApportionment;
            records.Add(apportionmentRecord);

            var data = new CalcResultLapcapData
            {
                Name = Fixture.Create<string>(),
                CalcResultLapcapDataDetails = records,
            };
           
            // Act
            var result = TestClass.Export(data);

            // Assert
            Assert.AreNotEqual(string.Empty, result);
        }
    }
}