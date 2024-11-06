namespace EPR.Calculator.Service.Common.UnitTests
{
    using EPR.Calculator.Service.Common.Utils;

    /// <summary>
    /// Unit tests for the <see cref="Util"/> class.
    /// </summary>
    [TestClass]
    public class UtilTests
    {
        /// <summary>
        /// Tests that GetFinancialYearAsYYYY returns the first year from a valid financial year string.
        /// </summary>
        /// <param name="financialYear">The financial year string to parse.</param>
        /// <param name="expectedFinancialYear">The expected first year as a string.</param>
        [TestMethod]
        [DataRow("2024-25", "2024")]
        [DataRow("2023-24", "2023")]
        [DataRow("2022-23", "2022")]
        public void GetFinancialYearAsYYYY_ValidString_ShouldReturnFirstYear(string financialYear, string expectedFinancialYear)
        {
            var result = Util.GetFinancialYearAsYYYY(financialYear);
            Assert.AreEqual(expectedFinancialYear, result);
        }

        /// <summary>
        /// Tests that GetFinancialYearAsYYYY throws a FormatException for an invalid string.
        /// </summary>
        /// <param name="financialYear">The invalid financial year string to parse.</param>
        [TestMethod]
        [DataRow("2024")]
        [DataRow("24-25")]
        [DataRow("2024-2025")]
        [DataRow("abcd-efgh")]
        public void GetFinancialYearAsYYYY_InvalidString_ShouldThrowFormatException(string financialYear)
        {
            var exception = Assert.ThrowsException<FormatException>(() => Util.GetFinancialYearAsYYYY(financialYear));
            Assert.AreEqual("Financial year format is invalid. Expected format is 'YYYY-YY'.", exception.Message);
        }

        /// <summary>
        /// Tests that GetFinancialYearAsYYYY throws a ArgumentException for a null or empty string.
        /// </summary>
        /// <param name="financialYear">The null or empty financial year string to convert.</param>
        [TestMethod]
        [DataRow(null)]
        [DataRow("")]
        [DataRow(" ")]
        public void GetFinancialYearAsYYYY_NullOrEmptyString_ShouldThrowArgumentException(string financialYear)
        {
            var exception = Assert.ThrowsException<ArgumentException>(() => Util.GetFinancialYearAsYYYY(financialYear));
            Assert.AreEqual("Financial year cannot be null or empty (Parameter 'value')", exception.Message);
        }

        /// <summary>
        /// Tests that GetCalendarYear returns the previous year as a string for a valid financial year string.
        /// </summary>
        /// <param name="financialYear">The financial year string to convert.</param>
        /// <param name="expectedCalendarYear">The expected previous calendar year as a string.</param>
        [TestMethod]
        [DataRow("2024-25", "2023")]
        [DataRow("2023-24", "2022")]
        [DataRow("2022-23", "2021")]
        public void GetCalendarYear_ValidString_ShouldReturnPreviousYearAsString(string financialYear, string expectedCalendarYear)
        {
            var result = Util.GetCalendarYear(financialYear);
            Assert.AreEqual(expectedCalendarYear, result);
        }

        /// <summary>
        /// Tests that GetCalendarYear throws an ArgumentException for a null or empty string.
        /// </summary>
        /// <param name="financialYear">The null or empty financial year string to convert.</param>
        [TestMethod]
        [DataRow(null)]
        [DataRow("")]
        [DataRow(" ")]
        public void GetCalendarYear_NullOrEmptyString_ShouldThrowArgumentException(string financialYear)
        {
            var exception = Assert.ThrowsException<ArgumentException>(() => Util.GetCalendarYear(financialYear));
            Assert.AreEqual("Financial year cannot be null or empty (Parameter 'financialYear')", exception.Message);
        }
    }
}