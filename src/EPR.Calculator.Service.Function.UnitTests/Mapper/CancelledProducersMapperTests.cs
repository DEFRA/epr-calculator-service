using AutoFixture;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Mapper;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Models.JsonExporter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;

namespace EPR.Calculator.Service.Function.UnitTests.Mapper
{
    [TestClass]
    public class CancelledProducersMapperTests
    {
        private CancelledProducersMapper _testClass = new CancelledProducersMapper();

        [TestMethod]
        public void CanCallMap()
        {
            // Arrange
            var fixture = new Fixture();
            var cancelledProducers = fixture.Create<CalcResultCancelledProducersResponse>();

            var counter = 1;
            foreach (var item in cancelledProducers.CancelledProducers)
            {
                item.ProducerId = counter;
                counter++;
            }

            // Act
            var result = ((ICancelledProducersMapper)_testClass).Map(cancelledProducers);

            // Assert
            Assert.IsNotNull(result);
        }
     
        [TestMethod]
        public void Map_ShouldMapCancelledProducersCorrectly()
        {
            // Arrange  
            var input = new CalcResultCancelledProducersResponse
            {
                CancelledProducers = new List<CalcResultCancelledProducersDto>
               {
                   new CalcResultCancelledProducersDto
                   {
                       ProducerId = 123,
                       ProducerOrSubsidiaryNameValue = "Producer A",
                       TradingNameValue = "Trading A",
                       LastTonnage = new LastTonnage
                       {
                           AluminiumValue = 10,
                           FibreCompositeValue = 20,
                           GlassValue = 30
                       }
                   }
               }
            };

            // Act  
            var result = _testClass.Map(input);

            // Assert  
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.CancelledProducerTonnageInvoice);
            Assert.AreEqual(CommonConstants.CancelledProducers, result.Name);
            Assert.AreEqual(1, result.CancelledProducerTonnageInvoice.Count());

            var invoice = result.CancelledProducerTonnageInvoice.First();
            Assert.AreEqual(123, invoice.ProducerId);
            Assert.AreEqual("Producer A", invoice.ProducerName);
            Assert.AreEqual("Trading A", invoice.TradingName);
            Assert.AreEqual(8, invoice.LastProducerTonnages.Count());
        }

        [TestMethod]
        public void Map_WhenCancelledProducersListEmpty_ReturnsEmptySectionWithEmptyArray()
        {
            // Arrange
            var input = new CalcResultCancelledProducersResponse
            {
                CancelledProducers = new List<CalcResultCancelledProducersDto>()
            };

            // Act
            var result = _testClass.Map(input);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(CommonConstants.CancelledProducers, result.Name);
            Assert.IsNotNull(result.CancelledProducerTonnageInvoice);
            Assert.AreEqual(0, result.CancelledProducerTonnageInvoice.Count());
        }



        [TestMethod]
        public void Map_WhenLatestInvoiceIsNull_UsesSafeDefaults()
        {
            // Arrange
            var input = new CalcResultCancelledProducersResponse
            {
                CancelledProducers = new List<CalcResultCancelledProducersDto>
                {
                    new CalcResultCancelledProducersDto
                    {
                        ProducerId = 1,
                        ProducerOrSubsidiaryNameValue = null,
                        TradingNameValue = null,
                        LastTonnage = new LastTonnage()
                    }
                }
            };

            // Act
            var result = _testClass.Map(input);

            // Assert
            var item = result.CancelledProducerTonnageInvoice!.Single();
            Assert.AreEqual(0, item.RunNumber);
            Assert.AreEqual(string.Empty, item.RunName);
            Assert.AreEqual(string.Empty, item.BillingInstructionID);
            Assert.AreEqual(0m, item.LastInvoicedTotal);
            Assert.AreEqual(string.Empty, item.ProducerName);
            Assert.AreEqual(string.Empty, item.TradingName);
        }


