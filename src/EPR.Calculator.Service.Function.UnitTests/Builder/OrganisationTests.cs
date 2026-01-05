namespace EPR.Calculator.Service.Function.UnitTests.Builder.ScaledupProducers
{
    using System;
    using AutoFixture;
    using EPR.Calculator.Service.Function.Builder.ScaledupProducers;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Unit tests for the <see cref="Organisation"/> class.
    /// </summary>
    [TestClass]
    public class OrganisationTests
    {
        private readonly Organisation scaledupOrganisation;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrganisationTests"/> class.
        /// </summary>
        public OrganisationTests()
        {
            this.scaledupOrganisation = new Organisation();
        }

        [TestMethod]
        public void CanSetAndGetOrganisationId()
        {
            // Arrange
            var fixture = new Fixture();

            var testValue = fixture.Create<int>();

            // Act
            this.scaledupOrganisation.OrganisationId = testValue;

            // Assert
            Assert.AreEqual(testValue, this.scaledupOrganisation.OrganisationId);
        }

        [TestMethod]
        public void CanSetAndGetOrganisationName()
        {
            // Arrange
            var fixture = new Fixture();

            var testValue = fixture.Create<string>();

            // Act
            this.scaledupOrganisation.OrganisationName = testValue;

            // Assert
            Assert.AreEqual(testValue, this.scaledupOrganisation.OrganisationName);
        }
    }
}