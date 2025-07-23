using EPR.Calculator.Service.Common.Utils;

namespace EPR.Calculator.Service.Common.UnitTests.Utils
{
    /// <summary>
    /// Unit tests for the <see cref="Util"/> class.
    /// </summary>
    [TestClass]
    public class UtilTests
    {
        /// <summary>
        /// Tests that GetStartYearFromFinancialYear returns the first year from a valid financial year string.
        /// </summary>
        /// <param name="financialYear">The financial year string to parse.</param>
        /// <param name="expectedFinancialYear">The expected first year as a string.</param>
        [TestMethod]
        [DataRow("2024-25", "2024")]
        [DataRow("2023-24", "2023")]
        [DataRow("2022-23", "2022")]
        public void GetStartYearFromFinancialYear_ValidString_ShouldReturnFirstYear(string financialYear, string expectedFinancialYear)
        {
            var result = Util.GetStartYearFromFinancialYear(financialYear);
            Assert.AreEqual(expectedFinancialYear, result);
        }

        /// <summary>
        /// Tests that GetStartYearFromFinancialYear throws a FormatException for an invalid string.
        /// </summary>
        /// <param name="financialYear">The invalid financial year string to parse.</param>
        [TestMethod]
        [DataRow("2024")]
        [DataRow("24-25")]
        [DataRow("2024-2025")]
        [DataRow("abcd-efgh")]
        public void GetStartYearFromFinancialYear_InvalidString_ShouldThrowFormatException(string financialYear)
        {
            var exception = Assert.ThrowsException<FormatException>(() => Util.GetStartYearFromFinancialYear(financialYear));
            Assert.AreEqual("Financial year format is invalid. Expected format is 'YYYY-YY'.", exception.Message);
        }

        /// <summary>
        /// Tests that GetStartYearFromFinancialYear throws an ArgumentException for a null or empty string.
        /// </summary>
        /// <param name="financialYear">The null or empty financial year string to convert.</param>
        [TestMethod]
        [DataRow(null)]
        [DataRow("")]
        [DataRow(" ")]
        public void GetStartYearFromFinancialYear_NullOrEmptyString_ShouldThrowArgumentException(string financialYear)
        {
            var exception = Assert.ThrowsException<ArgumentException>(() => Util.GetStartYearFromFinancialYear(financialYear));
            Assert.AreEqual("Financial year cannot be null or empty (Parameter 'value')", exception.Message);
        }

        /// <summary>
        /// Tests that GetCalendarYearFromFinancialYear returns the previous year as a string for a valid financial year string.
        /// </summary>
        /// <param name="financialYear">The financial year string to convert.</param>
        /// <param name="expectedCalendarYear">The expected previous calendar year as a string.</param>
        [TestMethod]
        [DataRow("2024-25", "2023")]
        [DataRow("2023-24", "2022")]
        [DataRow("2022-23", "2021")]
        public void GetCalendarYearFromFinancialYear_ValidString_ShouldReturnPreviousYearAsString(string financialYear, string expectedCalendarYear)
        {
            var result = Util.GetCalendarYearFromFinancialYear(financialYear);
            Assert.AreEqual((CalendarYear)expectedCalendarYear, result);
        }

        [TestMethod]
        [DataRow("2024")]
        [DataRow("24-25")]
        [DataRow("2024-2025")]
        [DataRow("abcd-efgh")]
        public void GetCalendarYearFromFinancialYearNew_InvalidFormat_ShouldThrowArgumentException(string financialYear)
        {
            var exception = Assert.ThrowsException<ArgumentException>(() => new FinancialYear(financialYear));
            Assert.AreEqual("The year must be in the format yyyy-yy.", exception.Message);
        }

        [TestMethod]
        [DataRow("")]
        [DataRow(" ")]
        public void GetCalendarYearFromFinancialYearNew_EmptyString_ShouldThrowArgumentException(string financialYear)
        {
            var exception = Assert.ThrowsException<ArgumentException>(() => new FinancialYear(financialYear));
            Assert.AreEqual("The year must be in the format yyyy-yy.", exception.Message);
        }

        [TestMethod]
        public void GetCalendarYearFromFinancialYearNew_NullString_ShouldThrowArgumentException()
        {
            FinancialYear fy = default;
            var exception = Assert.ThrowsException<ArgumentException>(() => Util.GetCalendarYearFromFinancialYearNew(fy));
            Assert.AreEqual("Financial year cannot be null or empty (Parameter 'value')", exception.Message);
        }
    }
}