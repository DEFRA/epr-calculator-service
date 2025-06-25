using EPR.Calculator.Service.Function.Builder.CommsCost;
using EPR.Calculator.Service.Function.Models;
using Microsoft.Azure.Amqp.Framing;
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
    public class CalcResultCommsCostOnePlusFourApportionmentExporter : ICalcResultCommsCostOnePlusFourApportionmentExporter
    {
        public virtual string ConvertToJsonByUKWide(CalcResultCommsCost data)
        {
            var ukWide = data.CalcResultCommsCostOnePlusFourApportionment
                .Single(r => r.Name == CalcResultCommsCostBuilder.TwoBCommsCostUkWide);

            return
                JsonSerializer.Serialize(
                new
                {
                    calcResult2bCommsDataByUkWide = MapUkWide(ukWide),
                },
                GetJsonSerializerOptions());
        }

        public virtual string ConvertToJsonByCountry(CalcResultCommsCost data)
        {
            var byCountry = data.CalcResultCommsCostOnePlusFourApportionment
                .Single(r => r.Name == CalcResultCommsCostBuilder.TwoCCommsCostByCountry);

            return
                JsonSerializer.Serialize(
                new
                {
                    calcResult2cCommsDataByCountry = MapByCountry(byCountry),
                },
                GetJsonSerializerOptions());
        }

        protected static object MapUkWide(CalcResultCommsCostOnePlusFourApportionment record)
        => new
        {
            name = record.Name,
            englandCommsCostUKWide = record.England,
            walesCommsCostUKWide = record.Wales,
            scotlandCommsCostUKWide = record.Scotland,
            northernIrelandCommsCostUKWide = record.NorthernIreland,
            totalCommsCostUKWide = record.Total,
        };

        protected static object MapByCountry(CalcResultCommsCostOnePlusFourApportionment record)
        => new
        {
            name = record.Name,
            englandCommsCostByCountry = record.England,
            walesCommsCostByCountry = record.Wales,
            scotlandCommsCostByCountry = record.Scotland,
            northernIrelandCommsCostByCountry = record.NorthernIreland,
            totalCommsCostByCountry = record.Total,
        };

        protected static JsonSerializerOptions GetJsonSerializerOptions()
        {
            return new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true,
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
            };
        }
    }
}
