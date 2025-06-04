using EPR.Calculator.Service.Function.Exceptions;

namespace EPR.Calculator.Service.Function.UnitTests.Exceptions
{
    [TestClass]
    public class RecordNotFoundExceptionTests
    {
        [TestMethod]
        public void Constructor_Should_Set_Message()
        {
            // Arrange
            var expectedMessage = "Record not found.";

            // Act
            var exception = new RecordNotFoundException(expectedMessage);

            // Assert
            Assert.AreEqual(expectedMessage, exception.Message);
        }

        [TestMethod]
        public void RecordNotFoundException_Should_Be_Exception_Type()
        {
            // Act
            var exception = new RecordNotFoundException("Test");

            // Assert
            Assert.IsInstanceOfType(exception, typeof(Exception));
        }

        [TestMethod]
        public void RecordNotFoundException_Should_Be_Catchable_As_Exception()
        {
            // Arrange
            Exception? caughtException = null;

            // Act
            try
            {
                throw new RecordNotFoundException("Record missing.");
            }
            catch (Exception ex)
            {
                caughtException = ex;
            }

            // Assert
            Assert.IsNotNull(caughtException);
            Assert.IsInstanceOfType(caughtException, typeof(RecordNotFoundException));
        }
    }
}
