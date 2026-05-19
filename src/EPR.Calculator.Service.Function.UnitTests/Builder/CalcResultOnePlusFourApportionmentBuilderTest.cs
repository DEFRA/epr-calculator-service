using EPR.Calculator.API.Data.Models;
using EPR.Calculator.Service.Function.Builder.OnePlusFourApportionment;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Misc;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.API.Data.DataModels;

namespace EPR.Calculator.Service.Function.UnitTests.Builder
{
    [TestClass]
    public class CalcResultOnePlusFourApportionmentBuilderTest : CalcResultOnePlusFourApportionmentBuilder
    {
        private Fixture Fixture { get; init; } = new Fixture();

        [TestMethod]
        public void Construct_ShouldReturnCorrectApportionment()
        {
            // Arrange
            var resultsDto = new CalcResultsRequestDto { RunId = 6, RelativeYear = new RelativeYear(2025) };
            var calcResult = new CalcResult
            {
                ApplyModulation = false,
                CalcResultDetail = new CalcResultDetail { RunId = resultsDto.RunId, RelativeYear = resultsDto.RelativeYear },
                CalcResultScaledupProducers = new CalcResultScaledupProducers(),
                CalcResultPartialObligations = new CalcResultPartialObligations(),
                CalcResultLapcapData = new CalcResultLapcapData
                {
                    ByMaterial = [],
                    Total = new ByCountryValue
                    {
                        England         = 13280.45m,
                        Wales           = 210.28m,
                        Scotland        = 91.00m,
                        NorthernIreland = 91.00m,
                        Total           = 13742.80m
                    },
                    CountryApportionment = new CountryApportionmentData()
                },

                CalcResultParameterOtherCost = new CalcResultParameterOtherCost
                {
                    LaDataPrepCharge = new CalcResultParameterOtherCostDetail
                    {
                        England         = 115.45m,
                        Wales           = 114.00m,
                        Scotland        = 117.00m,
                        NorthernIreland = 19.00m,
                        Total           = 365.45m
                    }
                },
                CalcResultLateReportingTonnageData = Fixture.Create<CalcResultLateReportingTonnage>(),
                CalcResultProjectedProducers = new CalcResultProjectedProducers()
            };

            var resultCalc = Construct(resultsDto, calcResult);
            // Assert
            Assert.IsNotNull(calcResult);

            // Check disposal cost row
            var disposalRow = resultCalc.LaDisposalCost;
            Assert.AreEqual(13280.45m, disposalRow.England);
            Assert.AreEqual(210.28m, disposalRow.Wales);
            Assert.AreEqual(91.00m, disposalRow.Scotland);
            Assert.AreEqual(91, disposalRow.NorthernIreland);
            Assert.AreEqual(13742.80m, disposalRow.Total);

            // Check data preparation charge row
            var prepchargeRow = resultCalc.LADataPrepCharge;
            Assert.AreEqual(115.45m, prepchargeRow.England);
            Assert.AreEqual(114.00m, prepchargeRow.Wales);
            Assert.AreEqual(117.00m, prepchargeRow.Scotland);
            Assert.AreEqual(19.00m, prepchargeRow.NorthernIreland);
            Assert.AreEqual(365.45m, prepchargeRow.Total);

            // Check total row
            var totalRow = resultCalc.TotalOnePlusFour;
            Assert.AreEqual(14108.25m, totalRow.Total); // 13,742.80 + 365.45
            Assert.AreEqual(13395.90m, totalRow.EnglandTotal); // 13,280.45 + 115.45
            Assert.AreEqual(324.28m, totalRow.WalesTotal); // 210 + 114.00
            Assert.AreEqual(208.00m, totalRow.ScotlandTotal); // 91.00 + 117.00
            Assert.AreEqual(110.00m, totalRow.NorthernIrelandTotal); // 91.00 + 19.00

            // Check apportionment row
            var apportionmentRow = resultCalc.OnePlusFourApportionment;
            Assert.AreEqual(94.95082664m, Math.Round(apportionmentRow.England, 8));
            Assert.AreEqual(2.29851328m, Math.Round(apportionmentRow.Wales, 8));
            Assert.AreEqual(1.47431467m, Math.Round(apportionmentRow.Scotland, 8));
            Assert.AreEqual(0.77968564m, Math.Round(apportionmentRow.NorthernIreland, 8));
        }
    }
}
