using EPR.Calculator.API.Data.Models;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Services;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.TestData;

namespace EPR.Calculator.Service.Function.UnitTests.Services;

[TestClass]
public class ProducerInvoiceNetTonnageServiceTests : TestsFor<ProducerInvoiceNetTonnageService>
{
    [TestMethod]
    public async Task CanCallCreateProducerInvoiceNetTonnage1()
    {
        // Arrange
        var calcResult = TestDataHelper.GetCalcResult();

        // Act
        var result = await testSubject.CreateProducerInvoiceNetTonnage(calcResult);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task CanCallCreateProducerInvoiceTonnageWithNoProducers()
    {
        // Arrange
        var calcResult = new CalcResult
        {
            CalcResultScaledupProducers = new CalcResultScaledupProducers(){
                ScaledupProducers = ImmutableList<CalcResultScaledupProducer>.Empty
            },
            CalcResultPartialObligations = new CalcResultPartialObligations(){
                PartialObligations = ImmutableList<CalcResultPartialObligation>.Empty
            },
            CalcResultDetail = new CalcResultDetail
            {
                RunId = 4,
                RunDate = DateTime.UtcNow,
                RunName = "RunName",
                RelativeYear = new RelativeYear(2024)
            },
            CalcResultLapcapData = new CalcResultLapcapData
            {
                ByMaterial = []
            },
            CalcResultParameterOtherCost = new CalcResultParameterOtherCost
            {
                SchemeSetupCost = new ByCountryCost
                {
                    England = 0,
                    Wales = 0,
                    Scotland = 0,
                    NorthernIreland = 0
                }
            },
            CalcResultLateReportingTonnageData = new() { ByMaterial = [] },
            CalcResultProjectedProducers = new CalcResultProjectedProducers(){
                H1ProjectedProducers = ImmutableList<CalcResultH1ProjectedProducer>.Empty,
                H2ProjectedProducers = ImmutableList<CalcResultH2ProjectedProducer>.Empty,
            },
        };

        // Act
        var result = await testSubject.CreateProducerInvoiceNetTonnage(calcResult);
        Assert.IsFalse(result);
    }
}
