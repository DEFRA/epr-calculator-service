namespace EPR.Calculator.Service.Function.UnitTests.Mapper
{
    using System;
    using AutoFixture;
    using EPR.Calculator.Service.Function.Mapper;
    using EPR.Calculator.Service.Function.Models;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class LateReportingTonnageMapperTests
    {
        private LateReportingTonnageMapper _testClass;

        [TestInitialize]
        public void SetUp()
        {
            _testClass = new LateReportingTonnageMapper();
        }

        [TestMethod]
        public void CanCallMap()
        {
            // Arrange
            var fixture = new Fixture();
            var calcResultLateReportingTonnage = fixture.Create<CalcResultLateReportingTonnage>();

            // Act
            var result = ((ILateReportingTonnageMapper)_testClass).Map(calcResultLateReportingTonnage);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.CalcResultLateReportingTonnageTotal);
        }

        [TestMethod]
        public void CannotCallMapWithNullCalcResultLateReportingTonnage()
        {
            var fixture = new Fixture();


            // Act
            var result = ((ILateReportingTonnageMapper)_testClass).Map(default(CalcResultLateReportingTonnage));
            Assert.IsNotNull(result);
            Assert.IsNull(result.calcResultLateReportingTonnageDetails);
        }

        [TestMethod]
        public void MapPerformsMapping()
        {
            // Arrange
            var fixture = new Fixture();
            var calcResultLateReportingTonnage = fixture.Create<CalcResultLateReportingTonnage>();

            // Act
            var result = ((ILateReportingTonnageMapper)_testClass).Map(calcResultLateReportingTonnage);

            // Assert            
            Assert.AreEqual(calcResultLateReportingTonnage.CalcResultLateReportingTonnageDetails.Count(), result.calcResultLateReportingTonnageDetails.Count);
            Assert.AreEqual(calcResultLateReportingTonnage.CalcResultLateReportingTonnageDetails.Sum(t => t.TotalLateReportingTonnage), result.CalcResultLateReportingTonnageTotal);
        }        

        [TestMethod]
        public void Map_ShouldMapDetailsCorrectly_WhenInputIsValid()
        {
            // Arrange  
            var input = new CalcResultLateReportingTonnage
            {
                CalcResultLateReportingTonnageDetails = new List<CalcResultLateReportingTonnageDetail>
               {
                   new() { Name = "Material1", TotalLateReportingTonnage = 10.5m },
                   new() { Name = "Material2", TotalLateReportingTonnage = 20.3m },
                   new() { Name = "Total", TotalLateReportingTonnage = 30.8m }
               }
            };

            // Act  
            var result = ((ILateReportingTonnageMapper)_testClass).Map(input);

            // Assert  
            Assert.IsNotNull(result);
            Assert.AreEqual("Late Reporting Tonnage", result.Name);
            Assert.AreEqual(2, result.calcResultLateReportingTonnageDetails.Count);
            Assert.IsTrue(result.calcResultLateReportingTonnageDetails.Any(d => d.MaterialName == "Material1" && d.TotalLateReportingTonnage == 10.5m));
            Assert.IsTrue(result.calcResultLateReportingTonnageDetails.Any(d => d.MaterialName == "Material2" && d.TotalLateReportingTonnage == 20.3m));
            Assert.AreEqual(30.8m, result.CalcResultLateReportingTonnageTotal);
        }
    }
}