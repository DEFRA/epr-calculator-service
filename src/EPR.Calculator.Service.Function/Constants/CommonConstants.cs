namespace EPR.Calculator.Service.Function.Constants
{
    public static class CommonConstants
    {
        public const bool True = true;
        public const bool False = false;

        public const int DefaultMinValue = 0;
        public const int LevelOne = 1;
        public const int LevelTwo = 2;
        public const int SecondaryHeaderMaxColumnSize = 298;

        public const string Material = "Material";
        public const string England = "England";
        public const string Wales = "Wales";
        public const string Scotland = "Scotland";
        public const string NorthernIreland = "Northern Ireland";
        public const string Total = "Total";
        public const string Totals = "Totals";
        public const string ProducerReportedHouseholdPackagingWasteTonnage = "Producer Household Packaging Tonnage";
        public const string ReportedPublicBinTonnage = "Public Bin Tonnage";
        public const string HouseholdDrinkContainers = "Household Drinks Containers Tonnage";
        public const string LateReportingTonnage = "Late Reporting Tonnage";
        public const string ProducerReportedTotalTonnage = "Producer Household Tonnage + Late Reporting Tonnage + Public Bin Tonnage + Household Drinks Containers Tonnage";
        public const string DisposalCostPricePerTonne = "Disposal Cost Price Per Tonne";
        public const string LADisposalCostData = "LA Disposal Cost Data";
        public const string PolicyName = "AllowAllOrigins";
        public const string ServiceBusClientName = "calculator";
        public const string CsvFileDelimiter = ",";
        public const string TabSpace = "\t";
        public const string DoubleQuote = "\"";
        public const string Hyphen = "-";
        public const string ScaledupProducersYes = "Yes";
        public const string ScaledupProducersNo = "No";
        public const string ParametersOther = "Parameters - Other";
        public const string ZeroCurrency = "£0";
        public const string Initial = "INITIAL";
        public const char Comma = ',';

        public const string OnePlusFourCommsCostApportionmentPercentages = "onePlusFourCommsCostApportionmentPercentages";
        public const string ParametersCommsCost = "parametersCommsCost";

        public const string CancelledProducers = "Cancelled Producers";
        
        public const string ProducerId = "Producer ID";
        public const string SubsidiaryId = "Subsidiary ID";
        public const string ProducerOrSubsidiaryName = "Producer / Subsidiary Name";
        public const string TradingName = "Trading Name";

        public const string LastTonnage = "Last Tonnage";
        public const int LastTonnageSubHeaderIndex = 4;

        public const string Aluminium = "Aluminium";
        public const string FibreComposite = "Fibre composite";
        public const string Glass = "Glass";
        public const string PaperOrCard = "Paper or Card";
        public const string Plastic = "Plastic";
        public const string Steel = "Steel";
        public const string Wood = "Wood";
        public const string OtherMaterials = "Other materials";

        public const string LatestInvoice = "Latest Invoice";
        public const int LatestInvoiceSubHeaderIndex = 11;

        public const string LastInvoicedTotal = "Last Invoiced Total";
        public const string RunNumber = "Run Number";
        public const string RunName = "Run Name";
        public const string BillingInstructionId = "Billing Instruction ID";
        public const string TwoACommsCostsbyMaterial = "2a Comms Costs - by Material";
    }
}