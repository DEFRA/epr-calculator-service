using AutoFixture;
using EPR.Calculator.API.Data;
using EPR.Calculator.Service.Function.Interface;
using EPR.Calculator.Service.Function.Misc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Moq;

namespace EPR.Calculator.Service.Function.UnitTests.Misc
{
    [TestClass]
    public class CommandTimeoutServiceTests
    {
        private CommandTimeoutService TestClass { get; }

        private IFixture Fixture { get; }

        private Mock<IConfigurationService> Configuration { get; }

        public CommandTimeoutServiceTests()
        {
            Fixture = new Fixture();
            Configuration = new Mock<IConfigurationService>();
            TestClass = new CommandTimeoutService(Configuration.Object);
        }

        [TestMethod]
        public void CanConstruct()
        {
            // Act
            var instance = new CommandTimeoutService(Configuration.Object);

            // Assert
            Assert.IsNotNull(instance);
        }

        [TestMethod]
        public void CanCallSetCommandTimeout()
        {
            // Arrange
            var database = new TestDbFacade(new ApplicationDBContext());
            var key = Fixture.Create<string>();
            var timeoutValue = Fixture.Create<TimeSpan>();
            var expectedResult = (int)(timeoutValue.TotalSeconds);

            var timeoutValueSection = new Mock<IConfigurationSection>();
            timeoutValueSection.Setup(s => s.Value).Returns(timeoutValue.ToString());
            var timeoutsSection = new Mock<IConfigurationSection>();
            timeoutsSection.Setup(s => s.GetSection(key)).Returns(timeoutValueSection.Object);
            Configuration.Setup(c => c.CommandTimeout).Returns(timeoutValue);

            // Act
            TestClass.SetCommandTimeout(database);
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