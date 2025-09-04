namespace EPR.Calculator.Service.Common.UnitTests
{
    using AutoFixture;
    using Common;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class BillingFileMessageTests
    {
        private BillingFileMessage _testClass = null!;

        [TestInitialize]
        public void SetUp()
        {
            _testClass = new BillingFileMessage() {MessageType="Billing", ApprovedBy = "TestUser"};
        }

        [TestMethod]
        public void ImplementsIEquatable_BillingFileMessage()
        {
            // Arrange
            var fixture = new Fixture();
            var same = new BillingFileMessage() {MessageType = "Billing", ApprovedBy = "TestUser" };
            var different = fixture.Create<BillingFileMessage>();

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
        public void CanSetAndGetId()
        {
            // Arrange
            var fixture = new Fixture();

            var testValue = fixture.Create<int>();

            // Act
            _testClass.Id = testValue;

            // Assert
            Assert.AreEqual(testValue, _testClass.Id);
        }

        [TestMethod]
        public void CanSetAndGetApprovedBy()
        {
            // Arrange
            var fixture = new Fixture();

            var testValue = fixture.Create<string>();

            // Act
            _testClass.ApprovedBy = testValue;

            // Assert
            Assert.AreEqual(testValue, _testClass.ApprovedBy);
        }

        [TestMethod]
        public void CanSetAndGetMessageType()
        {
            // Arrange
            var fixture = new Fixture();

            var testValue = fixture.Create<string>();

            // Act
            _testClass.MessageType = testValue;

            // Assert
            Assert.AreEqual(testValue, _testClass.MessageType);
        }
    }
}