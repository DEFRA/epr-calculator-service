namespace EPR.Calculator.Service.Function.UnitTests.Models
{
    using System;
    using AutoFixture;
    using EPR.Calculator.Service.Function.Models;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Unit tests for the <see cref="CalcResultLateReportingTonnageDetail"/> class.
    /// </summary>
    [TestClass]
    public class CalcResultLateReportingTonnageDetailTests
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CalcResultLateReportingTonnageDetailTests"/> class.
        /// </summary>
        public CalcResultLateReportingTonnageDetailTests()
        {
            this.Fixture = new Fixture();
            this.Name = this.Fixture.Create<string>();
            this.TotalLateReportingTonnage = this.Fixture.Create<decimal>();
            this.TestClass = new CalcResultLateReportingTonnageDetail
            {
                Name = this.Name,
                TotalLateReportingTonnage = this.TotalLateReportingTonnage,
            };
        }

        private CalcResultLateReportingTonnageDetail TestClass { get; init; }

        private IFixture Fixture { get; init; }

        private string Name { get; init; }

        private decimal TotalLateReportingTonnage { get; init; }

        [TestMethod]
        public void CanInitialize()
        {
            // Act
            var instance = new CalcResultLateReportingTonnageDetail
            {
                Name = this.Name,
                TotalLateReportingTonnage = this.TotalLateReportingTonnage,
            };

            // Assert
            Assert.IsNotNull(instance);
        }

        [TestMethod]
        public void ImplementsIEquatable_CalcResultLateReportingTonnageDetail()
        {
            // Arrange
            var same = new CalcResultLateReportingTonnageDetail
            {
                Name = this.Name,
                TotalLateReportingTonnage = this.TotalLateReportingTonnage,
            };
            var different = this.Fixture.Create<CalcResultLateReportingTonnageDetail>();

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

        [TestMethod]
        public void NameIsInitializedCorrectly()
            => Assert.AreEqual(this.Name, this.TestClass.Name);

        [TestMethod]
        public void TotalLateReportingTonnageIsInitializedCorrectly()
            => Assert.AreEqual(this.TotalLateReportingTonnage, this.TestClass.TotalLateReportingTonnage);
    }
}