using AutoFixture;
using EPR.Calculator.Service.Function.Models;
using Moq;

namespace EPR.Calculator.Service.Function.UnitTests.Models
{
    [TestClass]
    public class CalcResultLateReportingTonnageTests
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CalcResultLateReportingTonnageTests"/> class.
        /// </summary>
        public CalcResultLateReportingTonnageTests()
        {
            Fixture = new Fixture();
            Name = Fixture.Create<string>();
            MaterialHeading = Fixture.Create<string>();
            TonnageHeading = Fixture.Create<string>();
            CalcResultLateReportingTonnageDetails = new Mock<IEnumerable<CalcResultLateReportingTonnageDetail>>().Object;
            TestClass = new CalcResultLateReportingTonnage
            {
                Name = Name,
                MaterialHeading = MaterialHeading,
                TonnageHeading = TonnageHeading,
                CalcResultLateReportingTonnageDetails = CalcResultLateReportingTonnageDetails,
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
                Name = Name,
                MaterialHeading = MaterialHeading,
                TonnageHeading = TonnageHeading,
                CalcResultLateReportingTonnageDetails = CalcResultLateReportingTonnageDetails,
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
                Name = Name,
                MaterialHeading = MaterialHeading,
                TonnageHeading = TonnageHeading,
                CalcResultLateReportingTonnageDetails = CalcResultLateReportingTonnageDetails,
            };
            var different = Fixture.Create<CalcResultLateReportingTonnage>();

            // Assert
            Assert.IsFalse(TestClass.Equals(default(object)));
            Assert.IsFalse(TestClass.Equals(new object()));
            Assert.IsTrue(TestClass.Equals((object)same));
            Assert.IsFalse(TestClass.Equals((object)different));
            Assert.IsTrue(TestClass.Equals(same));
            Assert.IsFalse(TestClass.Equals(different));
            Assert.AreEqual(same.GetHashCode(), TestClass.GetHashCode());
            Assert.AreNotEqual(different.GetHashCode(), TestClass.GetHashCode());
            Assert.IsTrue(TestClass == same);
            Assert.IsFalse(TestClass == different);
            Assert.IsFalse(TestClass != same);
            Assert.IsTrue(TestClass != different);
        }

        [TestMethod]
        public void NameIsInitializedCorrectly()
            => Assert.AreEqual(Name, TestClass.Name);

        [TestMethod]
        public void MaterialHeadingIsInitializedCorrectly()
            => Assert.AreEqual(MaterialHeading, TestClass.MaterialHeading);

        [TestMethod]
        public void TonnageHeadingIsInitializedCorrectly()
            => Assert.AreEqual(TonnageHeading, TestClass.TonnageHeading);

        [TestMethod]
        public void CalcResultLateReportingTonnageDetailsIsInitializedCorrectly()
            => Assert.AreSame(CalcResultLateReportingTonnageDetails, TestClass.CalcResultLateReportingTonnageDetails);
    }
}