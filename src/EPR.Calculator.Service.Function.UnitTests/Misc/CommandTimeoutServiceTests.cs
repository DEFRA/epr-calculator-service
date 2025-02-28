namespace EPR.Calculator.Service.Function.UnitTests
{
    using System;
    using AutoFixture;
    using EPR.Calculator.Service.Function.Data;
    using EPR.Calculator.Service.Function.Interface;
    using EPR.Calculator.Service.Function.Services;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Infrastructure;
    using Microsoft.Extensions.Configuration;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Newtonsoft.Json.Linq;

    [TestClass]
    public class CommandTimeoutServiceTests
    {
        private CommandTimeoutService TestClass { get; }

        private IFixture Fixture { get; }

        private Mock<IConfigurationService> Configuration { get; }

        public CommandTimeoutServiceTests()
        {
            this.Fixture = new Fixture();
            this.Configuration = new Mock<IConfigurationService>();
            this.TestClass = new CommandTimeoutService(this.Configuration.Object);
        }

        [TestMethod]
        public void CanConstruct()
        {
            // Act
            var instance = new CommandTimeoutService(this.Configuration.Object);

            // Assert
            Assert.IsNotNull(instance);
        }

        [TestMethod]
        public void CanCallSetCommandTimeout()
        {
            // Arrange
            var database = new TestDbFacade(new ApplicationDBContext());
            var key = this.Fixture.Create<string>();
            var timeoutValue = this.Fixture.Create<TimeSpan>();
            var expectedResult = (int)(timeoutValue.TotalSeconds);

            var timeoutValueSection = new Mock<IConfigurationSection>();
            timeoutValueSection.Setup(s => s.Value).Returns(timeoutValue.ToString());
            var timeoutsSection = new Mock<IConfigurationSection>();
            timeoutsSection.Setup(s => s.GetSection(key)).Returns(timeoutValueSection.Object);
            this.Configuration.Setup(c => c.CommandTimeout).Returns(timeoutValue);

            // Act
            this.TestClass.SetCommandTimeout(database);
            var result = database.GetCommandTimeout();

            // Assert
            Assert.AreEqual(expectedResult, result);
        }

        private class TestDbFacade : DatabaseFacade
        {
            public TestDbFacade(DbContext context)
                : base(context)
            {
            }
        }
    }
}