namespace EPR.Calculator.Service.Function.UnitTests.Models
{
    using System.Collections.Generic;
    using AutoFixture;
    using EPR.Calculator.Service.Function.Models;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    /// <summary>Unit tests for the <see cref="CalcResultScaledupProducers"/> class.</summary>
    [TestClass]
    public class CalcResultScaledupProducersTests
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CalcResultScaledupProducersTests"/> class.
        /// </summary>
        public CalcResultScaledupProducersTests()
        {
            this.Fixture = new Fixture();
            this.TestClass = this.Fixture.Create<CalcResultScaledupProducers>();
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
            var testValue = this.Fixture.Create<CalcResultScaledupProducerHeader>();

            // Act
            this.TestClass.TitleHeader = testValue;

            // Assert
            Assert.AreSame(testValue, this.TestClass.TitleHeader);
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
            this.TestClass.MaterialBreakdownHeaders = testValue;

            // Assert
            Assert.AreSame(testValue, this.TestClass.MaterialBreakdownHeaders);
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
            this.TestClass.ColumnHeaders = testValue;

            // Assert
            Assert.AreSame(testValue, this.TestClass.ColumnHeaders);
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
            this.TestClass.ScaledupProducers = testValue;

            // Assert
            Assert.AreSame(testValue, this.TestClass.ScaledupProducers);
        }
    }
}