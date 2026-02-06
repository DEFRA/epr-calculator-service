namespace EPR.Calculator.Service.Function.UnitTests.Models.JsonExporter
{
    using System.Collections.Generic;
    using EPR.Calculator.API.Data;
    using EPR.Calculator.Service.Function.Builder.CommsCost;
    using EPR.Calculator.Service.Function.Builder.Lapcap;
    using EPR.Calculator.Service.Function.Models;

    using EPR.Calculator.Service.Function.Models.JsonExporter;
    using EPR.Calculator.Service.Function.UnitTests.Builder;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class BillingFileJsonTests
    {
        [TestMethod]
        public void From_MapFieldsCorrectly()
        {
            var calcResult = CreateCalcResult();
            var materials = TestDataHelper.GetMaterials();
            var lapcapObj = new { Name = "lapcap" } as object;
            var ukWide = new { Uk = 1 } as object;
            var byCountry = new { Country = 2 } as object;
            var calculationResults = new { Results = "x" } as object;
            var acceptedProducerIds = new List<int> { 1, 2 };

            var result = BillingFileJson.From(
                calcResult,
                lapcapObj,
                ukWide,
                byCountry,
                calculationResults,
                acceptedProducerIds,
                materials);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.CalcResultDetail);
            Assert.AreEqual(1, result.CalcResultDetail!.RunId);
            Assert.AreEqual(lapcapObj, result.CalcResultLapcapData);
            Assert.AreEqual(ukWide, result.CalcResult2bCommsDataByUkWide);
            Assert.AreEqual(byCountry, result.CalcResult2cCommsDataByCountry);
            var ladetails = result.CalcResultLaDisposalCostData!.CalcResultLaDisposalCostDetails.ToList();
            Assert.IsTrue(ladetails.Any(d => d.DisposalCostPricePerTonne == "20"));
            Assert.AreEqual(calculationResults, result.CalculationResults);
            Assert.IsNotNull(result.CalcResult2aCommsDataByMaterial);
            var comms = result.CalcResult2aCommsDataByMaterial!.CalcResult2aCommsDataDetails;
            Assert.IsTrue(comms.Any(d => d.MaterialName == "Aluminium"));
            var aluminium = comms.Single(d => d.MaterialName == "Aluminium");
            Assert.AreEqual("£0.4200", aluminium.CommsCostByMaterialPricePerTonne);
            Assert.IsNotNull(result.ScaleUpProducers!.ProducerSubmissions);
            var subs = result.ScaleUpProducers.ProducerSubmissions!.ToList();
            Assert.IsTrue(subs.Count >= 1);
            Assert.AreEqual(1, subs[0].ProducerId);
        }

        private EPR.Calculator.Service.Function.Models.CalcResult CreateCalcResult()
        {
            return new EPR.Calculator.Service.Function.Models.CalcResult
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
                            CommsCostByMaterialPricePerTonneValue = 0.42m,
                            Name = "Aluminium",
                        },
                        new CalcResultCommsCostCommsCostByMaterial
                        {
                            CommsCostByMaterialPricePerTonne = "0.3",
                            CommsCostByMaterialPricePerTonneValue = 0.3m,
                            Name = "Glass",
                        },
                        new CalcResultCommsCostCommsCostByMaterial
                        {
                            CommsCostByMaterialPricePerTonne = "0.51",
                            CommsCostByMaterialPricePerTonneValue = 0.51m,
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
                CalcResultSummary = TestDataHelper.GetCalcResultSummary(),
                CalcResultPartialObligations = new CalcResultPartialObligations(),
            };
        }
    }
}
