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
        public virtual object? ConvertToJsonByUKWide(CalcResultCommsCost data)
        {
            var ukWide = data.CalcResultCommsCostOnePlusFourApportionment
                .SingleOrDefault(r => r.Name == CalcResultCommsCostBuilder.TwoBCommsCostUkWide);

            return MapUkWide(ukWide!);
        }

        public virtual object? ConvertToJsonByCountry(CalcResultCommsCost data)
        {
            var byCountry = data.CalcResultCommsCostOnePlusFourApportionment
                .SingleOrDefault(r => r.Name == CalcResultCommsCostBuilder.TwoCCommsCostByCountry);

            return MapByCountry(byCountry!);
        }

        protected static object? MapUkWide(CalcResultCommsCostOnePlusFourApportionment record)
        {
            if (record == null)
            {
                return null;
            }

            return new
            {
                name = record.Name,
                englandCommsCostUKWide = record.England,
                walesCommsCostUKWide = record.Wales,
                scotlandCommsCostUKWide = record.Scotland,
                northernIrelandCommsCostUKWide = record.NorthernIreland,
                totalCommsCostUKWide = record.Total,
            };
        }

        protected static object? MapByCountry(CalcResultCommsCostOnePlusFourApportionment record)
        {
            if (record == null)
            {
                return null;
            }

            return new
            {
                name = record.Name,
                englandCommsCostByCountry = record.England,
                walesCommsCostByCountry = record.Wales,
                scotlandCommsCostByCountry = record.Scotland,
                northernIrelandCommsCostByCountry = record.NorthernIreland,
                totalCommsCostByCountry = record.Total,
            };
        }
    }
}
