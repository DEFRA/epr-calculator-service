using System.Text;

namespace EPR.Calculator.Service.Function.UnitTests.Exporter.CsvExporter
{
    public static class CsvTestUtils {

        public static string[][] GetRows(StringBuilder csvContent){ 
            return csvContent.ToString()
                        .Split(Environment.NewLine)
                        .Select(r => r.Split(','))
                        .ToArray();
        }
        public static Dictionary<string, List<string>> GetColumnHeaderValues(string[] headers, string[] values)
        {
            return headers
                    .Select((h, i) => new
                    {
                        Key = h?.Trim().Trim('"'),
                        Value = (i < values.Length ? values[i] : null)?
                                    .Trim()
                                    .Trim('"') 
                                ?? string.Empty
                    })
                    .Where(x => !string.IsNullOrWhiteSpace(x.Key))
                    .GroupBy(x => x.Key!)
                    .ToDictionary(
                        g => g.Key!,
                        g => g.Select(x => x.Value).ToList()
                    );
        }

        public static int[] FindAllHeaderIndexes(string[] columnHeaders, string headerToFind)
        {
            return columnHeaders
                        .Select((h, i) => new { h = h?.Trim().Trim('"'), i = i })
                        .Where(x => !string.IsNullOrWhiteSpace(x.h))
                        .Where(x => x.h!.Equals(headerToFind))
                        .Select(x => x.i)
                        .ToArray();
        }
    }
}
