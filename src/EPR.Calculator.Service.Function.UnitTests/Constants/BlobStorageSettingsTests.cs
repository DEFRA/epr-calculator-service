using AutoFixture;
using EPR.Calculator.Service.Function.Constants;

namespace EPR.Calculator.Service.Function.UnitTests.Constants
{
    [TestClass]
    public class BlobStorageSettingsTests
    {
        private BlobStorageSettings TestClass;
        private IFixture Fixture;

        public BlobStorageSettingsTests()
        {
            Fixture = new Fixture();
            TestClass = new BlobStorageSettings();
        }

        [TestMethod]
        public void CanSetAndGetConnectionString()
        {
            // Arrange
            var testValue = Fixture.Create<string>();

            // Act
            TestClass.ConnectionString = testValue;

            // Assert
            Assert.AreEqual(testValue, TestClass.ConnectionString);
        }

        [TestMethod]
        public void CanSetAndGetContainerName()
        {
            // Arrange
            var testValue = Fixture.Create<string>();

            // Act
            TestClass.ContainerName = testValue;

            // Assert
            Assert.AreEqual(testValue, TestClass.ContainerName);
        }

        [TestMethod]
        public void CanSetAndGetCsvFileName()
        {
            // Arrange
            var testValue = Fixture.Create<string>();

            // Act
            TestClass.CsvFileName = testValue;

            // Assert
            Assert.AreEqual(testValue, TestClass.CsvFileName);
        }
    }
}