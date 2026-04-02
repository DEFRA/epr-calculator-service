using AutoFixture;
using EPR.Calculator.Service.Function.Models;
using Moq;

namespace EPR.Calculator.Service.Function.UnitTests.Models
{
    /// <summary>Unit tests for the <see cref="CalcResultScaledupProducers"/> class.</summary>
    [TestClass]
    public class CalcResultScaledupProducersTests
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CalcResultScaledupProducersTests"/> class.
        /// </summary>
        public CalcResultScaledupProducersTests()
        {
            Fixture = new Fixture();
            TestClass = Fixture.Create<CalcResultScaledupProducers>();
        }

        private CalcResultScaledupProducers TestClass { get; init; }

        private IFixture Fixture { get; init; }

        /// <summary>
        /// Checks that the <see cref="CalcResultScaledupProducers.TitleHeader"/> property can be get and set.
        /// </summary>
        [TestMethod]
        public void CanSetAndGetTitleHeader()
        {
            // Arrange
            var testValue = Fixture.Create<CalcResultScaledupProducerHeader>();

            // Act
            TestClass.TitleHeader = testValue;

            // Assert
            Assert.AreSame(testValue, TestClass.TitleHeader);
        }

        /// <summary>
        /// Checks that the <see cref="CalcResultScaledupProducers.TitleHeader"/> property can be get and set.
        /// </summary>
        [TestMethod]
        public void CanSetAndGetMaterialBreakdownHeaders()
        {
            // Arrange
            var testValue = new Mock<IEnumerable<CalcResultScaledupProducerHeader>>().Object;

            // Act
            TestClass.MaterialBreakdownHeaders = testValue;

            // Assert
            Assert.AreSame(testValue, TestClass.MaterialBreakdownHeaders);
        }

        /// <summary>
        /// Checks that the <see cref="CalcResultScaledupProducers.TitleHeader"/> property can be get and set.
        /// </summary>
        [TestMethod]
        public void CanSetAndGetColumnHeaders()
        {
            // Arrange
            var testValue = new Mock<IEnumerable<CalcResultScaledupProducerHeader>>().Object;

            // Act
            TestClass.ColumnHeaders = testValue;

            // Assert
            Assert.AreSame(testValue, TestClass.ColumnHeaders);
        }

        /// <summary>
        /// Checks that the <see cref="CalcResultScaledupProducers.TitleHeader"/> property can be get and set.
        /// </summary>
        [TestMethod]
        public void CanSetAndGetScaledupProducers()
        {
            // Arrange
            var testValue = new Mock<IEnumerable<CalcResultScaledupProducer>>().Object;

            // Act
            TestClass.ScaledupProducers = testValue;

            // Assert
            Assert.AreSame(testValue, TestClass.ScaledupProducers);
        }
    }
}