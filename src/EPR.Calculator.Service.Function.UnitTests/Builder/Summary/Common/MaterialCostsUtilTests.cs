using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Builder.Summary.Common;
using EPR.Calculator.Service.Function.Enums;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.UnitTests.Builder.Summary.Common
{
    [TestClass]
    public class MaterialCostsUtilTests
    {
        private IEnumerable<ProducerDetail> producersAndSubsidiaries;
        private IEnumerable<CalcResultScaledupProducer> scaledUpProducers;
        private IEnumerable<CalcResultSummaryProducerDisposalFees> producerDisposalFees;
        private MaterialDetail material;
        private CalcResult calcResult;

        public MaterialCostsUtilTests()
        {
            producersAndSubsidiaries = TestDataHelper.GetProducers();
            scaledUpProducers = TestDataHelper.GetScaledupProducers().ScaledupProducers!;
            producerDisposalFees = TestDataHelper.GetProducerDisposalFees();
            material = TestDataHelper.GetMaterials()[0];
            calcResult = TestDataHelper.GetCalcResult();
        }

        [TestMethod]
        public void GetNetReportedTonnage_OverallTotal_CallsOverallTotal()
        {
            // Arrange
            bool isOverAllTotalRow = true;
            decimal expected = 910;

            // Act
            var result = MaterialCostsUtil.GetNetReportedTonnage(
                producerDisposalFees, producersAndSubsidiaries, scaledUpProducers, material, isOverAllTotalRow);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void GetNetReportedTonnage_ProducerTotal_CallsProducerTotal()
        {
            bool isOverAllTotalRow = false;
            decimal expected = 2940.00m;

            var result = MaterialCostsUtil.GetNetReportedTonnage(
                producerDisposalFees, producersAndSubsidiaries, scaledUpProducers, material, isOverAllTotalRow);

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void GetProducerDisposalFee_OverallTotal_CallsOverallTotal()
        {
            // Arrange
            bool isOverAllTotalRow = true;
            decimal expected = 607.52m;

            // Act
            var result = MaterialCostsUtil.GetProducerDisposalFee(
                producerDisposalFees, producersAndSubsidiaries, scaledUpProducers, material, calcResult, isOverAllTotalRow);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void GetProducerDisposalFee_ProducerTotal_CallsProducerTotal()
        {
            // Arrange
            bool isOverAllTotalRow = false;
            decimal expected = 1962.744000m;

            // Act
            var result = MaterialCostsUtil.GetProducerDisposalFee(
                producerDisposalFees, producersAndSubsidiaries, scaledUpProducers, material, calcResult, isOverAllTotalRow);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void GetBadDebtProvision_OverallTotal_CallsOverallTotal()
        {
            // Arrange
            bool isOverAllTotalRow = true;
            decimal expected = 36.45m;

            // Act
            var result = MaterialCostsUtil.GetBadDebtProvision(
                producerDisposalFees, producersAndSubsidiaries, scaledUpProducers, material, calcResult, isOverAllTotalRow);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void GetBadDebtProvision_ProducerTotal_CallsProducerTotal()
        {
            // Arrange
            bool isOverAllTotalRow = false;
            decimal expected = 117.76464000m;

            // Act
            var result = MaterialCostsUtil.GetBadDebtProvision(
                producerDisposalFees, producersAndSubsidiaries, scaledUpProducers, material, calcResult, isOverAllTotalRow);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void GetProducerDisposalFeeWithBadDebtProvision_OverallTotal_CallsOverallTotal()
        {
            // Arrange
            bool isOverAllTotalRow = true;
            decimal expected = 643.97m;

            // Act
            var result = MaterialCostsUtil.GetProducerDisposalFeeWithBadDebtProvision(
                producerDisposalFees, producersAndSubsidiaries, scaledUpProducers, material, calcResult, isOverAllTotalRow);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void GetProducerDisposalFeeWithBadDebtProvision_ProducerTotal_CallsProducerTotal()
        {
            // Arrange
            bool isOverAllTotalRow = false;
            decimal expected = 2080.50864000m;

            // Act
            var result = MaterialCostsUtil.GetProducerDisposalFeeWithBadDebtProvision(
                producerDisposalFees, producersAndSubsidiaries, scaledUpProducers, material, calcResult, isOverAllTotalRow);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void GetCountryDisposalFeeWithBadDebtProvision_OverallTotal_CallsOverallTotal()
        {
            // Arrange
            var country = Countries.England;
            bool isOverAllTotalRow = true;
            decimal expected = 348.06m;

            // Act
            var result = MaterialCostsUtil.GetCountryDisposalFeeWithBadDebtProvision(
                producerDisposalFees, producersAndSubsidiaries, scaledUpProducers, material, calcResult, country, isOverAllTotalRow);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void GetCountryDisposalFeeWithBadDebtProvision_ProducerTotal_CallsProducerTotal()
        {
            // Arrange
            var country = Countries.Scotland;
            bool isOverAllTotalRow = false;
            decimal expected = 504.8933101925519520m;

            // Act
            var result = MaterialCostsUtil.GetCountryDisposalFeeWithBadDebtProvision(
                producerDisposalFees, producersAndSubsidiaries, scaledUpProducers, material, calcResult, country, isOverAllTotalRow);

            // Assert
            Assert.AreEqual(expected, result);
        }
    }
}
