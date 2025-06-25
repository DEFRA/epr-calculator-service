using EPR.Calculator.Service.Function.Builder.CancelledProducers;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPR.Calculator.Service.Function.UnitTests.Builder.CancelledProducers
{
    [TestClass]
    public class CalcResultCancelledProducersBuilderTests
    {
        private CalcResultCancelledProducersBuilder _builder;

        public CalcResultCancelledProducersBuilderTests()
        {
            _builder = new CalcResultCancelledProducersBuilder();
        }

        [TestMethod]
        public async Task Construct_ShouldReturnResponseWithCorrectHeaders()
        {
            // Arrange
            var requestDto = new CalcResultsRequestDto();

            // Act
            var result = await _builder.Construct(requestDto);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(CommonConstants.CancelledProducers, result.TitleHeader);

            var headerRow = result.CancelledProducers.FirstOrDefault();
            Assert.IsNotNull(headerRow);

            // Check main headers
            Assert.AreEqual(CommonConstants.ProducerId, headerRow.ProducerId_Header);
            Assert.AreEqual(CommonConstants.SubsidiaryId, headerRow.SubsidiaryId_Header);
            Assert.AreEqual(CommonConstants.ProducerOrSubsidiaryName, headerRow.ProducerOrSubsidiaryName_Header);
            Assert.AreEqual(CommonConstants.TradingName, headerRow.TradingName_Header);

            // Check LastTonnage headers
            Assert.IsNotNull(headerRow.LastTonnage);
            Assert.AreEqual(CommonConstants.LastTonnage, headerRow.LastTonnage.LastTonnage_Header);
            Assert.AreEqual(CommonConstants.Aluminium, headerRow.LastTonnage.Aluminium_Header);
            Assert.AreEqual(CommonConstants.FibreComposite, headerRow.LastTonnage.FibreComposite_Header);
            Assert.AreEqual(CommonConstants.Glass, headerRow.LastTonnage.Glass_Header);
            Assert.AreEqual(CommonConstants.PaperOrCard, headerRow.LastTonnage.PaperOrCard_Header);
            Assert.AreEqual(CommonConstants.Plastic, headerRow.LastTonnage.Plastic_Header);
            Assert.AreEqual(CommonConstants.Steel, headerRow.LastTonnage.Steel_Header);
            Assert.AreEqual(CommonConstants.Wood, headerRow.LastTonnage.Wood_Header);
            Assert.AreEqual(CommonConstants.OtherMaterials, headerRow.LastTonnage.OtherMaterials_Header);

            // Check LatestInvoice headers
            Assert.IsNotNull(headerRow.LatestInvoice);
            Assert.AreEqual(CommonConstants.LatestInvoice, headerRow.LatestInvoice.LatestInvoice_Header);
            Assert.AreEqual(CommonConstants.LastInvoicedTotal, headerRow.LatestInvoice.LastInvoicedTotal_Header);
            Assert.AreEqual(CommonConstants.RunNumber, headerRow.LatestInvoice.RunNumber_Header);
            Assert.AreEqual(CommonConstants.RunName, headerRow.LatestInvoice.RunName_Header);
            Assert.AreEqual(CommonConstants.BillingInstructionId, headerRow.LatestInvoice.BillingInstructionId_Header);
        }
    }
}
