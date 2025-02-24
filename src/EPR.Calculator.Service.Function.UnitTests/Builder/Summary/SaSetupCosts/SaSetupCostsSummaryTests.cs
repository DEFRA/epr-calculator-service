﻿namespace EPR.Calculator.Service.Function.UnitTests.Builder.Summary.SchemeAdministratorSetupCosts
{
    using EPR.Calculator.Service.Function.Builder.Summary.SaSetupCosts;
    using Models;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using AutoFixture;
    using EPR.Calculator.Service.Function.Constants;
    using EPR.Calculator.Service.Function.Builder.Summary.Common;
    using EPR.Calculator.Service.Function.Enums;
    using EPR.Calculator.Service.Function.Models;

    [TestClass]
    public class SaSetupCostsSummaryTests
    {
        private readonly CalcResult _calcResult;

        private Fixture Fixture { get; init; } = new Fixture();

        public SaSetupCostsSummaryTests()
        {
            _calcResult = new CalcResult
            {
                CalcResultParameterOtherCost = new CalcResultParameterOtherCost
                {
                    BadDebtProvision = new KeyValuePair<string, string>("key1", "6%"),
                    BadDebtValue = 6m,
                    Details = [
                        new CalcResultParameterOtherCostDetail {
                            Name = "4 LA Data Prep Charge",
                            OrderId = 1,
                            England = "£40.00",
                            EnglandValue = 40,
                            Wales = "£30.00",
                            WalesValue = 30,
                            Scotland = "£20.00",
                            ScotlandValue = 20,
                            NorthernIreland = "£10.00",
                            NorthernIrelandValue = 10,
                            Total = "£100.00",
                            TotalValue = 100
                        }
                    ],
                    Materiality = [
                        new CalcResultMateriality {
                            Amount = "Amount £s",
                            AmountValue = 0,
                            Percentage = "%",
                            PercentageValue = 0,
                            SevenMateriality = "7 Materiality"
                        }
                    ],
                    Name = "Parameters - Other",
                    SaOperatingCost = [
                        new CalcResultParameterOtherCostDetail {
                            Name = string.Empty,
                            OrderId = 0,
                            England = "England",
                            EnglandValue = 0,
                            Wales = "Wales",
                            WalesValue = 0,
                            Scotland = "Scotland",
                            ScotlandValue = 0,
                            NorthernIreland = "Northern Ireland",
                            NorthernIrelandValue = 0,
                            Total = "Total",
                            TotalValue = 0
                        }
                    ],
                    SchemeSetupCost = {
                        Name = "5 Scheme set up cost Yearly Cost",
                        OrderId = 1,
                        England = "£40.00",
                        EnglandValue = 40,
                        Wales = "£30.00",
                        WalesValue = 30,
                        Scotland = "£20.00",
                        ScotlandValue = 20,
                        NorthernIreland = "£10.00",
                        NorthernIrelandValue = 10,
                        Total = "£100.00",
                        TotalValue = 100
                    }
                },
                CalcResultDetail = new CalcResultDetail() { },
                CalcResultLaDisposalCostData = new CalcResultLaDisposalCostData()
                {
                    Name = Fixture.Create<string>(),
                    CalcResultLaDisposalCostDetails = new List<CalcResultLaDisposalCostDataDetail>()
                    {
                        new CalcResultLaDisposalCostDataDetail()
                        {
                            DisposalCostPricePerTonne="20",
                            England="EnglandTest",
                            Wales="WalesTest",
                            Name="ScotlandTest",
                            Scotland="ScotlandTest",
                            Material = "Material1",
                            NorthernIreland = "NorthernIrelandTest",
                            Total = "TotalTest",
                            ProducerReportedHouseholdPackagingWasteTonnage = Fixture.Create<string>(),
                            ReportedPublicBinTonnage = Fixture.Create<string>(),
                            ProducerReportedTotalTonnage = Fixture.Create<string>(),
                        },
                         new CalcResultLaDisposalCostDataDetail()
                        {
                            DisposalCostPricePerTonne="20",
                            England="EnglandTest",
                            Wales="WalesTest",
                            Name="Material1",
                            Scotland="ScotlandTest",
                            NorthernIreland = "NorthernIrelandTest",
                            Total = "TotalTest",
                            ProducerReportedHouseholdPackagingWasteTonnage = Fixture.Create<string>(),
                            ReportedPublicBinTonnage = Fixture.Create<string>(),
                            ProducerReportedTotalTonnage = Fixture.Create<string>(),
                        },
                          new CalcResultLaDisposalCostDataDetail()
                        {
                            DisposalCostPricePerTonne="10",
                            England="EnglandTest",
                            Wales="WalesTest",
                            Name="Material2",
                            Scotland="ScotlandTest",
                            NorthernIreland = "NorthernIrelandTest",
                            Total = "TotalTest",
                            ProducerReportedHouseholdPackagingWasteTonnage = Fixture.Create<string>(),
                            ReportedPublicBinTonnage = Fixture.Create<string>(),
                            ProducerReportedTotalTonnage = Fixture.Create<string>(),
                        }
                    }
                },
                CalcResultLapcapData = new CalcResultLapcapData() { CalcResultLapcapDataDetails = new List<CalcResultLapcapDataDetails>() { } },
                CalcResultOnePlusFourApportionment = new CalcResultOnePlusFourApportionment()
                {
                    Name = Fixture.Create<string>(),
                    CalcResultOnePlusFourApportionmentDetails =
                    [
                        new()
                        {
                            EnglandDisposalTotal="80",
                            NorthernIrelandDisposalTotal="70",
                            ScotlandDisposalTotal="30",
                            WalesDisposalTotal="20",
                            AllTotal=0.1M,
                            EnglandTotal=0.10M,
                            NorthernIrelandTotal=0.15M,
                            ScotlandTotal=0.15M,
                            WalesTotal=020M,
                            Name="Test",
                        },
                        new()
                        {
                            EnglandDisposalTotal="80",
                            NorthernIrelandDisposalTotal="70",
                            ScotlandDisposalTotal="30",
                            WalesDisposalTotal="20",
                            AllTotal=0.1M,
                            EnglandTotal=0.10M,
                            NorthernIrelandTotal=0.15M,
                            ScotlandTotal=0.15M,
                            WalesTotal=020M,
                            Name="Test",
                        },
                        new()
                        {
                            EnglandDisposalTotal="80",
                            NorthernIrelandDisposalTotal="70",
                            ScotlandDisposalTotal="30",
                            WalesDisposalTotal="20",
                            AllTotal=0.1M,
                            EnglandTotal=0.10M,
                            NorthernIrelandTotal=0.15M,
                            ScotlandTotal=0.15M,
                            WalesTotal=020M,
                            Name="Test",
                        },
                        new()
                        {
                            EnglandDisposalTotal="80",
                            NorthernIrelandDisposalTotal="70",
                            ScotlandDisposalTotal="30",
                            WalesDisposalTotal="20",
                            AllTotal=0.1M,
                            EnglandTotal=14.53M,
                            NorthernIrelandTotal=0.15M,
                            ScotlandTotal=0.15M,
                            WalesTotal=020M,
                            Name="Test",
                        },
                     new()
                        {
                            EnglandDisposalTotal="80",
                            NorthernIrelandDisposalTotal="70",
                            ScotlandDisposalTotal="30",
                            WalesDisposalTotal="20",
                            AllTotal=0.1M,
                            EnglandTotal=14.53M,
                            NorthernIrelandTotal=0.15M,
                            ScotlandTotal=0.15M,
                            WalesTotal=0.20M,
                            Name="Test",
                            OrderId=4
                        }]
                },
                CalcResultParameterCommunicationCost = Fixture.Create<CalcResultParameterCommunicationCost>(),
                CalcResultSummary = new CalcResultSummary
                {
                    ProducerDisposalFees = new List<CalcResultSummaryProducerDisposalFees>() { new()
                {
                     ProducerCommsFeesByMaterial =  new Dictionary<MaterialDetail, CalcResultSummaryProducerCommsFeesCostByMaterial>(){ },
                      ProducerDisposalFeesByMaterial = new Dictionary<MaterialDetail, CalcResultSummaryProducerDisposalFeesByMaterial>(){ },
                       ProducerId ="1",
                        ProducerName ="Test",
                     TotalProducerDisposalFeeWithBadDebtProvision =100,
                     TotalProducerCommsFeeWithBadDebtProvision =100,
                      SubsidiaryId ="1",

                } }
                },
                CalcResultCommsCostReportDetail = new CalcResultCommsCost()
                {
                    CalcResultCommsCostCommsCostByMaterial =
                    [
                        new ()
                        {
                            CommsCostByMaterialPricePerTonne="0.42",
                            Name ="Material1",

                        },
                        new ()
                        {
                            CommsCostByMaterialPricePerTonne="0.3",
                            Name ="Material2",

                        }
                    ]
                },
                CalcResultLateReportingTonnageData = Fixture.Create<CalcResultLateReportingTonnage>(),
            };
        }

        [TestMethod]
        public void CanCallGetHeaders()
        {
            // Act
            var result = SaSetupCostsSummary.GetHeaders().ToList();
            var columnIndex = 259;

            var expectedResult = new List<CalcResultSummaryHeader>();
            expectedResult.AddRange([
                new CalcResultSummaryHeader { Name = $"{SaSetupCostsHeaders.OneOffFeeSetupCostsWithoutBadDebtProvisionTitle}", ColumnIndex = columnIndex },
                new CalcResultSummaryHeader { Name = $"{SaSetupCostsHeaders.BadDebtProvisionTitle}", ColumnIndex = columnIndex+1 },
                new CalcResultSummaryHeader { Name = $"{SaSetupCostsHeaders.OneOffFeeSetupCostsWithBadDebtProvisionTitle}", ColumnIndex = columnIndex+2 }
            ]);

            // Assert
            Assert.AreEqual(expectedResult[0].Name, result[0].Name);
            Assert.AreEqual(expectedResult[0].ColumnIndex, result[0].ColumnIndex);
            Assert.AreEqual(expectedResult[1].Name, result[1].Name);
            Assert.AreEqual(expectedResult[1].ColumnIndex, result[1].ColumnIndex);
            Assert.AreEqual(expectedResult[2].Name, result[2].Name);
            Assert.AreEqual(expectedResult[2].ColumnIndex, result[2].ColumnIndex);
        }

        [TestMethod]
        public void CanCallGetSaSetupCostsWithoutBadDebtProvision()
        {
            // Act
            var result = SaSetupCostsSummary.GetOneOffFeeSetupCostsWithoutBadDebtProvision(_calcResult);

            // Assert
            Assert.AreEqual(100, result);
        }

        [TestMethod]
        public void CanCallGetBadDebtProvision()
        {
            // Act
            var result = SaSetupCostsSummary.GetBadDebtProvision(_calcResult);

            // Assert
            Assert.AreEqual(6, result);
        }

        [TestMethod]
        public void CanCallGetSaSetupCostsWithBadDebtProvision()
        {
            // Act
            var result = SaSetupCostsSummary.GetOneOffFeeSetupCostsWithBadDebtProvision(_calcResult);

            // Assert
            Assert.AreEqual(106, result);
        }
    }
}
