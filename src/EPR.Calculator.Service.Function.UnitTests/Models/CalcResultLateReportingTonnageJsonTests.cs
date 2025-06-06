namespace EPR.Calculator.Service.Function.UnitTests.Models
{
    using System;
    using System.Collections.Generic;
    using AutoFixture;
    using EPR.Calculator.Service.Function.Models;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class CalcResultLateReportingTonnageDetailsJsonTests
    {
        private CalcResultLateReportingTonnageDetailsJson _testClass;

        [TestInitialize]
        public void SetUp()
        {
            _testClass = new CalcResultLateReportingTonnageDetailsJson();
        }

        [TestMethod]
        public void CanSetAndGetMaterialName()
        {
            // Arrange
            var fixture = new Fixture();

            var testValue = fixture.Create<string>();

            // Act
            _testClass.MaterialName = testValue;

            // Assert
            Assert.AreEqual(testValue, _testClass.MaterialName);
        }

        [TestMethod]
        public void CanSetAndGetTotalLateReportingTonnage()
        {
            // Arrange
            var fixture = new Fixture();

            var testValue = fixture.Create<decimal>();

            // Act
            _testClass.TotalLateReportingTonnage = testValue;

            // Assert
            Assert.AreEqual(testValue, _testClass.TotalLateReportingTonnage);
        }
    }

    [TestClass]
    public class CalcResultLateReportingTonnageJsonTests
    {
        private CalcResultLateReportingTonnageJson _testClass;

        [TestInitialize]
        public void SetUp()
        {
            _testClass = new CalcResultLateReportingTonnageJson();
        }

        [TestMethod]
        public void CanSetAndGetName()
        {
            // Arrange
            var fixture = new Fixture();

            var testValue = fixture.Create<string>();

            // Act
            _testClass.Name = testValue;

            // Assert
            Assert.AreEqual(testValue, _testClass.Name);
        }

        [TestMethod]
        public void CanSetAndGetcalcResultLateReportingTonnageDetails()
        {
            // Arrange
            var fixture = new Fixture();

            var testValue = fixture.Create<List<CalcResultLateReportingTonnageDetailsJson>>();

            // Act
            _testClass.calcResultLateReportingTonnageDetails = testValue;

            // Assert
            Assert.AreSame(testValue, _testClass.calcResultLateReportingTonnageDetails);
        }

        [TestMethod]
        public void CanSetAndGetCalcResultLateReportingTonnageTotal()
        {
            // Arrange
            var fixture = new Fixture();

            var testValue = fixture.Create<decimal>();

            // Act
            _testClass.CalcResultLateReportingTonnageTotal = testValue;

            // Assert
            Assert.AreEqual(testValue, _testClass.CalcResultLateReportingTonnageTotal);
        }
    }
}