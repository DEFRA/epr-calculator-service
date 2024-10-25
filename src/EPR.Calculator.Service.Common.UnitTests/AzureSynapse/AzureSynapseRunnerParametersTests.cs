namespace EPR.Calculator.Service.Common.UnitTests.AzureSynapse
{
    using System;
    using EPR.Calculator.Service.Common.AzureSynapse;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class AzureSynapseRunnerParametersTests
    {
        private AzureSynapseRunnerParameters _testClass;
        private int _calculatorRunId;
        private FinancialYear _financialYear;

        [TestInitialize]
        public void SetUp()
        {
            _calculatorRunId = 72177078;
            _financialYear = new FinancialYear(DateTime.UtcNow);
            _testClass = new AzureSynapseRunnerParameters
            {
                CalculatorRunId = _calculatorRunId,
                FinancialYear = _financialYear
            };
        }

        [TestMethod]
        public void CanInitialize()
        {
            // Act
            var instance = new AzureSynapseRunnerParameters
            {
                CalculatorRunId = _calculatorRunId,
                FinancialYear = _financialYear
            };

            // Assert
            Assert.IsNotNull(instance);
        }

        [TestMethod]
        public void ImplementsIEquatable_AzureSynapseRunnerParameters()
        {
            // Arrange
            var same = new AzureSynapseRunnerParameters
            {
                CalculatorRunId = _calculatorRunId,
                FinancialYear = _financialYear
            };
            var different = new AzureSynapseRunnerParameters();

            // Assert
            Assert.IsFalse(_testClass.Equals(default(object)));
            Assert.IsFalse(_testClass.Equals(new object()));
            Assert.IsTrue(_testClass.Equals((object)same));
            Assert.IsFalse(_testClass.Equals((object)different));
            Assert.IsTrue(_testClass.Equals(same));
            Assert.IsFalse(_testClass.Equals(different));
            Assert.AreEqual(same.GetHashCode(), _testClass.GetHashCode());
            Assert.AreNotEqual(different.GetHashCode(), _testClass.GetHashCode());
            Assert.IsTrue(_testClass == same);
            Assert.IsFalse(_testClass == different);
            Assert.IsFalse(_testClass != same);
            Assert.IsTrue(_testClass != different);
        }

        [TestMethod]
        public void CalculatorRunIdIsInitializedCorrectly()
        {
            Assert.AreEqual(_calculatorRunId, _testClass.CalculatorRunId);
        }

        [TestMethod]
        public void FinancialYearIsInitializedCorrectly()
        {
            Assert.AreSame(_financialYear, _testClass.FinancialYear);
        }
    }
}