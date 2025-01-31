namespace EPR.Calculator.Service.Function.UnitTests
{
    using System;
    using AutoFixture;
    using EPR.Calculator.Service.Function.Data;
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

        private Mock<IConfiguration> Configuration { get; }

        public CommandTimeoutServiceTests()
        {
            this.Fixture = new Fixture();
            this.Configuration = new Mock<IConfiguration>();
            this.TestClass = new CommandTimeoutService(this.Configuration.Object);
        }

        [TestMethod]
        public void CanConstruct()
        {
            // Act
            var instance = new CommandTimeoutService();

            // Assert
            Assert.IsNotNull(instance);

            // Act
            instance = new CommandTimeoutService(this.Configuration.Object);

            // Assert
            Assert.IsNotNull(instance);
        }

        [TestMethod]
        public void CanCallSetCommandTimeout()
        {
            // Arrange
            var context = new Mock<DbContext>();
            var database = new TestDbFacade(new ApplicationDBContext());
            var key = this.Fixture.Create<string>();
            var timeoutValue = this.Fixture.Create<double>();
            var expectedResult = (int)(timeoutValue * 60);

            var timeoutValueSection = new Mock<IConfigurationSection>();
            timeoutValueSection.Setup(s => s.Value).Returns(timeoutValue.ToString());
            var timeoutsSection = new Mock<IConfigurationSection>();
            timeoutsSection.Setup(s => s.GetSection(key)).Returns(timeoutValueSection.Object);
            this.Configuration.Setup(c => c.GetSection("Timeouts")).Returns(timeoutsSection.Object);

            // Act
            this.TestClass.SetCommandTimeout(database, key);
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