using Newtonsoft.Json;
using System.Collections.Generic;

namespace EPR.Calculator.Service.Function.Models
{    
    public class CalcResultOnePlusFourApportionmentJson
    {
        [JsonProperty(PropertyName = "oneFeeForLADisposalCosts")]
        public CalcResultOnePlusFourApportionmentDetailJson OneFeeForLADisposalCosts { get; set; } = new CalcResultOnePlusFourApportionmentDetailJson();

        [JsonProperty(PropertyName = "fourLADataPrepCharge")]
        public CalcResultOnePlusFourApportionmentDetailJson FourLADataPrepCharge { get; set; } = new CalcResultOnePlusFourApportionmentDetailJson();

        [JsonProperty(PropertyName = "totalOfonePlusFour")]
        public CalcResultOnePlusFourApportionmentDetailJson TotalOfonePlusFour { get; set; } = new CalcResultOnePlusFourApportionmentDetailJson();

        [JsonProperty(PropertyName = "onePlusFourApportionmentPercentages")]
        public CalcResultOnePlusFourApportionmentDetailJson OnePlusFourApportionmentPercentages { get; set; } = new CalcResultOnePlusFourApportionmentDetailJson();
    }
}
