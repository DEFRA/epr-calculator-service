using EPR.Calculator.Service.Function.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPR.Calculator.Service.Function.Exporter.CsvExporter.CommsCost
{
    public interface ICommsCostExporter
    {
        void Export(CalcResultCommsCost communicationCost, StringBuilder csvContent);
    }
}