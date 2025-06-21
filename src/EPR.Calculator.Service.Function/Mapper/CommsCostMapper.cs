using System.Data;
using System.Linq;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Models.JsonExporter;
using Microsoft.Azure.Amqp.Framing;

namespace EPR.Calculator.Service.Function.Mapper
{
    public class CommsCostMapper : ICommsCostMapper
    {
        public CalcResultCommsCostJson Map(CalcResultCommsCost calcResultCommsCost)
        {
            // Get the second row (index 1) which contains the actual values
            var dataRow = calcResultCommsCost.CalcResultCommsCostOnePlusFourApportionment.Skip(1).FirstOrDefault();

            if (dataRow == null)
            {
                return new CalcResultCommsCostJson { ParametersCommsCost = new ParametersCommsCost { Percentages = new OnePlusFourCommsCostApportionmentPercentages() } };
            }

            return new CalcResultCommsCostJson
            {
                ParametersCommsCost = new ParametersCommsCost
                {
                    Percentages = OnePlusFourCommsCostApportionmentPercentagesMap(dataRow)
                }
            };
        }

        private static OnePlusFourCommsCostApportionmentPercentages OnePlusFourCommsCostApportionmentPercentagesMap(CalcResultCommsCostOnePlusFourApportionment dataRow)
        {

            return new OnePlusFourCommsCostApportionmentPercentages
            {
                England = AppendPercent(dataRow.England),
                Wales = AppendPercent(dataRow.Wales),
                Scotland = AppendPercent(dataRow.Scotland),
                NorthernIreland = AppendPercent(dataRow.NorthernIreland),
                Total = AppendPercent(dataRow.Total)
            };
        }

        private static string AppendPercent(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return "0.00%";
            }

            string trimmedInput = input.Trim();
            return trimmedInput.EndsWith('%') ? trimmedInput : trimmedInput + "%";
        }
    }
}
