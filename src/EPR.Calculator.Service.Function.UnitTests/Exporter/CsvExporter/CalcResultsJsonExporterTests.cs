using AutoFixture;
using EPR.Calculator.API.Data;
using EPR.Calculator.Service.Function.Builder.CommsCost;
using EPR.Calculator.Service.Function.Builder.Lapcap;
using EPR.Calculator.Service.Function.Exporter.JsonExporter;
using EPR.Calculator.Service.Function.Exporter.JsonExporter.CalculationResults;
using EPR.Calculator.Service.Function.Exporter.JsonExporter.CancelledProducersData;
using EPR.Calculator.Service.Function.Exporter.JsonExporter.CommsCostByMaterial2A;
using EPR.Calculator.Service.Function.Exporter.JsonExporter.Detail;
using EPR.Calculator.Service.Function.Exporter.JsonExporter.LaDisposalCostData;
using EPR.Calculator.Service.Function.Exporter.JsonExporter.Lapcap;
using EPR.Calculator.Service.Function.Exporter.JsonExporter.LateReportingTonnage;
using EPR.Calculator.Service.Function.Exporter.JsonExporter.OnePlusFourApportionment;
using EPR.Calculator.Service.Function.Exporter.JsonExporter.ScaledupProducers;
using EPR.Calculator.Service.Function.Mapper;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Models.JsonExporter;
using EPR.Calculator.Service.Function.Services;
using EPR.Calculator.Service.Function.UnitTests.Builder;
using Moq;

namespace EPR.Calculator.Service.Function.UnitTests.Exporter.CsvExporter
{
    [TestClass]
    public class CalcResultsJsonExporterTests
    {
        private Fixture fixture;
        private CalcResultsJsonExporter testClass;
        private Mock<IMaterialService> mockMaterialService;
        private ICalcResultDetailJsonExporter mockCalcResultDetailExporter;
        private ICalcResultLapcapExporter mockCalcResultLapcapExporter;
        private ILateReportingTonnage mockLateReportingTonnage;
        private IParametersOtherJsonExporter mockParametersOtherJsonExporter;
        private IOnePlusFourApportionmentJsonExporter mockOnePlusFourApportionmentJsonExporter;
        private ICommsCostJsonExporter mockCommsCostExporter;
        private ICommsCostByMaterial2AExporter mockCommsCostByMaterial2AExporter;
        private ICalcResultCommsCostOnePlusFourApportionmentExporter mockCalcResultCommsCostOnePlusFourApportionmentExporter;
        private ICalcResultLaDisposalCostDataExporter mockCalcResultLaDisposalCostDataExporter;
        private ICancelledProducersExporter mockCancelledProducersExporter;
        private ICalcResultScaledupProducersJsonExporter mockCalcResultScaledupProducersJsonExporter;
        private ICalculationResultsExporter mockCalculationResultsExporter;

        public CalcResultsJsonExporterTests()
        {
            fixture = new Fixture();
            mockMaterialService = new Mock<IMaterialService>();
            mockCalcResultDetailExporter = new CalcResultDetailJsonExporter();
            mockCalcResultLapcapExporter = new CalcResultLapcapExporter();
            mockLateReportingTonnage = new LateReportingTonnage(new LateReportingTonnageMapper());
            mockParametersOtherJsonExporter = new ParametersOtherJsonExporter(new ParametersOtherMapper());
            mockOnePlusFourApportionmentJsonExporter = new OnePlusFourApportionmentJsonExporter(new OnePlusFourApportionmentMapper());
            mockCommsCostExporter = new CommsCostJsonExporter(new CommsCostMapper());
            mockCommsCostByMaterial2AExporter = new CommsCostByMaterial2AExporter(new CalcResult2ACommsDataByMaterialMapper());
            mockCalcResultCommsCostOnePlusFourApportionmentExporter = new CalcResultCommsCostOnePlusFourApportionmentExporter();
            mockCalcResultLaDisposalCostDataExporter = new CalcResultLaDisposalCostDataExporter(new CalcResultLaDisposalCostDataMapper());
            mockCancelledProducersExporter = new CancelledProducersExporter(new CancelledProducersMapper());
            mockCalcResultScaledupProducersJsonExporter = new CalcResultScaledupProducersJsonExporter(new CalcResultScaledupProducersJsonMapper());
            mockCalculationResultsExporter = new CalculationResultsExporter(
                new ProducerDisposalFeesWithBadDebtProvision1JsonMapper(),
                new CommsCostsByMaterialFeesSummary2AMapper(),
                new CalcResultCommsCostByMaterial2AJsonMapper(),
                new SAOperatingCostsWithBadDebtProvisionMapper(),
                new CalcResultLADataPrepCostsWithBadDebtProvision4Mapper(),
                new FeeForCommsCostsWithBadDebtProvision2AMapper(),
                new FeeForCommsCostsWithBadDebtProvision2BMapper(),
                new TotalProducerFeeWithBadDebtProvisionFor2Con1And2AAnd2BAnd2CMapper(),
                new FeeForSaSetUpCostsWithBadDebtProvision5Mapper(),
                new CalcResultCommsCostsWithBadDebtProvision2CMapper(),
                new CalculationOfSuggestedBillingInstructionsAndInvoiceAmountsMapper(),
                new TotalProducerBillWithBadDebtProvisionMapper(),
                new CalculationResultsProducerCalculationResultsFeeForLADisposalCosts1Mapper(),
                new CalcResultProducerCalculationResultsTotalMapper(),
                new DisposalFeeSummary1Mapper());

            testClass = new CalcResultsJsonExporter(
                mockMaterialService.Object,
                mockCalcResultDetailExporter,
                mockCalcResultLapcapExporter,
                mockLateReportingTonnage,
                mockParametersOtherJsonExporter,
                mockOnePlusFourApportionmentJsonExporter,
                mockCommsCostExporter,
                mockCommsCostByMaterial2AExporter,
                mockCalcResultCommsCostOnePlusFourApportionmentExporter,
                mockCalcResultLaDisposalCostDataExporter,
                mockCancelledProducersExporter,
                mockCalcResultScaledupProducersJsonExporter,
                mockCalculationResultsExporter);
        }

