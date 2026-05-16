using EPR.Calculator.API.Data.Models;
using EPR.Calculator.Service.Function.Builder.CommsCost;
using EPR.Calculator.Service.Function.Builder.Lapcap;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Models.JsonExporter;
using EPR.Calculator.Service.Function.UnitTests.Builder;
using Newtonsoft.Json;

namespace EPR.Calculator.Service.Function.UnitTests.Models.JsonExporter
{
    [TestClass]
    public class BillingFileJsonTests
    {
        [TestMethod]
        public void From_MapFieldsCorrectly()
        {
            var calcResult = CreateCalcResult();
            var materials = TestDataHelper.GetMaterials();
            var acceptedProducerIds = new List<int> { 1, 2 };

            var result = BillingFileJson.From(calcResult, acceptedProducerIds, materials);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.CalcResultDetail);
            Assert.AreEqual(1, result.CalcResultDetail!.RunId);
            Assert.IsNotNull(result.CalcResultLapcapData);
            // Lapcap mapping assertions
            var lapcap = result.CalcResultLapcapData as CalcResultLapcapDataJson;
            Assert.IsNotNull(lapcap);
            Assert.IsNotNull(lapcap.CalcResultLapcapDataTotal);
            Assert.AreEqual("£13,742.80", lapcap.CalcResultLapcapDataTotal!.TotalLaDisposalCost);
            var ladetails = result.CalcResultLaDisposalCostData!.CalcResultLaDisposalCostDetails.ToList();
            Assert.IsTrue(ladetails.Any(d => d.DisposalCostPricePerTonne == "£20.0000"));
            Assert.IsNotNull(result.CalcResult2aCommsDataByMaterial);
            var comms = result.CalcResult2aCommsDataByMaterial!.CalcResult2aCommsDataDetails;
            Assert.IsTrue(comms.Any(d => d.MaterialName == "Aluminium"));
            var aluminium = comms.Single(d => d.MaterialName == "Aluminium");
            Assert.AreEqual("£0.4200", aluminium.CommsCostByMaterialPricePerTonne);
            Assert.IsNotNull(result.CalcResult2bCommsDataByUkWide);
            Assert.AreEqual("£10.00", result.CalcResult2bCommsDataByUkWide!.EnglandCommsCostUKWide);
            Assert.IsNotNull(result.CalcResult2cCommsDataByCountry);
            Assert.AreEqual("£20.00", result.CalcResult2cCommsDataByCountry.WalesCommsCostByCountry);
            Assert.IsNotNull(result.ParametersCommsCost);
            var onePlusFourPct = result.ParametersCommsCost!.OnePlusFourCommsCostApportionmentPercentages;
            Assert.IsNotNull(onePlusFourPct);
            Assert.AreEqual("10.00%", onePlusFourPct.England);
            Assert.AreEqual("20.00%", onePlusFourPct.Wales);
            Assert.IsNotNull(result.ScaleUpProducers!.ProducerSubmissions);
            var subs = result.ScaleUpProducers.ProducerSubmissions!.ToList();
            Assert.IsTrue(subs.Count >= 1);
            Assert.AreEqual(1, subs[0].ProducerId);
            var calcResults = result.CalculationResults as CalculationResultsJson;
            Assert.IsNotNull(calcResults);
            Assert.IsNotNull(calcResults.ProducerCalculationResultsSummary);

        }

