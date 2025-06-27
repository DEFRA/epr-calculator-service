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
                Humidity = fixture.Create<double>()
            };

            // Act
            ResetObjectUtil.ResetObject(j);

            // Assert
            Assert.IsNotNull(j);
            Assert.AreEqual(j.Name, string.Empty);
            Assert.AreEqual(0, j.Temperature);
            Assert.AreEqual(0, j.Fruits);
            Assert.AreEqual(0, j.Humidity);
            Assert.IsTrue(j.IsTotalRow);
            Assert.IsFalse(j.IsTested);
            Assert.IsFalse(j.childTest.IsChild);
        }        
    }

    public class TestClass
    {
        public decimal Temperature { get; set; }
        public int Fruits { get; set; }
        public required string Name { get; set; }
        public bool IsTotalRow { get; set; }

        public bool IsTested { get; set; }

        public double Humidity { get; set; }

        public required ChildTestClass childTest { get; set; }
    }

    public class ChildTestClass
    {
        public bool IsChild { get; set; }
    }
}