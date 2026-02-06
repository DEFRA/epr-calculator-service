namespace EPR.Calculator.Service.Common.UnitTests.Misc
{
    using AutoFixture;
    using EPR.Calculator.Service.Common.UnitTests.AutoFixtureCustomisations;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>Unit tests for the <see cref="RelativeYear"/> class.</summary>
    [TestClass]
    public class RelativeYearTests
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RelativeYearTests"/> class.
        /// </summary>
        public RelativeYearTests()
        {
            this.Fixture = new Fixture();
            this.Fixture.Customizations.Add(new RelativeYearCustomisation());
            this.Fixture.Customizations.Add(new FinancialYearCustomisation());
            this.Value = this.Fixture.Create<RelativeYear>().ToInt();
            this.TestClass = new RelativeYear(this.Value);
        }

        private Fixture Fixture { get; init; }

        private RelativeYear TestClass { get; init; }

        private int Value { get; init; }

        /// <summary>Tests the constructor.</summary>
        [TestMethod]
        public void CanConstruct()
        {
            // Act
            var instance = new RelativeYear(this.Value);

            // Assert
            Assert.IsNotNull(instance);
        }

        /// <summary>Tests for equality and inequality.</summary>
        [TestMethod]
        public void ImplementsIEquatable_FinancialYear()
        {
            // Arrange
            var same = new RelativeYear(this.Value);
            var different = new RelativeYear(1234);

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
            Assert.AreEqual(this.Value, this.TestClass.ToInt());
        }

        /// <summary>Tests the ToInt method.</summary>
        [TestMethod]
        public void CanCallToInt()
        {
            Assert.AreEqual($"{this.Value}", this.TestClass.ToString());
        }
    }
}