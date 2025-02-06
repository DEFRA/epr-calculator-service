namespace EPR.Calculator.Service.Function.UnitTests
{
    using System;
    using AutoFixture;
    using EPR.Calculator.Service.Function.Data.DataModels;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class PomDataTests
    {
        private PomData _testClass;

        [TestInitialize]
        public void SetUp()
        {
            _testClass = new PomData
            {
                SubmissionPeriod = "2024-25",
                SubmissionPeriodDesc = "test",
                LoadTimeStamp = default
            };
        }

        [TestMethod]
        public void CanSetAndGetOrganisationId()
        {
            // Arrange
            var fixture = new Fixture();

            var testValue = fixture.Create<int?>();

            // Act
            _testClass.OrganisationId = testValue;

            // Assert
            Assert.AreEqual(testValue, _testClass.OrganisationId);
        }

        [TestMethod]
        public void CanSetAndGetSubsidaryId()
        {
            // Arrange
            var fixture = new Fixture();

            var testValue = fixture.Create<string>();

            // Act
            _testClass.SubsidaryId = testValue;

            // Assert
            Assert.AreEqual(testValue, _testClass.SubsidaryId);
        }

        [TestMethod]
        public void CanSetAndGetSubmissionPeriod()
        {
            // Arrange
            var fixture = new Fixture();

            var testValue = fixture.Create<string>();

            // Act
            _testClass.SubmissionPeriod = testValue;

            // Assert
            Assert.AreEqual(testValue, _testClass.SubmissionPeriod);
        }

        [TestMethod]
        public void CanSetAndGetPackagingActivity()
        {
            // Arrange
            var fixture = new Fixture();

            var testValue = fixture.Create<string>();

            // Act
            _testClass.PackagingActivity = testValue;

            // Assert
            Assert.AreEqual(testValue, _testClass.PackagingActivity);
        }

        [TestMethod]
        public void CanSetAndGetPackagingType()
        {
            // Arrange
            var fixture = new Fixture();

            var testValue = fixture.Create<string>();

            // Act
            _testClass.PackagingType = testValue;

            // Assert
            Assert.AreEqual(testValue, _testClass.PackagingType);
        }

        [TestMethod]
        public void CanSetAndGetPackagingClass()
        {
            // Arrange
            var fixture = new Fixture();

            var testValue = fixture.Create<string>();

            // Act
            _testClass.PackagingClass = testValue;

            // Assert
            Assert.AreEqual(testValue, _testClass.PackagingClass);
        }

        [TestMethod]
        public void CanSetAndGetPackagingMaterial()
        {
            // Arrange
            var fixture = new Fixture();

            var testValue = fixture.Create<string>();

            // Act
            _testClass.PackagingMaterial = testValue;

            // Assert
            Assert.AreEqual(testValue, _testClass.PackagingMaterial);
        }

        [TestMethod]
        public void CanSetAndGetPackagingMaterialWeight()
        {
            // Arrange
            var fixture = new Fixture();

            var testValue = fixture.Create<double?>();

            // Act
            _testClass.PackagingMaterialWeight = testValue;

            // Assert
            Assert.AreEqual(testValue, _testClass.PackagingMaterialWeight);
        }

        [TestMethod]
        public void CanSetAndGetSubmissionPeriodDesc()
        {
            // Arrange
            var fixture = new Fixture();

            var testValue = fixture.Create<string>();

            // Act
            _testClass.SubmissionPeriodDesc = testValue;

            // Assert
            Assert.AreEqual(testValue, _testClass.SubmissionPeriodDesc);
        }

        [TestMethod]
        public void CanSetAndGetLoadTimeStamp()
        {
            // Arrange
            var fixture = new Fixture();

            var testValue = fixture.Create<DateTime>();

            // Act
            _testClass.LoadTimeStamp = testValue;

            // Assert
            Assert.AreEqual(testValue, _testClass.LoadTimeStamp);
        }
    }
}