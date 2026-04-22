
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.EntityFrameworkCore;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Services;
using EPR.Calculator.Service.Common.Logging;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Misc;
using EPR.Calculator.API.Data.Models;

namespace EPR.Calculator.Service.Function.UnitTests.Services
{
    [TestClass]
    public class SelfManagedConsumerWasteServiceTests
    {
        private ApplicationDBContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDBContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new ApplicationDBContext(options);
        }

        private void SeedBasicData(ApplicationDBContext context)
        {
            var material = new Material
            {
                Id = 1,
                Code = MaterialCodes.Aluminium,
                Name = MaterialNames.Aluminium
            };

            var producer = new ProducerDetail
            {
                Id = 1,
                ProducerId = 1,
                SubsidiaryId = null,
                CalculatorRunId = 1,
            };

            var hh = new ProducerReportedMaterial
            {
                ProducerDetail = producer,
                ProducerDetailId = producer.Id,
                Material = material,
                MaterialId = material.Id,
                PackagingType = PackagingTypes.Household,
                PackagingTonnage = 100,
                SubmissionPeriod = "2025-H1"
            };

            var cw = new ProducerReportedMaterial
            {
                ProducerDetail = producer,
                ProducerDetailId = producer.Id,
                Material = material,
                MaterialId = material.Id,
                PackagingType = PackagingTypes.ConsumerWaste,
                PackagingTonnage = 40,
                SubmissionPeriod = "2025-H1"
            };

            producer.ProducerReportedMaterials.Add(hh);
            producer.ProducerReportedMaterials.Add(cw);

            context.Material.Add(material);
            context.ProducerDetail.Add(producer);
            context.ProducerReportedMaterial.AddRange(hh, cw);

            context.SaveChanges();

            var loadedProducer = context.ProducerDetail.First();
            loadedProducer.ProducerReportedMaterials.Clear();
            loadedProducer.ProducerReportedMaterials.Add(hh);
            loadedProducer.ProducerReportedMaterials.Add(cw);
        }

        [TestMethod]
        public void Sum_Should_Handle_Nulls()
        {
            var items = new List<SelfManagedConsumerWasteData?>
            {
                null,
                new()
                {
                    SelfManagedConsumerWasteTonnage = 10,
                    ActionedSelfManagedConsumerWasteTonnage = 5,
                    NetReportedTonnage = (1,1,1,1)
                },
                null,
                new()
                {
                    SelfManagedConsumerWasteTonnage = 20,
                    ActionedSelfManagedConsumerWasteTonnage = null,
                    NetReportedTonnage = (2,2,2,2)
                }
            };

            var result = items.Sum();

            Assert.AreEqual(30, result.SelfManagedConsumerWasteTonnage);
            Assert.AreEqual(5, result.ActionedSelfManagedConsumerWasteTonnage);
            Assert.AreEqual(3, result.NetReportedTonnage.total);
            Assert.AreEqual(3, result.NetReportedTonnage.red);
            Assert.AreEqual(3, result.NetReportedTonnage.amber);
            Assert.AreEqual(3, result.NetReportedTonnage.green);
        }

        [TestMethod]
        public void Sum_Should_Return_Zero_For_Empty()
        {
            var result = new List<SelfManagedConsumerWasteData?>().Sum();

            Assert.AreEqual(0, result.SelfManagedConsumerWasteTonnage);
            Assert.AreEqual(0, result.ActionedSelfManagedConsumerWasteTonnage);
            Assert.AreEqual(0, result.NetReportedTonnage.total);
        }

        [TestMethod]
        public async Task Calculate_Should_Aggregate_OverallTotals_Correctly()
        {
            var context = CreateContext();
            SeedBasicData(context);

            var service = new SelfManagedConsumerWasteService(context);

            var materials = new[]
            {
                new MaterialDetail { Code = "AL", Name = "Aluminium", Description = "" }
            };

            var result = await service.Calculate(
                new CalcResultsRequestDto { RunId = 1, RelativeYear = new RelativeYear(2025) },
                materials,
                scaledUpProducers: [],
                partialObligations: [],
                showModulations: false);

            var total = result.OverallTotalPerMaterials["AL"];

            foreach (var p in result.ProducerTotals)
            {
                Console.WriteLine($"Producer {p.producerDetail.Id}, Level {p.Level}");

                foreach (var kv in p.SelfManagedConsumerWasteDataPerMaterials)
                {
                    Console.WriteLine($"Material {kv.Key}: SMCW={kv.Value.SelfManagedConsumerWasteTonnage} Actioned SMCW={kv.Value.ActionedSelfManagedConsumerWasteTonnage} NetReportedTonnage={kv.Value.NetReportedTonnage.total}");
                }
            }

            // HH = 100, CW = 40 → Net = 60
            Assert.AreEqual(40, total.SelfManagedConsumerWasteTonnage);
            Assert.AreEqual(40, total.ActionedSelfManagedConsumerWasteTonnage);
            Assert.AreEqual(60, total.NetReportedTonnage.total);
        }

