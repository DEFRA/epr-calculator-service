namespace EPR.Calculator.Service.Function.UnitTests.Builder
{
    using EPR.Calculator.API.Data.DataModels;
    using EPR.Calculator.Service.Function.Constants;
    using EPR.Calculator.Service.Function.Models;

    public static class TestDataHelper
    {
        public static CalcResult GetCalcResult()
        {
            return new CalcResult
            {
                CalcResultScaledupProducers = GetScaledupProducers(),
                CalcResultParameterOtherCost = GetCalcResultParameterOtherCost(),
                CalcResultDetail = GetCalcResultDetail(),
                CalcResultLapcapData = GetCalcResultLapcapData(),
                CalcResultLateReportingTonnageData = GetCalcResultLateReportingTonnage(),
                CalcResultSummary = GetCalcResultSummary()
            };
        }

        public static CalcResultParameterOtherCost GetCalcResultParameterOtherCost()
        {
            return new CalcResultParameterOtherCost
            {
                Name = "Parameters - Other",
                SaOperatingCost = new List<CalcResultParameterOtherCostDetail>
                {
                    new CalcResultParameterOtherCostDetail
                    {
                        Name = null,
                        England = "England",
                        Wales = "Wales",
                        Scotland = "Scotland",
                        NorthernIreland = "Northern Ireland",
                        Total = "Total",
                        EnglandValue = 0,
                        WalesValue = 0,
                        ScotlandValue = 0,
                        NorthernIrelandValue = 0,
                        TotalValue = 0,
                        OrderId = 0,
                    },
                    new CalcResultParameterOtherCostDetail
                    {
                        Name = "3 SA Operating Costs",
                        England = "£25,000.00",
                        Wales = "£14,000.00",
                        Scotland = "£17,000.00",
                        NorthernIreland = "£9,000.00",
                        Total = "£65,000.00",
                        EnglandValue = 25000,
                        WalesValue = 14000,
                        ScotlandValue = 17000,
                        NorthernIrelandValue = 9000,
                        TotalValue = 65000,
                        OrderId = 2,
                    },
                },
                Details = new List<CalcResultParameterOtherCostDetail>
                {
                    new CalcResultParameterOtherCostDetail
                    {
                        Name = "4 LA Data Prep Charge",
                        England = "£16,000.00",
                        Wales = "£7,000.00",
                        Scotland = "£9,000.00",
                        NorthernIreland = "£4,500.00",
                        Total = "£36,500.00",
                        EnglandValue = 16000,
                        WalesValue = 7000,
                        ScotlandValue = 9000,
                        NorthernIrelandValue = 4500,
                        TotalValue = 36500,
                        OrderId = 1,
                    },
                    new CalcResultParameterOtherCostDetail
                    {
                        Name = "4 Country Apportionment %s",
                        England = "43.83561644%",
                        Wales = "19.17808219%",
                        Scotland = "24.65753425%",
                        NorthernIreland = "12.32876712%",
                        Total = "100.00000000%",
                        EnglandValue = 43.83561643835616m,
                        WalesValue = 19.17808219178082m,
                        ScotlandValue = 24.65753424657534m,
                        NorthernIrelandValue = 12.32876712328767m,
                        TotalValue = 100,
                        OrderId = 2,
                    },
                },
                SchemeSetupCost =
                {
                    Name = "5 Scheme set up cost Yearly Cost",
                    England = "£17,500.00",
                    Wales = "£23,400.00",
                    Scotland = "£12,400.00",
                    NorthernIreland = "£9,450.00",
                    Total = "£62,750.00",
                    EnglandValue = 17500,
                    WalesValue = 23400,
                    ScotlandValue = 12400,
                    NorthernIrelandValue = 9450,
                    TotalValue = 62750,
                    OrderId = 1,
                },
                BadDebtProvision = new KeyValuePair<string, string>("6 Bad Debt Provision", "6.00%"),
                Materiality = new List<CalcResultMateriality>
                {
                    new CalcResultMateriality
                    {
                        SevenMateriality = "7 Materiality",
                        Amount = "Amount £s",
                        Percentage = "%",
                        AmountValue = 0,
                        PercentageValue = 0,
                    },
                    new CalcResultMateriality
                    {
                        SevenMateriality = "Increase",
                        Amount = "£5,000.00",
                        Percentage = "2.00%",
                        AmountValue = 5000,
                        PercentageValue = 2,
                    },
                    new CalcResultMateriality
                    {
                        SevenMateriality = "Decrease",
                        Amount = "-£1,000.00",
                        Percentage = "-1.00%",
                        AmountValue = -1000,
                        PercentageValue = -1,
                    },
                    new CalcResultMateriality
                    {
                        SevenMateriality = "8 Tonnage Change",
                        Amount = "Amount £s",
                        Percentage = "%",
                        AmountValue = 0,
                        PercentageValue = 0,
                    },
                    new CalcResultMateriality
                    {
                        SevenMateriality = "Increase",
                        Amount = "£50.00",
                        Percentage = "2.00%",
                        AmountValue = 50,
                        PercentageValue = 2,
                    },
                    new CalcResultMateriality
                    {
                        SevenMateriality = "Decrease",
                        Amount = "-£10.00",
                        Percentage = "-0.50%",
                        AmountValue = -10,
                        PercentageValue = -0.5m,
                    },
                },
                BadDebtValue = 6,
            };
        }

        public static CalcResultDetail GetCalcResultDetail()
        {
            return new CalcResultDetail() { };
        }

        public static CalcResultLaDisposalCostData GetCalcResultLaDisposalCostData()
        {
            return new CalcResultLaDisposalCostData()
            {
                Name = "Disposal Cost Data",
                CalcResultLaDisposalCostDetails = new List<CalcResultLaDisposalCostDataDetail>()
                {
                    new CalcResultLaDisposalCostDataDetail()
                    {
                        Name = "Material",
                        Material = null,
                        England = "England",
                        Wales = "Wales",
                        Scotland = "Scotland",
                        NorthernIreland = "Northern Ireland",
                        Total = "Total",
                        ProducerReportedHouseholdPackagingWasteTonnage = "Producer Reported Household Packaging Tonnage",
                        ReportedPublicBinTonnage = "Reported Public Bin Tonnage",
                        LateReportingTonnage = "Late Reporting Tonnage",
                        ProducerReportedTotalTonnage = "Producer Reported Household Tonnage + Late Reporting Tonnage",
                        DisposalCostPricePerTonne = "Disposal Cost Price Per Tonne",
                        OrderId = 1,
                    },
                    new CalcResultLaDisposalCostDataDetail()
                    {
                        Name = "Aluminium",
                        Material = null,
                        England = "£5,000.00",
                        Wales = "£1,750.00",
                        Scotland = "£2,000.00",
                        NorthernIreland = "£1,250.00",
                        Total = "£10,000.00",
                        ProducerReportedHouseholdPackagingWasteTonnage = "6980.000",
                        ReportedPublicBinTonnage = "2000.000",
                        LateReportingTonnage = "8000.000",
                        ProducerReportedTotalTonnage = "14980.000",
                        DisposalCostPricePerTonne = "£0.6676",
                        OrderId = 2,
                    },
                    new CalcResultLaDisposalCostDataDetail()
                    {
                        Name = "Fibre composite",
                        Material = null,
                        England = "£7,500.00",
                        Wales = "£2,100.00",
                        Scotland = "£3,400.00",
                        NorthernIreland = "£1,750.00",
                        Total = "£14,750.00",
                        ProducerReportedHouseholdPackagingWasteTonnage = "11850.000",
                        ReportedPublicBinTonnage = "2000.000",
                        LateReportingTonnage = "7000.000",
                        ProducerReportedTotalTonnage = "18850.000",
                        DisposalCostPricePerTonne = "£0.7825",
                        OrderId = 3,
                    },
                    new CalcResultLaDisposalCostDataDetail()
                    {
                        Name = "Glass",
                        Material = null,
                        England = "£45,000.00",
                        Wales = "£0.00",
                        Scotland = "£20,700.00",
                        NorthernIreland = "£4,500.00",
                        Total = "£70,200.00",
                        ProducerReportedHouseholdPackagingWasteTonnage = "4900.000",
                        ReportedPublicBinTonnage = "2000.000",
                        LateReportingTonnage = "6000.000",
                        ProducerReportedTotalTonnage = "10900.000",
                        DisposalCostPricePerTonne = "£6.4404",
                        OrderId = 4,
                    },
                    new CalcResultLaDisposalCostDataDetail()
                    {
                        Name = "Paper or card",
                        Material = null,
                        England = "£12,500.00",
                        Wales = "£2,300.00",
                        Scotland = "£4,500.00",
                        NorthernIreland = "£3,400.00",
                        Total = "£22,700.00",
                        ProducerReportedHouseholdPackagingWasteTonnage = "4270.000",
                        ReportedPublicBinTonnage = "2000.000",
                        LateReportingTonnage = "5000.000",
                        ProducerReportedTotalTonnage = "9270.000",
                        DisposalCostPricePerTonne = "£2.4488",
                        OrderId = 5,
                    },
                    new CalcResultLaDisposalCostDataDetail()
                    {
                        Name = "Plastic",
                        Material = null,
                        England = "£23,000.00",
                        Wales = "£4,500.00",
                        Scotland = "£6,700.00",
                        NorthernIreland = "£2,100.00",
                        Total = "£36,300.00",
                        ProducerReportedHouseholdPackagingWasteTonnage = "12805.000",
                        ReportedPublicBinTonnage = "2000.000",
                        LateReportingTonnage = "4000.000",
                        ProducerReportedTotalTonnage = "16805.000",
                        DisposalCostPricePerTonne = "£2.1601",
                        OrderId = 6,
                    },
                    new CalcResultLaDisposalCostDataDetail()
                    {
                        Name = "Steel",
                        Material = null,
                        England = "£13,400.00",
                        Wales = "£0.00",
                        Scotland = "£7,800.00",
                        NorthernIreland = "£0.00",
                        Total = "£21,200.00",
                        ProducerReportedHouseholdPackagingWasteTonnage = "7700.000",
                        ReportedPublicBinTonnage = "2000.000",
                        LateReportingTonnage = "3000.000",
                        ProducerReportedTotalTonnage = "10700.000",
                        DisposalCostPricePerTonne = "£1.9813",
                        OrderId = 7,
                    },
                    new CalcResultLaDisposalCostDataDetail()
                    {
                        Name = "Wood",
                        Material = null,
                        England = "£0.00",
                        Wales = "£12,000.00",
                        Scotland = "£0.00",
                        NorthernIreland = "£5,600.00",
                        Total = "£17,600.00",
                        ProducerReportedHouseholdPackagingWasteTonnage = "6800.000",
                        ReportedPublicBinTonnage = "2000.000",
                        LateReportingTonnage = "2000.000",
                        ProducerReportedTotalTonnage = "8800.000",
                        DisposalCostPricePerTonne = "£2.0000",
                        OrderId = 8,
                    },
                    new CalcResultLaDisposalCostDataDetail()
                    {
                        Name = "Other materials",
                        Material = null,
                        England = "£3,400.00",
                        Wales = "£2,100.00",
                        Scotland = "£4,200.00",
                        NorthernIreland = "£700.00",
                        Total = "£10,400.00",
                        ProducerReportedHouseholdPackagingWasteTonnage = "7700.000",
                        ReportedPublicBinTonnage = "2000.000",
                        LateReportingTonnage = "1000.000",
                        ProducerReportedTotalTonnage = "8700.000",
                        DisposalCostPricePerTonne = "£1.1954",
                        OrderId = 9,
                    },
                    new CalcResultLaDisposalCostDataDetail()
                    {
                        Name = "Total",
                        Material = null,
                        England = "£109,800.00",
                        Wales = "£24,750.00",
                        Scotland = "£49,300.00",
                        NorthernIreland = "£19,300.00",
                        Total = "£203,150.00",
                        ProducerReportedHouseholdPackagingWasteTonnage = "63005.000",
                        ReportedPublicBinTonnage = "2000.000",
                        LateReportingTonnage = "36000.000",
                        ProducerReportedTotalTonnage = "99005.000",
                        DisposalCostPricePerTonne = null,
                        OrderId = 10,
                    },
                },
            };
        }

        public static CalcResultLapcapData GetCalcResultLapcapData()
        {
            return new CalcResultLapcapData()
            {
                Name = "LAPCAP Data",
                CalcResultLapcapDataDetails = new List<CalcResultLapcapDataDetails>()
                {
                    new CalcResultLapcapDataDetails
                    {
                        Name = "Material",
                        EnglandDisposalCost = "England LA Disposal Cost",
                        WalesDisposalCost = "Wales LA Disposal Cost",
                        ScotlandDisposalCost = "Scotland LA Disposal Cost",
                        NorthernIrelandDisposalCost = "Northern Ireland LA Disposal Cost",
                        TotalDisposalCost = "1 LA Disposal Cost Total",
                        EnglandCost = 0,
                        WalesCost = 0,
                        ScotlandCost = 0,
                        NorthernIrelandCost = 0,
                        TotalCost = 0,
                        OrderId = 1,
                    },
                    new CalcResultLapcapDataDetails
                    {
                        Name = "Aluminium",
                        EnglandDisposalCost = "£5,000.00",
                        WalesDisposalCost = "£1,750.00",
                        ScotlandDisposalCost = "£2,000.00",
                        NorthernIrelandDisposalCost = "£1,250.00",
                        TotalDisposalCost = "£10,000.00",
                        EnglandCost = 5000,
                        WalesCost = 1750,
                        ScotlandCost = 2000,
                        NorthernIrelandCost = 1250,
                        TotalCost = 10000,
                        OrderId = 2,
                    },
                    new CalcResultLapcapDataDetails
                    {
                        Name = "Fibre composite",
                        EnglandDisposalCost = "£7,500.00",
                        WalesDisposalCost = "£2,100.00",
                        ScotlandDisposalCost = "£3,400.00",
                        NorthernIrelandDisposalCost = "£1,750.00",
                        TotalDisposalCost = "£14,750.00",
                        EnglandCost = 7500,
                        WalesCost = 2100,
                        ScotlandCost = 3400,
                        NorthernIrelandCost = 1750,
                        TotalCost = 14750,
                        OrderId = 3,
                    },
                    new CalcResultLapcapDataDetails
                    {
                        Name = "Glass",
                        EnglandDisposalCost = "£45,000.00",
                        WalesDisposalCost = "£0.00",
                        ScotlandDisposalCost = "£20,700.00",
                        NorthernIrelandDisposalCost = "£4,500.00",
                        TotalDisposalCost = "£70,200.00",
                        EnglandCost = 45000,
                        WalesCost = 0,
                        ScotlandCost = 20700,
                        NorthernIrelandCost = 4500,
                        TotalCost = 70200,
                        OrderId = 4,
                    },
                    new CalcResultLapcapDataDetails
                    {
                        Name = "Paper or card",
                        EnglandDisposalCost = "£12,500.00",
                        WalesDisposalCost = "£2,300.00",
                        ScotlandDisposalCost = "£4,500.00",
                        NorthernIrelandDisposalCost = "£3,400.00",
                        TotalDisposalCost = "£22,700.00",
                        EnglandCost = 12500,
                        WalesCost = 2300,
                        ScotlandCost = 4500,
                        NorthernIrelandCost = 3400,
                        TotalCost = 22700,
                        OrderId = 5,
                    },
                    new CalcResultLapcapDataDetails
                    {
                        Name = "Plastic",
                        EnglandDisposalCost = "£23,000.00",
                        WalesDisposalCost = "£4,500.00",
                        ScotlandDisposalCost = "£6,700.00",
                        NorthernIrelandDisposalCost = "£2,100.00",
                        TotalDisposalCost = "£36,300.00",
                        EnglandCost = 23000,
                        WalesCost = 4500,
                        ScotlandCost = 6700,
                        NorthernIrelandCost = 2100,
                        TotalCost = 36300,
                        OrderId = 6,
                    },
                    new CalcResultLapcapDataDetails
                    {
                        Name = "Steel",
                        EnglandDisposalCost = "£13,400.00",
                        WalesDisposalCost = "£0.00",
                        ScotlandDisposalCost = "£7,800.00",
                        NorthernIrelandDisposalCost = "£0.00",
                        TotalDisposalCost = "£21,200.00",
                        EnglandCost = 13400,
                        WalesCost = 0,
                        ScotlandCost = 7800,
                        NorthernIrelandCost = 0,
                        TotalCost = 21200,
                        OrderId = 7,
                    },
                    new CalcResultLapcapDataDetails
                    {
                        Name = "Wood",
                        EnglandDisposalCost = "£0.00",
                        WalesDisposalCost = "£12,000.00",
                        ScotlandDisposalCost = "£0.00",
                        NorthernIrelandDisposalCost = "£5,600.00",
                        TotalDisposalCost = "£17,600.00",
                        EnglandCost = 0,
                        WalesCost = 12000,
                        ScotlandCost = 0,
                        NorthernIrelandCost = 5600,
                        TotalCost = 17600,
                        OrderId = 8,
                    },
                    new CalcResultLapcapDataDetails
                    {
                        Name = "Other materials",
                        EnglandDisposalCost = "£3,400.00",
                        WalesDisposalCost = "£2,100.00",
                        ScotlandDisposalCost = "£4,200.00",
                        NorthernIrelandDisposalCost = "£700.00",
                        TotalDisposalCost = "£10,400.00",
                        EnglandCost = 3400,
                        WalesCost = 2100,
                        ScotlandCost = 4200,
                        NorthernIrelandCost = 700,
                        TotalCost = 10400,
                        OrderId = 9,
                    },
                    new CalcResultLapcapDataDetails
                    {
                        Name = "Total",
                        EnglandDisposalCost = "£109,800.00",
                        WalesDisposalCost = "£24,750.00",
                        ScotlandDisposalCost = "£49,300.00",
                        NorthernIrelandDisposalCost = "£19,300.00",
                        TotalDisposalCost = "£203,150.00",
                        EnglandCost = 109800,
                        WalesCost = 24750,
                        ScotlandCost = 49300,
                        NorthernIrelandCost = 19300,
                        TotalCost = 203150,
                        OrderId = 10,
                    },
                    new CalcResultLapcapDataDetails
                    {
                        Name = "1 Country Apportionment %s",
                        EnglandDisposalCost = "54.04873246%",
                        WalesDisposalCost = "12.18311592%",
                        ScotlandDisposalCost = "24.26778243%",
                        NorthernIrelandDisposalCost = "9.50036919%",
                        TotalDisposalCost = "100.00000000%",
                        EnglandCost = 54.04873246369677m,
                        WalesCost = 12.183115924193945m,
                        ScotlandCost = 24.267782426778243m,
                        NorthernIrelandCost = 9.500369185331037m,
                        TotalCost = 100,
                        OrderId = 11,
                    },
                },
            };
        }

        public static CalcResultOnePlusFourApportionment GetCalcResultOnePlusFourApportionment()
        {
            return new CalcResultOnePlusFourApportionment()
            {
                Name = "One Plus Four Apportionment",
                CalcResultOnePlusFourApportionmentDetails = new List<CalcResultOnePlusFourApportionmentDetail>
                {
                    new CalcResultOnePlusFourApportionmentDetail()
                    {
                        EnglandDisposalTotal = "80",
                        NorthernIrelandDisposalTotal = "70",
                        ScotlandDisposalTotal = "30",
                        WalesDisposalTotal = "20",
                        AllTotal = 0.1M,
                        EnglandTotal = 0.10M,
                        NorthernIrelandTotal = 0.15M,
                        ScotlandTotal = 0.15M,
                        WalesTotal = 020M,
                        Name = "Test",
                    },
                    new CalcResultOnePlusFourApportionmentDetail()
                    {
                        EnglandDisposalTotal = "80",
                        NorthernIrelandDisposalTotal = "70",
                        ScotlandDisposalTotal = "30",
                        WalesDisposalTotal = "20",
                        AllTotal = 0.1M,
                        EnglandTotal = 0.10M,
                        NorthernIrelandTotal = 0.15M,
                        ScotlandTotal = 0.15M,
                        WalesTotal = 020M,
                        Name = "Test",
                    },
                    new CalcResultOnePlusFourApportionmentDetail()
                    {
                        EnglandDisposalTotal = "80",
                        NorthernIrelandDisposalTotal = "70",
                        ScotlandDisposalTotal = "30",
                        WalesDisposalTotal = "20",
                        AllTotal = 0.1M,
                        EnglandTotal = 0.10M,
                        NorthernIrelandTotal = 0.15M,
                        ScotlandTotal = 0.15M,
                        WalesTotal = 020M,
                        Name = "Test",
                    },
                    new CalcResultOnePlusFourApportionmentDetail()
                    {
                        EnglandDisposalTotal = "80",
                        NorthernIrelandDisposalTotal = "70",
                        ScotlandDisposalTotal = "30",
                        WalesDisposalTotal = "20",
                        AllTotal = 0.1M,
                        EnglandTotal = 14.53M,
                        NorthernIrelandTotal = 0.15M,
                        ScotlandTotal = 0.15M,
                        WalesTotal = 020M,
                        Name = "Test",
                    },
                    new CalcResultOnePlusFourApportionmentDetail()
                    {
                        EnglandDisposalTotal = "80",
                        NorthernIrelandDisposalTotal = "70",
                        ScotlandDisposalTotal = "30",
                        WalesDisposalTotal = "20",
                        AllTotal = 0.1M,
                        EnglandTotal = 14.53M,
                        NorthernIrelandTotal = 0.15M,
                        ScotlandTotal = 0.15M,
                        WalesTotal = 020M,
                        Name = OnePlus4ApportionmentColumnHeaders.OnePluseFourApportionment,
                    },
                },
            };
        }

        public static CalcResultCommsCost GetCalcResultCommsCostReportDetail()
        {
            return new CalcResultCommsCost()
            {
                CalcResultCommsCostCommsCostByMaterial =
                [
                    new ()
                    {
                        CommsCostByMaterialPricePerTonne = "0.42",
                        Name = "Material1",
                    },
                    new ()
                    {
                        CommsCostByMaterialPricePerTonne = "0.3",
                        Name = "Material2",
                    },
                ],
                CommsCostByCountry =
                [
                    new ()
                    {
                        Total = "Total",
                    },
                    new ()
                    {
                        TotalValue = 2530,
                    },
                ],
            };
        }

        public static CalcResultLateReportingTonnage GetCalcResultLateReportingTonnage()
        {
            return new CalcResultLateReportingTonnage
            {
                Name = "Late Reporting Tonnage",
                CalcResultLateReportingTonnageDetails = new[]
                {
                    new CalcResultLateReportingTonnageDetail()
                    {
                        Name = "Aluminium",
                        TotalLateReportingTonnage = 8000.00m,
                    },
                    new CalcResultLateReportingTonnageDetail()
                    {
                        Name = "Plastic",
                        TotalLateReportingTonnage = 2000.00m,
                    },
                    new CalcResultLateReportingTonnageDetail()
                    {
                        Name = "Total",
                        TotalLateReportingTonnage = 10000.00m,
                    },
                },
            };
        }

        public static CalcResultSummary GetCalcResultSummary()
        {
            return new CalcResultSummary
            {
                TotalFeeforLADisposalCostswoBadDebtprovision1 = 4423.39438M,
                BadDebtProvisionFor1 = 6021.3677166M,
                TotalFeeforLADisposalCostswithBadDebtprovision1 = 4688.7980428M,
                TotalFeeforCommsCostsbyMaterialwoBadDebtProvision2A = 1290.778M,
                BadDebtProvisionFor2A = 2098.887360M,
                TotalFeeforCommsCostsbyMaterialwithBadDebtprovision2A = 1368.22468M,
                SaOperatingCostsWoTitleSection3 = 3077.2243678810364M,
                BadDebtProvisionTitleSection3 = 3900.000000M,
                SaOperatingCostsWithTitleSection3 = 3261.8578299538985M,
                LaDataPrepCostsTitleSection4 = 1727.9798373485821M,
                LaDataPrepCostsBadDebtProvisionTitleSection4 = 103.67879024091492M,
                LaDataPrepCostsWithBadDebtProvisionTitleSection4 = 1831.658627589497M,
                SaSetupCostsTitleSection5 = 17500.00M,
                SaSetupCostsBadDebtProvisionTitleSection5 = 1050.00M,
                SaSetupCostsWithBadDebtProvisionTitleSection5 = 18550.00M,
                TwoCCommsCostsByCountryWithoutBadDebtProvision = 1339.100071422903M,
                TwoCBadDebtProvision = 80.34600428537418M,
                TwoCCommsCostsByCountryWithBadDebtProvision = 1419.446075708277M,
                CommsCostHeaderWithoutBadDebtFor2bTitle = 1339.100071422903M,
                CommsCostHeaderWithBadDebtFor2bTitle = 1419.446075708277M,
                CommsCostHeaderBadDebtProvisionFor2bTitle = 80.34600428537418M,
                TotalOnePlus2A2B2CFeeWithBadDebtProvision = 10230.2550766M,
                ProducerDisposalFees = GetProducerDisposalFees(),
            };
        }

        public static List<CalcResultSummaryProducerDisposalFees> GetProducerDisposalFees()
        {
            return new List<CalcResultSummaryProducerDisposalFees>()
            {
                new CalcResultSummaryProducerDisposalFees()
                {
                    ProducerId = "1",
                    ProducerIdInt = 1,
                    SubsidiaryId = string.Empty,
                    ProducerName = "Allied Packaging",
                    Level = "1",
                    isTotalRow = false,
                    TotalProducerDisposalFee = 4423.39438m,
                    BadDebtProvision = 265.4036628m,
                    TotalProducerDisposalFeeWithBadDebtProvision = 4688.7980428m,
                    EnglandTotal = 2534.2359097426884m,
                    WalesTotal = 571.2417008090152m,
                    ScotlandTotal = 1137.8673076088023m,
                    NorthernIrelandTotal = 445.4531246394942m,
                    TotalProducerCommsFee = 1290.778m,
                    BadDebtProvisionComms = 77.44668m,
                    TotalProducerCommsFeeWithBadDebtProvision = 1368.22468m,
                    EnglandTotalComms = 718.2251815154783m,
                    WalesTotalComms = 181.2690740598454m,
                    ScotlandTotalComms = 332.8499847265775m,
                    NorthernIrelandTotalComms = 135.88043969809883m,
                    LocalAuthorityDisposalCostsSectionOne = new CalcResultSummaryBadDebtProvision
                    {
                        TotalProducerFeeWithoutBadDebtProvision = 4423.39438m,
                        BadDebtProvision = 265.4036628m,
                        TotalProducerFeeWithBadDebtProvision = 4688.7980428m,
                        EnglandTotalWithBadDebtProvision = 2534.2359097426884m,
                        WalesTotalWithBadDebtProvision = 571.2417008090152m,
                        ScotlandTotalWithBadDebtProvision = 1137.8673076088023m,
                        NorthernIrelandTotalWithBadDebtProvision = 445.4531246394942m
                    },
                    CommunicationCostsSectionTwoA = new CalcResultSummaryBadDebtProvision
                    {
                        TotalProducerFeeWithoutBadDebtProvision = 1290.778m,
                        BadDebtProvision = 77.44668m,
                        TotalProducerFeeWithBadDebtProvision = 1368.22468m,
                        EnglandTotalWithBadDebtProvision = 718.2251815154783m,
                        WalesTotalWithBadDebtProvision = 181.2690740598454m,
                        ScotlandTotalWithBadDebtProvision = 332.8499847265775m,
                        NorthernIrelandTotalWithBadDebtProvision = 135.88043969809883m
                    },
                    TwoCTotalProducerFeeForCommsCostsWithoutBadDebt = 1339.100071422903m,
                    TwoCBadDebtProvision = 80.34600428537418m,
                    TwoCTotalProducerFeeForCommsCostsWithBadDebt = 1419.446075708277m,
                    TwoCEnglandTotalWithBadDebt = 607.4748035870169m,
                    TwoCWalesTotalWithBadDebt = 300.7301007856519m,
                    TwoCScotlandTotalWithBadDebt = 360.87612094278234m,
                    TwoCNorthernIrelandTotalWithBadDebt = 150.36505039282596m,
                    PercentageofProducerReportedTonnagevsAllProducers = 5.6741528450123m,
                    ProducerTotalOnePlus2A2B2CWithBadDeptProvision = 10491.167766844124m,
                    ProducerOverallPercentageOfCostsForOnePlus2A2B2C = 4.7341913352015945m,
                    SchemeAdministratorOperatingCosts = new CalcResultSummaryBadDebtProvision
                    {
                        TotalProducerFeeWithoutBadDebtProvision = 3077.2243678810364m,
                        BadDebtProvision = 184.6334620728622m,
                        TotalProducerFeeWithBadDebtProvision = 3261.8578299538985m,
                        EnglandTotalWithBadDebtProvision = 1712.2541832180282m,
                        WalesTotalWithBadDebtProvision = 432.1468228710047m,
                        ScotlandTotalWithBadDebtProvision = 793.5168432560496m,
                        NorthernIrelandTotalWithBadDebtProvision = 323.93998060881614m
                    },
                    LocalAuthorityDataPreparationCosts = new CalcResultSummaryBadDebtProvision
                    {
                        TotalProducerFeeWithoutBadDebtProvision = 1727.9798373485821m,
                        BadDebtProvision = 103.67879024091492m,
                        TotalProducerFeeWithBadDebtProvision = 1831.658627589497m,
                        EnglandTotalWithBadDebtProvision = 802.9188504501905m,
                        WalesTotalWithBadDebtProvision = 351.2769970719583m,
                        ScotlandTotalWithBadDebtProvision = 451.6418533782321m,
                        NorthernIrelandTotalWithBadDebtProvision = 225.82092668911605m
                    },
                    OneOffSchemeAdministrationSetupCosts = new CalcResultSummaryBadDebtProvision
                    {
                        TotalProducerFeeWithoutBadDebtProvision = 2970.7050628390007m,
                        BadDebtProvision = 178.24230377034004m,
                        TotalProducerFeeWithBadDebtProvision = 3148.947366609341m,
                        EnglandTotalWithBadDebtProvision = 1652.983846106635m,
                        WalesTotalWithBadDebtProvision = 417.1878943870084m,
                        ScotlandTotalWithBadDebtProvision = 766.0489525279556m,
                        NorthernIrelandTotalWithBadDebtProvision = 312.72667358774174m
                    },
                    TotalProducerBillBreakdownCosts = new CalcResultSummaryBadDebtProvision
                    {
                        TotalProducerFeeWithoutBadDebtProvision = 9897.32808192842m,
                        BadDebtProvision = 593.8396849157051m,
                        TotalProducerFeeWithBadDebtProvision = 10491.167766844124m,
                        EnglandTotalWithBadDebtProvision = 5442.448434925617m,
                        WalesTotalWithBadDebtProvision = 1452.6428880194774m,
                        ScotlandTotalWithBadDebtProvision = 2564.98356493499m,
                        NorthernIrelandTotalWithBadDebtProvision = 1031.0928789640386m
                    },
                    BillingInstructionSection = new CalcResultSummaryBillingInstruction
                    {
                        CurrentYearInvoiceTotalToDate = 1250.89m,
                        TonnageChangeSinceLastInvoice = "Tonnage Changed",
                        LiabilityDifference = 580.73m,
                        MaterialThresholdBreached = string.Empty,
                        TonnageThresholdBreached = string.Empty,
                        PercentageLiabilityDifference = 123.45m,
                        MaterialPercentageThresholdBreached = string.Empty,
                        TonnagePercentageThresholdBreached = string.Empty,
                        SuggestedBillingInstruction = string.Empty,
                        SuggestedInvoiceAmount = "4039"
                    },
                    CommunicationCostsSectionTwoB = new CalcResultSummaryBadDebtProvision
                    {
                        TotalProducerFeeWithoutBadDebtProvision = 2844.0556305055156m,
                        BadDebtProvision = 170.64333783033092m,
                        TotalProducerFeeWithBadDebtProvision = 3014.6989683358465m,
                        EnglandTotalWithBadDebtProvision = 1582.5125400804336m,
                        WalesTotalWithBadDebtProvision = 399.4020123649648m,
                        ScotlandTotalWithBadDebtProvision = 733.3901516568284m,
                        NorthernIrelandTotalWithBadDebtProvision = 299.39426423361965m
                    },
                    ProducerDisposalFeesByMaterial = GetProducerDisposalFeesByMaterial(),
                    ProducerCommsFeesByMaterial = GetProducerCommsFeesByMaterial(),
                    TonnageChangeCount = "0",
                    TonnageChangeAdvice = "",
                },
            };
        }

        public static List<CalcResultSummaryProducerDisposalFees> GetProducerDisposalFeesForOverAllTotal()
        {
            return new List<CalcResultSummaryProducerDisposalFees>()
            {
                new CalcResultSummaryProducerDisposalFees()
                {
                    ProducerId = "1",
                    ProducerIdInt = 1,
                    SubsidiaryId = string.Empty,
                    ProducerName = "Allied Packaging",
                    Level = "1",
                    isTotalRow = false,
                    TotalProducerDisposalFee = 4423.39438m,
                    BadDebtProvision = 265.4036628m,
                    TotalProducerDisposalFeeWithBadDebtProvision = 4688.7980428m,
                    EnglandTotal = 2534.2359097426884m,
                    WalesTotal = 571.2417008090152m,
                    ScotlandTotal = 1137.8673076088023m,
                    NorthernIrelandTotal = 445.4531246394942m,
                    TotalProducerCommsFee = 1290.778m,
                    BadDebtProvisionComms = 77.44668m,
                    TotalProducerCommsFeeWithBadDebtProvision = 1368.22468m,
                    EnglandTotalComms = 718.2251815154783m,
                    WalesTotalComms = 181.2690740598454m,
                    ScotlandTotalComms = 332.8499847265775m,
                    NorthernIrelandTotalComms = 135.88043969809883m,
                    LocalAuthorityDisposalCostsSectionOne = new CalcResultSummaryBadDebtProvision
                    {
                        TotalProducerFeeWithoutBadDebtProvision = 4423.39438m,
                        BadDebtProvision = 265.4036628m,
                        TotalProducerFeeWithBadDebtProvision = 4688.7980428m,
                        EnglandTotalWithBadDebtProvision = 2534.2359097426884m,
                        WalesTotalWithBadDebtProvision = 571.2417008090152m,
                        ScotlandTotalWithBadDebtProvision = 1137.8673076088023m,
                        NorthernIrelandTotalWithBadDebtProvision = 445.4531246394942m
                    },
                    CommunicationCostsSectionTwoA = new CalcResultSummaryBadDebtProvision
                    {
                        TotalProducerFeeWithoutBadDebtProvision = 1290.778m,
                        BadDebtProvision = 77.44668m,
                        TotalProducerFeeWithBadDebtProvision = 1368.22468m,
                        EnglandTotalWithBadDebtProvision = 718.2251815154783m,
                        WalesTotalWithBadDebtProvision = 181.2690740598454m,
                        ScotlandTotalWithBadDebtProvision = 332.8499847265775m,
                        NorthernIrelandTotalWithBadDebtProvision = 135.88043969809883m
                    },
                    TwoCTotalProducerFeeForCommsCostsWithoutBadDebt = 1339.100071422903m,
                    TwoCBadDebtProvision = 80.34600428537418m,
                    TwoCTotalProducerFeeForCommsCostsWithBadDebt = 1419.446075708277m,
                    TwoCEnglandTotalWithBadDebt = 607.4748035870169m,
                    TwoCWalesTotalWithBadDebt = 300.7301007856519m,
                    TwoCScotlandTotalWithBadDebt = 360.87612094278234m,
                    TwoCNorthernIrelandTotalWithBadDebt = 150.36505039282596m,
                    PercentageofProducerReportedTonnagevsAllProducers = 5.6741528450123m,
                    ProducerTotalOnePlus2A2B2CWithBadDeptProvision = 10491.167766844124m,
                    ProducerOverallPercentageOfCostsForOnePlus2A2B2C = 4.7341913352015945m,
                    SchemeAdministratorOperatingCosts = new CalcResultSummaryBadDebtProvision
                    {
                        TotalProducerFeeWithoutBadDebtProvision = 3077.2243678810364m,
                        BadDebtProvision = 184.6334620728622m,
                        TotalProducerFeeWithBadDebtProvision = 3261.8578299538985m,
                        EnglandTotalWithBadDebtProvision = 1712.2541832180282m,
                        WalesTotalWithBadDebtProvision = 432.1468228710047m,
                        ScotlandTotalWithBadDebtProvision = 793.5168432560496m,
                        NorthernIrelandTotalWithBadDebtProvision = 323.93998060881614m
                    },
                    LocalAuthorityDataPreparationCosts = new CalcResultSummaryBadDebtProvision
                    {
                        TotalProducerFeeWithoutBadDebtProvision = 1727.9798373485821m,
                        BadDebtProvision = 103.67879024091492m,
                        TotalProducerFeeWithBadDebtProvision = 1831.658627589497m,
                        EnglandTotalWithBadDebtProvision = 802.9188504501905m,
                        WalesTotalWithBadDebtProvision = 351.2769970719583m,
                        ScotlandTotalWithBadDebtProvision = 451.6418533782321m,
                        NorthernIrelandTotalWithBadDebtProvision = 225.82092668911605m
                    },
                    OneOffSchemeAdministrationSetupCosts = new CalcResultSummaryBadDebtProvision
                    {
                        TotalProducerFeeWithoutBadDebtProvision = 2970.7050628390007m,
                        BadDebtProvision = 178.24230377034004m,
                        TotalProducerFeeWithBadDebtProvision = 3148.947366609341m,
                        EnglandTotalWithBadDebtProvision = 1652.983846106635m,
                        WalesTotalWithBadDebtProvision = 417.1878943870084m,
                        ScotlandTotalWithBadDebtProvision = 766.0489525279556m,
                        NorthernIrelandTotalWithBadDebtProvision = 312.72667358774174m
                    },
                    TotalProducerBillBreakdownCosts = new CalcResultSummaryBadDebtProvision
                    {
                        TotalProducerFeeWithoutBadDebtProvision = 9897.32808192842m,
                        BadDebtProvision = 593.8396849157051m,
                        TotalProducerFeeWithBadDebtProvision = 10491.167766844124m,
                        EnglandTotalWithBadDebtProvision = 5442.448434925617m,
                        WalesTotalWithBadDebtProvision = 1452.6428880194774m,
                        ScotlandTotalWithBadDebtProvision = 2564.98356493499m,
                        NorthernIrelandTotalWithBadDebtProvision = 1031.0928789640386m
                    },
                    BillingInstructionSection = new CalcResultSummaryBillingInstruction
                    {
                        CurrentYearInvoiceTotalToDate = null,
                        TonnageChangeSinceLastInvoice = null,
                        LiabilityDifference = 580.73m,
                        MaterialThresholdBreached = string.Empty,
                        TonnageThresholdBreached = string.Empty,
                        PercentageLiabilityDifference = null,
                        MaterialPercentageThresholdBreached = string.Empty,
                        TonnagePercentageThresholdBreached = string.Empty,
                        SuggestedBillingInstruction = string.Empty,
                        SuggestedInvoiceAmount = "4039"
                    },
                    CommunicationCostsSectionTwoB = new CalcResultSummaryBadDebtProvision
                    {
                        TotalProducerFeeWithoutBadDebtProvision = 2844.0556305055156m,
                        BadDebtProvision = 170.64333783033092m,
                        TotalProducerFeeWithBadDebtProvision = 3014.6989683358465m,
                        EnglandTotalWithBadDebtProvision = 1582.5125400804336m,
                        WalesTotalWithBadDebtProvision = 399.4020123649648m,
                        ScotlandTotalWithBadDebtProvision = 733.3901516568284m,
                        NorthernIrelandTotalWithBadDebtProvision = 299.39426423361965m
                    },
                    ProducerDisposalFeesByMaterial = GetProducerDisposalFeesByMaterial(),
                    ProducerCommsFeesByMaterial = GetProducerCommsFeesByMaterial(),
                    TonnageChangeCount = "0",
                    TonnageChangeAdvice = "",
                    isOverallTotalRow = true,
                },
            };
        }

        public static Dictionary<string, CalcResultSummaryProducerDisposalFeesByMaterial> GetProducerDisposalFeesByMaterial()
        {
            return new Dictionary<string, CalcResultSummaryProducerDisposalFeesByMaterial>
            {
                {
                   "AL",
                   new CalcResultSummaryProducerDisposalFeesByMaterial
                    {
                        HouseholdPackagingWasteTonnage = 1000,
                        ManagedConsumerWasteTonnage = 90,
                        NetReportedTonnage = 910,
                        PricePerTonne = 0.6676m,
                        ProducerDisposalFee = 607.52m,
                        BadDebtProvision = 36.45m,
                        ProducerDisposalFeeWithBadDebtProvision = 643.97m,
                        EnglandWithBadDebtProvision = 348.06m,
                        WalesWithBadDebtProvision = 78.46m,
                        ScotlandWithBadDebtProvision = 156.28m,
                        NorthernIrelandWithBadDebtProvision = 61.18m,
                        PreviousInvoicedTonnage = null,
                        TonnageChange = 0
                    }
                },
                {
                    "FC",
                    new CalcResultSummaryProducerDisposalFeesByMaterial
                    {
                        HouseholdPackagingWasteTonnage = 2000,
                        ManagedConsumerWasteTonnage = 140,
                        NetReportedTonnage = 1860,
                        PricePerTonne = 0.7825m,
                        ProducerDisposalFee = 1455.45m,
                        BadDebtProvision = 87.33m,
                        ProducerDisposalFeeWithBadDebtProvision = 1542.78m,
                        EnglandWithBadDebtProvision = 833.85m,
                        WalesWithBadDebtProvision = 187.96m,
                        ScotlandWithBadDebtProvision = 374.40m,
                        NorthernIrelandWithBadDebtProvision = 146.57m,
                        PreviousInvoicedTonnage = 0,
                        TonnageChange = 0
                    }
                },
                {
                   "GL",
                   new CalcResultSummaryProducerDisposalFeesByMaterial
                    {
                        HouseholdPackagingWasteTonnage = 500,
                        ManagedConsumerWasteTonnage = 150,
                        NetReportedTonnage = 350,
                        PricePerTonne = 6.4404m,
                        ProducerDisposalFee = 2254.14m,
                        BadDebtProvision = 135.25m,
                        ProducerDisposalFeeWithBadDebtProvision = 2389.39m,
                        EnglandWithBadDebtProvision = 1291.43m,
                        WalesWithBadDebtProvision = 291.10m,
                        ScotlandWithBadDebtProvision = 579.85m,
                        NorthernIrelandWithBadDebtProvision = 227,
                        HouseholdDrinksContainersTonnage = 220,
                        PreviousInvoicedTonnage = 0,
                        TonnageChange = 0
                    }
                },
                {
                    "PC",
                    new CalcResultSummaryProducerDisposalFeesByMaterial
                    {
                        HouseholdPackagingWasteTonnage = 20,
                        ManagedConsumerWasteTonnage = 2.200m,
                        NetReportedTonnage = 17.800m,
                        PricePerTonne = 2.4488m,
                        ProducerDisposalFee = 43.59m,
                        BadDebtProvision = 2.62m,
                        ProducerDisposalFeeWithBadDebtProvision = 46.20m,
                        EnglandWithBadDebtProvision = 24.97m,
                        WalesWithBadDebtProvision = 5.63m,
                        ScotlandWithBadDebtProvision = 11.21m,
                        NorthernIrelandWithBadDebtProvision = 4.39m,
                        PreviousInvoicedTonnage = 0,
                        TonnageChange = 0
                    }
                },
                {
                    "PL",
                    new CalcResultSummaryProducerDisposalFeesByMaterial
                    {
                        HouseholdPackagingWasteTonnage = 5.000m,
                        ManagedConsumerWasteTonnage = 0.600m,
                        NetReportedTonnage = 4.400m,
                        PricePerTonne = 2.1601m,
                        ProducerDisposalFee = 9.50m,
                        BadDebtProvision = 0.57m,
                        ProducerDisposalFeeWithBadDebtProvision = 10.07m,
                        EnglandWithBadDebtProvision = 5.45m,
                        WalesWithBadDebtProvision = 1.23m,
                        ScotlandWithBadDebtProvision = 2.44m,
                        NorthernIrelandWithBadDebtProvision = 0.96m,
                        PreviousInvoicedTonnage = 0,
                        TonnageChange = 0
                    }
                },
                {
                    "ST",
                    new CalcResultSummaryProducerDisposalFeesByMaterial
                    {
                        HouseholdPackagingWasteTonnage = 0.000m,
                        ManagedConsumerWasteTonnage = 0.000m,
                        NetReportedTonnage = 0.000m,
                        PricePerTonne = 1.9813m,
                        ProducerDisposalFee = 0.00m,
                        BadDebtProvision = 0.00m,
                        ProducerDisposalFeeWithBadDebtProvision = 0.00m,
                        EnglandWithBadDebtProvision = 0.00m,
                        WalesWithBadDebtProvision = 0.00m,
                        ScotlandWithBadDebtProvision = 0.00m,
                        NorthernIrelandWithBadDebtProvision = 0.00m,
                        PreviousInvoicedTonnage = 0,
                        TonnageChange = 0
                    }
                },
                {
                   "WD",
                   new CalcResultSummaryProducerDisposalFeesByMaterial
                    {
                        HouseholdPackagingWasteTonnage = 500.000m,
                        ManagedConsumerWasteTonnage = 95.000m,
                        NetReportedTonnage = 405.000m,
                        PricePerTonne = 2.0000m,
                        ProducerDisposalFee = 810.00m,
                        BadDebtProvision = 48.60m,
                        ProducerDisposalFeeWithBadDebtProvision = 858.60m,
                        EnglandWithBadDebtProvision = 464.06m,
                        WalesWithBadDebtProvision = 104.60m,
                        ScotlandWithBadDebtProvision = 208.36m,
                        NorthernIrelandWithBadDebtProvision = 81.57m,
                        PreviousInvoicedTonnage = 0,
                        TonnageChange = 0
                    }
                },
                {
                    "OT",
                    new CalcResultSummaryProducerDisposalFeesByMaterial
                    {
                        HouseholdPackagingWasteTonnage = 50.000m,
                        ManagedConsumerWasteTonnage = 5.500m,
                        NetReportedTonnage = 44.500m,
                        PricePerTonne = 1.1954m,
                        ProducerDisposalFee = 53.20m,
                        BadDebtProvision = 3.19m,
                        ProducerDisposalFeeWithBadDebtProvision = 56.39m,
                        EnglandWithBadDebtProvision = 30.48m,
                        WalesWithBadDebtProvision = 6.87m,
                        ScotlandWithBadDebtProvision = 13.68m,
                        NorthernIrelandWithBadDebtProvision = 5.36m,
                        PreviousInvoicedTonnage = 0,
                        TonnageChange = 0
                    }
                },
            };
        }

        public static Dictionary<string, CalcResultSummaryProducerCommsFeesCostByMaterial> GetProducerCommsFeesByMaterial()
        {
            return new Dictionary<string, CalcResultSummaryProducerCommsFeesCostByMaterial>
            {
                {
                    "AL",
                    new CalcResultSummaryProducerCommsFeesCostByMaterial
                    {
                        HouseholdPackagingWasteTonnage = 1000,
                        PriceperTonne = 0.1916m,
                        ProducerTotalCostWithoutBadDebtProvision = 191.60m,
                        BadDebtProvision = 11.50m,
                        ProducerTotalCostwithBadDebtProvision = 203.10m,
                        EnglandWithBadDebtProvision = 106.61m,
                        WalesWithBadDebtProvision = 26.91m,
                        ScotlandWithBadDebtProvision = 49.41m,
                        NorthernIrelandWithBadDebtProvision = 20.17m,
                    }
                },
                {
                    "FC",
                    new CalcResultSummaryProducerCommsFeesCostByMaterial
                    {
                        HouseholdPackagingWasteTonnage = 2000.000m,
                        PriceperTonne = 0.4032m,
                        ProducerTotalCostWithoutBadDebtProvision = 806.40m,
                        BadDebtProvision = 48.38m,
                        ProducerTotalCostwithBadDebtProvision = 854.78m,
                        EnglandWithBadDebtProvision = 448.70m,
                        WalesWithBadDebtProvision = 113.25m,
                        ScotlandWithBadDebtProvision = 207.94m,
                        NorthernIrelandWithBadDebtProvision = 84.89m,
                    }
                },
                {
                    "GL",
                    new CalcResultSummaryProducerCommsFeesCostByMaterial
                    {
                        HouseholdPackagingWasteTonnage = 500.000m,
                        PriceperTonne = 0.4404m,
                        ProducerTotalCostWithoutBadDebtProvision = 220.20m,
                        BadDebtProvision = 13.21m,
                        ProducerTotalCostwithBadDebtProvision = 233.41m,
                        EnglandWithBadDebtProvision = 122.53m,
                        WalesWithBadDebtProvision = 30.92m,
                        ScotlandWithBadDebtProvision = 56.78m,
                        NorthernIrelandWithBadDebtProvision = 23.18m,
                    }
                },
                {
                    "PC",
                    new CalcResultSummaryProducerCommsFeesCostByMaterial
                    {
                        HouseholdPackagingWasteTonnage = 20.000m,
                        PriceperTonne = 1.1100m,
                        ProducerTotalCostWithoutBadDebtProvision = 22.20m,
                        BadDebtProvision = 1.33m,
                        ProducerTotalCostwithBadDebtProvision = 23.53m,
                        EnglandWithBadDebtProvision = 12.35m,
                        WalesWithBadDebtProvision = 3.12m,
                        ScotlandWithBadDebtProvision = 5.72m,
                        NorthernIrelandWithBadDebtProvision = 2.34m,
                    }
                },
                {
                    "PL",
                    new CalcResultSummaryProducerCommsFeesCostByMaterial
                    {
                        HouseholdPackagingWasteTonnage = 5.000m,
                        PriceperTonne = 0.5356m,
                        ProducerTotalCostWithoutBadDebtProvision = 2.68m,
                        BadDebtProvision = 0.16m,
                        ProducerTotalCostwithBadDebtProvision = 2.84m,
                        EnglandWithBadDebtProvision = 1.49m,
                        WalesWithBadDebtProvision = 0.38m,
                        ScotlandWithBadDebtProvision = 0.69m,
                        NorthernIrelandWithBadDebtProvision = 0.28m,
                    }
                },
                {
                    "ST",
                    new CalcResultSummaryProducerCommsFeesCostByMaterial
                    {
                        HouseholdPackagingWasteTonnage = 0.000m,
                        PriceperTonne = 0.8879m,
                        ProducerTotalCostWithoutBadDebtProvision = 0.00m,
                        BadDebtProvision = 0.00m,
                        ProducerTotalCostwithBadDebtProvision = 0.00m,
                        EnglandWithBadDebtProvision = 0.00m,
                        WalesWithBadDebtProvision = 0.00m,
                        ScotlandWithBadDebtProvision = 0.00m,
                        NorthernIrelandWithBadDebtProvision = 0.00m,
                    }
                },
                {
                   "WD",
                   new CalcResultSummaryProducerCommsFeesCostByMaterial
                    {
                        HouseholdPackagingWasteTonnage = 500.000m,
                        PriceperTonne = 0.1364m,
                        ProducerTotalCostWithoutBadDebtProvision = 68.20m,
                        BadDebtProvision = 4.09m,
                        ProducerTotalCostwithBadDebtProvision = 72.29m,
                        EnglandWithBadDebtProvision = 37.95m,
                        WalesWithBadDebtProvision = 9.58m,
                        ScotlandWithBadDebtProvision = 17.59m,
                        NorthernIrelandWithBadDebtProvision = 7.18m,
                    }
                },
                {
                   "OT",
                    new CalcResultSummaryProducerCommsFeesCostByMaterial
                    {
                        HouseholdPackagingWasteTonnage = 50.000m,
                        PriceperTonne = 0.9540m,
                        ProducerTotalCostWithoutBadDebtProvision = 47.70m,
                        BadDebtProvision = 2.86m,
                        ProducerTotalCostwithBadDebtProvision = 50.56m,
                        EnglandWithBadDebtProvision = 26.54m,
                        WalesWithBadDebtProvision = 6.70m,
                        ScotlandWithBadDebtProvision = 12.30m,
                        NorthernIrelandWithBadDebtProvision = 5.02m,
                    }
                },
            };
        }

        public static CalcResultScaledupProducers GetScaledupProducers()
        {
            return new CalcResultScaledupProducers
            {
                ColumnHeaders = new List<CalcResultScaledupProducerHeader>(),
                MaterialBreakdownHeaders = new List<CalcResultScaledupProducerHeader>(),
                TitleHeader = new CalcResultScaledupProducerHeader()
                {
                    Name = "Scaled-up Producers",
                    ColumnIndex = 1,
                },
                ScaledupProducers = new List<CalcResultScaledupProducer>()
                {
                    new CalcResultScaledupProducer
                    {
                        ProducerId = 1,
                        ProducerName = "Producer Name",
                        DaysInSubmissionPeriod = 91,
                        DaysInWholePeriod = 91,
                        IsSubtotalRow = false,
                        IsTotalRow = false,
                        Level = "1",
                        ScaleupFactor = 1,
                        SubmissionPeriodCode = "2024-P2",
                        SubsidiaryId = string.Empty,
                        ScaledupProducerTonnageByMaterial = new Dictionary<string, CalcResultScaledupProducerTonnage>
                        {
                            {
                                "AL",
                                new CalcResultScaledupProducerTonnage
                                {
                                    ReportedHouseholdPackagingWasteTonnage = 100,
                                    ReportedPublicBinTonnage = 20,
                                    TotalReportedTonnage = 120,
                                    ReportedSelfManagedConsumerWasteTonnage = 60,
                                    NetReportedTonnage = 180,
                                    ScaledupReportedHouseholdPackagingWasteTonnage = 200,
                                    ScaledupReportedPublicBinTonnage = 40,
                                    ScaledupTotalReportedTonnage = 240,
                                    ScaledupReportedSelfManagedConsumerWasteTonnage = 120,
                                    ScaledupNetReportedTonnage = 360,
                                }
                            },
                            {
                                "GL",
                                new CalcResultScaledupProducerTonnage
                                {
                                    ReportedHouseholdPackagingWasteTonnage = 100,
                                    ReportedPublicBinTonnage = 20,
                                    TotalReportedTonnage = 120,
                                    ReportedSelfManagedConsumerWasteTonnage = 60,
                                    HouseholdDrinksContainersTonnageGlass = 70,
                                    NetReportedTonnage = 180,
                                    ScaledupReportedHouseholdPackagingWasteTonnage = 200,
                                    ScaledupReportedPublicBinTonnage = 40,
                                    ScaledupTotalReportedTonnage = 240,
                                    ScaledupReportedSelfManagedConsumerWasteTonnage = 120,
                                    ScaledupHouseholdDrinksContainersTonnageGlass = 140,
                                    ScaledupNetReportedTonnage = 360,
                                }
                            },
                        },
                    },
                },
            };
        }

        public static List<MaterialDetail> GetMaterials()
        {
            return new List<MaterialDetail>
            {
                new MaterialDetail
                {
                    Id = 1,
                    Code = "AL",
                    Name = "Aluminium",
                    Description = "Aluminium",
                },
                new MaterialDetail
                {
                    Id = 2,
                    Code = "FC",
                    Name = "Fibre composite",
                    Description = "Fibre composite",
                },
                new MaterialDetail
                {
                    Id = 3,
                    Code = "GL",
                    Name = "Glass",
                    Description = "Glass",
                },
                new MaterialDetail
                {
                    Id = 4,
                    Code = "PC",
                    Name = "Paper or card",
                    Description = "Paper or card",
                },
                new MaterialDetail
                {
                    Id = 5,
                    Code = "PL",
                    Name = "Plastic",
                    Description = "Plastic",
                },
                new MaterialDetail
                {
                    Id = 6,
                    Code = "ST",
                    Name = "Steel",
                    Description = "Steel",
                },
                new MaterialDetail
                {
                    Id = 7,
                    Code = "WD",
                    Name = "Wood",
                    Description = "Wood",
                },
                new MaterialDetail
                {
                    Id = 8,
                    Code = "OT",
                    Name = "Other materials",
                    Description = "Other materials",
                },
            };
        }

        public static List<ProducerDetail> GetProducers()
        {
            var calculatorRunFinancialYear = new CalculatorRunFinancialYear { Name = "2024-25" };
            var producers = new List<ProducerDetail>
            {
                new ProducerDetail
                {
                    Id = 1,
                    ProducerId = 1,
                    ProducerName = "Allied Packaging",
                    CalculatorRunId = 1,
                    CalculatorRun = new CalculatorRun { Financial_Year = calculatorRunFinancialYear, Name = "Test Run 1" },
                },
                new ProducerDetail
                {
                    Id = 2,
                    ProducerId = 2,
                    ProducerName = "Beeline Materials",
                    CalculatorRunId = 1,
                    CalculatorRun = new CalculatorRun { Financial_Year = calculatorRunFinancialYear, Name = "Test Run 1" },
                },
                new ProducerDetail
                {
                    Id = 3,
                    ProducerId = 3,
                    ProducerName = "Cloud Boxes",
                    CalculatorRunId = 1,
                    CalculatorRun = new CalculatorRun { Financial_Year = calculatorRunFinancialYear, Name = "Test Run 1" },
                },
            };

            var producerReportedMaterials = GetProducerReportedMaterials();

            producers.ForEach(producer =>
            {
                producerReportedMaterials.ForEach(producerReportedMaterial =>
                {
                    producer.ProducerReportedMaterials.Add(producerReportedMaterial);
                });
            });

            return producers;
        }

        public static List<ProducerReportedMaterial> GetProducerReportedMaterials()
        {
            return new List<ProducerReportedMaterial>()
            {
                new ProducerReportedMaterial
                {
                    Material = new Material { Id = 1, Code = "AL", Name = "Aluminium", Description = "Aluminium" },
                    PackagingTonnage = 1000.00m,
                    PackagingType = "HH",
                    MaterialId = 1,
                    ProducerDetail = null,
                },
                new ProducerReportedMaterial
                {
                    Material = new Material { Id = 1, Code = "AL", Name = "Aluminium", Description = "Aluminium" },
                    PackagingTonnage = 20.00m,
                    PackagingType = "CW",
                    MaterialId = 1,
                    ProducerDetail = null,
                },
                new ProducerReportedMaterial
                {
                    Material = new Material { Id = 3, Code = "PL", Name = "Plastic", Description = "Plastic" },
                    PackagingTonnage = 20.00m,
                    PackagingType = "PB",
                    MaterialId = 3,
                    ProducerDetail = null,
                },
                new ProducerReportedMaterial
                {
                    Material = new Material { Id = 4, Code = "GL", Name = "Glass", Description = "Glass" },
                    PackagingTonnage = 40.00m,
                    PackagingType = "HH",
                    MaterialId = 4,
                    ProducerDetail = null,
                },
                new ProducerReportedMaterial
                {
                    Material = new Material { Id = 4, Code = "GL", Name = "Glass", Description = "Glass" },
                    PackagingTonnage = 20.00m,
                    PackagingType = "HDC",
                    MaterialId = 4,
                    ProducerDetail = null,
                },
                new ProducerReportedMaterial
                {
                    Material = new Material { Id = 1, Code = "GL", Name = "Glass", Description = "Glass" },
                    PackagingTonnage = 100.00m,
                    PackagingType = "CW",
                    MaterialId = 1,
                    ProducerDetail = null,
                },
            };
        }

        public static IEnumerable<DefaultParameterTemplateMaster> GetDefaultParameterTemplateMasterData()
        {
            var list = new List<DefaultParameterTemplateMaster>();
            list.Add(new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "COMC-ENG",
                ParameterCategory = "England",
                ParameterType = "Communication costs by country",
                ValidRangeFrom = 0m,
                ValidRangeTo = 999999999.99m,
            });
            list.Add(new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "COMC-NIR",
                ParameterCategory = "Northern Ireland",
                ParameterType = "Communication costs by country",
                ValidRangeFrom = 0m,
                ValidRangeTo = 999999999.99m,
            });
            list.Add(new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "COMC-SCT",
                ParameterCategory = "Scotland",
                ParameterType = "Communication costs by country",
                ValidRangeFrom = 0m,
                ValidRangeTo = 999999999.99m,
            });
            list.Add(new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "COMC-UK",
                ParameterCategory = "United Kingdom",
                ParameterType = "Communication costs by country",
                ValidRangeFrom = 0m,
                ValidRangeTo = 999999999.99m,
            });
            list.Add(new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "COMC-WLS",
                ParameterCategory = "Wales",
                ParameterType = "Communication costs by country",
                ValidRangeFrom = 0m,
                ValidRangeTo = 999999999.99m,
            });
            list.Add(new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "COMC-AL",
                ParameterCategory = "Aluminium",
                ParameterType = "Communication costs by material",
                ValidRangeFrom = 0m,
                ValidRangeTo = 999999999.99m,
            });
            list.Add(new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "COMC-FC",
                ParameterCategory = "Fibre composite",
                ParameterType = "Communication costs by material",
                ValidRangeFrom = 0m,
                ValidRangeTo = 999999999.99m,
            });
            list.Add(new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "COMC-GL",
                ParameterCategory = "Glass",
                ParameterType = "Communication costs by material",
                ValidRangeFrom = 0m,
                ValidRangeTo = 999999999.99m,
            });
            list.Add(new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "COMC-PC",
                ParameterCategory = "Paper or card",
                ParameterType = "Communication costs by material",
                ValidRangeFrom = 0m,
                ValidRangeTo = 999999999.99m,
            });
            list.Add(new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "COMC-PL",
                ParameterCategory = "Plastic",
                ParameterType = "Communication costs by material",
                ValidRangeFrom = 0m,
                ValidRangeTo = 999999999.99m,
            });
            list.Add(new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "COMC-ST",
                ParameterCategory = "Steel",
                ParameterType = "Communication costs by material",
                ValidRangeFrom = 0m,
                ValidRangeTo = 999999999.99m,
            });
            list.Add(new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "COMC-WD",
                ParameterCategory = "Wood",
                ParameterType = "Communication costs by material",
                ValidRangeFrom = 0m,
                ValidRangeTo = 999999999.99m,
            });
            list.Add(new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "LRET-FC",
                ParameterCategory = "Fibre composite",
                ParameterType = "Late reporting tonnage",
                ValidRangeFrom = 0m,
                ValidRangeTo = 999999999.99m,
            });
            list.Add(new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "LRET-GL",
                ParameterCategory = "Glass",
                ParameterType = "Late reporting tonnage",
                ValidRangeFrom = 0m,
                ValidRangeTo = 999999999.99m,
            });
            list.Add(new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "LRET-AL",
                ParameterCategory = "Aluminium",
                ParameterType = "Late reporting tonnage",
                ValidRangeFrom = 0m,
                ValidRangeTo = 999999999.99m,
            });
            list.Add(new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "LRET-WD",
                ParameterCategory = "Wood",
                ParameterType = "Late reporting tonnage",
                ValidRangeFrom = 0m,
                ValidRangeTo = 999999999.99m,
            });
            list.Add(new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "LRET-ST",
                ParameterCategory = "Steel",
                ParameterType = "Late reporting tonnage",
                ValidRangeFrom = 0m,
                ValidRangeTo = 999999999.99m,
            });
            list.Add(new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "LRET-PC",
                ParameterCategory = "Paper or card",
                ParameterType = "Late reporting tonnage",
                ValidRangeFrom = 0m,
                ValidRangeTo = 999999999.99m,
            });
            list.Add(new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "LAPC-ENG",
                ParameterCategory = "England",
                ParameterType = "Local authority data preparation costs",
                ValidRangeFrom = 0m,
                ValidRangeTo = 999999999.99m,
            });
            list.Add(new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "LAPC-NIR",
                ParameterCategory = "Northern Ireland",
                ParameterType = "Local authority data preparation costs",
                ValidRangeFrom = 0m,
                ValidRangeTo = 999999999.99m,
            });
            list.Add(new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "LAPC-SCT",
                ParameterCategory = "Scotland",
                ParameterType = "Local authority data preparation costs",
                ValidRangeFrom = 0m,
                ValidRangeTo = 999999999.99m,
            });
            list.Add(new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "LAPC-WLS",
                ParameterCategory = "Wales",
                ParameterType = "Local authority data preparation costs",
                ValidRangeFrom = 0m,
                ValidRangeTo = 999999999.99m,
            });
            list.Add(new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "MATT-AD",
                ParameterCategory = "Amount Decrease",
                ParameterType = "Materiality threshold",
                ValidRangeFrom = -999999999.990m,
                ValidRangeTo = 0.00m,
            });
            list.Add(new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "MATT-AI",
                ParameterCategory = "Amount Increase",
                ParameterType = "Materiality threshold",
                ValidRangeFrom = 0m,
                ValidRangeTo = 999999999.99m,
            });
            list.Add(new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "MATT-PD",
                ParameterCategory = "Percent Decrease",
                ParameterType = "Materiality threshold",
                ValidRangeFrom = -999.990m,
                ValidRangeTo = 0.00m,
            });
            list.Add(new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "MATT-PI",
                ParameterCategory = "Percent Increase",
                ParameterType = "Materiality threshold",
                ValidRangeFrom = 0m,
                ValidRangeTo = 999.990m,
            });
            list.Add(new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "COMC-OT",
                ParameterCategory = "Other",
                ParameterType = "Other materials",
                ValidRangeFrom = 0m,
                ValidRangeTo = 999999999.99m,
            });
            list.Add(new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "LRET-OT",
                ParameterCategory = "Other materials",
                ParameterType = "Late reporting tonnage",
                ValidRangeFrom = 0m,
                ValidRangeTo = 999999999.99m,
            });
            list.Add(new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "BADEBT-P",
                ParameterCategory = "Bad debt provision",
                ParameterType = "Percentage",
                ValidRangeFrom = 0m,
                ValidRangeTo = 1000.000m,
            });
            list.Add(new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "LRET-PL",
                ParameterCategory = "Plastic",
                ParameterType = "Late reporting tonnage",
                ValidRangeFrom = 0m,
                ValidRangeTo = 999999999.99m,
            });
            list.Add(new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "SAOC-ENG",
                ParameterCategory = "England",
                ParameterType = "Scheme administrator operating costs",
                ValidRangeFrom = 0m,
                ValidRangeTo = 999999999.99m,
            });
            list.Add(new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "SAOC-NIR",
                ParameterCategory = "Northern Ireland",
                ParameterType = "Scheme administrator operating costs",
                ValidRangeFrom = 0m,
                ValidRangeTo = 999999999.99m,
            });
            list.Add(new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "SAOC-SCT",
                ParameterCategory = "Scotland",
                ParameterType = "Scheme administrator operating costs",
                ValidRangeFrom = 0m,
                ValidRangeTo = 999999999.99m,
            });
            list.Add(new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "SAOC-WLS",
                ParameterCategory = "Wales",
                ParameterType = "Scheme administrator operating costs",
                ValidRangeFrom = 0m,
                ValidRangeTo = 999999999.99m,
            });
            list.Add(new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "SCSC-ENG",
                ParameterCategory = "England",
                ParameterType = "Scheme setup costs",
                ValidRangeFrom = 0m,
                ValidRangeTo = 999999999.99m,
            });
            list.Add(new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "SCSC-NIR",
                ParameterCategory = "Northern Ireland",
                ParameterType = "Scheme setup costs",
                ValidRangeFrom = 0m,
                ValidRangeTo = 999999999.99m,
            });
            list.Add(new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "SCSC-SCT",
                ParameterCategory = "Scotland",
                ParameterType = "Scheme setup costs",
                ValidRangeFrom = 0m,
                ValidRangeTo = 999999999.99m,
            });
            list.Add(new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "SCSC-WLS",
                ParameterCategory = "Wales",
                ParameterType = "Scheme setup costs",
                ValidRangeFrom = 0m,
                ValidRangeTo = 999999999.99m,
            });
            list.Add(new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "TONT-AD",
                ParameterCategory = "Amount Decrease",
                ParameterType = "Tonnage change threshold",
                ValidRangeFrom = -999999999.990m,
                ValidRangeTo = 0.00m,
            });
            list.Add(new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "TONT-AI",
                ParameterCategory = "Amount Increase",
                ParameterType = "Tonnage change threshold",
                ValidRangeFrom = 0m,
                ValidRangeTo = 999999999.99m,
            });
            list.Add(new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "TONT-PD",
                ParameterCategory = "Percent Decrease",
                ParameterType = "Tonnage change threshold",
                ValidRangeFrom = -999.990m,
                ValidRangeTo = 0.00m,
            });
            list.Add(new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "TONT-PI",
                ParameterCategory = "Percent Increase",
                ParameterType = "Tonnage change threshold",
                ValidRangeFrom = 0m,
                ValidRangeTo = 999.990m,
            });
            return list;
        }

        public static IEnumerable<CalculatorRun> GetCaculatorRuns()
        {
            var calculatorRunFinancialYear = new CalculatorRunFinancialYear { Name = "2024-25" };
            var list = new List<CalculatorRun>();
            list.Add(new CalculatorRun
            {
                Id = 1,
                CalculatorRunClassificationId = 3,
                Name = "Test Run 1",
                Financial_Year = calculatorRunFinancialYear,
                CreatedAt = DateTime.Now,
                CreatedBy = "Test user",
                CalculatorRunOrganisationDataMasterId = 1,
                CalculatorRunPomDataMasterId = 1,
                DefaultParameterSettingMasterId = 1,
                LapcapDataMasterId = 1
            });
            list.Add(new CalculatorRun
            {
                Id = 2,
                CalculatorRunClassificationId = 2,
                Name = "Test Run 2",
                Financial_Year = calculatorRunFinancialYear,
                CreatedAt = DateTime.Now,
                CreatedBy = "Test user",
                CalculatorRunOrganisationDataMasterId = 2,
                CalculatorRunPomDataMasterId = 2,
                DefaultParameterSettingMasterId = 1,
                LapcapDataMasterId = 1
            });
            return list;
        }

        public static IEnumerable<CalculatorRunOrganisationDataDetail> GetCalculatorRunOrganisationDataDetails()
        {
            var list = new List<CalculatorRunOrganisationDataDetail>();
            list.Add(new CalculatorRunOrganisationDataDetail
            {
                Id = 1,
                OrganisationId = 1,
                SubsidaryId = null,
                OrganisationName = "Allied Packaging",
                LoadTimeStamp = DateTime.Now,
                SubmissionPeriodDesc = "January to June 2023",
                CalculatorRunOrganisationDataMasterId = GetCalculatorRunOrganisationDataMaster().ToList()[0].Id,
                CalculatorRunOrganisationDataMaster = GetCalculatorRunOrganisationDataMaster().ToList()[0]
            });
            list.Add(new CalculatorRunOrganisationDataDetail
            {
                Id = 1,
                OrganisationId = 1,
                SubsidaryId = "901",
                OrganisationName = "Allied Subsidiary",
                LoadTimeStamp = DateTime.Now,
                SubmissionPeriodDesc = "January to June 2023",
                CalculatorRunOrganisationDataMasterId = GetCalculatorRunOrganisationDataMaster().ToList()[0].Id,
                CalculatorRunOrganisationDataMaster = GetCalculatorRunOrganisationDataMaster().ToList()[0]
            });
            return list;
        }

        public static IEnumerable<CalculatorRunOrganisationDataMaster> GetCalculatorRunOrganisationDataMaster()
        {
            var list = new List<CalculatorRunOrganisationDataMaster>();
            list.Add(new CalculatorRunOrganisationDataMaster
            {
                Id = 1,
                CalendarYear = "2024",
                EffectiveFrom = DateTime.Now,
                EffectiveTo = DateTime.Now,
                CreatedAt = DateTime.Now,
                CreatedBy = "Test user",
            });
            list.Add(new CalculatorRunOrganisationDataMaster
            {
                Id = 2,
                CalendarYear = "2024",
                EffectiveFrom = DateTime.Now,
                EffectiveTo = DateTime.Now,
                CreatedAt = DateTime.Now,
                CreatedBy = "Test user",
            });
            return list;
        }

        public static IEnumerable<CalculatorRunPomDataDetail> GetCalculatorRunPomDataDetails()
        {
            var list = new List<CalculatorRunPomDataDetail>();
            list.Add(new CalculatorRunPomDataDetail
            {
                Id = 1,
                OrganisationId = 1,
                SubsidaryId = null,
                SubmissionPeriod = "2024-P2",
                PackagingActivity = null,
                PackagingType = "HH",
                PackagingClass = "O1",
                PackagingMaterial = "AL",
                PackagingMaterialWeight = 1000,
                LoadTimeStamp = DateTime.Now,
                CalculatorRunPomDataMasterId = 1,
                SubmissionPeriodDesc = "January to June 2024",
                CalculatorRunPomDataMaster = GetCalculatorRunPomDataMaster().ToList()[0]
            });
            list.Add(new CalculatorRunPomDataDetail
            {
                Id = 1,
                OrganisationId = 1,
                SubsidaryId = null,
                SubmissionPeriod = "2024-P4",
                PackagingActivity = null,
                PackagingType = "HH",
                PackagingClass = "O1",
                PackagingMaterial = "AL",
                PackagingMaterialWeight = 2000,
                LoadTimeStamp = DateTime.Now,
                CalculatorRunPomDataMasterId = 1,
                SubmissionPeriodDesc = "July to December 2024",
                CalculatorRunPomDataMaster = GetCalculatorRunPomDataMaster().ToList()[0]
            });
            return list;
        }

        public static IEnumerable<CalculatorRunPomDataMaster> GetCalculatorRunPomDataMaster()
        {
            var list = new List<CalculatorRunPomDataMaster>();
            list.Add(new CalculatorRunPomDataMaster
            {
                Id = 1,
                CalendarYear = "2024",
                EffectiveFrom = DateTime.Now,
                CreatedAt = DateTime.Now,
                CreatedBy = "Test User",
            });
            list.Add(new CalculatorRunPomDataMaster
            {
                Id = 2,
                CalendarYear = "2024",
                EffectiveFrom = DateTime.Now,
                CreatedAt = DateTime.Now,
                CreatedBy = "Test User",
            });
            return list;
        }
    }
}
