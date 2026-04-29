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

        private RAMTonnage EmptyRAMTonnage() { return new RAMTonnage { Tonnage = 0, RedTonnage = 0, AmberTonnage = 0, GreenTonnage = 0, RedMedicalTonnage = 0, AmberMedicalTonnage = 0, GreenMedicalTonnage = 0 }; }

        [TestMethod]
        public void GetProjectedProducers_With_H1_NoRam_H2_FullRam()
        {
            var (h1HHAlm, h1HdcGlass, h1NoRamReportedMaterials) = GetH1NoRamReportedMaterials();
            var (h2Alm, h2Glass, h2FullRamProjectedProducers) = GetH2FullRamProjectedProducers();
            var result = H1ProjectedProducersBuilderUtils.GetProjectedProducers(h1NoRamReportedMaterials, h2FullRamProjectedProducers, materials);

            Assert.AreEqual(1, result.Count());
            Assert.AreEqual(3, result.First().H1ProjectedTonnageByMaterial.Count());

            var projectedAluminium = result.First().H1ProjectedTonnageByMaterial[MaterialCodes.Aluminium];
            var projectedGlass = result.First().H1ProjectedTonnageByMaterial[MaterialCodes.Glass];

            Assert.AreEqual(ProducerReportedMaterialToRAMTonnage(h1HHAlm), projectedAluminium.HouseholdRAMTonnage);
            Assert.AreEqual(EmptyRAMTonnage(), projectedAluminium.PublicBinRAMTonnage);
            Assert.IsNull(projectedAluminium.HouseholdDrinksContainerRAMTonnage);
            Assert.AreEqual(EmptyRAMTonnage(), projectedGlass.HouseholdRAMTonnage);
            Assert.AreEqual(EmptyRAMTonnage(), projectedGlass.PublicBinRAMTonnage);
            Assert.AreEqual(ProducerReportedMaterialToRAMTonnage(h1HdcGlass), projectedGlass.HouseholdDrinksContainerRAMTonnage);

            Assert.AreEqual(h1HHAlm.PackagingTonnage, projectedAluminium.HouseholdTonnageWithoutRAM);
            Assert.AreEqual(0, projectedAluminium.PublicBinTonnageWithoutRAM);
            Assert.IsNull(projectedAluminium.HouseholdDrinksContainerTonnageWithoutRAM);
            Assert.AreEqual(0, projectedGlass.HouseholdTonnageWithoutRAM);
            Assert.AreEqual(0, projectedGlass.PublicBinTonnageWithoutRAM);
            Assert.AreEqual(h1HdcGlass.PackagingTonnage, projectedGlass.HouseholdDrinksContainerTonnageWithoutRAM);

            var expAlmH2Proportions = new RAMProportions{
                Red = To6DP((h2Alm.ProjectedHouseholdRAMTonnage.RedTonnage + h2Alm.ProjectedPublicBinRAMTonnage.RedTonnage) / h2Alm.TotalTonnage),
                Amber = To6DP((h2Alm.ProjectedHouseholdRAMTonnage.AmberTonnage + h2Alm.ProjectedPublicBinRAMTonnage.AmberTonnage) / h2Alm.TotalTonnage),
                Green = To6DP((h2Alm.ProjectedHouseholdRAMTonnage.GreenTonnage + h2Alm.ProjectedPublicBinRAMTonnage.GreenTonnage) / h2Alm.TotalTonnage),
                RedMedical = To6DP((h2Alm.ProjectedHouseholdRAMTonnage.RedMedicalTonnage + h2Alm.ProjectedPublicBinRAMTonnage.RedMedicalTonnage) / h2Alm.TotalTonnage),
                AmberMedical = To6DP((h2Alm.ProjectedHouseholdRAMTonnage.AmberMedicalTonnage + h2Alm.ProjectedPublicBinRAMTonnage.AmberMedicalTonnage) / h2Alm.TotalTonnage),
                GreenMedical = To6DP((h2Alm.ProjectedHouseholdRAMTonnage.GreenMedicalTonnage + h2Alm.ProjectedPublicBinRAMTonnage.GreenMedicalTonnage) / h2Alm.TotalTonnage)
            };
            Assert.AreEqual(expAlmH2Proportions, projectedAluminium.H2RamProportions);

            var expGlassH2Proportions = new RAMProportions{
                Red = To6DP((h2Glass.ProjectedHouseholdRAMTonnage.RedTonnage + h2Glass.ProjectedPublicBinRAMTonnage.RedTonnage + (h2Glass.ProjectedHouseholdDrinksContainerRAMTonnage?.RedTonnage ?? 0)) / h2Glass.TotalTonnage),
                Amber = To6DP((h2Glass.ProjectedHouseholdRAMTonnage.AmberTonnage + h2Glass.ProjectedPublicBinRAMTonnage.AmberTonnage + (h2Glass.ProjectedHouseholdDrinksContainerRAMTonnage?.AmberTonnage ?? 0)) / h2Glass.TotalTonnage),
                Green = To6DP((h2Glass.ProjectedHouseholdRAMTonnage.GreenTonnage + h2Glass.ProjectedPublicBinRAMTonnage.GreenTonnage + (h2Glass.ProjectedHouseholdDrinksContainerRAMTonnage?.GreenTonnage ?? 0)) / h2Glass.TotalTonnage),
                RedMedical = To6DP((h2Glass.ProjectedHouseholdRAMTonnage.RedMedicalTonnage + h2Glass.ProjectedPublicBinRAMTonnage.RedMedicalTonnage + (h2Glass.ProjectedHouseholdDrinksContainerRAMTonnage?.RedMedicalTonnage ?? 0)) / h2Glass.TotalTonnage),
                AmberMedical = To6DP((h2Glass.ProjectedHouseholdRAMTonnage.AmberMedicalTonnage + h2Glass.ProjectedPublicBinRAMTonnage.AmberMedicalTonnage + (h2Glass.ProjectedHouseholdDrinksContainerRAMTonnage?.AmberMedicalTonnage ?? 0)) / h2Glass.TotalTonnage),
                GreenMedical = To6DP((h2Glass.ProjectedHouseholdRAMTonnage.GreenMedicalTonnage + h2Glass.ProjectedPublicBinRAMTonnage.GreenMedicalTonnage + (h2Glass.ProjectedHouseholdDrinksContainerRAMTonnage?.GreenMedicalTonnage ?? 0)) / h2Glass.TotalTonnage)
            };
            Assert.AreEqual(expGlassH2Proportions, projectedGlass.H2RamProportions);

            Assert.AreEqual(h1HHAlm.PackagingTonnage, projectedAluminium.ProjectedHouseholdRAMTonnage.Tonnage);
            AssertWithin(To3DP(h1HHAlm.PackagingTonnage * expAlmH2Proportions.Red), projectedAluminium.ProjectedHouseholdRAMTonnage.RedTonnage);
            AssertWithin(To3DP(h1HHAlm.PackagingTonnage * expAlmH2Proportions.Amber), projectedAluminium.ProjectedHouseholdRAMTonnage.AmberTonnage);
            AssertWithin(To3DP(h1HHAlm.PackagingTonnage * expAlmH2Proportions.Green), projectedAluminium.ProjectedHouseholdRAMTonnage.GreenTonnage);
            AssertWithin(To3DP(h1HHAlm.PackagingTonnage * expAlmH2Proportions.RedMedical), projectedAluminium.ProjectedHouseholdRAMTonnage.RedMedicalTonnage);
            AssertWithin(To3DP(h1HHAlm.PackagingTonnage * expAlmH2Proportions.AmberMedical), projectedAluminium.ProjectedHouseholdRAMTonnage.AmberMedicalTonnage);
            AssertWithin(To3DP(h1HHAlm.PackagingTonnage * expAlmH2Proportions.GreenMedical), projectedAluminium.ProjectedHouseholdRAMTonnage.GreenMedicalTonnage);

            Assert.AreEqual(EmptyRAMTonnage(), projectedAluminium.ProjectedPublicBinRAMTonnage);
            Assert.IsNull(projectedAluminium.ProjectedHouseholdDrinksContainerRAMTonnage);
            Assert.AreEqual(EmptyRAMTonnage(), projectedGlass.ProjectedHouseholdRAMTonnage);
            Assert.AreEqual(EmptyRAMTonnage(), projectedGlass.ProjectedPublicBinRAMTonnage);

            Assert.AreEqual(h1HdcGlass.PackagingTonnage, projectedGlass.ProjectedHouseholdDrinksContainerRAMTonnage!.Tonnage);
            AssertWithin(To3DP(h1HdcGlass.PackagingTonnage * expGlassH2Proportions.Red), projectedGlass.ProjectedHouseholdDrinksContainerRAMTonnage.RedTonnage);
            AssertWithin(To3DP(h1HdcGlass.PackagingTonnage * expGlassH2Proportions.Amber), projectedGlass.ProjectedHouseholdDrinksContainerRAMTonnage.AmberTonnage);
            AssertWithin(To3DP(h1HdcGlass.PackagingTonnage * expGlassH2Proportions.Green), projectedGlass.ProjectedHouseholdDrinksContainerRAMTonnage.GreenTonnage);
            AssertWithin(To3DP(h1HdcGlass.PackagingTonnage * expGlassH2Proportions.RedMedical), projectedGlass.ProjectedHouseholdDrinksContainerRAMTonnage.RedMedicalTonnage);
            AssertWithin(To3DP(h1HdcGlass.PackagingTonnage * expGlassH2Proportions.AmberMedical), projectedGlass.ProjectedHouseholdDrinksContainerRAMTonnage.AmberMedicalTonnage);
            AssertWithin(To3DP(h1HdcGlass.PackagingTonnage * expGlassH2Proportions.GreenMedical), projectedGlass.ProjectedHouseholdDrinksContainerRAMTonnage.GreenMedicalTonnage);
        }

        [TestMethod]
        public void GetProjectedProducers_With_H1_NoRam_H2_PartialRam()
        {
            var (h1HHAlm, h1HdcGlass, h1NoRamReportedMaterials) = GetH1NoRamReportedMaterials();
            var (h2Alm, h2Glass, h2PartialRamProjectedProducers) = GetH2PartialRamProjectedProducers();    
            
            var result = H1ProjectedProducersBuilderUtils.GetProjectedProducers(h1NoRamReportedMaterials, h2PartialRamProjectedProducers, materials);

            Assert.AreEqual(1, result.Count());
            Assert.AreEqual(3, result.First().H1ProjectedTonnageByMaterial.Count());

            var projectedAluminium = result.First().H1ProjectedTonnageByMaterial[MaterialCodes.Aluminium];
            var projectedGlass = result.First().H1ProjectedTonnageByMaterial[MaterialCodes.Glass];

            var expAlmH2Proportions = new RAMProportions {
                Red = To6DP((h2Alm.ProjectedHouseholdRAMTonnage.RedTonnage + h2Alm.ProjectedPublicBinRAMTonnage.RedTonnage) / h2Alm.TotalTonnage),
                Amber = To6DP((h2Alm.ProjectedHouseholdRAMTonnage.AmberTonnage + h2Alm.ProjectedPublicBinRAMTonnage.AmberTonnage) / h2Alm.TotalTonnage),
                Green = To6DP((h2Alm.ProjectedHouseholdRAMTonnage.GreenTonnage + h2Alm.ProjectedPublicBinRAMTonnage.GreenTonnage) / h2Alm.TotalTonnage),
                RedMedical = To6DP((h2Alm.ProjectedHouseholdRAMTonnage.RedMedicalTonnage + h2Alm.ProjectedPublicBinRAMTonnage.RedMedicalTonnage) / h2Alm.TotalTonnage),
                AmberMedical = To6DP((h2Alm.ProjectedHouseholdRAMTonnage.AmberMedicalTonnage + h2Alm.ProjectedPublicBinRAMTonnage.AmberMedicalTonnage) / h2Alm.TotalTonnage),
                GreenMedical = To6DP((h2Alm.ProjectedHouseholdRAMTonnage.GreenMedicalTonnage + h2Alm.ProjectedPublicBinRAMTonnage.GreenMedicalTonnage) / h2Alm.TotalTonnage)
            };
            Assert.AreEqual(expAlmH2Proportions, projectedAluminium.H2RamProportions);

            var expGlassH2Proportions = new RAMProportions {
                Red = To6DP((h2Glass.ProjectedHouseholdRAMTonnage.RedTonnage + h2Glass.ProjectedPublicBinRAMTonnage.RedTonnage + (h2Glass.ProjectedHouseholdDrinksContainerRAMTonnage?.RedTonnage ?? 0)) / h2Glass.TotalTonnage),
                Amber = To6DP((h2Glass.ProjectedHouseholdRAMTonnage.AmberTonnage + h2Glass.ProjectedPublicBinRAMTonnage.AmberTonnage + (h2Glass.ProjectedHouseholdDrinksContainerRAMTonnage?.AmberTonnage ?? 0)) / h2Glass.TotalTonnage),
                Green = To6DP((h2Glass.ProjectedHouseholdRAMTonnage.GreenTonnage + h2Glass.ProjectedPublicBinRAMTonnage.GreenTonnage + (h2Glass.ProjectedHouseholdDrinksContainerRAMTonnage?.GreenTonnage ?? 0)) / h2Glass.TotalTonnage),
                RedMedical = To6DP((h2Glass.ProjectedHouseholdRAMTonnage.RedMedicalTonnage + h2Glass.ProjectedPublicBinRAMTonnage.RedMedicalTonnage + (h2Glass.ProjectedHouseholdDrinksContainerRAMTonnage?.RedMedicalTonnage ?? 0)) / h2Glass.TotalTonnage),
                AmberMedical = To6DP((h2Glass.ProjectedHouseholdRAMTonnage.AmberMedicalTonnage + h2Glass.ProjectedPublicBinRAMTonnage.AmberMedicalTonnage + (h2Glass.ProjectedHouseholdDrinksContainerRAMTonnage?.AmberMedicalTonnage ?? 0)) / h2Glass.TotalTonnage),
                GreenMedical = To6DP((h2Glass.ProjectedHouseholdRAMTonnage.GreenMedicalTonnage + h2Glass.ProjectedPublicBinRAMTonnage.GreenMedicalTonnage + (h2Glass.ProjectedHouseholdDrinksContainerRAMTonnage?.GreenMedicalTonnage ?? 0)) / h2Glass.TotalTonnage)
            };
            Assert.AreEqual(expGlassH2Proportions, projectedGlass.H2RamProportions);

            Assert.AreEqual(h1HHAlm.PackagingTonnage, projectedAluminium.ProjectedHouseholdRAMTonnage.Tonnage);
            AssertWithin(To3DP(h1HHAlm.PackagingTonnage * expAlmH2Proportions.Red), projectedAluminium.ProjectedHouseholdRAMTonnage.RedTonnage);
            AssertWithin(To3DP(h1HHAlm.PackagingTonnage * expAlmH2Proportions.Amber), projectedAluminium.ProjectedHouseholdRAMTonnage.AmberTonnage);
            AssertWithin(To3DP(h1HHAlm.PackagingTonnage * expAlmH2Proportions.Green), projectedAluminium.ProjectedHouseholdRAMTonnage.GreenTonnage);
            AssertWithin(To3DP(h1HHAlm.PackagingTonnage * expAlmH2Proportions.RedMedical), projectedAluminium.ProjectedHouseholdRAMTonnage.RedMedicalTonnage);
            AssertWithin(To3DP(h1HHAlm.PackagingTonnage * expAlmH2Proportions.AmberMedical), projectedAluminium.ProjectedHouseholdRAMTonnage.AmberMedicalTonnage);
            AssertWithin(To3DP(h1HHAlm.PackagingTonnage * expAlmH2Proportions.GreenMedical), projectedAluminium.ProjectedHouseholdRAMTonnage.GreenMedicalTonnage);

            Assert.AreEqual(EmptyRAMTonnage(), projectedAluminium.ProjectedPublicBinRAMTonnage);
            Assert.IsNull(projectedAluminium.ProjectedHouseholdDrinksContainerRAMTonnage);
            Assert.AreEqual(EmptyRAMTonnage(), projectedGlass.ProjectedHouseholdRAMTonnage);
            Assert.AreEqual(EmptyRAMTonnage(), projectedGlass.ProjectedPublicBinRAMTonnage);

            Assert.AreEqual(h1HdcGlass.PackagingTonnage, projectedGlass.ProjectedHouseholdDrinksContainerRAMTonnage!.Tonnage);
            AssertWithin(To3DP(h1HdcGlass.PackagingTonnage * expGlassH2Proportions.Red), projectedGlass.ProjectedHouseholdDrinksContainerRAMTonnage.RedTonnage);
            AssertWithin(To3DP(h1HdcGlass.PackagingTonnage * expGlassH2Proportions.Amber), projectedGlass.ProjectedHouseholdDrinksContainerRAMTonnage.AmberTonnage);
            AssertWithin(To3DP(h1HdcGlass.PackagingTonnage * expGlassH2Proportions.Green), projectedGlass.ProjectedHouseholdDrinksContainerRAMTonnage.GreenTonnage);
            AssertWithin(To3DP(h1HdcGlass.PackagingTonnage * expGlassH2Proportions.RedMedical), projectedGlass.ProjectedHouseholdDrinksContainerRAMTonnage.RedMedicalTonnage);
            AssertWithin(To3DP(h1HdcGlass.PackagingTonnage * expGlassH2Proportions.AmberMedical), projectedGlass.ProjectedHouseholdDrinksContainerRAMTonnage.AmberMedicalTonnage);
            AssertWithin(To3DP(h1HdcGlass.PackagingTonnage * expGlassH2Proportions.GreenMedical), projectedGlass.ProjectedHouseholdDrinksContainerRAMTonnage.GreenMedicalTonnage);
        }
    
        [TestMethod]
        public void GetProjectedProducers_With_H1_PartialRam_H2_FullRam()
        {
            var (h1HHAlm, h1HdcGlass, h1PartialRamReportedMaterials) = GetH1PartialRamReportedMaterials();
            var (h2Alm, h2Glass, h2FullRamProjectedProducers) = GetH2FullRamProjectedProducers();    
            var result = H1ProjectedProducersBuilderUtils.GetProjectedProducers(h1PartialRamReportedMaterials, h2FullRamProjectedProducers, materials);

            Assert.AreEqual(1, result.Count());
            Assert.AreEqual(3, result.First().H1ProjectedTonnageByMaterial.Count());

            var projectedAluminium = result.First().H1ProjectedTonnageByMaterial[MaterialCodes.Aluminium];
            var projectedGlass = result.First().H1ProjectedTonnageByMaterial[MaterialCodes.Glass];

            Assert.AreEqual(ProducerReportedMaterialToRAMTonnage(h1HHAlm), projectedAluminium.HouseholdRAMTonnage);
            Assert.AreEqual(EmptyRAMTonnage(), projectedAluminium.PublicBinRAMTonnage);
            Assert.IsNull(projectedAluminium.HouseholdDrinksContainerRAMTonnage);
            Assert.AreEqual(EmptyRAMTonnage(), projectedGlass.HouseholdRAMTonnage);
            Assert.AreEqual(EmptyRAMTonnage(), projectedGlass.PublicBinRAMTonnage);
            Assert.AreEqual(ProducerReportedMaterialToRAMTonnage(h1HdcGlass), projectedGlass.HouseholdDrinksContainerRAMTonnage);

            var expH1AlmWithoutRam = CalcWithoutRam(h1HHAlm);
            var expH1HdcGlassWithoutRam = CalcWithoutRam(h1HdcGlass);
            Assert.AreEqual(expH1AlmWithoutRam, projectedAluminium.HouseholdTonnageWithoutRAM);
            Assert.AreEqual(0, projectedAluminium.PublicBinTonnageWithoutRAM);
            Assert.IsNull(projectedAluminium.HouseholdDrinksContainerTonnageWithoutRAM);
            Assert.AreEqual(0, projectedGlass.HouseholdTonnageWithoutRAM);
            Assert.AreEqual(0, projectedGlass.PublicBinTonnageWithoutRAM);
            Assert.AreEqual(expH1HdcGlassWithoutRam, projectedGlass.HouseholdDrinksContainerTonnageWithoutRAM);
            
            var expAlmH2Proportions = new RAMProportions {
                Red = To6DP((h2Alm.ProjectedHouseholdRAMTonnage.RedTonnage + h2Alm.ProjectedPublicBinRAMTonnage.RedTonnage) / h2Alm.TotalTonnage),
                Amber = To6DP((h2Alm.ProjectedHouseholdRAMTonnage.AmberTonnage + h2Alm.ProjectedPublicBinRAMTonnage.AmberTonnage) / h2Alm.TotalTonnage),
                Green = To6DP((h2Alm.ProjectedHouseholdRAMTonnage.GreenTonnage + h2Alm.ProjectedPublicBinRAMTonnage.GreenTonnage) / h2Alm.TotalTonnage),
                RedMedical = To6DP((h2Alm.ProjectedHouseholdRAMTonnage.RedMedicalTonnage + h2Alm.ProjectedPublicBinRAMTonnage.RedMedicalTonnage) / h2Alm.TotalTonnage),
                AmberMedical = To6DP((h2Alm.ProjectedHouseholdRAMTonnage.AmberMedicalTonnage + h2Alm.ProjectedPublicBinRAMTonnage.AmberMedicalTonnage) / h2Alm.TotalTonnage),
                GreenMedical = To6DP((h2Alm.ProjectedHouseholdRAMTonnage.GreenMedicalTonnage + h2Alm.ProjectedPublicBinRAMTonnage.GreenMedicalTonnage) / h2Alm.TotalTonnage)
            };
            Assert.AreEqual(expAlmH2Proportions, projectedAluminium.H2RamProportions);

            var expGlassH2Proportions = new RAMProportions {
                Red = To6DP((h2Glass.ProjectedHouseholdRAMTonnage.RedTonnage + h2Glass.ProjectedPublicBinRAMTonnage.RedTonnage + (h2Glass.ProjectedHouseholdDrinksContainerRAMTonnage?.RedTonnage ?? 0)) / h2Glass.TotalTonnage),
                Amber = To6DP((h2Glass.ProjectedHouseholdRAMTonnage.AmberTonnage + h2Glass.ProjectedPublicBinRAMTonnage.AmberTonnage + (h2Glass.ProjectedHouseholdDrinksContainerRAMTonnage?.AmberTonnage ?? 0)) / h2Glass.TotalTonnage),
                Green = To6DP((h2Glass.ProjectedHouseholdRAMTonnage.GreenTonnage + h2Glass.ProjectedPublicBinRAMTonnage.GreenTonnage + (h2Glass.ProjectedHouseholdDrinksContainerRAMTonnage?.GreenTonnage ?? 0)) / h2Glass.TotalTonnage),
                RedMedical = To6DP((h2Glass.ProjectedHouseholdRAMTonnage.RedMedicalTonnage + h2Glass.ProjectedPublicBinRAMTonnage.RedMedicalTonnage + (h2Glass.ProjectedHouseholdDrinksContainerRAMTonnage?.RedMedicalTonnage ?? 0)) / h2Glass.TotalTonnage),
                AmberMedical = To6DP((h2Glass.ProjectedHouseholdRAMTonnage.AmberMedicalTonnage + h2Glass.ProjectedPublicBinRAMTonnage.AmberMedicalTonnage + (h2Glass.ProjectedHouseholdDrinksContainerRAMTonnage?.AmberMedicalTonnage ?? 0)) / h2Glass.TotalTonnage),
                GreenMedical = To6DP((h2Glass.ProjectedHouseholdRAMTonnage.GreenMedicalTonnage + h2Glass.ProjectedPublicBinRAMTonnage.GreenMedicalTonnage + (h2Glass.ProjectedHouseholdDrinksContainerRAMTonnage?.GreenMedicalTonnage ?? 0)) / h2Glass.TotalTonnage)
            };
            Assert.AreEqual(expGlassH2Proportions, projectedGlass.H2RamProportions);

            Assert.AreEqual(h1HHAlm.PackagingTonnage, projectedAluminium.ProjectedHouseholdRAMTonnage.Tonnage);
            AssertWithin(To3DP(expH1AlmWithoutRam * expAlmH2Proportions.Red), projectedAluminium.ProjectedHouseholdRAMTonnage.RedTonnage);  
            AssertWithin(To3DP((h1HHAlm.PackagingTonnageAmber ?? 0) + (expH1AlmWithoutRam * expAlmH2Proportions.Amber)), projectedAluminium.ProjectedHouseholdRAMTonnage.AmberTonnage); 
            AssertWithin(To3DP(expH1AlmWithoutRam * expAlmH2Proportions.Green), projectedAluminium.ProjectedHouseholdRAMTonnage.GreenTonnage);  
            AssertWithin(To3DP(expH1AlmWithoutRam * expAlmH2Proportions.RedMedical), projectedAluminium.ProjectedHouseholdRAMTonnage.RedMedicalTonnage);    
            AssertWithin(To3DP((h1HHAlm.PackagingTonnageAmberMedical ?? 0) + (expH1AlmWithoutRam * expAlmH2Proportions.AmberMedical)), projectedAluminium.ProjectedHouseholdRAMTonnage.AmberMedicalTonnage);    
            AssertWithin(To3DP(expH1AlmWithoutRam * expAlmH2Proportions.GreenMedical), projectedAluminium.ProjectedHouseholdRAMTonnage.GreenMedicalTonnage);

            Assert.AreEqual(EmptyRAMTonnage(), projectedAluminium.ProjectedPublicBinRAMTonnage);
            Assert.IsNull(projectedAluminium.ProjectedHouseholdDrinksContainerRAMTonnage);
            Assert.AreEqual(EmptyRAMTonnage(), projectedGlass.ProjectedHouseholdRAMTonnage);
            Assert.AreEqual(EmptyRAMTonnage(), projectedGlass.ProjectedPublicBinRAMTonnage);

            Assert.AreEqual(h1HdcGlass.PackagingTonnage, projectedGlass.ProjectedHouseholdDrinksContainerRAMTonnage!.Tonnage);
            AssertWithin(To3DP((h1HdcGlass.PackagingTonnageRed ?? 0) + (expH1HdcGlassWithoutRam * expGlassH2Proportions.Red)), projectedGlass.ProjectedHouseholdDrinksContainerRAMTonnage.RedTonnage);
            AssertWithin(To3DP(expH1HdcGlassWithoutRam * expGlassH2Proportions.Amber), projectedGlass.ProjectedHouseholdDrinksContainerRAMTonnage.AmberTonnage);
            AssertWithin(To3DP((h1HdcGlass.PackagingTonnageGreen ?? 0) + (expH1HdcGlassWithoutRam * expGlassH2Proportions.Green)), projectedGlass.ProjectedHouseholdDrinksContainerRAMTonnage.GreenTonnage);
            AssertWithin(To3DP((h1HdcGlass.PackagingTonnageRedMedical ?? 0) + (expH1HdcGlassWithoutRam * expGlassH2Proportions.RedMedical)), projectedGlass.ProjectedHouseholdDrinksContainerRAMTonnage.RedMedicalTonnage);
            AssertWithin(To3DP(expH1HdcGlassWithoutRam * expGlassH2Proportions.AmberMedical), projectedGlass.ProjectedHouseholdDrinksContainerRAMTonnage.AmberMedicalTonnage);
            AssertWithin(To3DP(expH1HdcGlassWithoutRam * expGlassH2Proportions.GreenMedical), projectedGlass.ProjectedHouseholdDrinksContainerRAMTonnage.GreenMedicalTonnage);
        }

        [TestMethod]
        public void GetProjectedProducers_With_H1_FullRam_H2_FullRam()
        {
            var (h1HHAlm, h1HdcGlass, h1FullRamReportedMaterials) = GetH1FullRamReportedMaterials();
            var (h2Alm, h2Glass, h2FullRamProjectedProducers) = GetH2FullRamProjectedProducers();    
            var result = H1ProjectedProducersBuilderUtils.GetProjectedProducers(h1FullRamReportedMaterials, h2FullRamProjectedProducers, materials);

            Assert.AreEqual(1, result.Count());
            Assert.AreEqual(3, result.First().H1ProjectedTonnageByMaterial.Count());

            var projectedAluminium = result.First().H1ProjectedTonnageByMaterial[MaterialCodes.Aluminium];
            var projectedGlass = result.First().H1ProjectedTonnageByMaterial[MaterialCodes.Glass];

            Assert.AreEqual(ProducerReportedMaterialToRAMTonnage(h1HHAlm), projectedAluminium.HouseholdRAMTonnage);
            Assert.AreEqual(EmptyRAMTonnage(), projectedAluminium.PublicBinRAMTonnage);
            Assert.IsNull(projectedAluminium.HouseholdDrinksContainerRAMTonnage);
            Assert.AreEqual(EmptyRAMTonnage(), projectedGlass.HouseholdRAMTonnage);
            Assert.AreEqual(EmptyRAMTonnage(), projectedGlass.PublicBinRAMTonnage);
            Assert.AreEqual(ProducerReportedMaterialToRAMTonnage(h1HdcGlass), projectedGlass.HouseholdDrinksContainerRAMTonnage);

            Assert.AreEqual(0, projectedAluminium.HouseholdTonnageWithoutRAM);
            Assert.AreEqual(0, projectedAluminium.PublicBinTonnageWithoutRAM);
            Assert.IsNull(projectedAluminium.HouseholdDrinksContainerTonnageWithoutRAM);
            Assert.AreEqual(0, projectedGlass.HouseholdTonnageWithoutRAM);
            Assert.AreEqual(0, projectedGlass.PublicBinTonnageWithoutRAM);
            Assert.AreEqual(0, projectedGlass.HouseholdDrinksContainerTonnageWithoutRAM);

            Assert.AreEqual(ProducerReportedMaterialToRAMTonnage(h1HHAlm),projectedAluminium.ProjectedHouseholdRAMTonnage);
            Assert.AreEqual(EmptyRAMTonnage(), projectedAluminium.ProjectedPublicBinRAMTonnage);
            Assert.IsNull(projectedAluminium.ProjectedHouseholdDrinksContainerRAMTonnage);
            Assert.AreEqual(EmptyRAMTonnage(), projectedGlass.ProjectedHouseholdRAMTonnage);
            Assert.AreEqual(EmptyRAMTonnage(), projectedGlass.ProjectedPublicBinRAMTonnage);
            Assert.AreEqual(ProducerReportedMaterialToRAMTonnage(h1HdcGlass),projectedGlass.ProjectedHouseholdDrinksContainerRAMTonnage);
        } 

        [TestMethod]
        public void GetProjectedProducers_With_H1_PartialRam_H2_NoReportedMaterial()
        {
            var (h1HHAlm, h1HdcGlass, h1NoRamReportedMaterials) = GetH1PartialRamReportedMaterials();
            var (h2Alm, h2Glass, h2NoReportedMaterialProjectedProducers) = GetH2NoReportedMaterialProjectedProducers();
            var result = H1ProjectedProducersBuilderUtils.GetProjectedProducers(h1NoRamReportedMaterials, h2NoReportedMaterialProjectedProducers, materials);

            Assert.AreEqual(1, result.Count());
            Assert.AreEqual(3, result.First().H1ProjectedTonnageByMaterial.Count());

            var projectedAluminium = result.First().H1ProjectedTonnageByMaterial[MaterialCodes.Aluminium];
            var projectedGlass = result.First().H1ProjectedTonnageByMaterial[MaterialCodes.Glass];

            var expAlmH2RamProportions = new RAMProportions { Red = 0, Amber = 0, Green = 0, RedMedical = 0, AmberMedical = 0, GreenMedical = 0 };
            var expGlassH2RamProportions = new RAMProportions { Red = 0, Amber = 0, Green = 0, RedMedical = 0, AmberMedical = 0, GreenMedical = 0 };
            Assert.AreEqual(expAlmH2RamProportions, projectedAluminium.H2RamProportions);
            Assert.AreEqual(expAlmH2RamProportions, projectedGlass.H2RamProportions);

            var expH1AlmWithoutRam = CalcWithoutRam(h1HHAlm);
            var expH1HdcGlassWithoutRam = CalcWithoutRam(h1HdcGlass);
            Assert.AreEqual(expH1AlmWithoutRam, projectedAluminium.HouseholdTonnageWithoutRAM);
            Assert.AreEqual(0, projectedAluminium.PublicBinTonnageWithoutRAM);
            Assert.IsNull(projectedAluminium.HouseholdDrinksContainerTonnageWithoutRAM);
            Assert.AreEqual(0, projectedGlass.HouseholdTonnageWithoutRAM);
            Assert.AreEqual(0, projectedGlass.PublicBinTonnageWithoutRAM);
            Assert.AreEqual(expH1HdcGlassWithoutRam, projectedGlass.HouseholdDrinksContainerTonnageWithoutRAM);

            Assert.AreEqual(
                new RAMTonnage { 
                    Tonnage = h1HHAlm.PackagingTonnage,
                    RedTonnage = (h1HHAlm.PackagingTonnageRed ?? 0) + expH1AlmWithoutRam,
                    AmberTonnage = h1HHAlm.PackagingTonnageAmber ?? 0, 
                    GreenTonnage = h1HHAlm.PackagingTonnageGreen ?? 0, 
                    RedMedicalTonnage = h1HHAlm.PackagingTonnageRedMedical ?? 0, 
                    AmberMedicalTonnage = h1HHAlm.PackagingTonnageAmberMedical ?? 0, 
                    GreenMedicalTonnage = h1HHAlm.PackagingTonnageGreenMedical ?? 0 
                },
                projectedAluminium.ProjectedHouseholdRAMTonnage
            );

            Assert.AreEqual(EmptyRAMTonnage(), projectedAluminium.ProjectedPublicBinRAMTonnage);
            Assert.IsNull(projectedAluminium.ProjectedHouseholdDrinksContainerRAMTonnage);
            Assert.AreEqual(EmptyRAMTonnage(), projectedGlass.ProjectedHouseholdRAMTonnage);
            Assert.AreEqual(EmptyRAMTonnage(), projectedGlass.ProjectedPublicBinRAMTonnage);

            Assert.AreEqual(
                new RAMTonnage { 
                    Tonnage = h1HdcGlass.PackagingTonnage,
                    RedTonnage = (h1HdcGlass.PackagingTonnageRed ?? 0) + expH1HdcGlassWithoutRam, 
                    AmberTonnage = h1HdcGlass.PackagingTonnageAmber ?? 0, 
                    GreenTonnage = h1HdcGlass.PackagingTonnageGreen ?? 0, 
                    RedMedicalTonnage = h1HdcGlass.PackagingTonnageRedMedical ?? 0, 
                    AmberMedicalTonnage = h1HdcGlass.PackagingTonnageAmberMedical ?? 0, 
                    GreenMedicalTonnage = h1HdcGlass.PackagingTonnageGreenMedical ?? 0 
                },
                projectedGlass.ProjectedHouseholdDrinksContainerRAMTonnage
            );
        }

        [TestMethod]
        public void GetProjectedProducers_With_H1_NoReportedMaterial()
        {
            var (h1HHAlm, h1HdcGlass, h1FullRamReportedMaterials) = GetH1FullRamReportedMaterials();
            var (h2Alm, h2Glass, h2FullRamProjectedProducers) = GetH2FullRamProjectedProducers();    
            var result = H1ProjectedProducersBuilderUtils.GetProjectedProducers(h1FullRamReportedMaterials, h2FullRamProjectedProducers, materials);

            Assert.AreEqual(1, result.Count());

            var projectedOtherMaterials = result.First().H1ProjectedTonnageByMaterial[MaterialCodes.OtherMaterials];
            Assert.AreEqual(EmptyRAMTonnage(), projectedOtherMaterials.HouseholdRAMTonnage);
            Assert.AreEqual(EmptyRAMTonnage(), projectedOtherMaterials.PublicBinRAMTonnage);
            Assert.IsNull(projectedOtherMaterials.HouseholdDrinksContainerRAMTonnage);
            Assert.AreEqual(0, projectedOtherMaterials.H2RamProportions.Red);
            Assert.AreEqual(0, projectedOtherMaterials.H2RamProportions.Amber);
            Assert.AreEqual(0, projectedOtherMaterials.H2RamProportions.Green);
            Assert.AreEqual(0, projectedOtherMaterials.H2RamProportions.RedMedical);
            Assert.AreEqual(0, projectedOtherMaterials.H2RamProportions.AmberMedical);
            Assert.AreEqual(0, projectedOtherMaterials.H2RamProportions.GreenMedical);
            Assert.AreEqual(EmptyRAMTonnage(), projectedOtherMaterials.HouseholdRAMTonnage);
            Assert.AreEqual(EmptyRAMTonnage(), projectedOtherMaterials.PublicBinRAMTonnage);
            Assert.IsNull(projectedOtherMaterials.HouseholdDrinksContainerRAMTonnage);
        }

        [TestMethod]
        public void GetProjectedProducers_With_H1_NoH2Producer()
        {
            var (h1HHAlm, h1HdcGlass, h1FullRamReportedMaterials) = GetH1FullRamReportedMaterials();
            var noH2Producers = new List<CalcResultH2ProjectedProducer>();    
            var result = H1ProjectedProducersBuilderUtils.GetProjectedProducers(h1FullRamReportedMaterials, noH2Producers, materials);

            Assert.AreEqual(1, result.Count());
            Assert.AreEqual(3, result.First().ProjectedTonnageByMaterial.Count());

            var projectedAluminium = result.First().H1ProjectedTonnageByMaterial[MaterialCodes.Aluminium];
            var projectedGlass = result.First().H1ProjectedTonnageByMaterial[MaterialCodes.Glass];

            Assert.AreEqual(ProducerReportedMaterialToRAMTonnage(h1HHAlm), projectedAluminium.HouseholdRAMTonnage);
            Assert.AreEqual(EmptyRAMTonnage(), projectedAluminium.PublicBinRAMTonnage);
            Assert.IsNull(projectedAluminium.HouseholdDrinksContainerRAMTonnage);
            Assert.AreEqual(EmptyRAMTonnage(), projectedGlass.HouseholdRAMTonnage);
            Assert.AreEqual(EmptyRAMTonnage(), projectedGlass.PublicBinRAMTonnage);
            Assert.AreEqual(ProducerReportedMaterialToRAMTonnage(h1HdcGlass), projectedGlass.HouseholdDrinksContainerRAMTonnage);

            Assert.AreEqual(0, projectedAluminium.HouseholdTonnageWithoutRAM);
            Assert.AreEqual(0, projectedAluminium.PublicBinTonnageWithoutRAM);
            Assert.IsNull(projectedAluminium.HouseholdDrinksContainerTonnageWithoutRAM);
            Assert.AreEqual(0, projectedGlass.HouseholdTonnageWithoutRAM);
            Assert.AreEqual(0, projectedGlass.PublicBinTonnageWithoutRAM);
            Assert.AreEqual(0, projectedGlass.HouseholdDrinksContainerTonnageWithoutRAM);

            Assert.AreEqual(ProducerReportedMaterialToRAMTonnage(h1HHAlm),projectedAluminium.ProjectedHouseholdRAMTonnage);
            Assert.AreEqual(EmptyRAMTonnage(), projectedAluminium.ProjectedPublicBinRAMTonnage);
            Assert.IsNull(projectedAluminium.ProjectedHouseholdDrinksContainerRAMTonnage);
            Assert.AreEqual(EmptyRAMTonnage(), projectedGlass.ProjectedHouseholdRAMTonnage);
            Assert.AreEqual(EmptyRAMTonnage(), projectedGlass.ProjectedPublicBinRAMTonnage);
            Assert.AreEqual(ProducerReportedMaterialToRAMTonnage(h1HdcGlass),projectedGlass.ProjectedHouseholdDrinksContainerRAMTonnage);
        } 

        [TestMethod]
        public void ReconcileRoundingDifference_NoMissingRam()
        {
            var tonnage = new RAMTonnage { Tonnage = 200, RedTonnage = 0, AmberTonnage = 20, GreenTonnage = 30, RedMedicalTonnage = 0, AmberMedicalTonnage = 50, GreenMedicalTonnage = 0 };
            var h2Proportions = new RAMProportions { Red = 0.5m, Amber = 0, Green = 0.5m, RedMedical = 0, AmberMedical = 0, GreenMedical = 0 };
            var withProportions = H1ProjectedProducersBuilderUtils.GetProportionateRam(tonnage, 100, h2Proportions);
            var roundingDiff = withProportions.Tonnage - withProportions.GetTotalRamTonnage();
            Assert.AreEqual(0, roundingDiff);
            var reconciled = H1ProjectedProducersBuilderUtils.ReconcileRoundingDifference(withProportions);
            Assert.AreEqual(withProportions, reconciled);
        }

        [TestMethod]
        public void ReconcileRoundingDifference_MissingRam()
        {
            var tonnage = new RAMTonnage { Tonnage = 200, RedTonnage = 0, AmberTonnage = 20, GreenTonnage = 30, RedMedicalTonnage = 0, AmberMedicalTonnage = 50, GreenMedicalTonnage = 0 };
            var h2Proportions = new RAMProportions { Red = 0.333333m, Amber = 0.333333m, Green = 0.333334m, RedMedical = 0, AmberMedical = 0, GreenMedical = 0 };
            var withProportions = H1ProjectedProducersBuilderUtils.GetProportionateRam(tonnage, 100, h2Proportions);
            var roundingDiff = withProportions.Tonnage - withProportions.GetTotalRamTonnage();
            Assert.IsTrue(roundingDiff > 0);
            var reconciled = H1ProjectedProducersBuilderUtils.ReconcileRoundingDifference(withProportions);
            Assert.AreEqual(0, reconciled.Tonnage - reconciled.GetTotalRamTonnage());
            Assert.AreEqual(withProportions with { GreenTonnage = withProportions.GreenTonnage + roundingDiff }, reconciled);
        }

        [TestMethod]
        public void ReconcileRoundingDifference_SurplusRam()
        {
            var tonnage = new RAMTonnage { Tonnage = 200, RedTonnage = 0, AmberTonnage = 20, GreenTonnage = 30, RedMedicalTonnage = 0, AmberMedicalTonnage = 50, GreenMedicalTonnage = 0 };
            var h2Proportions = new RAMProportions { Red = 0.333335m, Amber = 0.333335m, Green = 0.333330m, RedMedical = 0, AmberMedical = 0, GreenMedical = 0 };
            var withProportions = H1ProjectedProducersBuilderUtils.GetProportionateRam(tonnage, 100, h2Proportions);
            var roundingDiff = withProportions.Tonnage - withProportions.GetTotalRamTonnage();
            Assert.IsTrue(roundingDiff < 0);
            var reconciled = H1ProjectedProducersBuilderUtils.ReconcileRoundingDifference(withProportions);
            Assert.AreEqual(0, reconciled.Tonnage - reconciled.GetTotalRamTonnage());
            Assert.AreEqual(withProportions with { GreenTonnage = withProportions.GreenTonnage + roundingDiff }, reconciled);
        }

        [TestMethod]
        public void ReconcileRoundingDifference_MissingRam_EqualDominantRam()
        {
            // Red and amber result in the same tonnages - assign missing ram to amber due to priority order
            var tonnage = new RAMTonnage { Tonnage = 200, RedTonnage = 30, AmberTonnage = 30, GreenTonnage = 0, RedMedicalTonnage = 0, AmberMedicalTonnage = 40, GreenMedicalTonnage = 0 };
            var h2Proportions = new RAMProportions { Red = 0.333333m, Amber = 0.333333m, Green = 0.333334m, RedMedical = 0, AmberMedical = 0, GreenMedical = 0 };
            var withProportions = H1ProjectedProducersBuilderUtils.GetProportionateRam(tonnage, 100, h2Proportions);
            var roundingDiff = withProportions.Tonnage - withProportions.GetTotalRamTonnage();
            Assert.IsTrue(roundingDiff > 0);
            Assert.AreEqual(withProportions.RedTonnage, withProportions.AmberTonnage);
            var reconciled = H1ProjectedProducersBuilderUtils.ReconcileRoundingDifference(withProportions);
            Assert.AreEqual(0, reconciled.Tonnage - reconciled.GetTotalRamTonnage());
            Assert.AreEqual(withProportions with { AmberTonnage = withProportions.AmberTonnage + roundingDiff }, reconciled);
        }

        private decimal To3DP(decimal value)
        {
            return Math.Round(value, 3);
        }

        private decimal To6DP(decimal value)
        {
            return Math.Round(value, 6);
        }

        private static void AssertWithin(decimal expected, decimal actual, decimal tolerance = 0.001m)
        {
            Assert.IsTrue(Math.Abs(expected - actual) <= tolerance);
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
                            HouseholdTonnageWithoutRAM = 0,
                            PublicBinTonnageWithoutRAM = 0,
                            TotalTonnage = 210,
                            ProjectedHouseholdRAMTonnage = new RAMTonnage { Tonnage = 75, RedTonnage = 10, AmberTonnage = 11, GreenTonnage = 12, RedMedicalTonnage = 13, AmberMedicalTonnage = 14, GreenMedicalTonnage = 15 },
                            ProjectedPublicBinRAMTonnage = new RAMTonnage { Tonnage = 135, RedTonnage = 20, AmberTonnage = 21, GreenTonnage = 22, RedMedicalTonnage = 23, AmberMedicalTonnage = 24, GreenMedicalTonnage = 25 },
                        };
            var glass = new CalcResultH2ProjectedProducerMaterialTonnage 
                        { 
                            HouseholdRAMTonnage = new RAMTonnage { Tonnage = 75, RedTonnage = 10, AmberTonnage = 11, GreenTonnage = 12, RedMedicalTonnage = 13, AmberMedicalTonnage = 14, GreenMedicalTonnage = 15 },
                            PublicBinRAMTonnage = new RAMTonnage { Tonnage = 135, RedTonnage = 20, AmberTonnage = 21, GreenTonnage = 22, RedMedicalTonnage = 23, AmberMedicalTonnage = 24, GreenMedicalTonnage = 25 },
                            HouseholdDrinksContainerRAMTonnage = new RAMTonnage { Tonnage = 195, RedTonnage = 30, AmberTonnage = 31, GreenTonnage = 32, RedMedicalTonnage = 33, AmberMedicalTonnage = 34, GreenMedicalTonnage = 35 },
                            HouseholdTonnageWithoutRAM = 0,
                            PublicBinTonnageWithoutRAM = 0,
                            HouseholdDrinksContainerTonnageWithoutRAM = 0,
                            TotalTonnage = 405,
                            ProjectedHouseholdRAMTonnage = new RAMTonnage { Tonnage = 75, RedTonnage = 10, AmberTonnage = 11, GreenTonnage = 12, RedMedicalTonnage = 13, AmberMedicalTonnage = 14, GreenMedicalTonnage = 15 },
                            ProjectedPublicBinRAMTonnage = new RAMTonnage { Tonnage = 135, RedTonnage = 20, AmberTonnage = 21, GreenTonnage = 22, RedMedicalTonnage = 23, AmberMedicalTonnage = 24, GreenMedicalTonnage = 25 },
                            ProjectedHouseholdDrinksContainerRAMTonnage = new RAMTonnage { Tonnage = 195, RedTonnage = 30, AmberTonnage = 31, GreenTonnage = 32, RedMedicalTonnage = 33, AmberMedicalTonnage = 34, GreenMedicalTonnage = 35 },
                        };
            return (alm, glass, new List<CalcResultH2ProjectedProducer>()
            {
                new()
                {
                    ProducerId = 11,
                    SubsidiaryId = null,
                    Level = string.Empty,
                    SubmissionPeriodCode = "2026-H2",
                    H2ProjectedTonnageByMaterial = new Dictionary<string, CalcResultH2ProjectedProducerMaterialTonnage>
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
                            HouseholdRAMTonnage = new RAMTonnage { Tonnage = 75, RedTonnage = 0, AmberTonnage = 11, GreenTonnage = 12, RedMedicalTonnage = 13, AmberMedicalTonnage = 14, GreenMedicalTonnage = 15 },
                            PublicBinRAMTonnage = new RAMTonnage { Tonnage = 135, RedTonnage = 0, AmberTonnage = 21, GreenTonnage = 22, RedMedicalTonnage = 23, AmberMedicalTonnage = 24, GreenMedicalTonnage = 25 },
                            HouseholdTonnageWithoutRAM = 10,
                            PublicBinTonnageWithoutRAM = 20,
                            TotalTonnage = 210, 
                            ProjectedHouseholdRAMTonnage = new RAMTonnage { Tonnage = 75, RedTonnage = 10, AmberTonnage = 11, GreenTonnage = 12, RedMedicalTonnage = 13, AmberMedicalTonnage = 14, GreenMedicalTonnage = 15 },
                            ProjectedPublicBinRAMTonnage = new RAMTonnage { Tonnage = 135, RedTonnage = 20, AmberTonnage = 21, GreenTonnage = 22, RedMedicalTonnage = 23, AmberMedicalTonnage = 24, GreenMedicalTonnage = 25 },
                            
                        };
            var glass = new CalcResultH2ProjectedProducerMaterialTonnage 
                        { 
                            HouseholdRAMTonnage = new RAMTonnage { Tonnage = 75, RedTonnage = 10, AmberTonnage = 11, GreenTonnage = 5, RedMedicalTonnage = 0, AmberMedicalTonnage = 14, GreenMedicalTonnage = 0 },
                            PublicBinRAMTonnage = new RAMTonnage { Tonnage = 135, RedTonnage = 10, AmberTonnage = 21, GreenTonnage = 22, RedMedicalTonnage = 23, AmberMedicalTonnage = 24, GreenMedicalTonnage = 25 },
                            HouseholdDrinksContainerRAMTonnage = new RAMTonnage { Tonnage = 195, RedTonnage = 30, AmberTonnage = 11, GreenTonnage = 32, RedMedicalTonnage = 33, AmberMedicalTonnage = 34, GreenMedicalTonnage = 35 },
                            HouseholdTonnageWithoutRAM = 35,
                            PublicBinTonnageWithoutRAM = 10,
                            HouseholdDrinksContainerTonnageWithoutRAM = 20,
                            TotalTonnage = 405,
                            ProjectedHouseholdRAMTonnage = new RAMTonnage { Tonnage = 75, RedTonnage = 45, AmberTonnage = 11, GreenTonnage = 5, RedMedicalTonnage = 0, AmberMedicalTonnage = 14, GreenMedicalTonnage = 0 },
                            ProjectedPublicBinRAMTonnage = new RAMTonnage { Tonnage = 135, RedTonnage = 20, AmberTonnage = 21, GreenTonnage = 22, RedMedicalTonnage = 23, AmberMedicalTonnage = 24, GreenMedicalTonnage = 25 },
                            ProjectedHouseholdDrinksContainerRAMTonnage = new RAMTonnage { Tonnage = 195, RedTonnage = 50, AmberTonnage = 11, GreenTonnage = 32, RedMedicalTonnage = 33, AmberMedicalTonnage = 34, GreenMedicalTonnage = 35 },
                        };
            return (alm, glass, new List<CalcResultH2ProjectedProducer>()
            {
                new()
                {
                    ProducerId = 11,
                    SubsidiaryId = null,
                    Level = string.Empty,
                    SubmissionPeriodCode = "2026-H2",
                    H2ProjectedTonnageByMaterial = new Dictionary<string, CalcResultH2ProjectedProducerMaterialTonnage>
                    {
                        { MaterialCodes.Aluminium, alm},
                        { MaterialCodes.Glass, glass }
                    } 
                }
            });
        }

        private (CalcResultH2ProjectedProducerMaterialTonnage, CalcResultH2ProjectedProducerMaterialTonnage, List<CalcResultH2ProjectedProducer>) GetH2NoReportedMaterialProjectedProducers()
        {
            var alm = new CalcResultH2ProjectedProducerMaterialTonnage 
                        { 
                            HouseholdRAMTonnage = EmptyRAMTonnage(),
                            PublicBinRAMTonnage = EmptyRAMTonnage(),
                            HouseholdTonnageWithoutRAM = 0,
                            PublicBinTonnageWithoutRAM = 0,
                            TotalTonnage = 0,
                            ProjectedHouseholdRAMTonnage = EmptyRAMTonnage(),
                            ProjectedPublicBinRAMTonnage = EmptyRAMTonnage(),
                        };
            var glass = new CalcResultH2ProjectedProducerMaterialTonnage 
                        { 
                            HouseholdRAMTonnage = EmptyRAMTonnage(),
                            PublicBinRAMTonnage = EmptyRAMTonnage(),
                            HouseholdDrinksContainerRAMTonnage = EmptyRAMTonnage(),
                            HouseholdTonnageWithoutRAM = 0,
                            PublicBinTonnageWithoutRAM = 0,
                            TotalTonnage = 0,
                            ProjectedHouseholdRAMTonnage = EmptyRAMTonnage(),
                            ProjectedPublicBinRAMTonnage = EmptyRAMTonnage(),
                            ProjectedHouseholdDrinksContainerRAMTonnage = EmptyRAMTonnage(),
                        };
            return (alm, glass, new List<CalcResultH2ProjectedProducer>()
            {
                new()
                {
                    ProducerId = 11,
                    SubsidiaryId = null,
                    Level = string.Empty,
                    SubmissionPeriodCode = "2026-H2",
                    H2ProjectedTonnageByMaterial = new Dictionary<string, CalcResultH2ProjectedProducerMaterialTonnage>
                    {
                        { MaterialCodes.Aluminium, alm},
                        { MaterialCodes.Glass, glass },
                        { MaterialCodes.OtherMaterials, 
                            new CalcResultH2ProjectedProducerMaterialTonnage { 
                                HouseholdRAMTonnage = new RAMTonnage { Tonnage = 75, RedTonnage = 10, AmberTonnage = 11, GreenTonnage = 12, RedMedicalTonnage = 13, AmberMedicalTonnage = 14, GreenMedicalTonnage = 15 },
                                PublicBinRAMTonnage = new RAMTonnage { Tonnage = 135, RedTonnage = 20, AmberTonnage = 21, GreenTonnage = 22, RedMedicalTonnage = 23, AmberMedicalTonnage = 24, GreenMedicalTonnage = 25 },
                                HouseholdTonnageWithoutRAM = 10,
                                PublicBinTonnageWithoutRAM = 20,
                                TotalTonnage = 310,
                                ProjectedHouseholdRAMTonnage = new RAMTonnage { Tonnage = 75, RedTonnage = 20, AmberTonnage = 11, GreenTonnage = 12, RedMedicalTonnage = 13, AmberMedicalTonnage = 14, GreenMedicalTonnage = 15 },
                                ProjectedPublicBinRAMTonnage = new RAMTonnage { Tonnage = 135, RedTonnage = 40, AmberTonnage = 21, GreenTonnage = 22, RedMedicalTonnage = 23, AmberMedicalTonnage = 24, GreenMedicalTonnage = 25 },
                                
                            }
                        }
                    } 
                }
            });
        }
    }
}