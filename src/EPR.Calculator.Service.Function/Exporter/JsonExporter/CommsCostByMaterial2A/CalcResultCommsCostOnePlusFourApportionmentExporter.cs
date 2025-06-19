using EPR.Calculator.Service.Function.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using System.Threading.Tasks;

namespace EPR.Calculator.Service.Function.Exporter.JsonExporter.CommsCostByMaterial2A
{
    public class CalcResultCommsCostOnePlusFourApportionmentExporter
    {
        public string ConvertToJson(CalcResultCommsCostOnePlusFourApportionment data)
            => JsonSerializer.Serialize(
                new
                {
                    calcResult2cCommsDataByCountry = new
                    {
                        name = data.Name,
                        englandCommsCostByCountry = data.England,
                        walesCommsCostByCountry = data.Wales,
                        scotlandCommsCostByCountry = data.Scotland,
                        northernIrelandCommsCostByCountry = data.NorthernIreland,
                        totalCommsCostByCountry = data.Total,
                    }
                },
                new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = true,
                    Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
                });
    }
}
