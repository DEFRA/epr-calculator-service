namespace EPR.Calculator.Service.Common.Logging.UnitTests
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Unit tests for the <see cref="ErrorMessage"/> class.
    /// </summary>
    [TestClass]
    public class ErrorMessageTests
    {
        /// <summary>
        /// Tests that the RunId property can be set and retrieved correctly.
        /// </summary>
        [TestMethod]
        public void ErrorMessageRunIdSetAndGet()
        {
            // Arrange
            var errorMessage = new ErrorMessage
            {
                Message = "Initial Message",
                Exception = new Exception("Initial Exception"),
            };

            // Act
            errorMessage.RunId = 123;

            // Assert
            Assert.AreEqual(123, errorMessage.RunId);
        }

        /// <summary>
        /// Tests that the RunName property can be set and retrieved correctly.
        /// </summary>
        [TestMethod]
        public void ErrorMessageRunNameSetAndGet()
        {
            // Arrange
            var errorMessage = new ErrorMessage
            {
                Message = "Initial Message",
                Exception = new Exception("Initial Exception"),
            };

            // Act
            errorMessage.RunName = "Test Run";

            // Assert
            Assert.AreEqual("Test Run", errorMessage.RunName);
        }

        /// <summary>
        /// Tests that the Message property can be set and retrieved correctly.
        /// </summary>
        [TestMethod]
        public void ErrorMessageMessageSetAndGet()
        {
            // Arrange
            var errorMessage = new ErrorMessage
            {
                Message = "Initial Message",
                Exception = new Exception("Initial Exception"),
            };

            // Act
            errorMessage.Message = "Test Message";

            // Assert
            Assert.AreEqual("Test Message", errorMessage.Message);
        }

        /// <summary>
        /// Tests that the Exception property can be set and retrieved correctly.
        /// </summary>
        [TestMethod]
        public void ErrorMessageExceptionSetAndGet()
        {
            // Arrange
            var errorMessage = new ErrorMessage
            {
                Message = "Initial Message",
                Exception = new Exception("Initial Exception"),
            };
            var exception = new Exception("Test Exception");

            // Act
            errorMessage.Exception = exception;

            // Assert
            Assert.AreEqual(exception, errorMessage.Exception);
        }

        /// <summary>
        /// Tests that the constructor initializes properties correctly.
        /// </summary>
        [TestMethod]
        public void ErrorMessageConstructorInitializesProperties()
        {
            // Arrange & Act
            var exception = new Exception("Test Exception");
            var errorMessage = new ErrorMessage
            {
                RunId = 123,
                RunName = "Test Run",
                Message = "Test Message",
                Exception = exception,
            };

            // Assert
            Assert.AreEqual(123, errorMessage.RunId);
            Assert.AreEqual("Test Run", errorMessage.RunName);
            Assert.AreEqual("Test Message", errorMessage.Message);
            Assert.AreEqual(exception, errorMessage.Exception);
        }
    }
}