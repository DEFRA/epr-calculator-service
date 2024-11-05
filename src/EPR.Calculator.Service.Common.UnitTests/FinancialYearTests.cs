namespace EPR.Calculator.Service.Common.UnitTests
{
    using EPR.Calculator.Service.Common.AzureSynapse;

    /// <summary>
    /// Unit tests for the FinancialYear class.
    /// </summary>
    [TestClass]
    public class FinancialYearTests
    {
        /// <summary>
        /// Tests that the Value property returns the correct DateTime.
        /// </summary>
        [TestMethod]
        public void FinancialYear_Value_ShouldReturnCorrectDateTime()
        {
            var date = new DateTime(2024, 1, 1);
            var financialYear = new FinancialYear(date);

            var result = financialYear.Value;

            Assert.AreEqual(date, result);
        }

        /// <summary>
        /// Tests that ToCalendarYear returns the previous calendar year.
        /// </summary>
        [TestMethod]
        public void ToCalendarYear_ShouldReturnPreviousYear()
        {
            var date = new DateTime(2024, 1, 1);
            var financialYear = new FinancialYear(date);

            var result = financialYear.ToCalendarYear();

            Assert.AreEqual(new DateTime(2023, 1, 1), result);
        }

        /// <summary>
        /// Tests that Parse correctly parses a valid financial year string.
        /// </summary>
        [TestMethod]
        public void Parse_ValidString_ShouldReturnFinancialYear()
        {
            var input = "2024-25";

            var result = FinancialYear.Parse(input);

            Assert.AreEqual(new DateTime(2024, 1, 1), result.Value);
        }

        /// <summary>
        /// Tests that Parse throws a FormatException for an invalid string.
        /// </summary>
        /// <param name="financialYear">The invalid financial year string to parse.</param>
        [TestMethod]
        [DataRow("20-21")]
        [DataRow("abcd-ef")]
        public void Parse_InvalidString_ShouldThrowsArgumentException(string financialYear)
        {
            var exception = Assert.ThrowsException<ArgumentException>(() => FinancialYear.Parse(financialYear));
            Assert.AreEqual("Invalid format. Please use the format 'YYYY-YY'.", exception.Message);
        }

        /// <summary>
        /// Tests that Parse throws an ArgumentException for a null or empty string.
        /// </summary>
        /// <param name="financialYear">The null or empty financial year string to convert.</param>
        [TestMethod]
        [DataRow(null)]
        [DataRow("")]
        [DataRow(" ")]
        public void Parse_NullOrEmptyString_ShouldThrowArgumentException(string financialYear)
        {
            var exception = Assert.ThrowsException<ArgumentException>(() => FinancialYear.Parse(financialYear));
            Assert.AreEqual("Financial year cannot be null or empty (Parameter 'value')", exception.Message);
        }

        /// <summary>
        /// Tests that FinancialYearAsString returns the first year from a valid financial year string.
        /// </summary>
        /// <param name="financialYear">The financial year string to parse.</param>
        /// <param name="expectedFinancialYear">The expected first year as a string.</param>
        [TestMethod]
        [DataRow("2024-25", "2024")]
        [DataRow("2023-24", "2023")]
        [DataRow("2022-23", "2022")]
        public void FinancialYearAsString_ValidString_ShouldReturnFirstYear(string financialYear, string expectedFinancialYear)
        {
            var result = FinancialYear.FinancialYearAsString(financialYear);
            Assert.AreEqual(expectedFinancialYear, result);
        }

        /// <summary>
        /// Tests that FinancialYearAsString throws a FormatException for an invalid string.
        /// </summary>
        /// <param name="financialYear">The invalid financial year string to parse.</param>
        [TestMethod]
        [DataRow("2024")]
        [DataRow("24-25")]
        [DataRow("2024-2025")]
        [DataRow("abcd-efgh")]
        public void FinancialYearAsString_InvalidString_ShouldThrowFormatException(string financialYear)
        {
            var exception = Assert.ThrowsException<FormatException>(() => FinancialYear.FinancialYearAsString(financialYear));
            Assert.AreEqual("Financial year format is invalid. Expected format is 'YYYY-YY'.", exception.Message);
        }

        /// <summary>
        /// Tests that FinancialYearAsString throws a ArgumentException for a null or empty string.
        /// </summary>
        /// <param name="financialYear">The null or empty financial year string to convert.</param>
        [TestMethod]
        [DataRow(null)]
        [DataRow("")]
        [DataRow(" ")]
        public void FinancialYearAsString_NullOrEmptyString_ShouldThrowArgumentException(string financialYear)
        {
            var exception = Assert.ThrowsException<ArgumentException>(() => FinancialYear.FinancialYearAsString(financialYear));
            Assert.AreEqual("Financial year cannot be null or empty (Parameter 'value')", exception.Message);
        }

        /// <summary>
        /// Tests that ToCalendarYearAsString returns the previous year as a string for a valid financial year string.
        /// </summary>
        /// <param name="financialYear">The financial year string to convert.</param>
        /// <param name="expectedCalendarYear">The expected previous calendar year as a string.</param>
        [TestMethod]
        [DataRow("2024-25", "2023")]
        [DataRow("2023-24", "2022")]
        [DataRow("2022-23", "2021")]
        public void ToCalendarYearAsString_ValidString_ShouldReturnPreviousYearAsString(string financialYear, string expectedCalendarYear)
        {
            var result = FinancialYear.ToCalendarYearAsString(financialYear);
            Assert.AreEqual(expectedCalendarYear, result);
        }

        /// <summary>
        /// Tests that ToCalendarYearAsString throws an ArgumentException for a null or empty string.
        /// </summary>
        /// <param name="financialYear">The null or empty financial year string to convert.</param>
        [TestMethod]
        [DataRow(null)]
        [DataRow("")]
        [DataRow(" ")]
        public void ToCalendarYearAsString_NullOrEmptyString_ShouldThrowArgumentException(string financialYear)
        {
            var exception = Assert.ThrowsException<ArgumentException>(() => FinancialYear.ToCalendarYearAsString(financialYear));
            Assert.AreEqual("Financial year cannot be null or empty (Parameter 'value')", exception.Message);
        }
    }
}