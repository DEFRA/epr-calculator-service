namespace EPR.Calculator.Service.Function.UnitTests.Models
{
    using System;
    using System.Collections.Generic;
    using AutoFixture;
    using EPR.Calculator.Service.Function.Models;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class CalcResultLateReportingTonnageTests
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CalcResultLateReportingTonnageTests"/> class.
        /// </summary>
        public CalcResultLateReportingTonnageTests()
        {
            this.Fixture = new Fixture();
            this.Name = this.Fixture.Create<string>();
            this.MaterialHeading = this.Fixture.Create<string>();
            this.TonnageHeading = this.Fixture.Create<string>();
            this.CalcResultLateReportingTonnageDetails = new Mock<IEnumerable<CalcResultLateReportingTonnageDetail>>().Object;
            this.TestClass = new CalcResultLateReportingTonnage
            {
                Name = this.Name,
                MaterialHeading = this.MaterialHeading,
                TonnageHeading = this.TonnageHeading,
                CalcResultLateReportingTonnageDetails = this.CalcResultLateReportingTonnageDetails,
            };
        }

        private CalcResultLateReportingTonnage TestClass { get; init; }

        private IFixture Fixture { get; init; }

        private string Name { get; init; }

        private string MaterialHeading { get; init; }

        private string TonnageHeading { get; init; }

        private IEnumerable<CalcResultLateReportingTonnageDetail> CalcResultLateReportingTonnageDetails { get; init; }

        [TestMethod]
        public void CanInitialize()
        {
            // Act
            var instance = new CalcResultLateReportingTonnage
            {
                Name = this.Name,
                MaterialHeading = this.MaterialHeading,
                TonnageHeading = this.TonnageHeading,
                CalcResultLateReportingTonnageDetails = this.CalcResultLateReportingTonnageDetails,
            };

            // Assert
            Assert.IsNotNull(instance);
        }

        [TestMethod]
        public void ImplementsIEquatable_CalcResultLateReportingTonnage()
        {
            // Arrange
            var same = new CalcResultLateReportingTonnage
            {
                Name = this.Name,
                MaterialHeading = this.MaterialHeading,
                TonnageHeading = this.TonnageHeading,
                CalcResultLateReportingTonnageDetails = this.CalcResultLateReportingTonnageDetails,
            };
            var different = this.Fixture.Create<CalcResultLateReportingTonnage>();

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
        public void MaterialHeadingIsInitializedCorrectly()
            => Assert.AreEqual(this.MaterialHeading, this.TestClass.MaterialHeading);

        [TestMethod]
        public void TonnageHeadingIsInitializedCorrectly()
            => Assert.AreEqual(this.TonnageHeading, this.TestClass.TonnageHeading);

        [TestMethod]
        public void CalcResultLateReportingTonnageDetailsIsInitializedCorrectly()
            => Assert.AreSame(this.CalcResultLateReportingTonnageDetails, this.TestClass.CalcResultLateReportingTonnageDetails);
    }
}