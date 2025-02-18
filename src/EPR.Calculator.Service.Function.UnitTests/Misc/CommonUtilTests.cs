namespace EPR.Calculator.Service.Function.UnitTests.Misc
{
    using EPR.Calculator.Service.Function.Misc;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class CommonUtilTests
    {
        [TestMethod]
        public void CanCallConvertKilogramToTonne()
        {
            // Arrange
            var weight = 1000;

            // Act
            var result = CommonUtil.ConvertKilogramToTonne(weight);

            // Assert
            Assert.AreEqual(1, result);
        }

        [TestMethod]
        public void CanCallConvertKilogramToTonneReturnZero()
        {
            // Arrange
            var weight = 0;

            // Act
            var result = CommonUtil.ConvertKilogramToTonne(weight);

            // Assert
            Assert.AreEqual(0, result);
        }
    }
}