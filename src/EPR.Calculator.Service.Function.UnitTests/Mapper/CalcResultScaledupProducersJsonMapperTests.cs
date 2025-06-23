using AutoFixture;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Mapper;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Models.JsonExporter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPR.Calculator.Service.Function.UnitTests.Mapper
{
    [TestClass]
    public class CalcResultScaledupProducersJsonMapperTests
    {
        private CalcResultScaledupProducersJsonMapper _testClass;

        [TestInitialize]
        public void Setup()
        {
            _testClass = new CalcResultScaledupProducersJsonMapper();
        }
        
        [TestMethod]
        public void CanCallMap()
        {
            // Arrange
            var fixture = new Fixture();
            var calcResultScaledupProducers = fixture.Create<CalcResultScaledupProducers>();
            var acceptedProducerIds = fixture.Create<IEnumerable<int>>();

            // Act
            var result = ((ICalcResultScaledupProducersJsonMapper)_testClass).Map(calcResultScaledupProducers, acceptedProducerIds);

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void CanCallMap_ConsidersOnlyAcceptedProducers()
        {
            // Arrange
            var scaledupProducers = GetScaledUpProducers();
            var acceptedProducerIds = new List<int>();
            acceptedProducerIds.Add(1);
            acceptedProducerIds.Add(3);

            // Act
            var result = _testClass.Map(scaledupProducers, acceptedProducerIds);

            // Assert
            Assert.AreEqual(2, result.ProducerSubmissions?.Count());
            Assert.AreEqual(1, result.ProducerSubmissions?.ToList()[0].ProducerId);
            Assert.AreEqual(3, result.ProducerSubmissions?.ToList()[1].ProducerId);
        }

        [TestMethod]
        public void CanCallMap_ReturnsEmptyObject()
        {
            // Arrange
            CalcResultScaledupProducers? scaledupProducers = null;
            var acceptedProducerIds = new List<int>();

            // Act
            var result = _testClass.Map(scaledupProducers!, acceptedProducerIds);

            // Assert
            Assert.AreEqual(new CalcResultScaledupProducersJson(), result);
        }

        private static CalcResultScaledupProducers GetScaledUpProducers()
        {
            return new CalcResultScaledupProducers()
            {
                ScaledupProducers = new List<CalcResultScaledupProducer>()
                {
                     new CalcResultScaledupProducer()
                     {
                        ProducerId = 1,
                        IsTotalRow = true,
                        Level = CommonConstants.LevelTwo.ToString(),
                        ScaledupProducerTonnageByMaterial = new()
                        {
                            ["Aluminium"] = new CalcResultScaledupProducerTonnage
                            {
                                ReportedHouseholdPackagingWasteTonnage = 1000,
                                ReportedPublicBinTonnage = 2000,
                                TotalReportedTonnage = 3000,
                                ReportedSelfManagedConsumerWasteTonnage = 1000,
                                NetReportedTonnage = 5000,
                                ScaledupReportedHouseholdPackagingWasteTonnage = 300,
                                ScaledupReportedPublicBinTonnage = 400,
                                ScaledupTotalReportedTonnage = 500,
                                ScaledupReportedSelfManagedConsumerWasteTonnage = 100,
                                ScaledupNetReportedTonnage = 100,
                            },
                        },
                     },
                     new CalcResultScaledupProducer()
                     {
                        ProducerId = 2,
                        IsTotalRow = true,
                        Level = CommonConstants.LevelTwo.ToString(),
                        ScaledupProducerTonnageByMaterial = new()
                        {
                            ["Glass"] = new CalcResultScaledupProducerTonnage
                            {
                                ReportedHouseholdPackagingWasteTonnage = 1000,
                                ReportedPublicBinTonnage = 2000,
                                TotalReportedTonnage = 3000,
                                ReportedSelfManagedConsumerWasteTonnage = 1000,
                                NetReportedTonnage = 5000,
                                ScaledupReportedHouseholdPackagingWasteTonnage = 300,
                                ScaledupReportedPublicBinTonnage = 400,
                                ScaledupTotalReportedTonnage = 500,
                                ScaledupReportedSelfManagedConsumerWasteTonnage = 100,
                                ScaledupNetReportedTonnage = 100,
                            },
                        },
                     },
                     new CalcResultScaledupProducer()
                     {
                        ProducerId = 3,
                        IsTotalRow = true,
                        Level = CommonConstants.LevelTwo.ToString(),
                        ScaledupProducerTonnageByMaterial = new()
                        {
                            ["Plastic"] = new CalcResultScaledupProducerTonnage
                            {
                                 ReportedHouseholdPackagingWasteTonnage = 1000,
                                 ReportedPublicBinTonnage = 2000,
                                 TotalReportedTonnage = 3000,
                                 ReportedSelfManagedConsumerWasteTonnage = 1000,
                                 NetReportedTonnage = 5000,
                                 ScaledupReportedHouseholdPackagingWasteTonnage = 300,
                                 ScaledupReportedPublicBinTonnage = 400,
                                 ScaledupTotalReportedTonnage = 500,
                                 ScaledupReportedSelfManagedConsumerWasteTonnage = 100,
                                 ScaledupNetReportedTonnage = 100,
                            },
                        },
                     },
                },
            };
        }
    }
}