        [DataTestMethod]
        [DataRow("250014", 250014)]
        [DataRow(" ", 0)]
        [DataRow("abc", 0)]
        [DataRow(null, 0)]
        public void Map_RunNumber_TryParse_IsSafe(string runNumberValue, int expected)
        {
            // Arrange
            var input = new CalcResultCancelledProducersResponse
            {
                CancelledProducers = new List<CalcResultCancelledProducersDto>
                {
                    new CalcResultCancelledProducersDto
                    {
                        ProducerId = 42,
                        ProducerOrSubsidiaryNameValue = "P",
                        TradingNameValue = "T",
                        LastTonnage = new LastTonnage(),
                        LatestInvoice = new LatestInvoice
                        {
                            RunNumberValue = runNumberValue
                        }
                    }
                }
            };

            // Act
            var mapped = _testClass.Map(input);

            // Assert
            var item = mapped.CancelledProducerTonnageInvoice!.Single();
            Assert.AreEqual(expected, item.RunNumber);
        }

        [TestMethod]
        public void Map_MapsLatestInvoiceFields_Correctly_WhenProvided()
        {
            // Arrange
            var input = new CalcResultCancelledProducersResponse
            {
                CancelledProducers = new List<CalcResultCancelledProducersDto>
                {
                    new CalcResultCancelledProducersDto
                    {
                        ProducerId = 101001,
                        ProducerOrSubsidiaryNameValue = "Allied Packaging LTD",
                        TradingNameValue = "Allied Packaging",
                        LastTonnage = new LastTonnage(),
                        LatestInvoice = new LatestInvoice
                        {
                            RunNumberValue = "250014",
                            RunNameValue = "Interim Run 1",
                            BillingInstructionIdValue = "250014_101006",
                            CurrentYearInvoicedTotalToDateValue = 57231m
                        }
                    }
                }
            };

            // Act
            var result = _testClass.Map(input);

            // Assert
            var item = result.CancelledProducerTonnageInvoice!.Single();
            Assert.AreEqual(250014, item.RunNumber);
            Assert.AreEqual("Interim Run 1", item.RunName);
            Assert.AreEqual("250014_101006", item.BillingInstructionID);
            Assert.AreEqual(57231m, item.LastInvoicedTotal);
        }

        [TestMethod]
        public void Map_SubsidiaryId_DefaultsToEmptyString_WhenSourceNull()
        {
            // Arrange
            var producer = new CalcResultCancelledProducersDto
            {
                ProducerId = 101003,
                SubsidiaryIdValue = null,
                ProducerOrSubsidiaryNameValue = "P",
                TradingNameValue = "T",
                LastTonnage = new LastTonnage()
            };
            var input = new CalcResultCancelledProducersResponse
            {
                CancelledProducers = new List<CalcResultCancelledProducersDto> { producer }
            };

            // Act
            var result = _testClass.Map(input);

            // Assert
            var item = result.CancelledProducerTonnageInvoice!.Single();
            Assert.AreEqual(string.Empty, item.SubsidiaryId);
        }

        [TestMethod]
        public void Json_Serializes_runNumber_runName_billingInstructionID_And_LastInvoicedTotal_AsGbpCurrency()
        {
            // Arrange
            var model = new CancelledProducerTonnageInvoice
            {
                ProducerId = 101001,
                SubsidiaryId = "",
                ProducerName = "Allied Packaging LTD",
                TradingName = "Allied Packaging",
                LastProducerTonnages = Array.Empty<LastProducerTonnages>(),
                RunNumber = 999,
                RunName = "Interim Run 1",
                BillingInstructionID = "250014_101006",
                LastInvoicedTotal = 57231m //Assert currency converter applied and value returned as £57,231.00 as per ICD
            };

            var options = new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

            // Act
            var json = JsonSerializer.Serialize(model, options);

            // Assert
            StringAssert.Contains(json, "\"runNumber\":999");
            StringAssert.Contains(json, "\"runName\":\"Interim Run 1\"");
            StringAssert.Contains(json, "\"billingInstructionID\":\"250014_101006\"");

            StringAssert.Contains(json, "\"lastInvoicedTotal\":\"£57,231.00\"");
        }
    }
}
