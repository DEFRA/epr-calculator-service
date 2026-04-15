using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.Models;
using EPR.Calculator.Service.Common;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Interface;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Services;
using EPR.Calculator.Service.Function.UnitTests.Builder;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPR.Calculator.Service.Function.UnitTests.Services
{
    [TestClass]
    public class ProjectedProducersServiceTest
    {
        private readonly ApplicationDBContext context;

        public ProjectedProducersServiceTest()
        {
            var options = new DbContextOptionsBuilder<ApplicationDBContext>()
                .UseInMemoryDatabase(databaseName: "PayCal")
                .Options;
            context = new ApplicationDBContext(options);
        }

        [TestCleanup]
        public void TearDown()
        {
            context?.Database.EnsureDeleted();
        }

        [TestMethod]
        public async Task StoreProjectedProducers_WorksAsExpected()
        {
            TestDataHelper.SeedDatabaseForInitialRun(context);

            var producerReportedMaterialProjectedChunker = new Mock<IDbLoadingChunkerService<ProducerReportedMaterialProjected>>();
            List<ProducerReportedMaterialProjected> savedProjectedProducers = new List<ProducerReportedMaterialProjected>();

            producerReportedMaterialProjectedChunker
                .Setup(c => c.InsertRecords(It.IsAny<IEnumerable<ProducerReportedMaterialProjected>>()))
                .Returns((IEnumerable<ProducerReportedMaterialProjected> arg) =>
                {
                    savedProjectedProducers = arg.ToList();
                    return Task.FromResult(arg);
                });

            var service = new ProjectedProducersService(context, producerReportedMaterialProjectedChunker.Object);
            
            var h2ProjectedProducers = new List<CalcResultH2ProjectedProducer>
            {
                new CalcResultH2ProjectedProducer
                {
                    ProducerId = 1,
                    Level = CommonConstants.LevelOne.ToString(),
                    SubmissionPeriodCode = "2025-H2",
                    ProjectedTonnageByMaterial = new Dictionary<string, CalcResultH2ProjectedProducerMaterialTonnage>
                    {
                        { 
                            "PL", 
                            new() { 
                                HouseholdRAMTonnage = new RAMTonnage(),
                                PublicBinRAMTonnage = new RAMTonnage(),   
                                HouseholdTonnageDefaultedRed = 10,
                                PublicBinTonnageDefaultedRed = 0,
                                TotalTonnage = 150
                            }
                        },
                        { 
                            "ST", 
                            new() { 
                                HouseholdRAMTonnage = new RAMTonnage(),
                                PublicBinRAMTonnage = new RAMTonnage(),   
                                HouseholdTonnageDefaultedRed = 0,
                                PublicBinTonnageDefaultedRed = 0,
                                TotalTonnage = 200
                            }
                        }
                    }
                },
                new CalcResultH2ProjectedProducer
                {
                    ProducerId = 2,
                    Level = CommonConstants.LevelOne.ToString(),
                    IsSubtotal = true,
                    SubmissionPeriodCode = "2025-H2",
                    ProjectedTonnageByMaterial = new Dictionary<string, CalcResultH2ProjectedProducerMaterialTonnage>
                    {
                        { 
                            "GL", 
                            new() { 
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
                new CalcResultH2ProjectedProducer
                {
                    ProducerId = 2,
                    Level = CommonConstants.LevelTwo.ToString(),
                    SubmissionPeriodCode = "2025-H2",
                    ProjectedTonnageByMaterial = new Dictionary<string, CalcResultH2ProjectedProducerMaterialTonnage>
                    {
                        { 
                            "GL", 
                            new() { 
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

            var h1ProjectedProducers = new List<CalcResultH1ProjectedProducer>
            {
                new CalcResultH1ProjectedProducer
                {
                    ProducerId = 1,
                    Level = CommonConstants.LevelOne.ToString(),
                    SubmissionPeriodCode = "2025-H1",
                    ProjectedTonnageByMaterial = new Dictionary<string, CalcResultH1ProjectedProducerMaterialTonnage>
                    {
                        { 
                            "PL", 
                            new() { 
                                HouseholdRAMTonnage = new RAMTonnage() { Tonnage = 100, RedTonnage = 10, AmberTonnage = 10, GreenTonnage = 10, RedMedicalTonnage = 10, AmberMedicalTonnage = 10, GreenMedicalTonnage = 0 },
                                PublicBinRAMTonnage = new RAMTonnage() { Tonnage = 0, RedTonnage = 0, AmberTonnage = 0, GreenTonnage = 0, RedMedicalTonnage = 0, AmberMedicalTonnage = 0, GreenMedicalTonnage = 0 },   
                                HouseholdTonnageWithoutRAM = 50,
                                PublicBinTonnageWithoutRAM = 0,
                                RedH2Proportion = 0.2m,
                                AmberH2Proportion = 0.2m,
                                GreenH2Proportion = 0.2m,
                                RedMedicalH2Proportion = 0.2m,
                                AmberMedicalH2Proportion = 0.2m,
                                GreenMedicalH2Proportion = 0,
                                ProjectedHouseholdRAMTonnage = new RAMTonnage() { Tonnage = 100, RedTonnage = 20, AmberTonnage = 20, GreenTonnage = 20, RedMedicalTonnage = 20, AmberMedicalTonnage = 20, GreenMedicalTonnage = 0 },
                                ProjectedPublicBinRAMTonnage = new RAMTonnage() { Tonnage = 0, RedTonnage = 0, AmberTonnage = 0, GreenTonnage = 0, RedMedicalTonnage = 0, AmberMedicalTonnage = 0, GreenMedicalTonnage = 0 }
                            }
                        },
                        { 
                            "ST", 
                            new() { 
                                HouseholdRAMTonnage = new RAMTonnage() { Tonnage = 0, RedTonnage = 0, AmberTonnage = 0, GreenTonnage = 0, RedMedicalTonnage = 0, AmberMedicalTonnage = 0, GreenMedicalTonnage = 0 },
                                PublicBinRAMTonnage = new RAMTonnage() { Tonnage = 50, RedTonnage = 0, AmberTonnage = 0, GreenTonnage = 0, RedMedicalTonnage = 0, AmberMedicalTonnage = 0, GreenMedicalTonnage = 0 },   
                                HouseholdTonnageWithoutRAM = 0,
                                PublicBinTonnageWithoutRAM = 50,
                                RedH2Proportion = 1m,
                                AmberH2Proportion = 0,
                                GreenH2Proportion = 0,
                                RedMedicalH2Proportion = 0m,
                                AmberMedicalH2Proportion = 0,
                                GreenMedicalH2Proportion = 0,
                                ProjectedHouseholdRAMTonnage = new RAMTonnage() { Tonnage = 0, RedTonnage = 0, AmberTonnage = 0, GreenTonnage = 0, RedMedicalTonnage = 0, AmberMedicalTonnage = 0, GreenMedicalTonnage = 0 },
                                ProjectedPublicBinRAMTonnage = new RAMTonnage() { Tonnage = 50, RedTonnage = 50, AmberTonnage = 0, GreenTonnage = 0, RedMedicalTonnage = 0, AmberMedicalTonnage = 0, GreenMedicalTonnage = 0 },   
                            }
                        }
                    }
                },
                new CalcResultH1ProjectedProducer
                {
                    ProducerId = 2,
                    Level = CommonConstants.LevelOne.ToString(),
                    IsSubtotal = true,
                    SubmissionPeriodCode = "2025-H1",
                    ProjectedTonnageByMaterial = new Dictionary<string, CalcResultH1ProjectedProducerMaterialTonnage>
                    {
                        { 
                            "GL", 
                            new() { 
                                HouseholdRAMTonnage = new RAMTonnage() { Tonnage = 0, RedTonnage = 0, AmberTonnage = 0, GreenTonnage = 0, RedMedicalTonnage = 0, AmberMedicalTonnage = 0, GreenMedicalTonnage = 0 },
                                PublicBinRAMTonnage = new RAMTonnage() { Tonnage = 50, RedTonnage = 20, AmberTonnage = 0, GreenTonnage = 0, RedMedicalTonnage = 0, AmberMedicalTonnage = 0, GreenMedicalTonnage = 0 },  
                                HouseholdDrinksContainerRAMTonnage = new RAMTonnage() { Tonnage = 200, RedTonnage = 0, AmberTonnage = 100, GreenTonnage = 0, RedMedicalTonnage = 0, AmberMedicalTonnage = 0, GreenMedicalTonnage = 0 },   
                                HouseholdTonnageWithoutRAM = 20,
                                PublicBinTonnageWithoutRAM = 0,
                                HouseholdDrinksContainerTonnageWithoutRAM = 100,
                                RedH2Proportion = 0,
                                AmberH2Proportion = 0,
                                GreenH2Proportion = 0,
                                RedMedicalH2Proportion = 0.5m,
                                AmberMedicalH2Proportion = 0,
                                GreenMedicalH2Proportion = 0.5m,
                                ProjectedHouseholdRAMTonnage = new RAMTonnage() { Tonnage = 50, RedTonnage = 20, AmberTonnage = 0, GreenTonnage = 0, RedMedicalTonnage = 10, AmberMedicalTonnage = 0, GreenMedicalTonnage = 10 },
                                ProjectedPublicBinRAMTonnage = new RAMTonnage() { Tonnage = 0, RedTonnage = 0, AmberTonnage = 0, GreenTonnage = 0, RedMedicalTonnage = 0, AmberMedicalTonnage = 0, GreenMedicalTonnage = 0 },
                                ProjectedHouseholdDrinksContainerRAMTonnage = new RAMTonnage() { Tonnage = 200, RedTonnage = 0, AmberTonnage = 100, GreenTonnage = 0, RedMedicalTonnage = 50, AmberMedicalTonnage = 0, GreenMedicalTonnage = 50 }
                            }
                        }
                    }
                },
                new CalcResultH1ProjectedProducer
                {
                    ProducerId = 2,
                    Level = CommonConstants.LevelTwo.ToString(),
                    SubmissionPeriodCode = "2025-H1",
                    ProjectedTonnageByMaterial = new Dictionary<string, CalcResultH1ProjectedProducerMaterialTonnage>
                    {
                        { 
                            "GL", 
                            new() { 
                                HouseholdRAMTonnage = new RAMTonnage() { Tonnage = 0, RedTonnage = 0, AmberTonnage = 0, GreenTonnage = 0, RedMedicalTonnage = 0, AmberMedicalTonnage = 0, GreenMedicalTonnage = 0 },
                                PublicBinRAMTonnage = new RAMTonnage() { Tonnage = 50, RedTonnage = 20, AmberTonnage = 0, GreenTonnage = 0, RedMedicalTonnage = 0, AmberMedicalTonnage = 0, GreenMedicalTonnage = 0 },  
                                HouseholdDrinksContainerRAMTonnage = new RAMTonnage() { Tonnage = 200, RedTonnage = 0, AmberTonnage = 100, GreenTonnage = 0, RedMedicalTonnage = 0, AmberMedicalTonnage = 0, GreenMedicalTonnage = 0 },   
                                HouseholdTonnageWithoutRAM = 0,
                                PublicBinTonnageWithoutRAM = 20,
                                HouseholdDrinksContainerTonnageWithoutRAM = 100,
                                RedH2Proportion = 0,
                                AmberH2Proportion = 0,
                                GreenH2Proportion = 0,
                                RedMedicalH2Proportion = 0.5m,
                                AmberMedicalH2Proportion = 0,
                                GreenMedicalH2Proportion = 0.5m,
                                ProjectedHouseholdRAMTonnage = new RAMTonnage() { Tonnage = 0, RedTonnage = 0, AmberTonnage = 0, GreenTonnage = 0, RedMedicalTonnage = 0, AmberMedicalTonnage = 0, GreenMedicalTonnage = 0 },
                                ProjectedPublicBinRAMTonnage = new RAMTonnage() { Tonnage = 50, RedTonnage = 20, AmberTonnage = 0, GreenTonnage = 0, RedMedicalTonnage = 10, AmberMedicalTonnage = 0, GreenMedicalTonnage = 10 },
                                ProjectedHouseholdDrinksContainerRAMTonnage = new RAMTonnage() { Tonnage = 200, RedTonnage = 0, AmberTonnage = 100, GreenTonnage = 0, RedMedicalTonnage = 50, AmberMedicalTonnage = 0, GreenMedicalTonnage = 50 }
                            }
                        }
                    }
                }
            };

            await service.StoreProjectedProducers(1, h2ProjectedProducers, h1ProjectedProducers);

            Assert.AreEqual(20, savedProjectedProducers.Count());
            var submissionPeriods = savedProjectedProducers.GroupBy(p => p.SubmissionPeriod).ToDictionary(g => g.Key, g => g.ToList());

            var h1Group = submissionPeriods["2025-H1"];
            var h2Group = submissionPeriods["2025-H2"];
            Assert.AreEqual(10, h1Group.Count());
            Assert.AreEqual(10, h2Group.Count());

            //Existing copied into projected
            Assert.IsTrue(h1Group.Any(p => p.ProducerDetailId == 1 && p.MaterialId == 1 && p.PackagingType == PackagingTypes.Household && p.PackagingTonnage == 50 && p.PackagingTonnageRed == 50));
            Assert.IsTrue(h1Group.Any(p => p.ProducerDetailId == 1 && p.MaterialId == 3 && p.PackagingType == PackagingTypes.HouseholdDrinksContainers && p.PackagingTonnage == 150 && p.PackagingTonnageAmberMedical == 150));
            Assert.IsTrue(h2Group.Any(p => p.ProducerDetailId == 1 && p.MaterialId == 1 && p.PackagingType == PackagingTypes.Household && p.PackagingTonnage == 50 && p.PackagingTonnageAmber == 50));
            Assert.IsTrue(h2Group.Any(p => p.ProducerDetailId == 2 && p.MaterialId == 2 && p.PackagingType == PackagingTypes.PublicBin && p.PackagingTonnage == 100 && p.PackagingTonnageRedMedical == 100));

            // Additional for H2 where missing RAM defaulted as red
            Assert.IsTrue(h2Group.Any(p => p.ProducerDetailId == 1 && p.MaterialId == 1 && p.PackagingType == PackagingTypes.Household && p.PackagingTonnage == 0 && p.PackagingTonnageRed == 10 && p.PackagingTonnageAmber == 0 && p.PackagingTonnageGreen == 0 && p.PackagingTonnageRedMedical == 0 && p.PackagingTonnageAmberMedical == 0 && p.PackagingTonnageGreenMedical == 0));
            Assert.IsTrue(h2Group.Any(p => p.ProducerDetailId == 2 && p.MaterialId == 3 && p.PackagingType == PackagingTypes.Household && p.PackagingTonnage == 0 && p.PackagingTonnageRed == 10 && p.PackagingTonnageAmber == 0 && p.PackagingTonnageGreen == 0 && p.PackagingTonnageRedMedical == 0 && p.PackagingTonnageAmberMedical == 0 && p.PackagingTonnageGreenMedical == 0));
            Assert.IsTrue(h2Group.Any(p => p.ProducerDetailId == 2 && p.MaterialId == 3 && p.PackagingType == PackagingTypes.PublicBin && p.PackagingTonnage == 0 && p.PackagingTonnageRed == 20 && p.PackagingTonnageAmber == 0 && p.PackagingTonnageGreen == 0 && p.PackagingTonnageRedMedical == 0 && p.PackagingTonnageAmberMedical == 0 && p.PackagingTonnageGreenMedical == 0));
            Assert.IsTrue(h2Group.Any(p => p.ProducerDetailId == 2 && p.MaterialId == 3 && p.PackagingType == PackagingTypes.HouseholdDrinksContainers && p.PackagingTonnage == 0 && p.PackagingTonnageRed == 50 && p.PackagingTonnageAmber == 0 && p.PackagingTonnageGreen == 0 && p.PackagingTonnageRedMedical == 0 && p.PackagingTonnageAmberMedical == 0 && p.PackagingTonnageGreenMedical == 0));

            // Additional for H1 where there is missing RAM
            Assert.IsTrue(h1Group.Any(p => p.ProducerDetailId == 1 && p.MaterialId == 1 && p.PackagingType == PackagingTypes.Household && p.PackagingTonnage == 0 && p.PackagingTonnageRed == 10 && p.PackagingTonnageAmber == 10 && p.PackagingTonnageGreen == 10 && p.PackagingTonnageRedMedical == 10 && p.PackagingTonnageAmberMedical == 10 && p.PackagingTonnageGreenMedical == 0));
            Assert.IsTrue(h1Group.Any(p => p.ProducerDetailId == 1 && p.MaterialId == 2 && p.PackagingType == PackagingTypes.PublicBin && p.PackagingTonnage == 0 && p.PackagingTonnageRed == 50 && p.PackagingTonnageAmber == 0 && p.PackagingTonnageGreen == 0 && p.PackagingTonnageRedMedical == 0 && p.PackagingTonnageAmberMedical == 0 && p.PackagingTonnageGreenMedical == 0));
            Assert.IsTrue(h1Group.Any(p => p.ProducerDetailId == 2 && p.MaterialId == 3 && p.PackagingType == PackagingTypes.PublicBin && p.PackagingTonnage == 0 && p.PackagingTonnageRed == 0 && p.PackagingTonnageAmber == 0 && p.PackagingTonnageGreen == 0 && p.PackagingTonnageRedMedical == 10 && p.PackagingTonnageAmberMedical == 0 && p.PackagingTonnageGreenMedical == 10));
            Assert.IsTrue(h1Group.Any(p => p.ProducerDetailId == 2 && p.MaterialId == 3 && p.PackagingType == PackagingTypes.HouseholdDrinksContainers && p.PackagingTonnage == 0 && p.PackagingTonnageRed == 0 && p.PackagingTonnageAmber == 0 && p.PackagingTonnageGreen == 0 && p.PackagingTonnageRedMedical == 50 && p.PackagingTonnageAmberMedical == 0 && p.PackagingTonnageGreenMedical == 50));

        }

    }
}
