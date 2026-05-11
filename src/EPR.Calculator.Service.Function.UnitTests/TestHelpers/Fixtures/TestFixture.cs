using EPR.Calculator.API.Data;
using EPR.Calculator.Service.Function.Services;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.Fixtures.Customizations;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace EPR.Calculator.Service.Function.UnitTests.TestHelpers.Fixtures;

public static class TestFixtures
{
    /// <summary>
    ///     Creates a new AutoFixture instance with many supporting services pre-registered and configured.
    /// </summary>
    public static IFixture New()
    {
        var fixture = new Fixture()
            .Customize(new AutoFreezeMoqCustomization())
            .Customize(new ImmutableCollectionsCustomization())
            .Customize(new IgnoreVirtualMembersCustomization());

        fixture.Register(() =>
        {
            var options = new DbContextOptionsBuilder<ApplicationDBContext>();

            options
                .UseInMemoryDatabase(new Fixture().Create<string>()) // Unique each time to avoid pollution across tests
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning));

            var dbContext = new ApplicationDBContext(options.Options);
            dbContext.Database.EnsureCreated();

            return dbContext;
        });

        fixture.Register<IBulkOperations>(() => new TestBulkOps());

        return fixture;
    }
}
