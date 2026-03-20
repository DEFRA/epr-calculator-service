using AutoFixture;
using EPR.Calculator.Service.Function.Builder;

namespace EPR.Calculator.Service.Function.UnitTests.Builder
{
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
            organisation = new Organisation();
        }

        [TestMethod]
        public void CanSetAndGetOrganisationId()
        {
            // Arrange
            var fixture = new Fixture();

            var testValue = fixture.Create<int>();

            // Act
            organisation.OrganisationId = testValue;

            // Assert
            Assert.AreEqual(testValue, organisation.OrganisationId);
        }

        [TestMethod]
        public void CanSetAndGetOrganisationName()
        {
            // Arrange
            var fixture = new Fixture();

            var testValue = fixture.Create<string>();

            // Act
            organisation.OrganisationName = testValue;

            // Assert
            Assert.AreEqual(testValue, organisation.OrganisationName);
        }
    }
}