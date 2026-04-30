namespace EPR.Calculator.Service.Function.UnitTests.Builder.ProjectedProducers
{
    using EPR.Calculator.API.Data.DataModels;
    using EPR.Calculator.Service.Function.Builder.ProjectedProducers;
    using EPR.Calculator.Service.Function.Constants;
    using EPR.Calculator.Service.Function.Models;

    [TestClass]
    public class H2ProjectedProducersBuilderUtilsTest
    {
        private ImmutableArray<MaterialDto> materials =
        [
            new MaterialDto { Id = 1, Code = "AL", Name = "Aluminium" },
            new MaterialDto { Id = 2, Code = "GL", Name = "Glass" }
        ];

        private List<ProducerReportedMaterialsForSubmissionPeriod> reportedMaterials = new List<ProducerReportedMaterialsForSubmissionPeriod>()
        {
            new(11, null, "2026-H2", new List<ProducerReportedMaterial>
                {
                    new ProducerReportedMaterial
                    {
                        MaterialId = 1,
                        PackagingType = PackagingTypes.Household,
                        PackagingTonnage = 100,
                        PackagingTonnageRed = 30,
                        PackagingTonnageRedMedical = 40,
                        PackagingTonnageAmber = 40,
                        PackagingTonnageAmberMedical = 0,
                        PackagingTonnageGreen = null,
                        PackagingTonnageGreenMedical = null,
                        SubmissionPeriod = "2026-H2"
                    }
                }
            ),
            new(11, "22", "2026-H2", new List<ProducerReportedMaterial>
                {
                    new ProducerReportedMaterial
                    {
                        MaterialId = 2,
                        PackagingType = PackagingTypes.HouseholdDrinksContainers,
                        PackagingTonnage = 500,
                        PackagingTonnageRed = null,
                        PackagingTonnageRedMedical = null,
                        PackagingTonnageAmber = null,
                        PackagingTonnageAmberMedical = null,
                        PackagingTonnageGreen = null,
                        PackagingTonnageGreenMedical = null,
                        SubmissionPeriod = "2026-H2"
                    }
                }
            )
        };

        [TestMethod]
        public void GetProjectedProducers()
        {
            var result = H2ProjectedProducersBuilderUtils.GetProjectedProducers(reportedMaterials, materials);

            Assert.AreEqual(2, result.Count());

            var prod11 = result.FirstOrDefault(p => p.ProducerId == 11 && p.SubsidiaryId == null);
            Assert.IsNotNull(prod11);
            Assert.AreEqual(11, prod11.ProducerId);
            Assert.AreEqual(null, prod11.SubsidiaryId);
            Assert.AreEqual(string.Empty, prod11.Level);
            Assert.AreEqual("2026-H2", prod11.SubmissionPeriodCode);

            var prod11Mats = prod11.H2ProjectedTonnageByMaterial;
            var expProd11MatAl = new RAMTonnage{ Tonnage = 100, RedTonnage = 30, RedMedicalTonnage = 40, AmberTonnage = 40, AmberMedicalTonnage = 0, GreenTonnage = 0, GreenMedicalTonnage = 0 };
            var expProd11AlDefaultRed = 0;
            var expProd11AlTotalTonnage = 100;
            var prod11Al = prod11Mats[MaterialCodes.Aluminium];
            Assert.AreEqual(2, prod11Mats.Count());
            Assert.AreEqual(expProd11MatAl, prod11Al.HouseholdRAMTonnage);
            Assert.AreEqual(expProd11AlDefaultRed, prod11Al.HouseholdTonnageWithoutRAM);
            Assert.AreEqual(expProd11AlTotalTonnage, prod11Al.TotalTonnage);
            Assert.AreEqual(expProd11MatAl, prod11Al.ProjectedHouseholdRAMTonnage);

            var prod11Sub22 = result.FirstOrDefault(p => p.ProducerId == 11 && p.SubsidiaryId == "22");
            Assert.IsNotNull(prod11Sub22);
            Assert.AreEqual(11, prod11Sub22.ProducerId);
            Assert.AreEqual("22", prod11Sub22.SubsidiaryId);
            Assert.AreEqual(string.Empty, prod11Sub22.Level);
            Assert.AreEqual("2026-H2", prod11Sub22.SubmissionPeriodCode);

            var prod11Sub22Mats = prod11Sub22.H2ProjectedTonnageByMaterial;
            var expProd11Sub22HDC = new RAMTonnage{ Tonnage = 500, RedTonnage = 0, RedMedicalTonnage = 0, AmberTonnage = 0, AmberMedicalTonnage = 0, GreenTonnage = 0, GreenMedicalTonnage = 0 };
            var expProd11Sub22HDCDefaultRed = 500;
            var expProd11Sub22HDCTotalTonnage = 500;
            var prod11Sub22Glass = prod11Sub22Mats[MaterialCodes.Glass];
            Assert.AreEqual(2, prod11Sub22Mats.Count());
            Assert.AreEqual(expProd11Sub22HDC, prod11Sub22Glass.HouseholdDrinksContainerRAMTonnage);
            Assert.AreEqual(expProd11Sub22HDCDefaultRed, prod11Sub22Glass.HouseholdDrinksContainerTonnageWithoutRAM);
            Assert.AreEqual(expProd11Sub22HDCTotalTonnage, prod11Sub22Glass.TotalTonnage);
            Assert.AreEqual(expProd11Sub22HDC with { RedTonnage = 500 }, prod11Sub22Glass.ProjectedHouseholdDrinksContainerRAMTonnage);
        }
    }
}