        private CalcResult CreateCalcResult()
        {
            return new CalcResult
            {
                ApplyModulation = true,
                CalcResultDetail = new CalcResultDetail
                {
                    RunId = 1,
                    RunDate = DateTime.UtcNow,
                    RunName = "CalculatorRunName",
                    RunBy = "Test user",
                    RelativeYear = new RelativeYear(2024),
                    RpdFileORG = "21/07/2017 17:32",
                    RpdFilePOM = "21/07/2017 17:32",
                    LapcapFile = "lapcap-data.csv,24/06/2025 10:00, test",
                    ParametersFile = "parameter-data.csv,24/06/2025 10:00, test"
                },
                CalcResultLapcapData = new CalcResultLapcapData
                {
                    CalcResultLapcapDataDetails = new List<CalcResultLapcapDataDetail>
                    {
                        new()
                        {
                            Name = "Total",
                            EnglandCost = 13280.45m,
                            WalesCost = 210.28m,
                            ScotlandCost = 91.00m,
                            NorthernIrelandCost = 91.00m,
                            TotalCost = 13742.80m,
                        }
                    },
                    CountryApportionment = new CountryApportionmentData()
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
                            RedLateReportingTonnage = 1000.00m,
                            AmberLateReportingTonnage = 2000.00m,
                            GreenLateReportingTonnage = 5000.00m,
                            TotalLateReportingTonnage = 8000.00m,
                        },
                        new CalcResultLateReportingTonnageDetail
                        {
                            Name = "Plastic",
                            RedLateReportingTonnage = 1000.00m,
                            AmberLateReportingTonnage = 500.00m,
                            GreenLateReportingTonnage = 500.00m,
                            TotalLateReportingTonnage = 2000.00m,
                        },
                        new CalcResultLateReportingTonnageDetail
                        {
                            Name = "Total",
                            RedLateReportingTonnage = 2000.00m,
                            AmberLateReportingTonnage = 2500.00m,
                            GreenLateReportingTonnage = 5500.00m,
                            TotalLateReportingTonnage = 10000.00m,
                        },
                    },
                },
                CalcResultParameterOtherCost = new CalcResultParameterOtherCost
                {
                    BadDebtValue = 6m,
                    LaDataPrepCharge = new CalcResultParameterOtherCostDetail
                    {
                        England = 40,
                        Wales = 30,
                        Scotland = 20,
                        NorthernIreland = 10,
                        Total = 100,
                    },
                    SaOperatingCost = new CalcResultParameterOtherCostDetail
                    {
                        England = 40,
                        Wales = 30,
                        Scotland = 20,
                        NorthernIreland = 10,
                        Total = 100,
                    },
                    SchemeSetupCost = new CalcResultParameterOtherCostDetail
                    {
                        England = 40,
                        Wales = 30,
                        Scotland = 20,
                        NorthernIreland = 10,
                        Total = 100
                    }
                },
                CalcResultOnePlusFourApportionment = new CalcResultOnePlusFourApportionment
                {
                    CalcResultOnePlusFourApportionmentDetails = new List<CalcResultOnePlusFourApportionmentDetail>
                    {
                        new CalcResultOnePlusFourApportionmentDetail
                        {
                            Name = "1 + 4 Apportionment %s",
                            EnglandTotal = 14.53M,
                            NorthernIrelandTotal = 0.15M,
                            ScotlandTotal = 1.15M,
                            WalesTotal = 0.20M,
                            Total = 0.1M
                        },
                        new CalcResultOnePlusFourApportionmentDetail
                        {
                            Name = "Test",
                            EnglandTotal = 0.10M,
                            NorthernIrelandTotal = 0.15M,
                            ScotlandTotal = 0.15M,
                            WalesTotal = 0.20M,
                            Total = 0.1M
                        },
                    }
                },
                CalcResultCommsCostReportDetail = new CalcResultCommsCost
                {
                    CalcResultCommsCostCommsCostByMaterial = new List<CalcResultCommsCostCommsCostByMaterial>
                    {
                        new CalcResultCommsCostCommsCostByMaterial
                        {
                            Name = "Aluminium",
                            CommsCostByMaterialPricePerTonne = 0.42m,
                            ProducerReportedHouseholdPackagingWasteTonnage = 0,
                            LateReportingTonnage = 0,
                            ReportedPublicBinTonnage = 0,
                            ProducerReportedTotalTonnage = 0
                        },
                        new CalcResultCommsCostCommsCostByMaterial
                        {
                            Name = "Glass",
                            CommsCostByMaterialPricePerTonne = 0.3m,
                            ProducerReportedHouseholdPackagingWasteTonnage = 0,
                            LateReportingTonnage = 0,
                            ReportedPublicBinTonnage = 0,
                            ProducerReportedTotalTonnage = 0
                        },
                        new CalcResultCommsCostCommsCostByMaterial
                        {
                            Name = "Total",
                            CommsCostByMaterialPricePerTonne = 0.51m,
                            ProducerReportedHouseholdPackagingWasteTonnage = 0,
                            LateReportingTonnage = 0,
                            ReportedPublicBinTonnage = 0,
                            ProducerReportedTotalTonnage = 0
                        }
                    },
                    CalcResultCommsCostOnePlusFourApportionment = new List<CalcResultCommsCostOnePlusFourApportionment>
                    {
                        new CalcResultCommsCostCommsCostByMaterial { Name = CalcResultCommsCostBuilder.TwoBCommsCostUkWide, England = 10, Wales = 20, Scotland = 30, NorthernIreland = 40, Total = 100, ProducerReportedHouseholdPackagingWasteTonnage = 50, ReportedPublicBinTonnage = 60, HouseholdDrinksContainers = 70, LateReportingTonnage = 80, ProducerReportedTotalTonnage = 90, CommsCostByMaterialPricePerTonne = 100 },
                        new CalcResultCommsCostOnePlusFourApportionment { Name = CalcResultCommsCostBuilder.OnePlusFourApportionment, England = 10, Wales = 20, Scotland = 30, NorthernIreland = 40, Total = 100 },
                        new CalcResultCommsCostOnePlusFourApportionment { Name = CalcResultCommsCostBuilder.TwoCCommsCostByCountry, England = 10, Wales = 20, Scotland = 30, NorthernIreland = 40, Total = 100 }
                    },
                    CommsCostByCountry = new List<CalcResultCommsCostOnePlusFourApportionment>
                    {
                        new CalcResultCommsCostOnePlusFourApportionment { Name = CalcResultCommsCostBuilder.TwoBCommsCostUkWide, England = 10, Wales = 20, Scotland = 30, NorthernIreland = 40, Total = 100 },
                        new CalcResultCommsCostOnePlusFourApportionment { Name = CalcResultCommsCostBuilder.TwoCCommsCostByCountry, England = 10, Wales = 20, Scotland = 30, NorthernIreland = 40, Total = 100 }
                    }
                },
                CalcResultLaDisposalCostData = new CalcResultLaDisposalCostData
                {
                    CalcResultLaDisposalCostDetails = new List<CalcResultLaDisposalCostDataDetail>
                    {
                        new CalcResultLaDisposalCostDataDetail
                        {
                            Name = "ScotlandTest",
                            DisposalCostPricePerTonne = 20,
                            England = 0,
                            Wales = 0,
                            Scotland = 0,
                            NorthernIreland = 0,
                            ProducerReportedHouseholdPackagingWasteTonnage = 0,
                            ReportedPublicBinTonnage = 0,
                            Total = 0
                        },
                        new CalcResultLaDisposalCostDataDetail
                        {
                            Name = "Material1",
                            DisposalCostPricePerTonne = 20,
                            England = 0,
                            Wales = 0,
                            Scotland = 0,
                            NorthernIreland = 0,
                            ProducerReportedHouseholdPackagingWasteTonnage = 0,
                            ReportedPublicBinTonnage = 0,
                            Total = 0
                        },
                        new CalcResultLaDisposalCostDataDetail
                        {
                            Name = "Material2",
                            DisposalCostPricePerTonne = 10,
                            England = 0,
                            Wales = 0,
                            Scotland = 0,
                            NorthernIreland = 0,
                            ProducerReportedHouseholdPackagingWasteTonnage = 0,
                            ReportedPublicBinTonnage = 0,
                            Total = 100
                        },
                    }
                },
                CalcResultScaledupProducers = TestDataHelper.GetScaledupProducers(),
                CalcResultSummary = TestDataHelper.GetCalcResultSummary(),
                CalcResultPartialObligations = new CalcResultPartialObligations(),
                CalcResultProjectedProducers = new CalcResultProjectedProducers(),
            };
        }
    }
}
