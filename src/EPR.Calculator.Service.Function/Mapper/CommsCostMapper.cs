using System.Data;
using System.Linq;
using EPR.Calculator.Service.Function.Builder.CommsCost;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Models.JsonExporter;
using Microsoft.Azure.Amqp.Framing;

namespace EPR.Calculator.Service.Function.Mapper
{
    public class CommsCostMapper : ICommsCostMapper
    {
        public CalcResultCommsCostJson Map(CalcResultCommsCost calcResultCommsCost)
        {
            var onePlusFourApportionment = calcResultCommsCost.CalcResultCommsCostOnePlusFourApportionment
                .SingleOrDefault(x => x.Name == CalcResultCommsCostBuilder.OnePlusFourApportionment);

            if (onePlusFourApportionment == null)
            {
                return new CalcResultCommsCostJson {  
                    OnePlusFourCommsCostApportionmentPercentages = new OnePlusFourCommsCostApportionmentPercentages() 
                };
            }

            return new CalcResultCommsCostJson
            {
                OnePlusFourCommsCostApportionmentPercentages = OnePlusFourCommsCostApportionmentPercentagesMap(onePlusFourApportionment)
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
