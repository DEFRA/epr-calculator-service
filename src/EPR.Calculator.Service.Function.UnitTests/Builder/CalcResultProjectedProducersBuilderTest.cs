using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.Models;
using EPR.Calculator.Service.Function.Builder.ProjectedProducers;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Features.Calculator.Contexts;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Services;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.Fixtures;

namespace EPR.Calculator.Service.Function.UnitTests.Builder;

[TestClass]
public class CalcResultProjectedProducersBuilderTest
{
    private Mock<IMaterialService> _materialService = null!;
    private CalcResultProjectedProducersBuilder _sut = null!;
    private ApplicationDBContext dbContext = null!;

    [TestInitialize]
    public void Init()
    {
        var fixture = TestFixtures.New();
        dbContext = fixture.Freeze<ApplicationDBContext>();
        _materialService = fixture.Freeze<Mock<IMaterialService>>();

        _sut = fixture.Create<CalcResultProjectedProducersBuilder>();
    }

    private void PrepareData()
    {
        dbContext.Material.AddRange(
            new Material { Id = 1, Code = "AL", Name = "Aluminium", Description = "Aluminium" },
            new Material { Id = 2, Code = "FC", Name = "Fibre composite", Description = "Fibre composite" },
            new Material { Id = 3, Code = "GL", Name = "Glass", Description = "Glass" },
            new Material { Id = 4, Code = "PC", Name = "Paper or card", Description = "Paper or card" },
            new Material { Id = 5, Code = "PL", Name = "Plastic", Description = "Plastic" },
            new Material { Id = 6, Code = "ST", Name = "Steel", Description = "Steel" },
            new Material { Id = 7, Code = "WD", Name = "Wood", Description = "Wood" },
            new Material { Id = 8, Code = "OT", Name = "Other materials", Description = "Other materials" }
        );

        _materialService
            .Setup(mock => mock.GetMaterials(It.IsAny<CancellationToken>()))
            .ReturnsAsync(dbContext.Material.Select(m => new MaterialDto{Id = m.Id, Code = m.Code, Name = m.Name}).ToImmutableArray);

        foreach (var run in new[] { 1, 2 })
        {
            dbContext.CalculatorRuns.Add(new CalculatorRun
            {
                Id = run,
                RelativeYear = new RelativeYear(2026),
                Name = "Run " + run
            });

            var prod11 = new ProducerDetail
            {
                CalculatorRunId = run,
                ProducerId = 11,
                SubsidiaryId = null,
                ProducerName = "Producer 11 - Parent"
            };
            dbContext.ProducerDetail.Add(prod11);

            var prod22 = new ProducerDetail
            {
                CalculatorRunId = run,
                ProducerId = 11,
                SubsidiaryId = "22",
                ProducerName = "Producer 11 - Sub 22"
            };
            dbContext.ProducerDetail.Add(prod22);

            var prod33 = new ProducerDetail
            {
                CalculatorRunId = run,
                ProducerId = 33,
                SubsidiaryId = null,
                ProducerName = "Producer 33 - No subs"
            };
            dbContext.ProducerDetail.Add(prod33);

            var prod44 = new ProducerDetail
            {
                CalculatorRunId = run,
                ProducerId = 44,
                SubsidiaryId = "444",
                ProducerName = "Producer 44 - Sub 444 - No parent"
            };
            dbContext.ProducerDetail.Add(prod44);

            foreach (var subPeriod in new[] { "2025-H1", "2025-H2" })
            {
                dbContext.ProducerReportedMaterial.Add(new ProducerReportedMaterial
                {
                    ProducerDetailId = prod11.Id,
                    MaterialId = 1,
                    PackagingType = "HH",
                    PackagingTonnage = 100,
                    PackagingTonnageRed = 30,
                    PackagingTonnageRedMedical = 40,
                    PackagingTonnageAmber = 40,
                    PackagingTonnageAmberMedical = 0,
                    PackagingTonnageGreen = null,
                    PackagingTonnageGreenMedical = null,
                    SubmissionPeriod = subPeriod
                });
                dbContext.ProducerReportedMaterial.Add(new ProducerReportedMaterial
                {
                    ProducerDetailId = prod22.Id,
                    MaterialId = 3,
                    PackagingType = "HDC",
                    PackagingTonnage = 500,
                    PackagingTonnageRed = null,
                    PackagingTonnageRedMedical = null,
                    PackagingTonnageAmber = null,
                    PackagingTonnageAmberMedical = null,
                    PackagingTonnageGreen = null,
                    PackagingTonnageGreenMedical = null,
                    SubmissionPeriod = subPeriod
                });
                dbContext.ProducerReportedMaterial.Add(new ProducerReportedMaterial
                {
                    ProducerDetailId = prod33.Id,
                    MaterialId = 1,
                    PackagingType = "PB",
                    PackagingTonnage = 150,
                    PackagingTonnageRed = 10,
                    PackagingTonnageRedMedical = 40,
                    PackagingTonnageAmber = 20,
                    PackagingTonnageAmberMedical = 30,
                    PackagingTonnageGreen = 5,
                    PackagingTonnageGreenMedical = 45,
                    SubmissionPeriod = subPeriod
                });
                dbContext.ProducerReportedMaterial.Add(new ProducerReportedMaterial
                {
                    ProducerDetailId = prod44.Id,
                    MaterialId = 1,
                    PackagingType = "HH",
                    PackagingTonnage = 150,
                    SubmissionPeriod = subPeriod
                });
            }
        }

        dbContext.SaveChanges();
    }

