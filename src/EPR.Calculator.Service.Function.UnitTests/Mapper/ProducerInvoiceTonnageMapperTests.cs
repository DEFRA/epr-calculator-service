namespace EPR.Calculator.Service.Function.UnitTests.Mapper
{
    using System;
    using AutoFixture;
    using EPR.Calculator.Service.Function.Mapper;
    using EPR.Calculator.Service.Function.Models;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ProducerInvoiceTonnageMapperTests
    {
        private ProducerInvoiceTonnageMapper _testClass;

        [TestInitialize]
        public void SetUp()
        {
            _testClass = new ProducerInvoiceTonnageMapper();
        }

        [TestMethod]
        public void CanCallMap()
        {
            // Arrange
            var fixture = new Fixture();
            var producerInvoiceTonnage = fixture.Create<ProducerInvoiceTonnage>();

            // Act
            var result = _testClass.Map(producerInvoiceTonnage);

            // Assert
            Assert.AreEqual(producerInvoiceTonnage.RunId, result.CalculatorRunId);
            Assert.AreEqual(producerInvoiceTonnage.ProducerId, result.ProducerId);
            Assert.AreEqual(producerInvoiceTonnage.MaterialId, result.MaterialId);
            Assert.AreEqual(producerInvoiceTonnage.NetTonnage, result.InvoicedNetTonnage);

        }       
    }
}