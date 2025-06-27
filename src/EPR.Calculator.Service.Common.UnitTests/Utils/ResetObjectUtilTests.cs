namespace EPR.Calculator.Service.Common.UnitTests.Utils
{
    using System;
    using AutoFixture;
    using EPR.Calculator.Service.Common.Utils;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ResetObjectUtilTests
    {
        [TestMethod]
        public void CanCallResetObject()
        {
            // Arrange
            var fixture = new Fixture();
            var j = new TestClass()
            {
                Temperature = 10.9m,
                Fruits = 10,
                Name = "Test",
                IsTotalRow = true,
                IsTested = true,
                childTest = new ChildTestClass() { IsChild = true },
                Humidity = fixture.Create<double>(),
                IsProducerScaledup = "Totals",
                isOverallTotalRow = true,
                Numbers = new List<int>() { 1, 2, 3 }
            };

            // Act
            ResetObjectUtil.ResetObject(j);

            // Assert
            Assert.IsNotNull(j);
            Assert.AreEqual(j.Name, string.Empty);
            Assert.AreEqual(0, j.Temperature);
            Assert.AreEqual(0, j.Fruits);
            Assert.AreEqual(0, j.Humidity);
            Assert.AreEqual("Totals", j.IsProducerScaledup);
            Assert.IsTrue(j.IsTotalRow);
            Assert.IsTrue(j.isOverallTotalRow);
            Assert.IsFalse(j.IsTested);
            Assert.IsFalse(j.childTest.IsChild);
            Assert.AreEqual(3 ,j.Numbers.Count);
        }
    }

    public class TestClass
    {
        public decimal Temperature { get; set; }
        public int Fruits { get; set; }
        public required string Name { get; set; }
        public bool IsTotalRow { get; set; }
        public bool isOverallTotalRow { get; set; }

        public bool IsTested { get; set; }

        public double Humidity { get; set; }

        public required ChildTestClass childTest { get; set; }

        public required string IsProducerScaledup { get; set; }

        public required List<int> Numbers { get; set; }
    }

    public class ChildTestClass
    {
        public bool IsChild { get; set; }
    }
}