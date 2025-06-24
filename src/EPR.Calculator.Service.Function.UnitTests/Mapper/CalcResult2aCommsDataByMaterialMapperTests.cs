namespace EPR.Calculator.Service.Function.UnitTests.Mapper
{
    using System;
    using System.Collections.Generic;
    using AutoFixture;
    using EPR.Calculator.Service.Function.Mapper;
    using EPR.Calculator.Service.Function.Models;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class CalcResult2aCommsDataByMaterialMapperTests
    {
        private CalcResult2ACommsDataByMaterialMapper? _testClass;

        [TestInitialize]
        public void SetUp()
        {
            _testClass = new CalcResult2ACommsDataByMaterialMapper();
        }

        [TestMethod]
        public void CanCallMap()
        {
            // Arrange
            var commsCostByMaterial = GetCommsCostMaterialData();

            // Act
            var result = _testClass?.Map(commsCostByMaterial);

            // Assert
            Assert.IsNotNull(result);
        }        

        [TestMethod]
        public void CanCallGetMaterialBreakdown()
        {
            // Arrange
            var fixture = new Fixture();
            var commsCostByMaterial = fixture.Create<List<CalcResultCommsCostCommsCostByMaterial>>();

            // Act
            var result = _testClass?.GetMaterialBreakdown(commsCostByMaterial);

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void CanCallGetTotalRow()
        {
            // Arrange
            var fixture = new Fixture();
            var commsCostByMaterial = fixture.Create<CalcResultCommsCostCommsCostByMaterial>();

            // Act
            var result = _testClass?.GetTotalRow(commsCostByMaterial);

            // Assert
            Assert.IsNotNull(result);
        }

        private static List<CalcResultCommsCostCommsCostByMaterial> GetCommsCostMaterialData()
        {
            // Arrange           
            return new List<CalcResultCommsCostCommsCostByMaterial>
                    {
                        new CalcResultCommsCostCommsCostByMaterial
                        {
                            CommsCostByMaterialPricePerTonne = "0.42",
                            Name = "Aluminium",
                            ProducerReportedHouseholdPackagingWasteTonnageValue = 100.25m,
                            CommsCostByMaterialPricePerTonneValue = 100,
                            EnglandValue = 100,
                            ScotlandValue = 200,
                            WalesValue = 300,
                            NorthernIrelandValue = 400,
                            HouseholdDrinksContainersValue = 100.85m,
                            ProducerReportedTotalTonnage = 100.67m,
                            ReportedPublicBinTonnageValue =100.55m,
                            TotalValue = 1000,
                            LateReportingTonnageValue =100
                        },
                        new CalcResultCommsCostCommsCostByMaterial
                        {
                            CommsCostByMaterialPricePerTonne = "0.3",
                            Name = "Glass",
                            ProducerReportedHouseholdPackagingWasteTonnageValue = 200,
                            CommsCostByMaterialPricePerTonneValue = 200,
                            EnglandValue = 100,
                            ScotlandValue = 200,
                            WalesValue = 300,
                            NorthernIrelandValue = 400,
                            HouseholdDrinksContainersValue = 100,
                            ProducerReportedTotalTonnage = 100,
                            ReportedPublicBinTonnageValue =100,
                            TotalValue = 1000,
                            LateReportingTonnageValue =100
                        },
                         new CalcResultCommsCostCommsCostByMaterial
                        {
                            CommsCostByMaterialPricePerTonne = "0.72",
                            Name = "Total",
                            ProducerReportedHouseholdPackagingWasteTonnageValue = 300,
                            CommsCostByMaterialPricePerTonneValue = 300,
                            EnglandValue = 200,
                            ScotlandValue = 400,
                            WalesValue = 600,
                            NorthernIrelandValue = 800,
                            HouseholdDrinksContainersValue = 200,
                            ProducerReportedTotalTonnage = 200,
                            ReportedPublicBinTonnageValue =200,
                            TotalValue = 2000,
                            LateReportingTonnageValue =200
                        },
            };
        }
    }
}