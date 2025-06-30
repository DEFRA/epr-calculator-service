using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using EPR.Calculator.Service.Common.Utils;

namespace EPR.Calculator.Service.Common.UnitTests.Utils
{
    /// <summary>
    /// Contains helper methods for comparing the contents of JsonNode objects to other values.
    /// </summary>
    public static class JsonNodeComparer
    {
        public static void AssertAreEqual(decimal expected, JsonNode? actual)
        {
            Assert.IsNotNull(actual, "Actual value should not be null.");
            Assert.AreEqual(
                CurrencyConverter.FormatCurrencyWithGbpSymbol(expected),
                actual.GetValue<string>(),
                $"Expected {expected} to be equal to {actual}");
        }

        public static void AssertAreEqual(string expected, JsonNode? actual)
        {
            Assert.IsNotNull(actual, "Actual value should not be null.");
            Assert.AreEqual(
                expected,
                actual.GetValue<string>(),
                $"Expected '{expected}' to be equal to '{actual}'");
        }
    }
}
