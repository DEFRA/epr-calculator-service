using System.Text;
using EPR.Calculator.Service.Function.Exporter.CsvExporter;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.UnitTests.Exporter.CsvExporter
{
    [TestClass]
    public class CalcResultOnePlusFourApportionmentExporterTests
    {
        private CalcResultOnePlusFourApportionmentExporter _testClass = new CalcResultOnePlusFourApportionmentExporter();
        private CalcResultOnePlusFourApportionment calcResult1Plus4Apportionment;

        public CalcResultOnePlusFourApportionmentExporterTests()
        {
            calcResult1Plus4Apportionment = new CalcResultOnePlusFourApportionment
            {
                CalcResultOnePlusFourApportionmentDetails = new List<CalcResultOnePlusFourApportionmentDetail>
                {
                    // TODO these should be modelled - fixed entries
                    new CalcResultOnePlusFourApportionmentDetail
                    {
                        Name = "1 Fee for LA Disposal Costs",
                        EnglandTotal         = 109800,
                        WalesTotal           = 24750,
                        ScotlandTotal        = 49300,
                        NorthernIrelandTotal = 19300,
                        Total                = 203150,
                    },
                    new CalcResultOnePlusFourApportionmentDetail
                    {
                        Name = "4 LA Data Prep Charge",
                        EnglandTotal         = 16000,
                        WalesTotal           = 7000,
                        ScotlandTotal        = 9000,
                        NorthernIrelandTotal = 4500,
                        Total                = 36500
                    },
                    new CalcResultOnePlusFourApportionmentDetail
                    {
                        Name = "Total of 1 + 4",
                        EnglandTotal         = 125800,
                        WalesTotal           = 31750,
                        ScotlandTotal        = 58300,
                        NorthernIrelandTotal = 23800,
                        Total                = 239650
                    },
                    // TODO reuse CountryApportionmentData
                    new CalcResultOnePlusFourApportionmentDetail
                    {
                        Name = "1 + 4 Apportionment %s",
                        EnglandTotal         = 0.5249321928m,
                        WalesTotal           = 0.1324848738m,
                        ScotlandTotal        = 0.2432714375m,
                        NorthernIrelandTotal = 0.0993114959m,
                        Total                = 1
                    }
                },
            };
        }

        [TestMethod]
        public void OnePlusFourApportionment_CanCallExport()
        {
            // Arrange
            var csvContent = new StringBuilder();

            // Act
            _testClass.Export(calcResult1Plus4Apportionment, csvContent);

            // Assert
            //var result = csvContent.ToString();
            //var rows = result.Split(Environment.NewLine);
            //Assert.AreEqual(7, rows.Length);
            //Assert.AreEqual("Apportionment", rows[2]);

            var result = csvContent.ToString().Split("\n").Select(s => s.TrimEnd(',')).ToArray();
            Console.WriteLine(string.Join("\n", result));

            var expected = new[] {
                new string[] {},
                new string[] {},
                new[] { "1 + 4 Apportionment %s" },
                new[] { "",
                        "England",
                        "Wales",
                        "Scotland",
                        "Northern Ireland",
                        "Total"
                },
                new[] { "1 Fee for LA Disposal Costs",   "£109800.00",   "£24750.00",   "£49300.00",  "£19300.00", "£203150.00" },
                new[] { "4 LA Data Prep Charge"      ,    "£16000.00",    "£7000.00",    "£9000.00",   "£4500.00",  "£36500.00" },
                new[] { "Total of 1 + 4"             ,   "£125800.00",   "£31750.00",   "£58300.00",  "£23800.00", "£239650.00" },
                new[] { "1 + 4 Apportionment %s"     , "52.49321928%","13.24848738%","24.32714375%","9.93114959%",       "100%" },
                new string[] { }
            };


            CsvTestUtils.AssertCsv(expected, result);
        }
    }
}