        [TestMethod]
        public void Export_ShouldReturnJsonContent()
        {
            // Arrange
            var calcResult = CreateCalcResult();
            var materials = TestDataHelper.GetMaterials();
            mockMaterialService.Setup(service => service.GetMaterials()).Returns(Task.FromResult(materials));

            // Act
            var result = testClass.Export(calcResult, new List<int> { 1, 2 });

            // Assert
            Assert.IsNotNull(result);
        }

        private static CalcResult CreateCalcResult()
        {
            return new CalcResult
            {
                CalcResultDetail = new CalcResultDetail
                {
                    RunId = 1,
                    RunDate = DateTime.UtcNow,
                    RunName = "CalculatorRunName",
                    RunBy = "Test user",
                    FinancialYear = "2024-25",
                    RpdFileORG = "21/07/2017 17:32",
                    RpdFilePOM = "21/07/2017 17:32",
                    LapcapFile = "lapcap-data.csv,24/06/2025 10:00, test",
                    ParametersFile = "parameter-data.csv,24/06/2025 10:00, test"
                },
                CalcResultLapcapData = new CalcResultLapcapData
                {
                    Name = "LAPCAP Data",
                    CalcResultLapcapDataDetails = new List<CalcResultLapcapDataDetails>
                    {
                        new()
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
                            TotalCost = 13742.80m,
                        },
                        new()
                        {
                            Name = CalcResultLapcapDataBuilder.CountryApportionment,
                        }
                    },
                },
                CalcResultLateReportingTonnageData = new CalcResultLateReportingTonnage
                {
                    Name = "Late Reporting Tonnage",
                    MaterialHeading = string.Empty,
                    TonnageHeading = string.Empty,
                    CalcResultLateReportingTonnageDetails = new List<CalcResultLateReportingTonnageDetail>
                    {
                        new CalcResultLateReportingTonnageDetail
                        {
                            Name = "Aluminium",
                            TotalLateReportingTonnage = 8000.00m,
                        },
                        new CalcResultLateReportingTonnageDetail
                        {
                            Name = "Plastic",
                            TotalLateReportingTonnage = 2000.00m,
                        },
                        new CalcResultLateReportingTonnageDetail
                        {
                            Name = "Total",
                            TotalLateReportingTonnage = 10000.00m,
                        },
                    },
                },
                CalcResultParameterOtherCost = new CalcResultParameterOtherCost
                {
                    BadDebtProvision = new KeyValuePair<string, string>("key1", "6%"),
                    BadDebtValue = 6m,
                    Details = new List<CalcResultParameterOtherCostDetail>
                    {
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
                            TotalValue = 100,
                        },

                    },
                    Materiality = new List<CalcResultMateriality>
                    {
                        new CalcResultMateriality
                        {
                            Amount = "Amount £s",
                            AmountValue = 0,
                            Percentage = "%",
                            PercentageValue = 0,
                            SevenMateriality = "7 Materiality",
                        },
                    },
                    Name = "Parameters - Other",
                    SaOperatingCost = new List<CalcResultParameterOtherCostDetail>
                    {
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
                            TotalValue = 100,
                        },
                    },
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
                        TotalValue = 100,
                    },
                },
                CalcResultOnePlusFourApportionment = new CalcResultOnePlusFourApportionment
                {
                    CalcResultOnePlusFourApportionmentDetails = new List<CalcResultOnePlusFourApportionmentDetail>
                    {
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
                            Name = "1 + 4 Apportionment %s",
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
                            Name = "Test",
                        },
                    },
                    Name = "some test",
                },
                CalcResultCommsCostReportDetail = new CalcResultCommsCost
                {
                    CalcResultCommsCostCommsCostByMaterial = new List<CalcResultCommsCostCommsCostByMaterial>
                    {
                        new CalcResultCommsCostCommsCostByMaterial
                        {
                            CommsCostByMaterialPricePerTonne = "0.42",
                            Name = "Aluminium",
                        },
                        new CalcResultCommsCostCommsCostByMaterial
                        {
                            CommsCostByMaterialPricePerTonne = "0.3",
                            Name = "Glass",
                        },
                        new CalcResultCommsCostCommsCostByMaterial
                        {
                            CommsCostByMaterialPricePerTonne = "0.51",
                            Name = "Total",
                        },
                    },
                    CalcResultCommsCostOnePlusFourApportionment = new List<CalcResultCommsCostOnePlusFourApportionment>
                    {
                        new CalcResultCommsCostCommsCostByMaterial { Name = CalcResultCommsCostBuilder.TwoBCommsCostUkWide, England = "10", Wales = "20", Scotland = "30", NorthernIreland = "40", Total = "100", ProducerReportedHouseholdPackagingWasteTonnage = "50", ReportedPublicBinTonnage = "60", HouseholdDrinksContainers = "70", LateReportingTonnage = "80", ProducerReportedHouseholdPlusLateReportingTonnage = "90", CommsCostByMaterialPricePerTonne = "100" },
                        new CalcResultCommsCostOnePlusFourApportionment { Name = CalcResultCommsCostBuilder.TwoCCommsCostByCountry, England = "10", Wales = "20", Scotland = "30", NorthernIreland = "40", Total = "100" }
                    },
                    CommsCostByCountry = new List<CalcResultCommsCostOnePlusFourApportionment>
                    {
                        new CalcResultCommsCostOnePlusFourApportionment { Name = CalcResultCommsCostBuilder.TwoCCommsCostByCountry, England = "10", Wales = "20", Scotland = "30", NorthernIreland = "40", Total = "100" }
                    }
                },
                CalcResultLaDisposalCostData = new CalcResultLaDisposalCostData
                {
                    CalcResultLaDisposalCostDetails = new List<CalcResultLaDisposalCostDataDetail>
                    {
                        new CalcResultLaDisposalCostDataDetail
                        {
                            DisposalCostPricePerTonne = "20",
                            England = "EnglandTest",
                            Wales = "WalesTest",
                            Name = "ScotlandTest",
                            Scotland = "ScotlandTest",
                            Material = "Material1",
                            NorthernIreland = "NorthernIrelandTest",
                            Total = "null",
                            ProducerReportedHouseholdPackagingWasteTonnage = "null",
                            ReportedPublicBinTonnage = string.Empty,
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
                            ReportedPublicBinTonnage =string.Empty,
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
                            ReportedPublicBinTonnage = string.Empty,
                        },
                    },
                    Name = "LA Disposal Cost Data",
                },
                CalcResultScaledupProducers = TestDataHelper.GetScaledupProducers(),
                CalcResultSummary = TestDataHelper.GetCalcResultSummary()
            };
        }

        private static Dictionary<string, CalcResultScaledupProducerTonnage> GetScaledupProducerTonnageByMaterial()
        {
            var tonnageByMaterial = new Dictionary<string, CalcResultScaledupProducerTonnage>();

            tonnageByMaterial.Add(
                "AL",
                new CalcResultScaledupProducerTonnage()
                {
                    ReportedHouseholdPackagingWasteTonnage = 1000,
                    ReportedPublicBinTonnage = 100,
                    TotalReportedTonnage = 1100,
                    ReportedSelfManagedConsumerWasteTonnage = 500,
                    NetReportedTonnage = 1100,
                    ScaledupReportedHouseholdPackagingWasteTonnage = 2000,
                    ScaledupReportedPublicBinTonnage = 200,
                    ScaledupTotalReportedTonnage = 2200,
                    ScaledupReportedSelfManagedConsumerWasteTonnage = 1000,
                    ScaledupNetReportedTonnage = 2200,
                });

            return tonnageByMaterial;
        }

        private static List<CalcResultScaledupProducer> GetCalcResultScaledupProducerList()
        {
            var scaledupProducerList = new List<CalcResultScaledupProducer>();

            scaledupProducerList.AddRange([
                new CalcResultScaledupProducer()
                {
                    ProducerId = 101001,
                    SubsidiaryId = string.Empty,
                    ProducerName = "Allied Packaging",
                    Level = "1",
                    SubmissionPeriodCode = "2024-P2",
                    DaysInSubmissionPeriod = 91,
                    DaysInWholePeriod = 91,
                    ScaleupFactor = 2,
                    ScaledupProducerTonnageByMaterial = GetScaledupProducerTonnageByMaterial(),
                },
                new CalcResultScaledupProducer()
                {
                    ProducerId = 101001,
                    SubsidiaryId = string.Empty,
                    ProducerName = "Allied Packaging",
                    Level = "1",
                    SubmissionPeriodCode = "2024-P2",
                    DaysInSubmissionPeriod = 91,
                    DaysInWholePeriod = 91,
                    ScaleupFactor = 2,
                    ScaledupProducerTonnageByMaterial = GetScaledupProducerTonnageByMaterial(),
                    IsTotalRow = true,
                },
            ]);

            return scaledupProducerList;
        }

    }
}