    [TestMethod]
    public async Task Construct_H2Correctly()
    {
        // Arrange
        PrepareData();
        var runContext = TestFixtures.Default.Create<CalculatorRunContext>();

        // Act
        var result = await _sut.ConstructAsync(runContext);

        // Assert
        Assert.AreEqual(6, result.H2ProjectedProducers!.Count());

        //Producer 11 - H2 projections
        var prod11 = result.H2ProjectedProducers!.FirstOrDefault(p => p.ProducerId == 11 && p.SubsidiaryId == null && !p.IsSubtotal);
        Assert.IsTrue(prod11 != null);
        Assert.IsTrue(prod11.ProducerId == 11);
        Assert.IsTrue(prod11.SubsidiaryId == null);
        Assert.IsTrue(prod11.Level == CommonConstants.LevelTwo.ToString());
        Assert.IsTrue(prod11.SubmissionPeriodCode == "2025-H2");

        var prod11Mats = prod11.ProjectedTonnageByMaterial!;
        var expProd11MatAl = new RAMTonnage { Tonnage = 100, RedTonnage = 30, RedMedicalTonnage = 40, AmberTonnage = 40, AmberMedicalTonnage = 0, GreenTonnage = 0, GreenMedicalTonnage = 0 };
        var expProd11AlDefaultRed = 0;
        var expProd11AlTotalTonnage = 100;
        Assert.AreEqual(8, prod11Mats.Count());
        var prod11Al = prod11Mats["AL"];
        Assert.AreEqual(expProd11MatAl, prod11Al.HouseholdRAMTonnage);
        Assert.AreEqual(expProd11AlDefaultRed, prod11Al.HouseholdTonnageDefaultedRed);
        Assert.AreEqual(expProd11AlTotalTonnage, prod11Al.TotalTonnage);

        //Producer 11 - Sub 22 - H2 projections
        var prod11Sub22 = result.H2ProjectedProducers!.FirstOrDefault(p => p.ProducerId == 11 && p.SubsidiaryId == "22" && !p.IsSubtotal);
        Assert.IsTrue(prod11Sub22 != null);
        Assert.IsTrue(prod11Sub22.ProducerId == 11);
        Assert.IsTrue(prod11Sub22.SubsidiaryId == "22");
        Assert.IsTrue(prod11Sub22.Level == CommonConstants.LevelTwo.ToString());
        Assert.IsTrue(prod11Sub22.SubmissionPeriodCode == "2025-H2");

        var prod11Sub22Mats = prod11Sub22.ProjectedTonnageByMaterial!;
        var expProd11Sub22HDC = new RAMTonnage { Tonnage = 500, RedTonnage = 0, RedMedicalTonnage = 0, AmberTonnage = 0, AmberMedicalTonnage = 0, GreenTonnage = 0, GreenMedicalTonnage = 0 };
        var expProd11Sub22HDCDefaultRed = 500;
        var expProd11Sub22HDCTotalTonnage = 500;
        Assert.AreEqual(8, prod11Sub22Mats.Count());
        var prod11Sub22Glass = prod11Sub22Mats["GL"];
        Assert.AreEqual(expProd11Sub22HDC, prod11Sub22Glass.HouseholdDrinksContainerRAMTonnage);
        Assert.AreEqual(expProd11Sub22HDCDefaultRed, prod11Sub22Glass.HouseholdDrinksContainerDefaultedRed);
        Assert.AreEqual(expProd11Sub22HDCTotalTonnage, prod11Sub22Glass.TotalTonnage);

        //Subtotal H2 projections for prod 11
        var prod11Subtotal = result.H2ProjectedProducers!.FirstOrDefault(p => p.ProducerId == 11 && p.SubsidiaryId == null && p.IsSubtotal);
        Assert.IsTrue(prod11Subtotal != null);
        Assert.IsTrue(prod11Subtotal.ProducerId == 11);
        Assert.IsTrue(prod11Subtotal.SubsidiaryId == null);
        Assert.IsTrue(prod11Subtotal.Level == CommonConstants.LevelOne.ToString());
        Assert.IsTrue(prod11Subtotal.SubmissionPeriodCode == "2025-H2");

        var prod11SubtotalMats = prod11Subtotal.ProjectedTonnageByMaterial!;
        Assert.AreEqual(8, prod11SubtotalMats.Count());
        var prod11SubtotalAl = prod11SubtotalMats["AL"];
        var prod11SubtotalGlass = prod11SubtotalMats["GL"];
        Assert.AreEqual(expProd11MatAl, prod11SubtotalAl.HouseholdRAMTonnage);
        Assert.AreEqual(expProd11AlDefaultRed, prod11SubtotalAl.HouseholdTonnageDefaultedRed);
        Assert.AreEqual(expProd11Sub22HDC, prod11SubtotalGlass.HouseholdDrinksContainerRAMTonnage);
        Assert.AreEqual(expProd11Sub22HDCDefaultRed, prod11SubtotalGlass.HouseholdDrinksContainerDefaultedRed);
        Assert.AreEqual(expProd11AlTotalTonnage, prod11SubtotalAl.TotalTonnage);
        Assert.AreEqual(expProd11Sub22HDCTotalTonnage, prod11SubtotalGlass.TotalTonnage);

        //Producer 33 - H2 projections
        var prod33 = result.H2ProjectedProducers!.FirstOrDefault(p => p.ProducerId == 33 && p.SubsidiaryId == null && !p.IsSubtotal);
        Assert.IsFalse(result.H2ProjectedProducers!.FirstOrDefault(p => p.ProducerId == 33 && p.SubsidiaryId == null && p.IsSubtotal) != null, "Producer 33 should not have a subtotal record as there is only one record for this producer");
        Assert.IsTrue(prod33 != null);
        Assert.IsTrue(prod33.ProducerId == 33);
        Assert.IsTrue(prod33.SubsidiaryId == null);
        Assert.IsTrue(prod33.Level == CommonConstants.LevelOne.ToString());
        Assert.IsTrue(prod33.SubmissionPeriodCode == "2025-H2");

        //Producer 44 - H2 projections
        var prod44Subtotal = result.H2ProjectedProducers!.FirstOrDefault(p => p.ProducerId == 44 && p.SubsidiaryId == null && p.IsSubtotal);
        var prod44 = result.H2ProjectedProducers!.FirstOrDefault(p => p.ProducerId == 44 && p.SubsidiaryId == "444" && !p.IsSubtotal);
        Assert.IsTrue(prod44Subtotal != null);
        Assert.IsTrue(prod44 != null);
        Assert.IsTrue(prod44Subtotal.ProducerId == 44);
        Assert.IsTrue(prod44Subtotal.SubsidiaryId == null);
        Assert.IsTrue(prod44Subtotal.Level == CommonConstants.LevelOne.ToString());
        Assert.IsTrue(prod44Subtotal.SubmissionPeriodCode == "2025-H2");

        Assert.IsTrue(prod44.ProducerId == 44);
        Assert.IsTrue(prod44.SubsidiaryId == "444");
        Assert.IsTrue(prod44.Level == CommonConstants.LevelTwo.ToString());
        Assert.IsTrue(prod44.SubmissionPeriodCode == "2025-H2");

        Assert.AreEqual(prod44Subtotal.ProjectedTonnageByMaterial!["AL"].TotalTonnage, prod44.ProjectedTonnageByMaterial!["AL"].TotalTonnage);
    }
}