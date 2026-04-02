using AutoFixture;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.UnitTests.Models
{
    [TestClass]
    public class CalcResultScaledupProducerHeaderTests
    {
        private readonly CalcResultScaledupProducerHeader calcResultScaledupProducerHeader;

        public CalcResultScaledupProducerHeaderTests()
        {
            calcResultScaledupProducerHeader = new CalcResultScaledupProducerHeader
            {
                Name = "Some column header name",
                ColumnIndex = 1,
            };
        }

        [TestMethod]
        public void CanSetAndGetName()
        {
            // Arrange
            var fixture = new Fixture();

            var testValue = fixture.Create<string>();

            // Act
            calcResultScaledupProducerHeader.Name = testValue;

            // Assert
            Assert.AreEqual(testValue, calcResultScaledupProducerHeader.Name);
        }

        [TestMethod]
        public void CanSetAndGetColumnIndex()
        {
            // Arrange
            var fixture = new Fixture();

            var testValue = fixture.Create<int?>();

            // Act
            calcResultScaledupProducerHeader.ColumnIndex = testValue;

            // Assert
            Assert.AreEqual(testValue, calcResultScaledupProducerHeader.ColumnIndex);
        }
    }
}