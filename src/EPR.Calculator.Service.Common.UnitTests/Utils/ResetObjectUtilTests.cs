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
            var j = new TestClass(){ Temperature = 10.9m, Fruits = 10, Name = "Test" };

            // Act
            ResetObjectUtil.ResetObject(j);

            // Assert
            Assert.IsNotNull(j);
            Assert.AreEqual(j.Name, string.Empty);
            Assert.AreEqual(j.Temperature, 0);
            Assert.AreEqual(j.Fruits, 0);
        }        
    }

    public class TestClass
    {
       public decimal Temperature { get; set; }
       public int Fruits { get; set; }
       public string Name { get; set; }
    }
}