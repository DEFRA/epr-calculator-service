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
    public class CommsCostByMaterial2ATests
    {
        private CommsCostByMaterial2A _testClass;
        private Mock<ICalcResult2aCommsDataByMaterialMapper> _mapper;

        [TestInitialize]
        public void SetUp()
        {
            _mapper = new Mock<ICalcResult2aCommsDataByMaterialMapper>();
            _testClass = new CommsCostByMaterial2A(_mapper.Object);
        }

        [TestMethod]
        public void CanConstruct()
        {
            // Act
            var instance = new CommsCostByMaterial2A(_mapper.Object);

            // Assert
            Assert.IsNotNull(instance);
        }

        
        [TestMethod]
        public void CanCallExport()
        {
            // Arrange
            var fixture = new Fixture();
            var commsCostByMaterial = fixture.Create<List<CalcResultCommsCostCommsCostByMaterial>>();

            _mapper.Setup(mock => mock.Map(It.IsAny<List<CalcResultCommsCostCommsCostByMaterial>>())).Returns(fixture.Create<CalcResult2aCommsDataByMaterial>());

            // Act
            var result = _testClass.Export(commsCostByMaterial);

            // Assert
            _mapper.Verify(mock => mock.Map(It.IsAny<List<CalcResultCommsCostCommsCostByMaterial>>()));

            Assert.IsNotNull(result);
        }
       
    }
}