        [TestMethod]
        public async Task Calculate_Should_Return_Zero_When_Material_Missing()
        {
            var context = CreateContext();
            SeedBasicData(context);

            var service = new SelfManagedConsumerWasteService(context);

            var result = await service.Calculate(
                new CalcResultsRequestDto { RunId = 1, RelativeYear = new RelativeYear(2025) },
                new[] { new MaterialDetail { Code = "NOT_EXIST", Name = "", Description = "" } },
                [],
                [],
                false);

            var total = result.OverallTotalPerMaterials["NOT_EXIST"];

            Assert.AreEqual(0, total.SelfManagedConsumerWasteTonnage);
            Assert.AreEqual(0, total.ActionedSelfManagedConsumerWasteTonnage);
            Assert.AreEqual(0, total.NetReportedTonnage.total);
        }

        [TestMethod]
        public async Task Calculate_Should_Only_Include_Level1_In_OverallTotals()
        {
            var context = CreateContext();

            var material = new Material { Id = 1, Code = "AL", Name = "Aluminium", Description = "" };

            var producer1 = new ProducerDetail
            {
                Id = 1,
                ProducerId = 1,
                CalculatorRunId = 1,
                SubsidiaryId = null
            };

            var producer2 = new ProducerDetail
            {
                Id = 2,
                ProducerId = 1,
                CalculatorRunId = 1,
                SubsidiaryId = "99" // forces Level 2
            };

            context.Material.Add(material);
            context.ProducerDetail.AddRange(producer1, producer2);

            context.SaveChanges();

            var service = new SelfManagedConsumerWasteService(context);

            var result = await service.Calculate(
                new CalcResultsRequestDto { RunId = 1, RelativeYear = new RelativeYear(2025) },
                new[] { new MaterialDetail { Code = "AL", Name = "Aluminium", Description = "" } },
                [],
                [],
                false);

            var total = result.OverallTotalPerMaterials["AL"];

            // Level 2 should not contribute
            Assert.AreEqual(0, total.SelfManagedConsumerWasteTonnage);
            Assert.AreEqual(0, total.NetReportedTonnage.total);
        }

        [TestMethod]
        public void GetNetReportedTonnage_LevelTwo_With_Modulations_ReturnsNulls()
        {
            var producer = new ProducerDetail
            {
                Id = 1,
                ProducerId = 1,
                SubsidiaryId = null,
                CalculatorRunId = 1,
            };

            var material = new MaterialDetail { Code = "AL", Name = "Aluminium", Description = "" };

            var result = SelfManagedConsumerWasteService.GetNetReportedTonnage(
                new[] { producer },
                material,
                [],
                [],
                showModulations: true,
                level: CommonConstants.LevelTwo);

            Assert.AreEqual(null, result.total);
            Assert.AreEqual(null, result.red);
            Assert.AreEqual(null, result.amber);
            Assert.AreEqual(null, result.green);
        }

