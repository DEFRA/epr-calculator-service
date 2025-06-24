namespace EPR.Calculator.Service.Function.UnitTests.Mapper
{
    using System;
    using System.Collections.Generic;
    using AutoFixture;
    using EPR.Calculator.Service.Function.Mapper;
    using EPR.Calculator.Service.Function.Models;
    using EPR.Calculator.Service.Function.Models.JsonExporter;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class CalcResultLaDisposalCostDataMapperTests
    {
        private  CalcResultLaDisposalCostDataMapper _testClass;
        List<CalcResultLaDisposalCostDataDetail> laDisposalCostDataDetail = new List<CalcResultLaDisposalCostDataDetail>();

        [TestInitialize]
        public void SetUp()
        {
            _testClass = new CalcResultLaDisposalCostDataMapper();
            laDisposalCostDataDetail.Add(new CalcResultLaDisposalCostDataDetail()
            {
                Name = "Aluminium",
                England = "England",
                Wales = "Wales",
                NorthernIreland = "NorthernIreland",
                Scotland = "Scotland",
                ProducerReportedHouseholdPackagingWasteTonnage = "Test",
                ReportedPublicBinTonnage = "Test",
                Total = "150.23",
                DisposalCostPricePerTonne = "550.65",
                HouseholdDrinkContainers = "250.44",
                LateReportingTonnage = "40.67",
                Material = "Aluminium",
                OrderId = 1,
                ProducerReportedTotalTonnage = "45.99",
                TotalReportedTonnage = "45.56"
            });
        }

        [TestMethod]
        public void CanCallMap()
        {
            // Act
            var result = _testClass.Map(laDisposalCostDataDetail);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.CalcResultLaDisposalCostDetails.Count());
        }

        [TestMethod]
        public void CanCallGetMaterialBreakdown()
        {
            // Act
            var result = _testClass.GetMaterialBreakdown(laDisposalCostDataDetail);

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void CanCallGetTotalRow()
        {
            //Arrange
            laDisposalCostDataDetail.Add(new CalcResultLaDisposalCostDataDetail()
            {
                Name = "Totals",
                England = "England",
                Wales = "Wales",
                NorthernIreland = "NorthernIreland",
                Scotland = "Scotland",
                ProducerReportedHouseholdPackagingWasteTonnage = "Test",
                ReportedPublicBinTonnage = "Test",
                Total = "150.23",
                DisposalCostPricePerTonne = "550.65",
                HouseholdDrinkContainers = "250.44",
                LateReportingTonnage = "40.67",
                Material = "Aluminium",
                OrderId = 1,
                ProducerReportedTotalTonnage = "45.99",
                TotalReportedTonnage = "45.56"
            });

            // Act
            var result = _testClass.GetTotalRow(laDisposalCostDataDetail);

            // Assert
            Assert.IsNotNull(result);
        }
    }
}