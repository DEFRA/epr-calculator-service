using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.Models;
using EPR.Calculator.Service.Common;
using EPR.Calculator.Service.Function.Builder.ProjectedProducers;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Interface;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Services;
using EPR.Calculator.Service.Function.UnitTests.Builder;
using Microsoft.EntityFrameworkCore;
using Moq;
using Newtonsoft.Json;
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
            List<ProducerReportedMaterialProjected> savedProducers = new List<ProducerReportedMaterialProjected>();

            producerReportedMaterialProjectedChunker
                .Setup(c => c.InsertRecords(It.IsAny<IEnumerable<ProducerReportedMaterialProjected>>()))
                .Returns((IEnumerable<ProducerReportedMaterialProjected> arg) =>
                {
                    savedProducers = arg.ToList();
                    return Task.FromResult(arg);
                });

            var service = new ProjectedProducersService(producerReportedMaterialProjectedChunker.Object);

            ProducerReportedMaterial mkProducerReportedMaterial(string submissionPeriod, string material, string packagingType, decimal total, decimal? r, decimal? a)
            {
                return new ProducerReportedMaterial
                {
                    //MaterialId = int.Parse(material), // TODO look up materialId
                    //ProducerDetailId = { get; set; } // TODO how to get this?
                    PackagingType = packagingType,
                    PackagingTonnage = total,
                    PackagingTonnageRed = r,
                    PackagingTonnageAmber = a,
                    PackagingTonnageGreen = 0m,
                    PackagingTonnageRedMedical = null,
                    PackagingTonnageAmberMedical = null,
                    PackagingTonnageGreenMedical = null,
                    SubmissionPeriod = submissionPeriod,
                    ProducerDetail = null,
                    Material = null
                };
            }

            var producer1 = new ProducerDetail{
                ProducerId = 1,
                SubsidiaryId = null
            };
            producer1.ProducerReportedMaterials.Add(mkProducerReportedMaterial(submissionPeriod: "2025-H1", material: "ST", packagingType: "PB", total:   7, r:   2, a: 5 ));
            producer1.ProducerReportedMaterials.Add(mkProducerReportedMaterial(submissionPeriod: "2025-H2", material: "PL", packagingType: "HH", total:  12, r:   0, a: 11));
            producer1.ProducerReportedMaterials.Add(mkProducerReportedMaterial(submissionPeriod: "2025-H1", material: "ST", packagingType: "HH", total: 201, r: 201, a: 0 ));
            var producer2 = new ProducerDetail{
                ProducerId = 1,
                SubsidiaryId = "A"
            };
            producer2.ProducerReportedMaterials.Add(mkProducerReportedMaterial(submissionPeriod: "2025-H2", material: "ST", packagingType: "PB", total:   5, r:   1, a: 4 ));
            producer2.ProducerReportedMaterials.Add(mkProducerReportedMaterial(submissionPeriod: "2025-H2", material: "PL", packagingType: "HH", total:  10, r:   0, a: 10));
            producer2.ProducerReportedMaterials.Add(mkProducerReportedMaterial(submissionPeriod: "2025-H2", material: "ST", packagingType: "HH", total: 200, r: 200, a: 0 ));
            var producers = new List<ProducerDetail>
            {
                producer1, producer2
            };

            var runId = 1;
            await service.StoreProjectedProducers(runId, producers);

            // TODO this is checking what went through the Chunker mock - not what was stored in db
            Assert.AreEqual(6, savedProducers.Count());

            // TODO assert entries
        }
    }
}
