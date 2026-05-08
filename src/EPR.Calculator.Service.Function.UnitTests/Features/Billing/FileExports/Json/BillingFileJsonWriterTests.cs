using System.Text.Json;
using EPR.Calculator.Service.Function.Builder.CommsCost;
using EPR.Calculator.Service.Function.Builder.Lapcap;
using EPR.Calculator.Service.Function.Features.Billing.Contexts;
using EPR.Calculator.Service.Function.Features.Billing.FileExports.Json;
using EPR.Calculator.Service.Function.Features.Common;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Services;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.Data;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.Fixtures;

namespace EPR.Calculator.Service.Function.UnitTests.Features.Billing.FileExports.Json;

[TestCategory(TestCategories.BillingRuns)]
[TestClass]
public class BillingFileJsonWriterTests
{
    private IFixture _fixture = null!;
    private Mock<IMaterialService> _materialService = null!;
    private BillingFileJsonWriter _sut = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        _fixture = TestFixtures.New();
        _materialService = _fixture.Freeze<Mock<IMaterialService>>();

        _materialService.Setup(mock =>
                mock.GetMaterials(It.IsAny<CancellationToken>()))
            .ReturnsAsync(DummyData.Materials);

        _sut = _fixture.Create<BillingFileJsonWriter>();
    }

    [TestMethod]
    public async Task Export_ShouldReturnJsonContent()
    {
        // Arrange
        var runContext = _fixture.Create<BillingRunContext>();
        var calcResult = CreateCalcResult(runContext);

        // Act
        var jsonString = await _sut.WriteToString(runContext, calcResult);

        // Assert
        jsonString.ShouldNotBeNull();
        var deserialized = Should.NotThrow(() => JsonDocument.Parse(jsonString));

        deserialized.ShouldNotBeNull();
    }

    private CalcResult CreateCalcResult(RunContext runContext)
    {
        return new CalcResult
        {
            CalcResultDetail = new CalcResultDetail
            {
                RunId = runContext.RunId,
                RunName = runContext.RunName,
                RunDate = DateTime.UtcNow,
                RunBy = runContext.User,
                RelativeYear = 2024,
                RpdFileORG = "21/07/2017 17:32",
                RpdFilePOM = "21/07/2017 17:32",
                LapcapFile = "lapcap-data.csv,24/06/2025 10:00, test",
                ParametersFile = "parameter-data.csv,24/06/2025 10:00, test"
            },
            CalcResultLapcapData = new CalcResultLapcapData
            {
                Name = "LAPCAP Data",
                CalcResultLapcapDataDetails =
                [
                    new CalcResultLapcapDataDetail
                    {
                        Name = "Total",
                        EnglandDisposalCost = "£13,280.45",
                        WalesDisposalCost = "£210.28",
                        ScotlandDisposalCost = "£161.07",
                        NorthernIrelandDisposalCost = "£91.00",
                        TotalDisposalCost = "£13,742.80",
                        EnglandCost = 13280.45m,
                        WalesCost = 210.28m,
                        ScotlandCost = 91.00m,
                        NorthernIrelandCost = 91.00m,
                        TotalCost = 13742.80m
                    },

                    new CalcResultLapcapDataDetail
                    {
                        Name = CalcResultLapcapDataBuilder.CountryApportionment
                    }
                ]
            },
            CalcResultLateReportingTonnageData = new CalcResultLateReportingTonnage
            {
                Name = "Late Reporting Tonnage",
                MaterialHeading = string.Empty,
                TonnageHeading = string.Empty,
                CalcResultLateReportingTonnageDetails =
                [
                    new CalcResultLateReportingTonnageDetail
                    {
                        Name = "Aluminium",
                        RedLateReportingTonnage = 1000.00m,
                        AmberLateReportingTonnage = 2000.00m,
                        GreenLateReportingTonnage = 5000.00m,
                        TotalLateReportingTonnage = 8000.00m
                    },

                    new CalcResultLateReportingTonnageDetail
                    {
                        Name = "Plastic",
                        RedLateReportingTonnage = 1000.00m,
                        AmberLateReportingTonnage = 500.00m,
                        GreenLateReportingTonnage = 500.00m,
                        TotalLateReportingTonnage = 2000.00m
                    },

                    new CalcResultLateReportingTonnageDetail
                    {
                        Name = "Total",
                        RedLateReportingTonnage = 2000.00m,
                        AmberLateReportingTonnage = 2500.00m,
                        GreenLateReportingTonnage = 5500.00m,
                        TotalLateReportingTonnage = 10000.00m
                    }
                ]
            },
            CalcResultParameterOtherCost = new CalcResultParameterOtherCost
            {
                BadDebtProvision = new KeyValuePair<string, string>("key1", "6%"),
                BadDebtValue = 6m,
                Details =
                [
                    new CalcResultParameterOtherCostDetail
                    {
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
                Materiality =
                [
                    new CalcResultMateriality
                    {
                        Amount = "Amount £s",
                        AmountValue = 0,
                        Percentage = "%",
                        PercentageValue = 0,
                        SevenMateriality = "7 Materiality"
                    }
                ],
                Name = "Parameters - Other",
                SaOperatingCost =
                [
                    new CalcResultParameterOtherCostDetail
                    {
                        Name = string.Empty,
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
                SchemeSetupCost = new CalcResultParameterOtherCostDetail
                {
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
            CalcResultOnePlusFourApportionment = new CalcResultOnePlusFourApportionment
            {
                CalcResultOnePlusFourApportionmentDetails =
                [
                    new CalcResultOnePlusFourApportionmentDetail
                    {
                        EnglandDisposalTotal = "80",
                        NorthernIrelandDisposalTotal = "70",
                        ScotlandDisposalTotal = "30",
                        WalesDisposalTotal = "20",
                        AllTotal = 0.1M,
                        EnglandTotal = 14.53M,
                        NorthernIrelandTotal = 0.15M,
                        ScotlandTotal = 1.15M,
                        WalesTotal = 0.20M,
                        Name = "1 + 4 Apportionment %s"
                    },

                    new CalcResultOnePlusFourApportionmentDetail
                    {
                        EnglandDisposalTotal = "80",
                        NorthernIrelandDisposalTotal = "70",
                        ScotlandDisposalTotal = "30",
                        WalesDisposalTotal = "20",
                        AllTotal = 0.1M,
                        EnglandTotal = 0.10M,
                        NorthernIrelandTotal = 0.15M,
                        ScotlandTotal = 0.15M,
                        WalesTotal = 0.20M,
                        Name = "Test"
                    }
                ],
                Name = "some test"
            },
            CalcResultCommsCostReportDetail = new CalcResultCommsCost
            {
                CalcResultCommsCostCommsCostByMaterial =
                [
                    new CalcResultCommsCostCommsCostByMaterial
                    {
                        CommsCostByMaterialPricePerTonne = "0.42",
                        CommsCostByMaterialPricePerTonneValue = 0.42m,
                        Name = "Aluminium"
                    },

                    new CalcResultCommsCostCommsCostByMaterial
                    {
                        CommsCostByMaterialPricePerTonne = "0.3",
                        CommsCostByMaterialPricePerTonneValue = 0.3m,
                        Name = "Glass"
                    },

                    new CalcResultCommsCostCommsCostByMaterial
                    {
                        CommsCostByMaterialPricePerTonne = "0.51",
                        CommsCostByMaterialPricePerTonneValue = 0.51m,
                        Name = "Total"
                    }
                ],
                CalcResultCommsCostOnePlusFourApportionment =
                [
                    new CalcResultCommsCostCommsCostByMaterial
                    {
                        Name = CalcResultCommsCostBuilder.TwoBCommsCostUkWide,
                        England = "10",
                        Wales = "20",
                        Scotland = "30",
                        NorthernIreland = "40",
                        Total = "100",
                        ProducerReportedHouseholdPackagingWasteTonnage = "50",
                        ReportedPublicBinTonnage = "60",
                        HouseholdDrinksContainers = "70",
                        LateReportingTonnage = "80",
                        ProducerReportedHouseholdPlusLateReportingTonnage = "90",
                        CommsCostByMaterialPricePerTonne = "100"
                    },
                    new CalcResultCommsCostOnePlusFourApportionment
                    {
                        Name = CalcResultCommsCostBuilder.OnePlusFourApportionment,
                        England = "10",
                        Wales = "20",
                        Scotland = "30",
                        NorthernIreland = "40",
                        Total = "100"
                    },
                    new CalcResultCommsCostOnePlusFourApportionment
                    {
                        Name = CalcResultCommsCostBuilder.TwoCCommsCostByCountry,
                        England = "10",
                        Wales = "20",
                        Scotland = "30",
                        NorthernIreland = "40",
                        Total = "100"
                    }
                ],
                CommsCostByCountry =
                [
                    new CalcResultCommsCostOnePlusFourApportionment
                    {
                        Name = CalcResultCommsCostBuilder.TwoBCommsCostUkWide,
                        England = "10",
                        Wales = "20",
                        Scotland = "30",
                        NorthernIreland = "40",
                        Total = "100"
                    },
                    new CalcResultCommsCostOnePlusFourApportionment
                    {
                        Name = CalcResultCommsCostBuilder.TwoCCommsCostByCountry,
                        England = "10",
                        Wales = "20",
                        Scotland = "30",
                        NorthernIreland = "40",
                        Total = "100"
                    }
                ]
            },
            CalcResultLaDisposalCostData = new CalcResultLaDisposalCostData
            {
                CalcResultLaDisposalCostDetails =
                [
                    new CalcResultLaDisposalCostDataDetail
                    {
                        DisposalCostPricePerTonne = "20",
                        England = "EnglandTest",
                        Wales = "WalesTest",
                        Name = "ScotlandTest",
                        Scotland = "ScotlandTest",
                        NorthernIreland = "NorthernIrelandTest",
                        Total = "null",
                        ProducerReportedHouseholdPackagingWasteTonnage = "null",
                        ReportedPublicBinTonnage = string.Empty
                    },

                    new CalcResultLaDisposalCostDataDetail
                    {
                        DisposalCostPricePerTonne = "20",
                        England = "EnglandTest",
                        Wales = "WalesTest",
                        Name = "Material1",
                        Scotland = "ScotlandTest",
                        NorthernIreland = "NorthernIrelandTest",
                        Total = "null",
                        ProducerReportedHouseholdPackagingWasteTonnage = "null",
                        ReportedPublicBinTonnage = string.Empty
                    },

                    new CalcResultLaDisposalCostDataDetail
                    {
                        DisposalCostPricePerTonne = "10",
                        England = "EnglandTest",
                        Wales = "WalesTest",
                        Name = "Material2",
                        Scotland = "ScotlandTest",
                        NorthernIreland = "NorthernIrelandTest",
                        Total = "100",
                        ProducerReportedHouseholdPackagingWasteTonnage = "null",
                        ReportedPublicBinTonnage = string.Empty
                    }
                ],
                Name = "LA Disposal Cost Data"
            },
            CalcResultScaledupProducers = DummyData.GetScaledupProducers(),
            CalcResultSummary = DummyData.GetCalcResultSummary(),
            CalcResultPartialObligations = new CalcResultPartialObligations(),
            CalcResultModulation = null,
            CalcResultProjectedProducers = new CalcResultProjectedProducers()
        };
    }
}
