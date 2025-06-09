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
            Assert.AreSame(calcResultLateReportingTonnage.Name, result.Name);
            Assert.AreEqual(calcResultLateReportingTonnage.CalcResultLateReportingTonnageDetails.Count(), result.calcResultLateReportingTonnageDetails.Count);
            Assert.AreEqual(calcResultLateReportingTonnage.CalcResultLateReportingTonnageDetails.Sum(t=>t.TotalLateReportingTonnage), result.CalcResultLateReportingTonnageTotal);
        }
    }
}