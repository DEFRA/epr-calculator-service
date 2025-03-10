namespace EPR.Calculator.Service.Common.UnitTests.Misc
{
    using AutoFixture;
    using EPR.Calculator.Service.Common.UnitTests.AutoFixtureCustomisations;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>Unit tests for the <see cref="CalendarYear"/> class.</summary>
    [TestClass]
    public class CalendarYearTests
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CalendarYearTests"/> class.
        /// </summary>
        public CalendarYearTests()
        {
            this.Fixture = new Fixture();
            this.Fixture.Customizations.Add(new CalendarYearCustomisation());
            this.Fixture.Customizations.Add(new FinancialYearCustomisation());
            this.Value = this.Fixture.Create<CalendarYear>();
            this.TestClass = new CalendarYear(this.Value);
        }

        private Fixture Fixture { get; init; }

        private CalendarYear TestClass { get; init; }

        private string Value { get; init; }

        /// <summary>Tests the constructor.</summary>
        [TestMethod]
        public void CanConstruct()
        {
            // Act
            var instance = new CalendarYear(this.Value);

            // Assert
            Assert.IsNotNull(instance);
        }

        /// <summary>
        /// Tests that trying to construct using an invalid value throws an argument exception.
        /// </summary>
        [TestMethod]
        [DataRow("")]
        [DataRow("2024-25")]
        public void CannotConstructWithInvalidValue(string invalidValue)
        {
            // Act
            Assert.ThrowsException<ArgumentException>(() => new CalendarYear(invalidValue));
        }

        /// <summary>Tests for equality and inequality.</summary>
        [TestMethod]
        public void ImplementsIEquatable_FinancialYear()
        {
            // Arrange
            var same = new CalendarYear(this.Value);
            var different = new CalendarYear("1234");

            // Assert
            Assert.IsFalse(this.TestClass.Equals(default(object)));
            Assert.IsFalse(this.TestClass.Equals(new object()));
            Assert.IsTrue(this.TestClass.Equals((object)same));
            Assert.IsFalse(this.TestClass.Equals((object)different));
            Assert.IsTrue(this.TestClass.Equals(same));
            Assert.IsFalse(this.TestClass.Equals(different));
            Assert.AreEqual(same.GetHashCode(), this.TestClass.GetHashCode());
            Assert.AreNotEqual(different.GetHashCode(), this.TestClass.GetHashCode());
            Assert.IsTrue(this.TestClass == same);
            Assert.IsFalse(this.TestClass == different);
            Assert.IsFalse(this.TestClass != same);
            Assert.IsTrue(this.TestClass != different);
        }

        /// <summary>Tests the ToString method.</summary>
        [TestMethod]
        public void CanCallToString()
        {
            // Act
            var result = this.TestClass.ToString();

            // Assert
            Assert.AreSame(this.Value, result);
        }
    }
}