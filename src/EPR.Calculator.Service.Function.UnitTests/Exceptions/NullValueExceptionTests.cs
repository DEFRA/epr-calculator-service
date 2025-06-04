using EPR.Calculator.Service.Function.Exceptions;

namespace EPR.Calculator.Service.Function.UnitTests.Exceptions
{
    [TestClass]
    public class NullValueExceptionTests
    {
        [TestMethod]
        public void Constructor_Should_Set_Message()
        {
            // Arrange
            var expectedMessage = "Value cannot be null.";

            // Act
            var exception = new NullValueException(expectedMessage);

            // Assert
            Assert.AreEqual(expectedMessage, exception.Message);
        }

        [TestMethod]
        public void NullValueException_Should_Be_Exception_Type()
        {
            // Act
            var exception = new NullValueException("Test");

            // Assert
            Assert.IsInstanceOfType(exception, typeof(Exception));
        }

        [TestMethod]
        public void NullValueException_Should_Be_Catchable_As_Exception()
        {
            // Arrange
            Exception caughtException = null;

            // Act
            try
            {
                throw new NullValueException("Null value encountered.");
            }
            catch (Exception ex)
            {
                caughtException = ex;
            }

            // Assert
            Assert.IsNotNull(caughtException);
            Assert.IsInstanceOfType(caughtException, typeof(NullValueException));
        }
    }
}
