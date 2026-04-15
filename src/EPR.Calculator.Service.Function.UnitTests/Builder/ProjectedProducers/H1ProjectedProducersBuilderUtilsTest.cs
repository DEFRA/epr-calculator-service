namespace EPR.Calculator.Service.Function.UnitTests.Builder.ProjectedProducers
{
    using EPR.Calculator.API.Data;
    using EPR.Calculator.API.Data.DataModels;
    using EPR.Calculator.API.Data.Models;
    using EPR.Calculator.Service.Common;
    using EPR.Calculator.Service.Function.Builder.ProjectedProducers;
    using EPR.Calculator.Service.Function.Constants;
    using EPR.Calculator.Service.Function.Mappers;
    using EPR.Calculator.Service.Function.Misc;

    using EPR.Calculator.Service.Function.Models;
    using EPR.Calculator.Service.Function.Services;

    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Diagnostics;
    using static EPR.Calculator.Service.Function.UnitTests.Builder.CalcRunLaDisposalCostBuilderTests;

    [TestClass]
    public class H1ProjectedProducersBuilderUtilsTest
    {
        private List<MaterialDetail> materials = new List<MaterialDetail>()
        {
            new MaterialDetail { Id = 1, Code = "AL", Name = "Aluminium", Description = "Aluminium" },
            new MaterialDetail { Id = 2, Code = "GL", Name = "Glass", Description = "Glass" },
            new MaterialDetail { Id = 3, Code = "OT", Name = "Other materials", Description = "Other materials" }
        };

        private RAMTonnage EmptyRAMTonnage = new RAMTonnage { Tonnage = 0, RedTonnage = 0, AmberTonnage = 0, GreenTonnage = 0, RedMedicalTonnage = 0, AmberMedicalTonnage = 0, GreenMedicalTonnage = 0 };

        [TestMethod]
        public async Task GetProjectedProducers_With_H1_NoRam_H2_FullRam()
        {
            var (h1HHAlm, h1HdcGlass, h1NoRamReportedMaterials) = GetH1NoRamReportedMaterials();
            var (h2Alm, h2Glass, h2FullRamProjectedProducers) = GetH2FullRamProjectedProducers();
            var result = await H1ProjectedProducersBuilderUtils.GetProjectedProducers(h1NoRamReportedMaterials, h2FullRamProjectedProducers, materials);

            Assert.AreEqual(1, result.Count());
            Assert.AreEqual(3, result.First().ProjectedTonnageByMaterial.Count());

            var projectedAluminium = result.First().ProjectedTonnageByMaterial[MaterialCodes.Aluminium];
            var projectedGlass = result.First().ProjectedTonnageByMaterial[MaterialCodes.Glass];

            Assert.AreEqual(ProducerReportedMaterialToRAMTonnage(h1HHAlm), projectedAluminium.HouseholdRAMTonnage);
            Assert.AreEqual(EmptyRAMTonnage, projectedAluminium.PublicBinRAMTonnage);
            Assert.IsNull(projectedAluminium.HouseholdDrinksContainerRAMTonnage);
            Assert.AreEqual(EmptyRAMTonnage, projectedGlass.HouseholdRAMTonnage);
            Assert.AreEqual(EmptyRAMTonnage, projectedGlass.PublicBinRAMTonnage);
            Assert.AreEqual(ProducerReportedMaterialToRAMTonnage(h1HdcGlass), projectedGlass.HouseholdDrinksContainerRAMTonnage);

            Assert.AreEqual(h1HHAlm.PackagingTonnage, projectedAluminium.HouseholdTonnageWithoutRAM);
            Assert.AreEqual(0, projectedAluminium.PublicBinTonnageWithoutRAM);
            Assert.IsNull(projectedAluminium.HouseholdDrinksContainerTonnageWithoutRAM);
            Assert.AreEqual(0, projectedGlass.HouseholdTonnageWithoutRAM);
            Assert.AreEqual(0, projectedGlass.PublicBinTonnageWithoutRAM);
            Assert.AreEqual(h1HdcGlass.PackagingTonnage, projectedGlass.HouseholdDrinksContainerTonnageWithoutRAM);

            var expAlmRedH2Proportion = Math.Round((h2Alm.HouseholdRAMTonnage.RedTonnage + h2Alm.PublicBinRAMTonnage.RedTonnage) / h2Alm.TotalTonnage, 6);
            var expAlmAmberH2Proportion = Math.Round((h2Alm.HouseholdRAMTonnage.AmberTonnage + h2Alm.PublicBinRAMTonnage.AmberTonnage) / h2Alm.TotalTonnage, 6);
            var expAlmGreenH2Proportion = Math.Round((h2Alm.HouseholdRAMTonnage.GreenTonnage + h2Alm.PublicBinRAMTonnage.GreenTonnage) / h2Alm.TotalTonnage, 6);
            var expAlmRedMedicalH2Proportion = Math.Round((h2Alm.HouseholdRAMTonnage.RedMedicalTonnage + h2Alm.PublicBinRAMTonnage.RedMedicalTonnage) / h2Alm.TotalTonnage, 6);
            var expAlmAmberMedicalH2Proportion = Math.Round((h2Alm.HouseholdRAMTonnage.AmberMedicalTonnage + h2Alm.PublicBinRAMTonnage.AmberMedicalTonnage) / h2Alm.TotalTonnage, 6);
            var expAlmGreenMedicalH2Proportion = Math.Round((h2Alm.HouseholdRAMTonnage.GreenMedicalTonnage + h2Alm.PublicBinRAMTonnage.GreenMedicalTonnage) / h2Alm.TotalTonnage, 6); 
            Assert.AreEqual(expAlmRedH2Proportion, projectedAluminium.RedH2Proportion);
            Assert.AreEqual(expAlmAmberH2Proportion, projectedAluminium.AmberH2Proportion);
            Assert.AreEqual(expAlmGreenH2Proportion, projectedAluminium.GreenH2Proportion);
            Assert.AreEqual(expAlmRedMedicalH2Proportion, projectedAluminium.RedMedicalH2Proportion);
            Assert.AreEqual(expAlmAmberMedicalH2Proportion, projectedAluminium.AmberMedicalH2Proportion);
            Assert.AreEqual(expAlmGreenMedicalH2Proportion, projectedAluminium.GreenMedicalH2Proportion);

            var expGlassRedH2Proportion = Math.Round((h2Glass.HouseholdRAMTonnage.RedTonnage + h2Glass.PublicBinRAMTonnage.RedTonnage + (h2Glass.HouseholdDrinksContainerRAMTonnage?.RedTonnage ?? 0)) / h2Glass.TotalTonnage, 6);
            var expGlassAmberH2Proportion = Math.Round((h2Glass.HouseholdRAMTonnage.AmberTonnage + h2Glass.PublicBinRAMTonnage.AmberTonnage + (h2Glass.HouseholdDrinksContainerRAMTonnage?.AmberTonnage ?? 0)) / h2Glass.TotalTonnage, 6);
            var expGlassGreenH2Proportion = Math.Round((h2Glass.HouseholdRAMTonnage.GreenTonnage + h2Glass.PublicBinRAMTonnage.GreenTonnage + (h2Glass.HouseholdDrinksContainerRAMTonnage?.GreenTonnage ?? 0)) / h2Glass.TotalTonnage, 6);
            var expGlassRedMedicalH2Proportion = Math.Round((h2Glass.HouseholdRAMTonnage.RedMedicalTonnage + h2Glass.PublicBinRAMTonnage.RedMedicalTonnage + (h2Glass.HouseholdDrinksContainerRAMTonnage?.RedMedicalTonnage ?? 0)) / h2Glass.TotalTonnage, 6);
            var expGlassAmberMedicalH2Proportion = Math.Round((h2Glass.HouseholdRAMTonnage.AmberMedicalTonnage + h2Glass.PublicBinRAMTonnage.AmberMedicalTonnage + (h2Glass.HouseholdDrinksContainerRAMTonnage?.AmberMedicalTonnage ?? 0)) / h2Glass.TotalTonnage, 6);
            var expGlassGreenMedicalH2Proportion = Math.Round((h2Glass.HouseholdRAMTonnage.GreenMedicalTonnage + h2Glass.PublicBinRAMTonnage.GreenMedicalTonnage + (h2Glass.HouseholdDrinksContainerRAMTonnage?.GreenMedicalTonnage ?? 0)) / h2Glass.TotalTonnage, 6);
            Assert.AreEqual(expGlassRedH2Proportion, projectedGlass.RedH2Proportion);
            Assert.AreEqual(expGlassAmberH2Proportion, projectedGlass.AmberH2Proportion);
            Assert.AreEqual(expGlassGreenH2Proportion, projectedGlass.GreenH2Proportion);
            Assert.AreEqual(expGlassRedMedicalH2Proportion, projectedGlass.RedMedicalH2Proportion);
            Assert.AreEqual(expGlassAmberMedicalH2Proportion, projectedGlass.AmberMedicalH2Proportion);
            Assert.AreEqual(expGlassGreenMedicalH2Proportion, projectedGlass.GreenMedicalH2Proportion);

            Assert.AreEqual(
                new RAMTonnage { Tonnage = h1HHAlm.PackagingTonnage, RedTonnage = h1HHAlm.PackagingTonnage * expAlmRedH2Proportion, AmberTonnage = h1HHAlm.PackagingTonnage * expAlmAmberH2Proportion, GreenTonnage = h1HHAlm.PackagingTonnage * expAlmGreenH2Proportion, RedMedicalTonnage = h1HHAlm.PackagingTonnage * expAlmRedMedicalH2Proportion, AmberMedicalTonnage = h1HHAlm.PackagingTonnage * expAlmAmberMedicalH2Proportion, GreenMedicalTonnage = h1HHAlm.PackagingTonnage * expAlmGreenMedicalH2Proportion },
                projectedAluminium.ProjectedHouseholdRAMTonnage
            );

            Assert.AreEqual(EmptyRAMTonnage, projectedAluminium.ProjectedPublicBinRAMTonnage);
            Assert.IsNull(projectedAluminium.ProjectedHouseholdDrinksContainerRAMTonnage);
            Assert.AreEqual(EmptyRAMTonnage, projectedGlass.ProjectedHouseholdRAMTonnage);
            Assert.AreEqual(EmptyRAMTonnage, projectedGlass.ProjectedPublicBinRAMTonnage);

            Assert.AreEqual(
                new RAMTonnage { Tonnage = h1HdcGlass.PackagingTonnage, RedTonnage = h1HdcGlass.PackagingTonnage * expGlassRedH2Proportion, AmberTonnage = h1HdcGlass.PackagingTonnage * expGlassAmberH2Proportion, GreenTonnage = h1HdcGlass.PackagingTonnage * expGlassGreenH2Proportion, RedMedicalTonnage = h1HdcGlass.PackagingTonnage * expGlassRedMedicalH2Proportion, AmberMedicalTonnage = h1HdcGlass.PackagingTonnage * expGlassAmberMedicalH2Proportion, GreenMedicalTonnage = h1HdcGlass.PackagingTonnage * expGlassGreenMedicalH2Proportion },
                projectedGlass.ProjectedHouseholdDrinksContainerRAMTonnage
            );
        }

        [TestMethod]
        public async Task GetProjectedProducers_With_H1_NoRam_H2_PartialRam()
        {
            var (h1HHAlm, h1HdcGlass, h1NoRamReportedMaterials) = GetH1NoRamReportedMaterials();
            var (h2Alm, h2Glass, h2PartialRamProjectedProducers) = GetH2PartialRamProjectedProducers();    
            
            var result = await H1ProjectedProducersBuilderUtils.GetProjectedProducers(h1NoRamReportedMaterials, h2PartialRamProjectedProducers, materials);

            Assert.AreEqual(1, result.Count());
            Assert.AreEqual(3, result.First().ProjectedTonnageByMaterial.Count());

            var projectedAluminium = result.First().ProjectedTonnageByMaterial[MaterialCodes.Aluminium];
            var projectedGlass = result.First().ProjectedTonnageByMaterial[MaterialCodes.Glass];

            var expAlmRedH2Proportion = Math.Round((
                h2Alm.HouseholdRAMTonnage.RedTonnage + h2Alm.PublicBinRAMTonnage.RedTonnage + 
                h2Alm.HouseholdTonnageDefaultedRed + h2Alm.PublicBinTonnageDefaultedRed
            ) / h2Alm.TotalTonnage, 6);
            var expAlmAmberH2Proportion = Math.Round((h2Alm.HouseholdRAMTonnage.AmberTonnage + h2Alm.PublicBinRAMTonnage.AmberTonnage) / h2Alm.TotalTonnage, 6);
            var expAlmGreenH2Proportion = Math.Round((h2Alm.HouseholdRAMTonnage.GreenTonnage + h2Alm.PublicBinRAMTonnage.GreenTonnage) / h2Alm.TotalTonnage, 6);
            var expAlmRedMedicalH2Proportion = Math.Round((h2Alm.HouseholdRAMTonnage.RedMedicalTonnage + h2Alm.PublicBinRAMTonnage.RedMedicalTonnage) / h2Alm.TotalTonnage, 6);
            var expAlmAmberMedicalH2Proportion = Math.Round((h2Alm.HouseholdRAMTonnage.AmberMedicalTonnage + h2Alm.PublicBinRAMTonnage.AmberMedicalTonnage) / h2Alm.TotalTonnage, 6);
            var expAlmGreenMedicalH2Proportion = Math.Round((h2Alm.HouseholdRAMTonnage.GreenMedicalTonnage + h2Alm.PublicBinRAMTonnage.GreenMedicalTonnage) / h2Alm.TotalTonnage, 6); 
            Assert.AreEqual(expAlmRedH2Proportion, projectedAluminium.RedH2Proportion);
            Assert.AreEqual(expAlmAmberH2Proportion, projectedAluminium.AmberH2Proportion);
            Assert.AreEqual(expAlmGreenH2Proportion, projectedAluminium.GreenH2Proportion);
            Assert.AreEqual(expAlmRedMedicalH2Proportion, projectedAluminium.RedMedicalH2Proportion);
            Assert.AreEqual(expAlmAmberMedicalH2Proportion, projectedAluminium.AmberMedicalH2Proportion);
            Assert.AreEqual(expAlmGreenMedicalH2Proportion, projectedAluminium.GreenMedicalH2Proportion);

            var expGlassRedH2Proportion = Math.Round((
                h2Glass.HouseholdRAMTonnage.RedTonnage + h2Glass.PublicBinRAMTonnage.RedTonnage + (h2Glass.HouseholdDrinksContainerRAMTonnage?.RedTonnage ?? 0) +
                h2Glass.HouseholdTonnageDefaultedRed + h2Glass.PublicBinTonnageDefaultedRed + (h2Glass.HouseholdDrinksContainerDefaultedRed ?? 0)
            ) / h2Glass.TotalTonnage, 6);
            var expGlassAmberH2Proportion = Math.Round((h2Glass.HouseholdRAMTonnage.AmberTonnage + h2Glass.PublicBinRAMTonnage.AmberTonnage + (h2Glass.HouseholdDrinksContainerRAMTonnage?.AmberTonnage ?? 0)) / h2Glass.TotalTonnage, 6);
            var expGlassGreenH2Proportion = Math.Round((h2Glass.HouseholdRAMTonnage.GreenTonnage + h2Glass.PublicBinRAMTonnage.GreenTonnage + (h2Glass.HouseholdDrinksContainerRAMTonnage?.GreenTonnage ?? 0)) / h2Glass.TotalTonnage, 6);
            var expGlassRedMedicalH2Proportion = Math.Round((h2Glass.HouseholdRAMTonnage.RedMedicalTonnage + h2Glass.PublicBinRAMTonnage.RedMedicalTonnage + (h2Glass.HouseholdDrinksContainerRAMTonnage?.RedMedicalTonnage ?? 0)) / h2Glass.TotalTonnage, 6);
            var expGlassAmberMedicalH2Proportion = Math.Round((h2Glass.HouseholdRAMTonnage.AmberMedicalTonnage + h2Glass.PublicBinRAMTonnage.AmberMedicalTonnage + (h2Glass.HouseholdDrinksContainerRAMTonnage?.AmberMedicalTonnage ?? 0)) / h2Glass.TotalTonnage, 6);
            var expGlassGreenMedicalH2Proportion = Math.Round((h2Glass.HouseholdRAMTonnage.GreenMedicalTonnage + h2Glass.PublicBinRAMTonnage.GreenMedicalTonnage + (h2Glass.HouseholdDrinksContainerRAMTonnage?.GreenMedicalTonnage ?? 0)) / h2Glass.TotalTonnage, 6);
            Assert.AreEqual(expGlassRedH2Proportion, projectedGlass.RedH2Proportion);
            Assert.AreEqual(expGlassAmberH2Proportion, projectedGlass.AmberH2Proportion);
            Assert.AreEqual(expGlassGreenH2Proportion, projectedGlass.GreenH2Proportion);
            Assert.AreEqual(expGlassRedMedicalH2Proportion, projectedGlass.RedMedicalH2Proportion);
            Assert.AreEqual(expGlassAmberMedicalH2Proportion, projectedGlass.AmberMedicalH2Proportion);
            Assert.AreEqual(expGlassGreenMedicalH2Proportion, projectedGlass.GreenMedicalH2Proportion);

            Assert.AreEqual(
                new RAMTonnage { Tonnage = h1HHAlm.PackagingTonnage, RedTonnage = h1HHAlm.PackagingTonnage * expAlmRedH2Proportion, AmberTonnage = h1HHAlm.PackagingTonnage * expAlmAmberH2Proportion, GreenTonnage = h1HHAlm.PackagingTonnage * expAlmGreenH2Proportion, RedMedicalTonnage = h1HHAlm.PackagingTonnage * expAlmRedMedicalH2Proportion, AmberMedicalTonnage = h1HHAlm.PackagingTonnage * expAlmAmberMedicalH2Proportion, GreenMedicalTonnage = h1HHAlm.PackagingTonnage * expAlmGreenMedicalH2Proportion },
                projectedAluminium.ProjectedHouseholdRAMTonnage
            );

            Assert.AreEqual(EmptyRAMTonnage, projectedAluminium.ProjectedPublicBinRAMTonnage);
            Assert.IsNull(projectedAluminium.ProjectedHouseholdDrinksContainerRAMTonnage);
            Assert.AreEqual(EmptyRAMTonnage, projectedGlass.ProjectedHouseholdRAMTonnage);
            Assert.AreEqual(EmptyRAMTonnage, projectedGlass.ProjectedPublicBinRAMTonnage);

            Assert.AreEqual(
                new RAMTonnage { Tonnage = h1HdcGlass.PackagingTonnage, RedTonnage = h1HdcGlass.PackagingTonnage * expGlassRedH2Proportion, AmberTonnage = h1HdcGlass.PackagingTonnage * expGlassAmberH2Proportion, GreenTonnage = h1HdcGlass.PackagingTonnage * expGlassGreenH2Proportion, RedMedicalTonnage = h1HdcGlass.PackagingTonnage * expGlassRedMedicalH2Proportion, AmberMedicalTonnage = h1HdcGlass.PackagingTonnage * expGlassAmberMedicalH2Proportion, GreenMedicalTonnage = h1HdcGlass.PackagingTonnage * expGlassGreenMedicalH2Proportion },
                projectedGlass.ProjectedHouseholdDrinksContainerRAMTonnage
            );
        }
    
        [TestMethod]
        public async Task GetProjectedProducers_With_H1_PartialRam_H2_FullRam()
        {
            var (h1HHAlm, h1HdcGlass, h1PartialRamReportedMaterials) = GetH1PartialRamReportedMaterials();
            var (h2Alm, h2Glass, h2FullRamProjectedProducers) = GetH2FullRamProjectedProducers();    
            var result = await H1ProjectedProducersBuilderUtils.GetProjectedProducers(h1PartialRamReportedMaterials, h2FullRamProjectedProducers, materials);

            Assert.AreEqual(1, result.Count());
            Assert.AreEqual(3, result.First().ProjectedTonnageByMaterial.Count());

            var projectedAluminium = result.First().ProjectedTonnageByMaterial[MaterialCodes.Aluminium];
            var projectedGlass = result.First().ProjectedTonnageByMaterial[MaterialCodes.Glass];

            Assert.AreEqual(ProducerReportedMaterialToRAMTonnage(h1HHAlm), projectedAluminium.HouseholdRAMTonnage);
            Assert.AreEqual(EmptyRAMTonnage, projectedAluminium.PublicBinRAMTonnage);
            Assert.IsNull(projectedAluminium.HouseholdDrinksContainerRAMTonnage);
            Assert.AreEqual(EmptyRAMTonnage, projectedGlass.HouseholdRAMTonnage);
            Assert.AreEqual(EmptyRAMTonnage, projectedGlass.PublicBinRAMTonnage);
            Assert.AreEqual(ProducerReportedMaterialToRAMTonnage(h1HdcGlass), projectedGlass.HouseholdDrinksContainerRAMTonnage);

            var expH1AlmWithoutRam = CalcWithoutRam(h1HHAlm);
            var expH1HdcGlassWithoutRam = CalcWithoutRam(h1HdcGlass);
            Assert.AreEqual(expH1AlmWithoutRam, projectedAluminium.HouseholdTonnageWithoutRAM);
            Assert.AreEqual(0, projectedAluminium.PublicBinTonnageWithoutRAM);
            Assert.IsNull(projectedAluminium.HouseholdDrinksContainerTonnageWithoutRAM);
            Assert.AreEqual(0, projectedGlass.HouseholdTonnageWithoutRAM);
            Assert.AreEqual(0, projectedGlass.PublicBinTonnageWithoutRAM);
            Assert.AreEqual(expH1HdcGlassWithoutRam, projectedGlass.HouseholdDrinksContainerTonnageWithoutRAM);

            var expAlmRedH2Proportion = Math.Round((h2Alm.HouseholdRAMTonnage.RedTonnage + h2Alm.PublicBinRAMTonnage.RedTonnage) / h2Alm.TotalTonnage, 6);
            var expAlmAmberH2Proportion = Math.Round((h2Alm.HouseholdRAMTonnage.AmberTonnage + h2Alm.PublicBinRAMTonnage.AmberTonnage) / h2Alm.TotalTonnage, 6);
            var expAlmGreenH2Proportion = Math.Round((h2Alm.HouseholdRAMTonnage.GreenTonnage + h2Alm.PublicBinRAMTonnage.GreenTonnage) / h2Alm.TotalTonnage, 6);
            var expAlmRedMedicalH2Proportion = Math.Round((h2Alm.HouseholdRAMTonnage.RedMedicalTonnage + h2Alm.PublicBinRAMTonnage.RedMedicalTonnage) / h2Alm.TotalTonnage, 6);
            var expAlmAmberMedicalH2Proportion = Math.Round((h2Alm.HouseholdRAMTonnage.AmberMedicalTonnage + h2Alm.PublicBinRAMTonnage.AmberMedicalTonnage) / h2Alm.TotalTonnage, 6);
            var expAlmGreenMedicalH2Proportion = Math.Round((h2Alm.HouseholdRAMTonnage.GreenMedicalTonnage + h2Alm.PublicBinRAMTonnage.GreenMedicalTonnage) / h2Alm.TotalTonnage, 6); 
            Assert.AreEqual(expAlmRedH2Proportion, projectedAluminium.RedH2Proportion);
            Assert.AreEqual(expAlmAmberH2Proportion, projectedAluminium.AmberH2Proportion);
            Assert.AreEqual(expAlmGreenH2Proportion, projectedAluminium.GreenH2Proportion);
            Assert.AreEqual(expAlmRedMedicalH2Proportion, projectedAluminium.RedMedicalH2Proportion);
            Assert.AreEqual(expAlmAmberMedicalH2Proportion, projectedAluminium.AmberMedicalH2Proportion);
            Assert.AreEqual(expAlmGreenMedicalH2Proportion, projectedAluminium.GreenMedicalH2Proportion);

            var expGlassRedH2Proportion = Math.Round((h2Glass.HouseholdRAMTonnage.RedTonnage + h2Glass.PublicBinRAMTonnage.RedTonnage + (h2Glass.HouseholdDrinksContainerRAMTonnage?.RedTonnage ?? 0)) / h2Glass.TotalTonnage, 6);
            var expGlassAmberH2Proportion = Math.Round((h2Glass.HouseholdRAMTonnage.AmberTonnage + h2Glass.PublicBinRAMTonnage.AmberTonnage + (h2Glass.HouseholdDrinksContainerRAMTonnage?.AmberTonnage ?? 0)) / h2Glass.TotalTonnage, 6);
            var expGlassGreenH2Proportion = Math.Round((h2Glass.HouseholdRAMTonnage.GreenTonnage + h2Glass.PublicBinRAMTonnage.GreenTonnage + (h2Glass.HouseholdDrinksContainerRAMTonnage?.GreenTonnage ?? 0)) / h2Glass.TotalTonnage, 6);
            var expGlassRedMedicalH2Proportion = Math.Round((h2Glass.HouseholdRAMTonnage.RedMedicalTonnage + h2Glass.PublicBinRAMTonnage.RedMedicalTonnage + (h2Glass.HouseholdDrinksContainerRAMTonnage?.RedMedicalTonnage ?? 0)) / h2Glass.TotalTonnage, 6);
            var expGlassAmberMedicalH2Proportion = Math.Round((h2Glass.HouseholdRAMTonnage.AmberMedicalTonnage + h2Glass.PublicBinRAMTonnage.AmberMedicalTonnage + (h2Glass.HouseholdDrinksContainerRAMTonnage?.AmberMedicalTonnage ?? 0)) / h2Glass.TotalTonnage, 6);
            var expGlassGreenMedicalH2Proportion = Math.Round((h2Glass.HouseholdRAMTonnage.GreenMedicalTonnage + h2Glass.PublicBinRAMTonnage.GreenMedicalTonnage + (h2Glass.HouseholdDrinksContainerRAMTonnage?.GreenMedicalTonnage ?? 0)) / h2Glass.TotalTonnage, 6);
            Assert.AreEqual(expGlassRedH2Proportion, projectedGlass.RedH2Proportion);
            Assert.AreEqual(expGlassAmberH2Proportion, projectedGlass.AmberH2Proportion);
            Assert.AreEqual(expGlassGreenH2Proportion, projectedGlass.GreenH2Proportion);
            Assert.AreEqual(expGlassRedMedicalH2Proportion, projectedGlass.RedMedicalH2Proportion);
            Assert.AreEqual(expGlassAmberMedicalH2Proportion, projectedGlass.AmberMedicalH2Proportion);
            Assert.AreEqual(expGlassGreenMedicalH2Proportion, projectedGlass.GreenMedicalH2Proportion);

            Assert.AreEqual(
                new RAMTonnage { 
                    Tonnage = h1HHAlm.PackagingTonnage,
                    RedTonnage = expH1AlmWithoutRam * expAlmRedH2Proportion, 
                    AmberTonnage = (h1HHAlm.PackagingTonnageAmber ?? 0) + (expH1AlmWithoutRam * expAlmAmberH2Proportion), 
                    GreenTonnage = expH1AlmWithoutRam * expAlmGreenH2Proportion, 
                    RedMedicalTonnage = expH1AlmWithoutRam * expAlmRedMedicalH2Proportion, 
                    AmberMedicalTonnage = (h1HHAlm.PackagingTonnageAmberMedical ?? 0) + (expH1AlmWithoutRam * expAlmAmberMedicalH2Proportion), 
                    GreenMedicalTonnage = expH1AlmWithoutRam * expAlmGreenMedicalH2Proportion 
                },
                projectedAluminium.ProjectedHouseholdRAMTonnage
            );

            Assert.AreEqual(EmptyRAMTonnage, projectedAluminium.ProjectedPublicBinRAMTonnage);
            Assert.IsNull(projectedAluminium.ProjectedHouseholdDrinksContainerRAMTonnage);
            Assert.AreEqual(EmptyRAMTonnage, projectedGlass.ProjectedHouseholdRAMTonnage);
            Assert.AreEqual(EmptyRAMTonnage, projectedGlass.ProjectedPublicBinRAMTonnage);

            Assert.AreEqual(
                new RAMTonnage { 
                    Tonnage = h1HdcGlass.PackagingTonnage,
                    RedTonnage = (h1HdcGlass.PackagingTonnageRed ?? 0) + (expH1HdcGlassWithoutRam * expGlassRedH2Proportion),
                    AmberTonnage = expH1HdcGlassWithoutRam * expGlassAmberH2Proportion,
                    GreenTonnage = (h1HdcGlass.PackagingTonnageGreen ?? 0) + (expH1HdcGlassWithoutRam * expGlassGreenH2Proportion),
                    RedMedicalTonnage = (h1HdcGlass.PackagingTonnageRedMedical ?? 0) + (expH1HdcGlassWithoutRam * expGlassRedMedicalH2Proportion),
                    AmberMedicalTonnage = expH1HdcGlassWithoutRam * expGlassAmberMedicalH2Proportion,
                    GreenMedicalTonnage = expH1HdcGlassWithoutRam * expGlassGreenMedicalH2Proportion 
                },
                projectedGlass.ProjectedHouseholdDrinksContainerRAMTonnage
            );
        }

        [TestMethod]
        public async Task GetProjectedProducers_With_H1_FullRam_H2_FullRam()
        {
            var (h1HHAlm, h1HdcGlass, h1FullRamReportedMaterials) = GetH1FullRamReportedMaterials();
            var (h2Alm, h2Glass, h2FullRamProjectedProducers) = GetH2FullRamProjectedProducers();    
            var result = await H1ProjectedProducersBuilderUtils.GetProjectedProducers(h1FullRamReportedMaterials, h2FullRamProjectedProducers, materials);

            Assert.AreEqual(1, result.Count());
            Assert.AreEqual(3, result.First().ProjectedTonnageByMaterial.Count());

            var projectedAluminium = result.First().ProjectedTonnageByMaterial[MaterialCodes.Aluminium];
            var projectedGlass = result.First().ProjectedTonnageByMaterial[MaterialCodes.Glass];

            Assert.AreEqual(ProducerReportedMaterialToRAMTonnage(h1HHAlm), projectedAluminium.HouseholdRAMTonnage);
            Assert.AreEqual(EmptyRAMTonnage, projectedAluminium.PublicBinRAMTonnage);
            Assert.IsNull(projectedAluminium.HouseholdDrinksContainerRAMTonnage);
            Assert.AreEqual(EmptyRAMTonnage, projectedGlass.HouseholdRAMTonnage);
            Assert.AreEqual(EmptyRAMTonnage, projectedGlass.PublicBinRAMTonnage);
            Assert.AreEqual(ProducerReportedMaterialToRAMTonnage(h1HdcGlass), projectedGlass.HouseholdDrinksContainerRAMTonnage);

            Assert.AreEqual(0, projectedAluminium.HouseholdTonnageWithoutRAM);
            Assert.AreEqual(0, projectedAluminium.PublicBinTonnageWithoutRAM);
            Assert.IsNull(projectedAluminium.HouseholdDrinksContainerTonnageWithoutRAM);
            Assert.AreEqual(0, projectedGlass.HouseholdTonnageWithoutRAM);
            Assert.AreEqual(0, projectedGlass.PublicBinTonnageWithoutRAM);
            Assert.AreEqual(0, projectedGlass.HouseholdDrinksContainerTonnageWithoutRAM);

            Assert.AreEqual(ProducerReportedMaterialToRAMTonnage(h1HHAlm),projectedAluminium.ProjectedHouseholdRAMTonnage);
            Assert.AreEqual(EmptyRAMTonnage, projectedAluminium.ProjectedPublicBinRAMTonnage);
            Assert.IsNull(projectedAluminium.ProjectedHouseholdDrinksContainerRAMTonnage);
            Assert.AreEqual(EmptyRAMTonnage, projectedGlass.ProjectedHouseholdRAMTonnage);
            Assert.AreEqual(EmptyRAMTonnage, projectedGlass.ProjectedPublicBinRAMTonnage);
            Assert.AreEqual(ProducerReportedMaterialToRAMTonnage(h1HdcGlass),projectedGlass.ProjectedHouseholdDrinksContainerRAMTonnage);
        } 

        [TestMethod]
        public async Task GetProjectedProducers_With_H1_NoReportedMaterial()
        {
            var (h1HHAlm, h1HdcGlass, h1FullRamReportedMaterials) = GetH1FullRamReportedMaterials();
            var (h2Alm, h2Glass, h2FullRamProjectedProducers) = GetH2FullRamProjectedProducers();    
            var result = await H1ProjectedProducersBuilderUtils.GetProjectedProducers(h1FullRamReportedMaterials, h2FullRamProjectedProducers, materials);

            Assert.AreEqual(1, result.Count());

            var projectedOtherMaterials = result.First().ProjectedTonnageByMaterial[MaterialCodes.OtherMaterials];
            Assert.AreEqual(EmptyRAMTonnage, projectedOtherMaterials.HouseholdRAMTonnage);
            Assert.AreEqual(EmptyRAMTonnage, projectedOtherMaterials.PublicBinRAMTonnage);
            Assert.IsNull(projectedOtherMaterials.HouseholdDrinksContainerRAMTonnage);
            Assert.AreEqual(0, projectedOtherMaterials.RedH2Proportion);
            Assert.AreEqual(0, projectedOtherMaterials.AmberH2Proportion);
            Assert.AreEqual(0, projectedOtherMaterials.GreenH2Proportion);
            Assert.AreEqual(0, projectedOtherMaterials.RedMedicalH2Proportion);
            Assert.AreEqual(0, projectedOtherMaterials.AmberMedicalH2Proportion);
            Assert.AreEqual(0, projectedOtherMaterials.GreenMedicalH2Proportion);
            Assert.AreEqual(EmptyRAMTonnage, projectedOtherMaterials.HouseholdRAMTonnage);
            Assert.AreEqual(EmptyRAMTonnage, projectedOtherMaterials.PublicBinRAMTonnage);
            Assert.IsNull(projectedOtherMaterials.HouseholdDrinksContainerRAMTonnage);
        } 

        private RAMTonnage ProducerReportedMaterialToRAMTonnage(ProducerReportedMaterial material)
        {
            return new RAMTonnage
            {
                Tonnage = material.PackagingTonnage,
                RedTonnage = material.PackagingTonnageRed ?? 0,
                AmberTonnage = material.PackagingTonnageAmber ?? 0,
                GreenTonnage = material.PackagingTonnageGreen ?? 0,
                RedMedicalTonnage = material.PackagingTonnageRedMedical ?? 0,
                AmberMedicalTonnage = material.PackagingTonnageAmberMedical ?? 0,
                GreenMedicalTonnage = material.PackagingTonnageGreenMedical ?? 0
            };
        } 

        private decimal CalcWithoutRam(ProducerReportedMaterial material)
        {
            return material.PackagingTonnage - (material.PackagingTonnageRed ?? 0) - (material.PackagingTonnageAmber ?? 0) - (material.PackagingTonnageGreen ?? 0) - (material.PackagingTonnageRedMedical ?? 0) - (material.PackagingTonnageAmberMedical ?? 0) - (material.PackagingTonnageGreenMedical ?? 0);
        }

        private (ProducerReportedMaterial, ProducerReportedMaterial, List<ProducerReportedMaterialsForSubmissionPeriod>) GetH1NoRamReportedMaterials()
        {
            var hhAlm = new ProducerReportedMaterial { MaterialId = 1, PackagingType = PackagingTypes.Household, PackagingTonnage = 100, SubmissionPeriod = "2026-H1" };
            var hdcGlass = new ProducerReportedMaterial { MaterialId = 2, PackagingType = PackagingTypes.HouseholdDrinksContainers, PackagingTonnage = 200, SubmissionPeriod = "2026-H1" };
            return (hhAlm, hdcGlass, new List<ProducerReportedMaterialsForSubmissionPeriod>()
            {
                new(
                    producerId: 11,
                    subsidiaryId: null,
                    submissionPeriod: "2026-H1",
                    new List<ProducerReportedMaterial>{ hhAlm, hdcGlass }
                )
            });
        }

        private (ProducerReportedMaterial, ProducerReportedMaterial, List<ProducerReportedMaterialsForSubmissionPeriod>) GetH1PartialRamReportedMaterials()
        {
            var hhAlm = new ProducerReportedMaterial { MaterialId = 1, PackagingType = PackagingTypes.Household, PackagingTonnage = 100, PackagingTonnageRed = 0, PackagingTonnageAmber = 50, PackagingTonnageGreen = 0, PackagingTonnageRedMedical = 0, PackagingTonnageAmberMedical = 25, PackagingTonnageGreenMedical = 0, SubmissionPeriod = "2026-H1" };
            var hdcGlass = new ProducerReportedMaterial { MaterialId = 2, PackagingType = PackagingTypes.HouseholdDrinksContainers, PackagingTonnage = 200, PackagingTonnageRed = 20, PackagingTonnageAmber = 0, PackagingTonnageGreen = 100, PackagingTonnageRedMedical = 20, PackagingTonnageAmberMedical = 0, PackagingTonnageGreenMedical = 0, SubmissionPeriod = "2026-H1" };
            return (hhAlm, hdcGlass, new List<ProducerReportedMaterialsForSubmissionPeriod>()
            {
                new(
                    producerId: 11,
                    subsidiaryId: null,
                    submissionPeriod: "2026-H1",
                    new List<ProducerReportedMaterial>{ hhAlm, hdcGlass }
                )
            });
        }

        private (ProducerReportedMaterial, ProducerReportedMaterial, List<ProducerReportedMaterialsForSubmissionPeriod>) GetH1FullRamReportedMaterials()
        {
            var hhAlm = new ProducerReportedMaterial { MaterialId = 1, PackagingType = PackagingTypes.Household, PackagingTonnage = 100, PackagingTonnageRed = 0, PackagingTonnageAmber = 50, PackagingTonnageGreen = 25, PackagingTonnageRedMedical = 0, PackagingTonnageAmberMedical = 25, PackagingTonnageGreenMedical = 0, SubmissionPeriod = "2026-H1" };
            var hdcGlass = new ProducerReportedMaterial { MaterialId = 2, PackagingType = PackagingTypes.HouseholdDrinksContainers, PackagingTonnage = 200, PackagingTonnageRed = 20, PackagingTonnageAmber = 60, PackagingTonnageGreen = 100, PackagingTonnageRedMedical = 20, PackagingTonnageAmberMedical = 0, PackagingTonnageGreenMedical = 0, SubmissionPeriod = "2026-H1" };
            return (hhAlm, hdcGlass, new List<ProducerReportedMaterialsForSubmissionPeriod>()
            {
                new(
                    producerId: 11,
                    subsidiaryId: null,
                    submissionPeriod: "2026-H1",
                    new List<ProducerReportedMaterial>{ hhAlm, hdcGlass }
                )
            });
        }

        private (CalcResultH2ProjectedProducerMaterialTonnage, CalcResultH2ProjectedProducerMaterialTonnage, List<CalcResultH2ProjectedProducer>) GetH2FullRamProjectedProducers()
        {
            var alm = new CalcResultH2ProjectedProducerMaterialTonnage 
                        { 
                            HouseholdRAMTonnage = new RAMTonnage { Tonnage = 75, RedTonnage = 10, AmberTonnage = 11, GreenTonnage = 12, RedMedicalTonnage = 13, AmberMedicalTonnage = 14, GreenMedicalTonnage = 15 },
                            PublicBinRAMTonnage = new RAMTonnage { Tonnage = 135, RedTonnage = 20, AmberTonnage = 21, GreenTonnage = 22, RedMedicalTonnage = 23, AmberMedicalTonnage = 24, GreenMedicalTonnage = 25 },
                            HouseholdTonnageDefaultedRed = 0,
                            PublicBinTonnageDefaultedRed = 0,
                            TotalTonnage = 310 
                        };
            var glass = new CalcResultH2ProjectedProducerMaterialTonnage 
                        { 
                            HouseholdRAMTonnage = new RAMTonnage { Tonnage = 75, RedTonnage = 10, AmberTonnage = 11, GreenTonnage = 12, RedMedicalTonnage = 13, AmberMedicalTonnage = 14, GreenMedicalTonnage = 15 },
                            PublicBinRAMTonnage = new RAMTonnage { Tonnage = 135, RedTonnage = 20, AmberTonnage = 21, GreenTonnage = 22, RedMedicalTonnage = 23, AmberMedicalTonnage = 24, GreenMedicalTonnage = 25 },
                            HouseholdDrinksContainerRAMTonnage = new RAMTonnage { Tonnage = 195, RedTonnage = 30, AmberTonnage = 31, GreenTonnage = 32, RedMedicalTonnage = 33, AmberMedicalTonnage = 34, GreenMedicalTonnage = 35 },
                            HouseholdTonnageDefaultedRed = 0,
                            PublicBinTonnageDefaultedRed = 0,
                            HouseholdDrinksContainerDefaultedRed = 0,
                            TotalTonnage = 405 
                        };
            return (alm, glass, new List<CalcResultH2ProjectedProducer>()
            {
                new()
                {
                    ProducerId = 11,
                    SubsidiaryId = null,
                    Level = string.Empty,
                    SubmissionPeriodCode = "2026-H2",
                    ProjectedTonnageByMaterial = new Dictionary<string, CalcResultH2ProjectedProducerMaterialTonnage>
                    {
                        { MaterialCodes.Aluminium, alm},
                        { MaterialCodes.Glass, glass }
                    } 
                }
            });
        }

        private (CalcResultH2ProjectedProducerMaterialTonnage, CalcResultH2ProjectedProducerMaterialTonnage, List<CalcResultH2ProjectedProducer>) GetH2PartialRamProjectedProducers()
        {
            var alm = new CalcResultH2ProjectedProducerMaterialTonnage 
                        { 
                            HouseholdRAMTonnage = new RAMTonnage { Tonnage = 75, RedTonnage = 10, AmberTonnage = 11, GreenTonnage = 12, RedMedicalTonnage = 13, AmberMedicalTonnage = 14, GreenMedicalTonnage = 15 },
                            PublicBinRAMTonnage = new RAMTonnage { Tonnage = 135, RedTonnage = 20, AmberTonnage = 21, GreenTonnage = 22, RedMedicalTonnage = 23, AmberMedicalTonnage = 24, GreenMedicalTonnage = 25 },
                            HouseholdTonnageDefaultedRed = 10,
                            PublicBinTonnageDefaultedRed = 20,
                            TotalTonnage = 310 
                        };
            var glass = new CalcResultH2ProjectedProducerMaterialTonnage 
                        { 
                            HouseholdRAMTonnage = new RAMTonnage { Tonnage = 75, RedTonnage = 10, AmberTonnage = 11, GreenTonnage = 12, RedMedicalTonnage = 13, AmberMedicalTonnage = 14, GreenMedicalTonnage = 15 },
                            PublicBinRAMTonnage = new RAMTonnage { Tonnage = 135, RedTonnage = 20, AmberTonnage = 21, GreenTonnage = 22, RedMedicalTonnage = 23, AmberMedicalTonnage = 24, GreenMedicalTonnage = 25 },
                            HouseholdDrinksContainerRAMTonnage = new RAMTonnage { Tonnage = 195, RedTonnage = 30, AmberTonnage = 31, GreenTonnage = 32, RedMedicalTonnage = 33, AmberMedicalTonnage = 34, GreenMedicalTonnage = 35 },
                            HouseholdTonnageDefaultedRed = 30,
                            PublicBinTonnageDefaultedRed = 10,
                            HouseholdDrinksContainerDefaultedRed = 20,
                            TotalTonnage = 405 
                        };
            return (alm, glass, new List<CalcResultH2ProjectedProducer>()
            {
                new()
                {
                    ProducerId = 11,
                    SubsidiaryId = null,
                    Level = string.Empty,
                    SubmissionPeriodCode = "2026-H2",
                    ProjectedTonnageByMaterial = new Dictionary<string, CalcResultH2ProjectedProducerMaterialTonnage>
                    {
                        { MaterialCodes.Aluminium, alm},
                        { MaterialCodes.Glass, glass }
                    } 
                }
            });
        }
    }
}