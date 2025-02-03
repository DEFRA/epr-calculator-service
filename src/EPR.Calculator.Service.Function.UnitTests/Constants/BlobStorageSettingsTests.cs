namespace EPR.Calculator.Service.Function.UnitTests.Constants
{
    using System;
    using AutoFixture;
    using EPR.Calculator.Service.Function.Constants;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class BlobStorageSettingsTests
    {
        private BlobStorageSettings TestClass;
        private IFixture Fixture;

        [TestInitialize]
        public void SetUp()
        {
            Fixture = new Fixture();
            this.TestClass = new BlobStorageSettings();
        }

        [TestMethod]
        public void CanSetAndGetConnectionString()
        {
            // Arrange
            var testValue = Fixture.Create<string>();

            // Act
            this.TestClass.ConnectionString = testValue;

            // Assert
            Assert.AreEqual(testValue, this.TestClass.ConnectionString);
        }

        [TestMethod]
        public void CanSetAndGetContainerName()
        {
            // Arrange
            var testValue = Fixture.Create<string>();

            // Act
            this.TestClass.ContainerName = testValue;

            // Assert
            Assert.AreEqual(testValue, this.TestClass.ContainerName);
        }

        [TestMethod]
        public void CanSetAndGetCsvFileName()
        {
            // Arrange
            var testValue = Fixture.Create<string>();

            // Act
            this.TestClass.CsvFileName = testValue;

            // Assert
            Assert.AreEqual(testValue, this.TestClass.CsvFileName);
        }
    }
}