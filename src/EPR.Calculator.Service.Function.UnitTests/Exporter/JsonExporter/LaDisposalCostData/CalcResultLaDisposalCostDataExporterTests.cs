namespace EPR.Calculator.Service.Function.UnitTests.Exporter.JsonExporter.CommsCostByMaterial2A
{
    using System;
    using System.Collections.Generic;
    using AutoFixture;
    using EPR.Calculator.Service.Function.Constants;
    using EPR.Calculator.Service.Function.Exporter.JsonExporter.LaDisposalCostData;
    using EPR.Calculator.Service.Function.Mapper;
    using EPR.Calculator.Service.Function.Models;
    using EPR.Calculator.Service.Function.Models.JsonExporter;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class CalcResultLaDisposalCostDataExporterTests
    {
        private CalcResultLaDisposalCostDataExporter _testClass;
        private Mock<ICalcResultLaDisposalCostDataMapper> _mapper;

        [TestInitialize]
        public void SetUp()
        {
            _mapper = new Mock<ICalcResultLaDisposalCostDataMapper>();
            _testClass = new CalcResultLaDisposalCostDataExporter(_mapper.Object);
        }

        [TestMethod]
        public void CanConstruct()
        {
            // Act
            var instance = new CalcResultLaDisposalCostDataExporter(_mapper.Object);

            // Assert
            Assert.IsNotNull(instance);
        }

        [TestMethod]
        public void CanCallExport()
        {
            // Arrange
            var fixture = new Fixture();
            var laDisposalCostData = fixture.Create<List<CalcResultLaDisposalCostDataDetail>>();

            var CalcResultLaDisposalCostDataJson = new CalcResultLaDisposalCostDataJson()
            {
                Name = CommonConstants.LADisposalCostData,
                CalcResultLaDisposalCostDetails = new List<CalcResultLaDisposalCostDetails>()
                {
                    new CalcResultLaDisposalCostDetails()
                    {
                        MaterialName = "Aluminium",
                        EnglandLaDisposalCost = "£45,000.00",
                        WalesLaDisposalCost = "£0.00",
                        NorthernIrelandLaDisposalCost = "£20,700.00",
                        ScotlandLaDisposalCost = "£4,500.00",
                        ProducerHouseholdPackagingWasteTonnage = 3899.999M,
                        PublicBinTonnage = 666.3M,
                        TotalLaDisposalCost = "£70,200.00",
                        DisposalCostPricePerTonne = "£6.3396",
                        HouseholdDrinksContainersTonnage = 507M,
                        LateReportingTonnage = 6000M,
                        TotalTonnage =11073.299M,
                    }
                },
                CalcResultLaDisposalCostDataDetailsTotal = new CalcResultLaDisposalCostDataDetailsTotal()
                {
                    EnglandLaDisposalCostTotal = "£109,800.00",
                    HouseholdDrinksContainersTonnageTotal = 507M,
                    LateReportingTonnageTotal = 36000M,
                    NorthernIrelandLaDisposalCostTotal = "£19,300.00",
                    ProducerHouseholdPackagingWasteTonnageTotal = 55404.996M,
                    PublicBinTonnage = 10947.62M,
                    ScotlandLaDisposalCostTotal = "£49,300.00",
                    Total = "Total",
                    TotalLaDisposalCostTotal = "£203,150.00",
                    TotalTonnageTotal = 102859.616M,
                    WalesLaDisposalCostTotal = "£24,750.00"
                }
            };

            _mapper.Setup(mock => mock.Map(It.IsAny<List<CalcResultLaDisposalCostDataDetail>>())).Returns(CalcResultLaDisposalCostDataJson);

            // Act
            var result = _testClass.Export(laDisposalCostData);

            // Assert
            _mapper.Verify(mock => mock.Map(It.IsAny<List<CalcResultLaDisposalCostDataDetail>>()));

            Assert.IsNotNull(result);
        }
    }
}