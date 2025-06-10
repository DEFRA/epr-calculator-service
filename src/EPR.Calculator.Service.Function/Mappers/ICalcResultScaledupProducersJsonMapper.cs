using EPR.Calculator.Service.Function.Exporter.JsonExporter.Model;
using EPR.Calculator.Service.Function.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPR.Calculator.Service.Function.Mappers
{
    public interface ICalcResultScaledupProducersJsonMapper
    {
        CalcResultScaledupProducerJson Map(CalcResultScaledupProducers calcResultScaledupProducers);
    }
}
