namespace EPR.Calculator.Service.Function.UnitTests.Builder
{
    using System;
    using AutoFixture;
    using EPR.Calculator.Service.Function.Builder;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Unit tests for the <see cref="Organisation"/> class.
    /// </summary>
    [TestClass]
    public class OrganisationTests
    {
        private readonly Organisation organisation;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrganisationTests"/> class.
        /// </summary>
        public OrganisationTests()
        {
            this.organisation = new Organisation();
        }

        [TestMethod]
        public void CanSetAndGetOrganisationId()
        {
            // Arrange
            var fixture = new Fixture();

            var testValue = fixture.Create<int>();

            // Act
            this.organisation.OrganisationId = testValue;

            // Assert
            Assert.AreEqual(testValue, this.organisation.OrganisationId);
        }

        [TestMethod]
        public void CanSetAndGetOrganisationName()
        {
            // Arrange
            var fixture = new Fixture();

            var testValue = fixture.Create<string>();

            // Act
            this.organisation.OrganisationName = testValue;

            // Assert
            Assert.AreEqual(testValue, this.organisation.OrganisationName);
        }
    }
}