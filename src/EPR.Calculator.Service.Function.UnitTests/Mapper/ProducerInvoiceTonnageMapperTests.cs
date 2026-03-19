using AutoFixture;
using EPR.Calculator.Service.Function.Mapper;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.UnitTests.Mapper
{
    [TestClass]
    public class ProducerInvoiceTonnageMapperTests
    {
        public required ProducerInvoiceTonnageMapper testClass { get; set; }

        [TestInitialize]
        public void SetUp()
        {
            testClass = new ProducerInvoiceTonnageMapper();
        }

        [TestMethod]
        public void CanCallMap()
        {
            // Arrange
            var fixture = new Fixture();
            var producerInvoiceTonnage = fixture.Create<ProducerInvoiceTonnage>();

            // Act
            var result = testClass.Map(producerInvoiceTonnage);

            // Assert
            Assert.AreEqual(producerInvoiceTonnage.RunId, result.CalculatorRunId);
            Assert.AreEqual(producerInvoiceTonnage.ProducerId, result.ProducerId);
            Assert.AreEqual(producerInvoiceTonnage.MaterialId, result.MaterialId);
            Assert.AreEqual(producerInvoiceTonnage.NetTonnage, result.InvoicedNetTonnage);

        }       
    }
}