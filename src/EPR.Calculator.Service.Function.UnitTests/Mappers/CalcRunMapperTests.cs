namespace EPR.Calculator.Service.Function.UnitTests
{
    using AutoFixture;
    using EPR.Calculator.API.Data.DataModels;
    using EPR.Calculator.Service.Function.Mappers;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Unit tests for the <see cref="CalcRunMapper"/> class.
    /// </summary>
    [TestClass]
    public class CalcRunMapperTests
    {
        private IFixture Fixture { get; } = new Fixture();

        /// <summary>
        /// Checks that <see cref="CalcRunMapper.Map(CalculatorRun, CalculatorRunClassification)"/>
        /// maps the properties correctly.
        /// </summary>
        [TestMethod]
        public void MapPerformsMapping()
        {
            // Arrange
            var run = this.Fixture.Create<CalculatorRun>();
            var classification = this.Fixture.Create<CalculatorRunClassification>();

            // Act
            var result = CalcRunMapper.Map(run, classification);

            // Assert
            Assert.AreEqual(run.CreatedAt, result.CreatedAt);
            Assert.AreSame(run.UpdatedBy, result.UpdatedBy);
            Assert.AreEqual(run.UpdatedAt, result.UpdatedAt);
        }
    }
}