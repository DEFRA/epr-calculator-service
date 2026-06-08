using EPR.Calculator.Service.Function.Builder.Summary;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.TestData;

namespace EPR.Calculator.Service.Function.UnitTests.Builder.Summary;

[TestCategory(TestCategories.ResultBuilder)]
[TestClass]
public class TotalBillBreakdownProducerTests
{
    private readonly CalcResult calcResult = TestDataHelper.GetCalcResult();

    /// <summary>
    ///     The CanCallSetValues
    /// </summary>
    [TestMethod]
    public void TotalBillBreakdownProducer_CanCallSetValues()
    {
        // Act
        TotalBillBreakdownProducer.SetValues(calcResult.CalcResultSummary);

        // Assert
        Assert.AreEqual(17673.2373499970378m , calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].TotalProducerBillBreakdownCosts!.FeeWithoutBadDebtProvision);
        Assert.AreEqual(1060.39424099982226m , calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].TotalProducerBillBreakdownCosts!.BadDebtProvision);
        Assert.AreEqual(18733.63159099686001m, calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].TotalProducerBillBreakdownCosts!.FeeWithBadDebtProvision.Total);
        Assert.AreEqual(9610.6053147004709m  , calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].TotalProducerBillBreakdownCosts!.FeeWithBadDebtProvision.England);
        Assert.AreEqual(2653.2546023494487m  , calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].TotalProducerBillBreakdownCosts!.FeeWithBadDebtProvision.Wales);
        Assert.AreEqual(4576.19121409722784m , calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].TotalProducerBillBreakdownCosts!.FeeWithBadDebtProvision.Scotland);
        Assert.AreEqual(1893.58045984971257m , calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].TotalProducerBillBreakdownCosts!.FeeWithBadDebtProvision.NorthernIreland);
    }

    /// <summary>
    ///     The CanCallSetValues
    /// </summary>
    [TestMethod]
    public void TotalBillBreakdownProducer_CanCallSetValues_NullValues()
    {
        // Arrange
        var data = calcResult.CalcResultSummary;
        data.ProducerDisposalFees.ToList()[0].LocalAuthorityDisposalCostsSectionOne = null;
        data.ProducerDisposalFees.ToList()[0].CommsCostsSectionTwoA = CalcResultSummaryBadDebtProvision.Empty;
        data.ProducerDisposalFees.ToList()[0].CommsCostsSectionTwoB = null;
        data.ProducerDisposalFees.ToList()[0].CommsCostsSectionTwoC = CalcResultSummaryBadDebtProvision.Empty;
        data.ProducerDisposalFees.ToList()[0].SchemeAdministratorOperatingCosts = null;
        data.ProducerDisposalFees.ToList()[0].LocalAuthorityDataPreparationCosts = null;
        data.ProducerDisposalFees.ToList()[0].OneOffSchemeAdministrationSetupCosts = null;

        // Act
        TotalBillBreakdownProducer.SetValues(data);

        // Assert
        Assert.AreEqual(0, data.ProducerDisposalFees.ToList()[0].TotalProducerBillBreakdownCosts!.FeeWithoutBadDebtProvision);
        Assert.AreEqual(0, data.ProducerDisposalFees.ToList()[0].TotalProducerBillBreakdownCosts!.BadDebtProvision);
        Assert.AreEqual(0, data.ProducerDisposalFees.ToList()[0].TotalProducerBillBreakdownCosts!.FeeWithBadDebtProvision.Total);
        Assert.AreEqual(0, data.ProducerDisposalFees.ToList()[0].TotalProducerBillBreakdownCosts!.FeeWithBadDebtProvision.England);
        Assert.AreEqual(0, data.ProducerDisposalFees.ToList()[0].TotalProducerBillBreakdownCosts!.FeeWithBadDebtProvision.Wales);
        Assert.AreEqual(0, data.ProducerDisposalFees.ToList()[0].TotalProducerBillBreakdownCosts!.FeeWithBadDebtProvision.Scotland);
        Assert.AreEqual(0, data.ProducerDisposalFees.ToList()[0].TotalProducerBillBreakdownCosts!.FeeWithBadDebtProvision.NorthernIreland);
    }
}
