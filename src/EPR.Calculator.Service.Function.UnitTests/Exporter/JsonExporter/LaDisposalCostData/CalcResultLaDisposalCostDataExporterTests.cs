namespace EPR.Calculator.Service.Function.UnitTests.Exporter.JsonExporter.CommsCostByMaterial2A
{
    using System;
    using System.Collections.Generic;
    using AutoFixture;
    using EPR.Calculator.Service.Function.Exporter.JsonExporter.CommsCostByMaterial2A;
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

            _mapper.Setup(mock => mock.Map(It.IsAny<List<CalcResultLaDisposalCostDataDetail>>())).Returns(fixture.Create<CalcResultLaDisposalCostDataJson>());

            // Act
            var result = _testClass.Export(laDisposalCostData);

            // Assert
            _mapper.Verify(mock => mock.Map(It.IsAny<List<CalcResultLaDisposalCostDataDetail>>()));

            Assert.IsNotNull(result);
        }
    }
}