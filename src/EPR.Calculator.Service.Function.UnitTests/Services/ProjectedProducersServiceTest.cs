using EPR.Calculator.API.Data;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Services;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.Fixtures;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.Service.Function.UnitTests.Services;

[TestClass]
public class ProjectedProducersServiceTest
{
    private ApplicationDBContext _dbContext = null!;
    private ProjectedProducersService _sut = null!;

    [TestInitialize]
    public void Init()
    {
        var fixture = TestFixtures.New();
        _dbContext = fixture.Create<ApplicationDBContext>();
        TestData.SeedDatabaseForInitialRun(_dbContext);

        var bulkOps = fixture.Freeze<IBulkOperations>();

        _sut = new ProjectedProducersService(_dbContext, bulkOps);
    }

    [TestMethod]
    public async Task StoreProjectedProducers_WorksAsExpected()
    {
        var h2ProjectedProducers = new List<CalcResultH2ProjectedProducer>
        {
            new()
            {
                ProducerId = 1,
                Level = CommonConstants.LevelOne.ToString(),
                SubmissionPeriodCode = "2025-H2",
                ProjectedTonnageByMaterial = new Dictionary<string, CalcResultH2ProjectedProducerMaterialTonnage>
                {
                    {
                        "PL",
                        new CalcResultH2ProjectedProducerMaterialTonnage
                        {
                            HouseholdRAMTonnage = new RAMTonnage(),
                            PublicBinRAMTonnage = new RAMTonnage(),
                            HouseholdTonnageDefaultedRed = 10,
                            PublicBinTonnageDefaultedRed = 0,
                            TotalTonnage = 150
                        }
                    },
                    {
                        "ST",
                        new CalcResultH2ProjectedProducerMaterialTonnage
                        {
                            HouseholdRAMTonnage = new RAMTonnage(),
                            PublicBinRAMTonnage = new RAMTonnage(),
                            HouseholdTonnageDefaultedRed = 0,
                            PublicBinTonnageDefaultedRed = 0,
                            TotalTonnage = 200
                        }
                    }
                }
            },
            new()
            {
                ProducerId = 2,
                Level = CommonConstants.LevelOne.ToString(),
                IsSubtotal = true,
                SubmissionPeriodCode = "2025-H2",
                ProjectedTonnageByMaterial = new Dictionary<string, CalcResultH2ProjectedProducerMaterialTonnage>
                {
                    {
                        "GL",
                        new CalcResultH2ProjectedProducerMaterialTonnage
                        {
                            HouseholdRAMTonnage = new RAMTonnage(),
                            PublicBinRAMTonnage = new RAMTonnage(),
                            HouseholdDrinksContainerRAMTonnage = new RAMTonnage(),
                            HouseholdTonnageDefaultedRed = 10,
                            PublicBinTonnageDefaultedRed = 20,
                            HouseholdDrinksContainerDefaultedRed = 50,
                            TotalTonnage = 150
                        }
                    }
                }
            },
            new()
            {
                ProducerId = 2,
                Level = CommonConstants.LevelTwo.ToString(),
                SubmissionPeriodCode = "2025-H2",
                ProjectedTonnageByMaterial = new Dictionary<string, CalcResultH2ProjectedProducerMaterialTonnage>
                {
                    {
                        "GL",
                        new CalcResultH2ProjectedProducerMaterialTonnage
                        {
                            HouseholdRAMTonnage = new RAMTonnage(),
                            PublicBinRAMTonnage = new RAMTonnage(),
                            HouseholdDrinksContainerRAMTonnage = new RAMTonnage(),
                            HouseholdTonnageDefaultedRed = 10,
                            PublicBinTonnageDefaultedRed = 20,
                            HouseholdDrinksContainerDefaultedRed = 50,
                            TotalTonnage = 150
                        }
                    }
                }
            }
        };

        await _sut.StoreProjectedProducers(1, h2ProjectedProducers);

        var savedProjectedProducers = await _dbContext.ProducerReportedMaterialProjected.ToListAsync();
        Assert.AreEqual(16, savedProjectedProducers.Count());
        var submissionPeriods = savedProjectedProducers.GroupBy(p => p.SubmissionPeriod).ToDictionary(g => g.Key, g => g.ToList());

        var h1Group = submissionPeriods["2025-H1"];
        var h2Group = submissionPeriods["2025-H2"];
        Assert.AreEqual(6, h1Group.Count());
        Assert.AreEqual(10, h2Group.Count());

        //Existing copied into projected
        Assert.IsTrue(h1Group.Any(p => p.ProducerDetailId == 1 && p.MaterialId == 1 && p.PackagingType == PackagingTypes.Household && p.PackagingTonnage == 50 && p.PackagingTonnageRed == 50));
        Assert.IsTrue(h1Group.Any(p => p.ProducerDetailId == 1 && p.MaterialId == 3 && p.PackagingType == PackagingTypes.HouseholdDrinksContainers && p.PackagingTonnage == 150 && p.PackagingTonnageAmberMedical == 150));
        Assert.IsTrue(h2Group.Any(p => p.ProducerDetailId == 1 && p.MaterialId == 1 && p.PackagingType == PackagingTypes.Household && p.PackagingTonnage == 50 && p.PackagingTonnageAmber == 50));
        Assert.IsTrue(h2Group.Any(p => p.ProducerDetailId == 2 && p.MaterialId == 2 && p.PackagingType == PackagingTypes.PublicBin && p.PackagingTonnage == 100 && p.PackagingTonnageRedMedical == 100));

        // Additional for H2 where missing RAM defaulted as red
        Assert.IsTrue(h2Group.Any(p => p.ProducerDetailId == 1 && p.MaterialId == 1 && p.PackagingType == PackagingTypes.Household && p.PackagingTonnage == 10 && p.PackagingTonnageRed == 10));
        Assert.IsTrue(h2Group.Any(p => p.ProducerDetailId == 2 && p.MaterialId == 3 && p.PackagingType == PackagingTypes.Household && p.PackagingTonnage == 10 && p.PackagingTonnageRed == 10));
        Assert.IsTrue(h2Group.Any(p => p.ProducerDetailId == 2 && p.MaterialId == 3 && p.PackagingType == PackagingTypes.PublicBin && p.PackagingTonnage == 20 && p.PackagingTonnageRed == 20));
        Assert.IsTrue(h2Group.Any(p => p.ProducerDetailId == 2 && p.MaterialId == 3 && p.PackagingType == PackagingTypes.HouseholdDrinksContainers && p.PackagingTonnage == 50 && p.PackagingTonnageRed == 50));
    }
}