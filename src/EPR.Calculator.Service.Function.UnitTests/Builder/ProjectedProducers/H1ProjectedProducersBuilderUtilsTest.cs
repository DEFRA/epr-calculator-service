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
                Red = To6DP((h2Alm.HouseholdRAMTonnage.RedTonnage + h2Alm.PublicBinRAMTonnage.RedTonnage) / h2Alm.TotalTonnage),
                Amber = To6DP((h2Alm.HouseholdRAMTonnage.AmberTonnage + h2Alm.PublicBinRAMTonnage.AmberTonnage) / h2Alm.TotalTonnage),
                Green = To6DP((h2Alm.HouseholdRAMTonnage.GreenTonnage + h2Alm.PublicBinRAMTonnage.GreenTonnage) / h2Alm.TotalTonnage),
                RedMedical = To6DP((h2Alm.HouseholdRAMTonnage.RedMedicalTonnage + h2Alm.PublicBinRAMTonnage.RedMedicalTonnage) / h2Alm.TotalTonnage),
                AmberMedical = To6DP((h2Alm.HouseholdRAMTonnage.AmberMedicalTonnage + h2Alm.PublicBinRAMTonnage.AmberMedicalTonnage) / h2Alm.TotalTonnage),
                GreenMedical = To6DP((h2Alm.HouseholdRAMTonnage.GreenMedicalTonnage + h2Alm.PublicBinRAMTonnage.GreenMedicalTonnage) / h2Alm.TotalTonnage)
            };
            Assert.AreEqual(expAlmH2Proportions, projectedAluminium.H2RamProportions);

            var expGlassH2Proportions = new RAMProportions{
                Red = To6DP((h2Glass.HouseholdRAMTonnage.RedTonnage + h2Glass.PublicBinRAMTonnage.RedTonnage + (h2Glass.HouseholdDrinksContainerRAMTonnage?.RedTonnage ?? 0)) / h2Glass.TotalTonnage),
                Amber = To6DP((h2Glass.HouseholdRAMTonnage.AmberTonnage + h2Glass.PublicBinRAMTonnage.AmberTonnage + (h2Glass.HouseholdDrinksContainerRAMTonnage?.AmberTonnage ?? 0)) / h2Glass.TotalTonnage),
                Green = To6DP((h2Glass.HouseholdRAMTonnage.GreenTonnage + h2Glass.PublicBinRAMTonnage.GreenTonnage + (h2Glass.HouseholdDrinksContainerRAMTonnage?.GreenTonnage ?? 0)) / h2Glass.TotalTonnage),
                RedMedical = To6DP((h2Glass.HouseholdRAMTonnage.RedMedicalTonnage + h2Glass.PublicBinRAMTonnage.RedMedicalTonnage + (h2Glass.HouseholdDrinksContainerRAMTonnage?.RedMedicalTonnage ?? 0)) / h2Glass.TotalTonnage),
                AmberMedical = To6DP((h2Glass.HouseholdRAMTonnage.AmberMedicalTonnage + h2Glass.PublicBinRAMTonnage.AmberMedicalTonnage + (h2Glass.HouseholdDrinksContainerRAMTonnage?.AmberMedicalTonnage ?? 0)) / h2Glass.TotalTonnage),
                GreenMedical = To6DP((h2Glass.HouseholdRAMTonnage.GreenMedicalTonnage + h2Glass.PublicBinRAMTonnage.GreenMedicalTonnage + (h2Glass.HouseholdDrinksContainerRAMTonnage?.GreenMedicalTonnage ?? 0)) / h2Glass.TotalTonnage)
            };
            Assert.AreEqual(expGlassH2Proportions, projectedGlass.H2RamProportions);

            Assert.AreEqual(
                new RAMTonnage { 
                    Tonnage = h1HHAlm.PackagingTonnage, 
                    RedTonnage = To3DP((h1HHAlm.PackagingTonnage * expAlmH2Proportions.Red)), 
                    AmberTonnage = To3DP(h1HHAlm.PackagingTonnage * expAlmH2Proportions.Amber),  
                    GreenTonnage = To3DP(h1HHAlm.PackagingTonnage * expAlmH2Proportions.Green), 
                    RedMedicalTonnage = To3DP(h1HHAlm.PackagingTonnage * expAlmH2Proportions.RedMedical),  
                    AmberMedicalTonnage = To3DP(h1HHAlm.PackagingTonnage * expAlmH2Proportions.AmberMedical),  
                    GreenMedicalTonnage = To3DP(h1HHAlm.PackagingTonnage * expAlmH2Proportions.GreenMedical)
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
                    RedTonnage = To3DP(h1HdcGlass.PackagingTonnage * expGlassH2Proportions.Red), 
                    AmberTonnage = To3DP(h1HdcGlass.PackagingTonnage * expGlassH2Proportions.Amber), 
                    GreenTonnage = To3DP(h1HdcGlass.PackagingTonnage * expGlassH2Proportions.Green), 
                    RedMedicalTonnage = To3DP(h1HdcGlass.PackagingTonnage * expGlassH2Proportions.RedMedical), 
                    AmberMedicalTonnage = To3DP(h1HdcGlass.PackagingTonnage * expGlassH2Proportions.AmberMedical),
                    GreenMedicalTonnage = To3DP(h1HdcGlass.PackagingTonnage * expGlassH2Proportions.GreenMedical) 
                },
                projectedGlass.ProjectedHouseholdDrinksContainerRAMTonnage
            );
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
                Red = To6DP((
                    h2Alm.HouseholdRAMTonnage.RedTonnage + h2Alm.PublicBinRAMTonnage.RedTonnage + 
                    h2Alm.HouseholdTonnageWithoutRAM + h2Alm.PublicBinTonnageWithoutRAM
                ) / h2Alm.TotalTonnage),
                Amber = To6DP((h2Alm.HouseholdRAMTonnage.AmberTonnage + h2Alm.PublicBinRAMTonnage.AmberTonnage) / h2Alm.TotalTonnage),
                Green = To6DP((h2Alm.HouseholdRAMTonnage.GreenTonnage + h2Alm.PublicBinRAMTonnage.GreenTonnage) / h2Alm.TotalTonnage),
                RedMedical = To6DP((h2Alm.HouseholdRAMTonnage.RedMedicalTonnage + h2Alm.PublicBinRAMTonnage.RedMedicalTonnage) / h2Alm.TotalTonnage),
                AmberMedical = To6DP((h2Alm.HouseholdRAMTonnage.AmberMedicalTonnage + h2Alm.PublicBinRAMTonnage.AmberMedicalTonnage) / h2Alm.TotalTonnage),
                GreenMedical = To6DP((h2Alm.HouseholdRAMTonnage.GreenMedicalTonnage + h2Alm.PublicBinRAMTonnage.GreenMedicalTonnage) / h2Alm.TotalTonnage)
            };
            Assert.AreEqual(expAlmH2Proportions, projectedAluminium.H2RamProportions);

            var expGlassH2Proportions = new RAMProportions {
                Red = To6DP((
                    h2Glass.HouseholdRAMTonnage.RedTonnage + h2Glass.PublicBinRAMTonnage.RedTonnage + (h2Glass.HouseholdDrinksContainerRAMTonnage?.RedTonnage ?? 0) +
                    h2Glass.HouseholdTonnageWithoutRAM + h2Glass.PublicBinTonnageWithoutRAM + (h2Glass.HouseholdDrinksContainerTonnageWithoutRAM ?? 0)
                ) / h2Glass.TotalTonnage),
                Amber = To6DP((h2Glass.HouseholdRAMTonnage.AmberTonnage + h2Glass.PublicBinRAMTonnage.AmberTonnage + (h2Glass.HouseholdDrinksContainerRAMTonnage?.AmberTonnage ?? 0)) / h2Glass.TotalTonnage),
                Green = To6DP((h2Glass.HouseholdRAMTonnage.GreenTonnage + h2Glass.PublicBinRAMTonnage.GreenTonnage + (h2Glass.HouseholdDrinksContainerRAMTonnage?.GreenTonnage ?? 0)) / h2Glass.TotalTonnage),
                RedMedical = To6DP((h2Glass.HouseholdRAMTonnage.RedMedicalTonnage + h2Glass.PublicBinRAMTonnage.RedMedicalTonnage + (h2Glass.HouseholdDrinksContainerRAMTonnage?.RedMedicalTonnage ?? 0)) / h2Glass.TotalTonnage),
                AmberMedical = To6DP((h2Glass.HouseholdRAMTonnage.AmberMedicalTonnage + h2Glass.PublicBinRAMTonnage.AmberMedicalTonnage + (h2Glass.HouseholdDrinksContainerRAMTonnage?.AmberMedicalTonnage ?? 0)) / h2Glass.TotalTonnage),
                GreenMedical = To6DP((h2Glass.HouseholdRAMTonnage.GreenMedicalTonnage + h2Glass.PublicBinRAMTonnage.GreenMedicalTonnage + (h2Glass.HouseholdDrinksContainerRAMTonnage?.GreenMedicalTonnage ?? 0)) / h2Glass.TotalTonnage)
            };
            Assert.AreEqual(expGlassH2Proportions, projectedGlass.H2RamProportions);

            Assert.AreEqual(
                new RAMTonnage { 
                    Tonnage = h1HHAlm.PackagingTonnage,
                    RedTonnage = To3DP(h1HHAlm.PackagingTonnage * expAlmH2Proportions.Red),
                    AmberTonnage = To3DP(h1HHAlm.PackagingTonnage * expAlmH2Proportions.Amber),
                    GreenTonnage = To3DP(h1HHAlm.PackagingTonnage * expAlmH2Proportions.Green),
                    RedMedicalTonnage = To3DP(h1HHAlm.PackagingTonnage * expAlmH2Proportions.RedMedical),
                    AmberMedicalTonnage = To3DP(h1HHAlm.PackagingTonnage * expAlmH2Proportions.AmberMedical),
                    GreenMedicalTonnage = To3DP(h1HHAlm.PackagingTonnage * expAlmH2Proportions.GreenMedical) 
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
                    RedTonnage = To3DP(h1HdcGlass.PackagingTonnage * expGlassH2Proportions.Red),
                    AmberTonnage = To3DP(h1HdcGlass.PackagingTonnage * expGlassH2Proportions.Amber), 
                    GreenTonnage = To3DP(h1HdcGlass.PackagingTonnage * expGlassH2Proportions.Green), 
                    RedMedicalTonnage = To3DP(h1HdcGlass.PackagingTonnage * expGlassH2Proportions.RedMedical), 
                    AmberMedicalTonnage = To3DP(h1HdcGlass.PackagingTonnage * expGlassH2Proportions.AmberMedical), 
                    GreenMedicalTonnage = To3DP(h1HdcGlass.PackagingTonnage * expGlassH2Proportions.GreenMedical) 
                },
                projectedGlass.ProjectedHouseholdDrinksContainerRAMTonnage
            );
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
                Red = To6DP((h2Alm.HouseholdRAMTonnage.RedTonnage + h2Alm.PublicBinRAMTonnage.RedTonnage) / h2Alm.TotalTonnage),
                Amber = To6DP((h2Alm.HouseholdRAMTonnage.AmberTonnage + h2Alm.PublicBinRAMTonnage.AmberTonnage) / h2Alm.TotalTonnage),
                Green = To6DP((h2Alm.HouseholdRAMTonnage.GreenTonnage + h2Alm.PublicBinRAMTonnage.GreenTonnage) / h2Alm.TotalTonnage),
                RedMedical = To6DP((h2Alm.HouseholdRAMTonnage.RedMedicalTonnage + h2Alm.PublicBinRAMTonnage.RedMedicalTonnage) / h2Alm.TotalTonnage),
                AmberMedical = To6DP((h2Alm.HouseholdRAMTonnage.AmberMedicalTonnage + h2Alm.PublicBinRAMTonnage.AmberMedicalTonnage) / h2Alm.TotalTonnage),
                GreenMedical = To6DP((h2Alm.HouseholdRAMTonnage.GreenMedicalTonnage + h2Alm.PublicBinRAMTonnage.GreenMedicalTonnage) / h2Alm.TotalTonnage)
            };
            Assert.AreEqual(expAlmH2Proportions, projectedAluminium.H2RamProportions);

            var expGlassH2Proportions = new RAMProportions {
                Red = To6DP((h2Glass.HouseholdRAMTonnage.RedTonnage + h2Glass.PublicBinRAMTonnage.RedTonnage + (h2Glass.HouseholdDrinksContainerRAMTonnage?.RedTonnage ?? 0)) / h2Glass.TotalTonnage),
                Amber = To6DP((h2Glass.HouseholdRAMTonnage.AmberTonnage + h2Glass.PublicBinRAMTonnage.AmberTonnage + (h2Glass.HouseholdDrinksContainerRAMTonnage?.AmberTonnage ?? 0)) / h2Glass.TotalTonnage),
                Green = To6DP((h2Glass.HouseholdRAMTonnage.GreenTonnage + h2Glass.PublicBinRAMTonnage.GreenTonnage + (h2Glass.HouseholdDrinksContainerRAMTonnage?.GreenTonnage ?? 0)) / h2Glass.TotalTonnage),
                RedMedical = To6DP((h2Glass.HouseholdRAMTonnage.RedMedicalTonnage + h2Glass.PublicBinRAMTonnage.RedMedicalTonnage + (h2Glass.HouseholdDrinksContainerRAMTonnage?.RedMedicalTonnage ?? 0)) / h2Glass.TotalTonnage),
                AmberMedical = To6DP((h2Glass.HouseholdRAMTonnage.AmberMedicalTonnage + h2Glass.PublicBinRAMTonnage.AmberMedicalTonnage + (h2Glass.HouseholdDrinksContainerRAMTonnage?.AmberMedicalTonnage ?? 0)) / h2Glass.TotalTonnage),
                GreenMedical = To6DP((h2Glass.HouseholdRAMTonnage.GreenMedicalTonnage + h2Glass.PublicBinRAMTonnage.GreenMedicalTonnage + (h2Glass.HouseholdDrinksContainerRAMTonnage?.GreenMedicalTonnage ?? 0)) / h2Glass.TotalTonnage)
            };
            Assert.AreEqual(expGlassH2Proportions, projectedGlass.H2RamProportions);

            Assert.AreEqual(
                new RAMTonnage { 
                    Tonnage = h1HHAlm.PackagingTonnage,
                    RedTonnage = To3DP(expH1AlmWithoutRam * expAlmH2Proportions.Red), 
                    AmberTonnage = To3DP((h1HHAlm.PackagingTonnageAmber ?? 0) + (expH1AlmWithoutRam * expAlmH2Proportions.Amber)), 
                    GreenTonnage = To3DP(expH1AlmWithoutRam * expAlmH2Proportions.Green), 
                    RedMedicalTonnage = To3DP(expH1AlmWithoutRam * expAlmH2Proportions.RedMedical), 
                    AmberMedicalTonnage = To3DP((h1HHAlm.PackagingTonnageAmberMedical ?? 0) + (expH1AlmWithoutRam * expAlmH2Proportions.AmberMedical)), 
                    GreenMedicalTonnage = To3DP(expH1AlmWithoutRam * expAlmH2Proportions.GreenMedical)
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
                    RedTonnage = To3DP((h1HdcGlass.PackagingTonnageRed ?? 0) + (expH1HdcGlassWithoutRam * expGlassH2Proportions.Red)),
                    AmberTonnage = To3DP((expH1HdcGlassWithoutRam * expGlassH2Proportions.Amber)),
                    GreenTonnage = To3DP((h1HdcGlass.PackagingTonnageGreen ?? 0) + (expH1HdcGlassWithoutRam * expGlassH2Proportions.Green)),
                    RedMedicalTonnage = To3DP((h1HdcGlass.PackagingTonnageRedMedical ?? 0) + (expH1HdcGlassWithoutRam * expGlassH2Proportions.RedMedical)),
                    AmberMedicalTonnage = To3DP(expH1HdcGlassWithoutRam * expGlassH2Proportions.AmberMedical),
                    GreenMedicalTonnage = To3DP(expH1HdcGlassWithoutRam * expGlassH2Proportions.GreenMedical) 
                },
                projectedGlass.ProjectedHouseholdDrinksContainerRAMTonnage
            );
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

        private decimal To3DP(decimal value)
        {
            return Math.Round(value, 3);
        }

        private decimal To6DP(decimal value)
        {
            return Math.Round(value, 6);
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
                            TotalTonnage = 310,
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
                            HouseholdRAMTonnage = new RAMTonnage { Tonnage = 75, RedTonnage = 10, AmberTonnage = 11, GreenTonnage = 12, RedMedicalTonnage = 13, AmberMedicalTonnage = 14, GreenMedicalTonnage = 15 },
                            PublicBinRAMTonnage = new RAMTonnage { Tonnage = 135, RedTonnage = 20, AmberTonnage = 21, GreenTonnage = 22, RedMedicalTonnage = 23, AmberMedicalTonnage = 24, GreenMedicalTonnage = 25 },
                            HouseholdTonnageWithoutRAM = 10,
                            PublicBinTonnageWithoutRAM = 20,
                            TotalTonnage = 310, 
                            ProjectedHouseholdRAMTonnage = new RAMTonnage { Tonnage = 75, RedTonnage = 20, AmberTonnage = 11, GreenTonnage = 12, RedMedicalTonnage = 13, AmberMedicalTonnage = 14, GreenMedicalTonnage = 15 },
                            ProjectedPublicBinRAMTonnage = new RAMTonnage { Tonnage = 135, RedTonnage = 40, AmberTonnage = 21, GreenTonnage = 22, RedMedicalTonnage = 23, AmberMedicalTonnage = 24, GreenMedicalTonnage = 25 },
                            
                        };
            var glass = new CalcResultH2ProjectedProducerMaterialTonnage 
                        { 
                            HouseholdRAMTonnage = new RAMTonnage { Tonnage = 75, RedTonnage = 10, AmberTonnage = 11, GreenTonnage = 12, RedMedicalTonnage = 13, AmberMedicalTonnage = 14, GreenMedicalTonnage = 15 },
                            PublicBinRAMTonnage = new RAMTonnage { Tonnage = 135, RedTonnage = 20, AmberTonnage = 21, GreenTonnage = 22, RedMedicalTonnage = 23, AmberMedicalTonnage = 24, GreenMedicalTonnage = 25 },
                            HouseholdDrinksContainerRAMTonnage = new RAMTonnage { Tonnage = 195, RedTonnage = 30, AmberTonnage = 31, GreenTonnage = 32, RedMedicalTonnage = 33, AmberMedicalTonnage = 34, GreenMedicalTonnage = 35 },
                            HouseholdTonnageWithoutRAM = 30,
                            PublicBinTonnageWithoutRAM = 10,
                            HouseholdDrinksContainerTonnageWithoutRAM = 20,
                            TotalTonnage = 405,
                            ProjectedHouseholdRAMTonnage = new RAMTonnage { Tonnage = 75, RedTonnage = 40, AmberTonnage = 11, GreenTonnage = 12, RedMedicalTonnage = 13, AmberMedicalTonnage = 14, GreenMedicalTonnage = 15 },
                            ProjectedPublicBinRAMTonnage = new RAMTonnage { Tonnage = 135, RedTonnage = 30, AmberTonnage = 21, GreenTonnage = 22, RedMedicalTonnage = 23, AmberMedicalTonnage = 24, GreenMedicalTonnage = 25 },
                            ProjectedHouseholdDrinksContainerRAMTonnage = new RAMTonnage { Tonnage = 195, RedTonnage = 50, AmberTonnage = 31, GreenTonnage = 32, RedMedicalTonnage = 33, AmberMedicalTonnage = 34, GreenMedicalTonnage = 35 },
                            
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