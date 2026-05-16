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
                    CalcResultLapcapDataDetails = new[]
                    {
                        new CalcResultLapcapDataDetail
                        {
                            Name                = "Total",
                            EnglandCost         = 13280.45m,
                            WalesCost           = 210.28m,
                            ScotlandCost        = 91.00m,
                            NorthernIrelandCost = 91.00m,
                            TotalCost           = 13742.80m
                        }
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
            Assert.AreEqual(4, resultCalc.CalcResultOnePlusFourApportionmentDetails.Count());

            // Check disposal cost row
            var disposalRow = resultCalc.CalcResultOnePlusFourApportionmentDetails.Single(x => x.Name == "1 Fee for LA Disposal Costs");
            Assert.AreEqual(13280.45m, disposalRow.EnglandTotal);
            Assert.AreEqual(210.28m, disposalRow.WalesTotal);
            Assert.AreEqual(91.00m, disposalRow.ScotlandTotal);
            Assert.AreEqual(91, disposalRow.NorthernIrelandTotal);
            Assert.AreEqual(13742.80m, disposalRow.Total);

            // Check data preparation charge row
            var prepchargeRow = resultCalc.CalcResultOnePlusFourApportionmentDetails.Single(x => x.Name == "4 LA Data Prep Charge");
            Assert.AreEqual(115.45m, prepchargeRow.EnglandTotal);
            Assert.AreEqual(114.00m, prepchargeRow.WalesTotal);
            Assert.AreEqual(117.00m, prepchargeRow.ScotlandTotal);
            Assert.AreEqual(19.00m, prepchargeRow.NorthernIrelandTotal);
            Assert.AreEqual(365.45m, prepchargeRow.Total);

            // Check total row
            var totalRow = resultCalc.CalcResultOnePlusFourApportionmentDetails.Single(x => x.OrderId == 3);
            Assert.AreEqual("Total of 1 + 4", totalRow.Name);
            Assert.AreEqual(14108.25m, totalRow.Total); // 13,742.80 + 365.45
            Assert.AreEqual(13395.90m, totalRow.EnglandTotal); // 13,280.45 + 115.45
            Assert.AreEqual(324.28m, totalRow.WalesTotal); // 210 + 114.00
            Assert.AreEqual(208.00m, totalRow.ScotlandTotal); // 91.00 + 117.00
            Assert.AreEqual(110.00m, totalRow.NorthernIrelandTotal); // 91.00 + 19.00

            // Check apportionment row
            var apportionmentRow = resultCalc.CalcResultOnePlusFourApportionmentDetails.Single(x => x.OrderId == 4);
            Assert.AreEqual("1 + 4 Apportionment %s", apportionmentRow.Name);
            // TODO should we round to 8 d.p in the Builder (i.e. precision to store in db eventually)
            Assert.AreEqual(100.00000000m, Math.Round(apportionmentRow.Total, 8));
            Assert.AreEqual(94.95082664m, Math.Round(apportionmentRow.EnglandTotal, 8));
            Assert.AreEqual(2.29851328m, Math.Round(apportionmentRow.WalesTotal, 8));
            Assert.AreEqual(1.47431467m, Math.Round(apportionmentRow.ScotlandTotal, 8));
            Assert.AreEqual(0.77968564m, Math.Round(apportionmentRow.NorthernIrelandTotal, 8));
        }
    }
}
