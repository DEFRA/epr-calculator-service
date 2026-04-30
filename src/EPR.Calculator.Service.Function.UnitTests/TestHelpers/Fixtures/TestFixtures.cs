using AutoFixture.AutoMoq;
using EPR.Calculator.API.Data;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Services;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.Fixtures.Customizations;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.Utils;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace EPR.Calculator.Service.Function.UnitTests.TestHelpers.Fixtures;

public interface IFixtureOptions
{
    /// <summary>
    ///     Use Sqlite instead of InMemoryDatabase.
    /// </summary>
    IFixtureOptions UseSqlLite();

    /// <summary>
    ///     Populate the database with seed data.
    /// </summary>
    IFixtureOptions IncludeSeedData();
}

public static class TestFixtures
{
    /// <summary>
    ///     ⛔ Don't use with new tests. This is a static fixture for legacy test usage.
    /// </summary>
    public static readonly IFixture Legacy = GetDefaultFixture();

    /// <summary>
    ///     To be used in tests that require ApplicationDBContext.
    /// </summary>
    public static IFixture New(Action<IFixtureOptions>? opts = null)
    {
        var fixtureOptions = new FixtureOptions();
        opts?.Invoke(fixtureOptions);

        var fixture = new Fixture()
            .Customize(new AutoMoqCustomization())
            .Customize(new ImmutableCollectionsCustomization())
            .Customize(new IgnoreVirtualMembersCustomization());

        fixture.Register(() =>
        {
            var options = new DbContextOptionsBuilder<ApplicationDBContext>();

            if (fixtureOptions.SqlLiteEnabled)
            {
                var connection = new SqliteConnection("Data Source=:memory:");
                connection.Open();
                options.UseSqlite(connection);
            }
            else
            {
                options
                    .UseInMemoryDatabase(new Fixture().Create<string>()) // Unique each time to avoid pollution across tests
                    .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning));
            }

            var dbContext = new ApplicationDBContext(options.Options);
            dbContext.Database.EnsureCreated();

            if (fixtureOptions.SeedDataEnabled)
            {
                // ⚠️ Incomplete: needs more seed data for SQLite to work without causing foreign key exceptions.
                dbContext.CalculatorRunRelativeYears.AddRange(DbSeedData.RelativeYears);
                dbContext.SaveChanges();

                dbContext.CalculatorRunOrganisationDataMaster.AddRange(DbSeedData.OrganisationMasters);
                dbContext.SaveChanges();

                dbContext.CalculatorRunPomDataMaster.AddRange(DbSeedData.PomMasters);
                dbContext.SaveChanges();

                dbContext.CalculatorRuns.AddRange(DbSeedData.CalculatorRuns);
                dbContext.SaveChanges();

                dbContext.Material.AddRange(DbSeedData.Materials);
                dbContext.SaveChanges();

                dbContext.ProducerResultFileSuggestedBillingInstruction.AddRange(DbSeedData.SuggestedBillingInstruction);
                dbContext.SaveChanges();
            }

            return dbContext;
        });

        fixture.Register<TimeProvider>(() => new FakeTimeProvider(new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero)));
        fixture.Register<IBulkOperations>(() => new TestBulkOps());

        fixture.Register(() => DbSeedData.Defaults.ValidCalculatorRunContext);
        fixture.Register(() => DbSeedData.Defaults.ValidBillingRunContext);

        return fixture;
    }

    private static IFixture GetDefaultFixture()
    {
        var fixture = new Fixture()
            .Customize(new AutoMoqCustomization())
            .Customize(new ImmutableCollectionsCustomization())
            .Customize(new IgnoreVirtualMembersCustomization());

        fixture.Register(() => TestDataHelper.GetCalcResult());
        fixture.Register(() => TestDataHelper.CalculatorRunContext);
        fixture.Register(() => TestDataHelper.BillingRunContext);

        return fixture;
    }

    private record FixtureOptions : IFixtureOptions
    {
        public bool SqlLiteEnabled { get; private set; }
        public bool SeedDataEnabled { get; private set; }

        public IFixtureOptions UseSqlLite()
        {
            SqlLiteEnabled = true;
            return this;
        }

        public IFixtureOptions IncludeSeedData()
        {
            SeedDataEnabled = true;
            return this;
        }
    }
}