        public static IEnumerable<object[]> NetReportedTonnageCases => new List<object[]>
        {
            //             hh        , red     , redM, amber     , amberM, green   , greenM, cw  ,                           expected tuple   (total     , red.    , amber     , green)       // ECV-430
            new object[] { 942.362m  , 464.266m, 0m  , 278.096m  , 0m    , 200m    , 0m    , 100m, ((decimal?, decimal?, decimal?, decimal?)) (842.362m  , 464.266m, 178.096m  , 200m    ) }, // AC1
            new object[] { 27522.359m, 11000m  , 0m  , 15899.754m, 0m    , 622.610m, 0m    , 500m, ((decimal?, decimal?, decimal?, decimal?)) (27022.359m, 11000m  , 15399.754m, 622.610m) }, // AC2
            new object[] { 3287.503m , 2190.39m, 0m  , 300m      , 0m    , 797.113m, 0m    , 500m, ((decimal?, decimal?, decimal?, decimal?)) (2787.503m , 1990.39m, 0m        , 797.113m) }, // AC3
            new object[] { 220m      , 25m     , 0m  , 50m       , 0m    , 145m    , 0m    , 100m, ((decimal?, decimal?, decimal?, decimal?)) (120m      , 0m      , 0m        , 120m    ) }, // AC4
            new object[] { 0m        , 0m      , 0m  , 0m        , 0m    , 0m      , 0m    , 100m, ((decimal?, decimal?, decimal?, decimal?)) (0m        , 0m      , 0m        , 0m      ) }, // AC5
            new object[] { 300m      , 100m    , 0m  , 100m      , 0m    , 100m    , 0m    , 50m , ((decimal?, decimal?, decimal?, decimal?)) (250m      , 100m    , 50m       , 100m    ) },
            new object[] { 300m      , 100m    , 0m  , 100m      , 0m    , 100m    , 0m    , 100m, ((decimal?, decimal?, decimal?, decimal?)) (200m      , 100m    , 0m        , 100m    ) },
            new object[] { 300m      , 0m      , 100m, 0m        , 100m  , 0m      , 100m  , 150m, ((decimal?, decimal?, decimal?, decimal?)) (150m      , 50m     , 0m        , 100m    ) }, // RAG Medical
            new object[] { 300m      , 50m     , 50m , 50m       , 50m   , 50m     , 50m   , 150m, ((decimal?, decimal?, decimal?, decimal?)) (150m      , 50m     , 0m        , 100m    ) }, // RAG + RAG Medical
            new object[] { 300m      , 100m    , 0m  , 100m      , 0m    , 100m    , 0m    , 200m, ((decimal?, decimal?, decimal?, decimal?)) (100m      , 0m      , 0m        , 100m    ) },
            new object[] { 300m      , 100m    , 0m  , 100m      , 0m    , 100m    , 0m    , 250m, ((decimal?, decimal?, decimal?, decimal?)) (50m       , 0m      , 0m        , 50m     ) },
            new object[] { 300m      , 100m    , 0m  , 100m      , 0m    , 100m    , 0m    , 300m, ((decimal?, decimal?, decimal?, decimal?)) (0m        , 0m      , 0m        , 0m      ) },
            new object[] { 300m      , 100m    , 0m  , 100m      , 0m    , 100m    , 0m    , 350m, ((decimal?, decimal?, decimal?, decimal?)) (0m        , 0m      , 0m        , 0m      ) },
        };

        [DataTestMethod]
        [DynamicData(nameof(NetReportedTonnageCases), DynamicDataSourceType.Property)]
        public void CanGetNetReportedTonnage_WithModulations(
            decimal hhTotal,
            decimal red,
            decimal redMedical,
            decimal amber,
            decimal amberMedical,
            decimal green,
            decimal greenMedical,
            decimal cw,
            (decimal? total, decimal? red, decimal? amber, decimal? green) expected)
        {
            var producer = new ProducerDetail
            {
                Id = 1,
                ProducerId = 1,
                SubsidiaryId = null,
                CalculatorRunId = 1,
            };

            var materialDetail = new MaterialDetail { Code = "AL", Name = "Aluminium", Description = "" };

            producer.ProducerReportedMaterials.Add(new ProducerReportedMaterial
            {
                SubmissionPeriod = "2025-H1",
                Material = new Material { Code = "AL", Name = "Aluminium", Description = "" },
                PackagingTonnage = hhTotal,
                PackagingType = PackagingTypes.Household,
                PackagingTonnageRed = red,
                PackagingTonnageRedMedical = redMedical,
                PackagingTonnageAmber = amber,
                PackagingTonnageAmberMedical = amberMedical,
                PackagingTonnageGreen = green,
                PackagingTonnageGreenMedical = greenMedical
            });

            producer.ProducerReportedMaterials.Add(new ProducerReportedMaterial
            {
                SubmissionPeriod = "2025-H1",
                Material = new Material { Code = "AL", Name = "Aluminium", Description = "" },
                PackagingTonnage = cw,
                PackagingType = PackagingTypes.ConsumerWaste
            });

            var result = SelfManagedConsumerWasteService.GetNetReportedTonnage(
                new[] { producer },
                materialDetail,
                [],
                [],
                showModulations: true,
                level: CommonConstants.LevelOne);

            Assert.AreEqual(expected.total, result.total, "Total mismatch");
            Assert.AreEqual(expected.red, result.red, "Red mismatch");
            Assert.AreEqual(expected.amber, result.amber, "Amber mismatch");
            Assert.AreEqual(expected.green, result.green, "Green mismatch");
        }
    }
}