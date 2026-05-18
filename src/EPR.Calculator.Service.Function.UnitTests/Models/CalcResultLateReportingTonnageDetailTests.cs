using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.UnitTests.Models
{
    [TestClass]
    public class CalcResultLateReportingTonnageDetailTests
    {
        public CalcResultLateReportingTonnageDetailTests()
        {
            Fixture = new Fixture();
            TotalLateReportingTonnage = Fixture.Create<decimal>();
            RedLateReportingTonnage   = Fixture.Create<decimal>();
            AmberLateReportingTonnage = Fixture.Create<decimal>();
            GreenLateReportingTonnage = Fixture.Create<decimal>();
            TestClass = new CalcResultLateReportingTonnageDetail
            {
                TotalLateReportingTonnage = TotalLateReportingTonnage,
                RedLateReportingTonnage   = RedLateReportingTonnage,
                AmberLateReportingTonnage = AmberLateReportingTonnage,
                GreenLateReportingTonnage = GreenLateReportingTonnage,
            };
        }

        private CalcResultLateReportingTonnageDetail TestClass { get; init; }
        private IFixture Fixture { get; init; }
        private decimal TotalLateReportingTonnage { get; init; }
        private decimal RedLateReportingTonnage { get; init; }
        private decimal AmberLateReportingTonnage { get; init; }
        private decimal GreenLateReportingTonnage { get; init; }

        [TestMethod]
        public void CanInitialize()
        {
            var instance = new CalcResultLateReportingTonnageDetail
            {
                TotalLateReportingTonnage = TotalLateReportingTonnage,
                RedLateReportingTonnage   = RedLateReportingTonnage,
                AmberLateReportingTonnage = AmberLateReportingTonnage,
                GreenLateReportingTonnage = GreenLateReportingTonnage,
            };
            Assert.IsNotNull(instance);
        }

        [TestMethod]
        public void ImplementsIEquatable_CalcResultLateReportingTonnageDetail()
        {
            var same = new CalcResultLateReportingTonnageDetail
            {
                TotalLateReportingTonnage = TotalLateReportingTonnage,
                RedLateReportingTonnage   = RedLateReportingTonnage,
                AmberLateReportingTonnage = AmberLateReportingTonnage,
                GreenLateReportingTonnage = GreenLateReportingTonnage,
            };
            var different = Fixture.Create<CalcResultLateReportingTonnageDetail>();

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
        public void TotalLateReportingTonnageIsInitializedCorrectly()
            => Assert.AreEqual(TotalLateReportingTonnage, TestClass.TotalLateReportingTonnage);
    }
}
