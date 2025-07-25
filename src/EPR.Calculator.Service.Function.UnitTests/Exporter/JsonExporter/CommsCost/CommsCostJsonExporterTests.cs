﻿using EPR.Calculator.Service.Function.Exporter.JsonExporter;
using EPR.Calculator.Service.Function.Mapper;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Models.JsonExporter;
using Moq;

namespace EPR.Calculator.Service.Function.UnitTests.Exporter.JsonExporter.CommsCost
{
    [TestClass]
    public class CommsCostJsonExporterTests
    {
        private Mock<ICommsCostMapper> mockMapper;
        private CommsCostJsonExporter exporter;

        public CommsCostJsonExporterTests()
        {
            mockMapper = new Mock<ICommsCostMapper>();
            exporter = new CommsCostJsonExporter(mockMapper.Object);
        }

        [TestMethod]
        public void Export_ShouldReturnSerializedJson()
        {
            // Arrange
            var input = new CalcResultCommsCost { };
            var mappedResult = new CalcResultCommsCostJson
            {
                OnePlusFourCommsCostApportionmentPercentages = new OnePlusFourCommsCostApportionmentPercentages
                {
                    England = "52.49321900%",
                    Wales = "13.24848700%",
                    Scotland = "24.32714400%",
                    NorthernIreland = "9.93115000%",
                    Total = "100.00000000%"
                }
            };

            mockMapper.Setup(m => m.Map(input)).Returns(mappedResult);

            var expectedJson = mappedResult;

            // Act
            var actualJson = exporter.Export(input);

            // Assert
            Assert.AreEqual(expectedJson, actualJson);
            mockMapper.Verify(m => m.Map(input), Times.Once);
        }

        [TestMethod]
        public void Export_ShouldReturnSerializedJson_WhenMapperReturnsValidObject()
        {
            // Arrange  
            var communicationCost = new CalcResultCommsCost();
            var mappedResult = new CalcResultCommsCostJson
            {
                OnePlusFourCommsCostApportionmentPercentages = new OnePlusFourCommsCostApportionmentPercentages
                {
                    England = "0.00%",
                    Wales = "0.00%",
                    Scotland = "0.00%",
                    NorthernIreland = "0.00%",
                    Total = "0.00%"
                }
            };
            mockMapper.Setup(m => m.Map(communicationCost)).Returns(mappedResult);

            // Act  
            var result = exporter.Export(communicationCost);

            // Assert  
            Assert.IsNotNull(result);
            Assert.AreEqual(mappedResult, result);
        }
    }
}
