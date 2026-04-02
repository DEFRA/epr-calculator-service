using AutoFixture;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.UnitTests.Models
{
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
            Fixture = new Fixture();
            Name = Fixture.Create<string>();
            TotalLateReportingTonnage = Fixture.Create<decimal>();
            TestClass = new CalcResultLateReportingTonnageDetail
            {
                Name = Name,
                RedLateReportingTonnage = RedLateReportingTonnage,
                AmberLateReportingTonnage = AmberLateReportingTonnage,
                GreenLateReportingTonnage = GreenLateReportingTonnage,
                TotalLateReportingTonnage = TotalLateReportingTonnage,
            };
        }

        private CalcResultLateReportingTonnageDetail TestClass { get; init; }

        private IFixture Fixture { get; init; }

        private string Name { get; init; }

        private decimal RedLateReportingTonnage { get; init; }
        private decimal AmberLateReportingTonnage { get; init; }
        private decimal GreenLateReportingTonnage { get; init; }
        private decimal TotalLateReportingTonnage { get; init; }

        [TestMethod]
        public void CanInitialize()
        {
            // Act
            var instance = new CalcResultLateReportingTonnageDetail
            {
                Name = Name,
                RedLateReportingTonnage = RedLateReportingTonnage,
                AmberLateReportingTonnage = AmberLateReportingTonnage,
                GreenLateReportingTonnage = GreenLateReportingTonnage,
                TotalLateReportingTonnage = TotalLateReportingTonnage,
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
                Name = Name,
                RedLateReportingTonnage = RedLateReportingTonnage,
                AmberLateReportingTonnage = AmberLateReportingTonnage,
                GreenLateReportingTonnage = GreenLateReportingTonnage,
                TotalLateReportingTonnage = TotalLateReportingTonnage,
            };
            var different = Fixture.Create<CalcResultLateReportingTonnageDetail>();

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
        public void TotalLateReportingTonnageIsInitializedCorrectly()
            => Assert.AreEqual(TotalLateReportingTonnage, TestClass.TotalLateReportingTonnage);
    }
}