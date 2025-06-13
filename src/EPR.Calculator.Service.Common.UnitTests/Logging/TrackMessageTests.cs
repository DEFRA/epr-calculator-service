namespace EPR.Calculator.Service.Common.Logging.UnitTests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Unit tests for the <see cref="TrackMessage"/> class.
    /// </summary>
    [TestClass]
    public class TrackMessageTests
    {
        /// <summary>
        /// Tests that the RunId property can be set and retrieved correctly.
        /// </summary>
        [TestMethod]
        public void TrackMessageRunIdSetAndGet()
        {
            // Arrange
            TrackMessage trackMessage = new ()
            {
                // Act
                RunId = 123,
                Message = "Default Message",
            };

            // Assert
            Assert.AreEqual(123, trackMessage.RunId);
        }

        /// <summary>
        /// Tests that the RunName property can be set and retrieved correctly.
        /// </summary>
        [TestMethod]
        public void TrackMessageRunNameSetAndGet()
        {
            // Arrange
            var trackMessage = new TrackMessage
            {
                Message = "Default Message",
            };

            // Act
            trackMessage.RunName = "Test Run";

            // Assert
            Assert.AreEqual("Test Run", trackMessage.RunName);
        }

        /// <summary>
        /// Tests that the Message property can be set and retrieved correctly.
        /// </summary>
        [TestMethod]
        public void TrackMessageMessageSetAndGet()
        {
            // Arrange
            var trackMessage = new TrackMessage
            {
                Message = "Test Message",
            };

            // Act
            trackMessage.Message = "Test Message";

            // Assert
            Assert.AreEqual("Test Message", trackMessage.Message);
        }

        /// <summary>
        /// Tests that the constructor initializes properties correctly.
        /// </summary>
        [TestMethod]
        public void TrackMessageConstructorInitializesProperties()
        {
            // Arrange & Act
            var trackMessage = new TrackMessage
            {
                RunId = 123,
                RunName = "Test Run",
                Message = "Test Message",
                MessageType = "Result"
            };

            // Assert
            Assert.AreEqual(123, trackMessage.RunId);
            Assert.AreEqual("Test Run", trackMessage.RunName);
            Assert.AreEqual("Test Message", trackMessage.Message);
            Assert.AreEqual("Result", trackMessage.MessageType);
        }
    }
}