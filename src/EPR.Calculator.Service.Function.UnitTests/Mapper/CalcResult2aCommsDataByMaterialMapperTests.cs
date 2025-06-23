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
            var fixture = new Fixture();
            var commsCostByMaterial = fixture.Create<List<CalcResultCommsCostCommsCostByMaterial>>();

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
    }
}