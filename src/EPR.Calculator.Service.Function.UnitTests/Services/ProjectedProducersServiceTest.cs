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

            await service.StoreProjectedProducers(1, h2ProjectedProducers);

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
}
