namespace EPR.Calculator.Service.Function.UnitTests
{
    using System;
    using AutoFixture;
    using EPR.Calculator.Service.Function.Data.DataModels;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class OrganisationDataTests
    {
        private OrganisationData _testClass;

        [TestInitialize]
        public void SetUp()
        {
            _testClass = new OrganisationData
            {
                OrganisationName = "Test",
                SubmissionPeriodDesc = "2024-25",
                LoadTimestamp = default
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
        public void CanSetAndGetOrganisationName()
        {
            // Arrange
            var fixture = new Fixture();

            var testValue = fixture.Create<string>();

            // Act
            _testClass.OrganisationName = testValue;

            // Assert
            Assert.AreEqual(testValue, _testClass.OrganisationName);
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
        public void CanSetAndGetLoadTimestamp()
        {
            // Arrange
            var fixture = new Fixture();

            var testValue = fixture.Create<DateTime>();

            // Act
            _testClass.LoadTimestamp = testValue;

            // Assert
            Assert.AreEqual(testValue, _testClass.LoadTimestamp);
        }
    }
}