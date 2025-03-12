namespace EPR.Calculator.Service.Common.UnitTests.Misc
{
    using System;
    using AutoFixture;
    using EPR.Calculator.Service.Common;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Unit tests for the <see cref="FinancialYear"/> class.
    /// </summary>
    [TestClass]
    public class FinancialYearTests
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FinancialYearTests"/> class.
        /// </summary>
        public FinancialYearTests()
        {
            this.Value = "2024-25";
            this.TestClass = new FinancialYear(this.Value);
        }

        private FinancialYear TestClass { get; init; }

        private string Value { get; init; }

        /// <summary>Tests the constructor.</summary>
        [TestMethod]
        public void CanConstruct()
        {
            // Act
            var instance = new FinancialYear(this.Value);

            // Assert
            Assert.IsNotNull(instance);
        }

        /// <summary>
        /// Tests that trying to construct using an invalid value throws an argument exception.
        /// </summary>
        [TestMethod]
        [DataRow("")]
        [DataRow("2024")]
        public void CannotConstructWithInvalidValue(string invalidValue)
        {
            // Act
            Assert.ThrowsException<ArgumentException>(() => new FinancialYear(invalidValue));
        }

        /// <summary>Tests for equality and inequality.</summary>
        [TestMethod]
        public void ImplementsIEquatable_FinancialYear()
        {
            // Arrange
            var same = new FinancialYear(this.Value);
            var different = new FinancialYear("1234-56");

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