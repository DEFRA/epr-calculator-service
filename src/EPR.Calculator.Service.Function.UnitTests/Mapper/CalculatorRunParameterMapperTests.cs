namespace EPR.Calculator.Service.Function.UnitTests.Mapper
{
    using EPR.Calculator.Service.Function;
    using EPR.Calculator.Service.Function.Mapper;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class CalculatorRunParameterMapperTests
    {
        private CalculatorRunParameterMapper _testClass;

        [TestInitialize]
        public void SetUp()
        {
            _testClass = new CalculatorRunParameterMapper();
        }

        [TestMethod]
        public void Calulator_Run_Mapper_Map_Test()
        {
            // Arrange
            var calculatorParameter = new CalculatorParameter() { FinancialYear = "2024-25", CreatedBy = "Test user", CalculatorRunId = 678767 };

            // Act
            var result = _testClass.Map(calculatorParameter);

            Assert.AreEqual(result.FinancialYear, calculatorParameter.FinancialYear);
            Assert.AreEqual(result.User, calculatorParameter.CreatedBy);
            Assert.AreEqual(result.Id, calculatorParameter.CalculatorRunId);
        }
    }
}