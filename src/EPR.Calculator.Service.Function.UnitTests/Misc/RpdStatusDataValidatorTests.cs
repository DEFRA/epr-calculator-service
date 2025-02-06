namespace EPR.Calculator.Service.Function.UnitTests
{
    using System;
    using System.Collections.Generic;
    using AutoFixture;
    using EPR.Calculator.API.Validators;
    using EPR.Calculator.Service.Function.Data.DataModels;
    using EPR.Calculator.Service.Function.Interface;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class RpdStatusDataValidatorTests
    {
        private RpdStatusDataValidator _testClass;
        private Mock<IOrgAndPomWrapper> _wrapper;

        [TestInitialize]
        public void SetUp()
        {
            _wrapper = new Mock<IOrgAndPomWrapper>();
            _testClass = new RpdStatusDataValidator(_wrapper.Object);
        }

        [TestMethod]
        public void CanConstruct()
        {
            // Act
            var instance = new RpdStatusDataValidator(_wrapper.Object);

            // Assert
            Assert.IsNotNull(instance);
        }

        [TestMethod]
        public void CannotConstructWithNullWrapper()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new RpdStatusDataValidator(default(IOrgAndPomWrapper)));
        }

        [TestMethod]
        public void CanCallIsValidRun()
        {
            // Arrange
            var fixture = new Fixture();
            var calcRun = fixture.Create<CalculatorRun>();
            var runId = fixture.Create<int>();
            var calculatorRunClassifications = new Mock<IEnumerable<CalculatorRunClassification>>().Object;

            // Act
            var result = _testClass.IsValidRun(calcRun, runId, calculatorRunClassifications);

            // Assert
            Assert.Fail("Create or modify test");
        }

        [TestMethod]
        public void CannotCallIsValidRunWithNullCalculatorRunClassifications()
        {
            // Arrange
            var fixture = new Fixture();
            Assert.ThrowsException<ArgumentNullException>(() => _testClass.IsValidRun(fixture.Create<CalculatorRun>(), fixture.Create<int>(), default(IEnumerable<CalculatorRunClassification>)));
        }

        [TestMethod]
        public void CanCallIsValidSuccessfulRun()
        {
            // Arrange
            var fixture = new Fixture();
            var runId = fixture.Create<int>();

            _wrapper.Setup(mock => mock.AnyPomData()).Returns(fixture.Create<bool>());
            _wrapper.Setup(mock => mock.AnyOrganisationData()).Returns(fixture.Create<bool>());

            // Act
            var result = _testClass.IsValidSuccessfulRun(runId);

            // Assert
            _wrapper.Verify(mock => mock.AnyPomData());
            _wrapper.Verify(mock => mock.AnyOrganisationData());

            Assert.Fail("Create or modify test");
        }
    }
}