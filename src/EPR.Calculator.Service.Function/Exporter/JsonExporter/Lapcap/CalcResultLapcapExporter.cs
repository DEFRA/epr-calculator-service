using EPR.Calculator.Service.Function.Builder.Lapcap;
using EPR.Calculator.Service.Function.Models;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace EPR.Calculator.Service.Function.Exporter.JsonExporter.Lapcap
{
    /// <summary>
    /// Converts lapcap data to JSON string format.
    /// </summary>
    public class CalcResultLapcapExporter : ICalcResultLapcapExporter
    {
        /// <summary>
        /// Records from the lapcap data that should be seperated into their own sub-sections
        /// rather than included in the main .
        /// </summary>
        private static readonly IEnumerable<string> SeperatedRecords = [
            CalcResultLapcapDataBuilder.Total,
            CalcResultLapcapDataBuilder.CountryApportionment,
            "Material"];

        /// <inheritdoc/>
        public object Export(CalcResultLapcapData data)
            =>  new CalcResultLapcapDataToSerialise(data);

        /// <summary>
        /// Holds the lapcap data in a structure that will be serialised to the expected JSON layout.
        /// </summary>
        private readonly record struct CalcResultLapcapDataToSerialise(
            string Name,
            IEnumerable<CalcResultLapcapDataDetailsToSerialise> CalcResultLapcapDataDetails,
            CalcResultLapcapDataDetailsToSerialiseWithoutName CalcResultLapcapDataTotal,
            CalcResultLapcapDataDetailsToSerialiseWithoutName OneCountryApportionmentPercentages)
        {
            public CalcResultLapcapDataToSerialise(CalcResultLapcapData data)
                : this(
                      data.Name,
                      
                      // The main list of records.
                      data.CalcResultLapcapDataDetails
                      .Where(record => !SeperatedRecords.Contains(record.Name))
                        .Select(details => new CalcResultLapcapDataDetailsToSerialise(details)),
                      
                      // The total record.
                      new CalcResultLapcapDataDetailsToSerialiseWithoutName(data.CalcResultLapcapDataDetails
                        .Single(record => record.Name == CalcResultLapcapDataBuilder.Total)),

                      // The country apportionment record.
                      new CalcResultLapcapDataDetailsToSerialiseWithoutName(data.CalcResultLapcapDataDetails
                        .Single(record => record.Name == CalcResultLapcapDataBuilder.CountryApportionment)))
            {
            }
        }

        /// <summary>
        /// Holds the lapcap data in a structure that will be serialised to the expected JSON layout.
        /// </summary>
        private readonly record struct CalcResultLapcapDataDetailsToSerialise(
            string MaterialName,
            string EnglandLaDisposalCost,
            string WalesLaDisposalCost,
            string ScotlandLaDisposalCost,
            string NorthernIrelandLaDisposalCost,
            string OneLaDisposalLaCostTotal)
        {
            public CalcResultLapcapDataDetailsToSerialise(CalcResultLapcapDataDetails data)
                : this(data.Name,
                      data.EnglandDisposalCost,
                      data.WalesDisposalCost,
                      data.ScotlandDisposalCost,
                      data.NorthernIrelandDisposalCost,
                      data.TotalDisposalCost)
            {
            }
        }

        /// <summary>
        /// Holds the lapcap data in a structure that will be serialised to the expected JSON layout.
        /// Excludes the name field - used for total and country apportment records.
        /// </summary>
        private readonly record struct CalcResultLapcapDataDetailsToSerialiseWithoutName(
            string EnglandLaDisposalCost,
            string WalesLaDisposalCost,
            string ScotlandLaDisposalCost,
            string NorthernIrelandLaDisposalCost,
            string OneLaDisposalCostTotal)
        {
            public CalcResultLapcapDataDetailsToSerialiseWithoutName(CalcResultLapcapDataDetails data)
                : this(data.EnglandDisposalCost,
                      data.WalesDisposalCost,
                      data.ScotlandDisposalCost,
                      data.NorthernIrelandDisposalCost,
                      data.TotalDisposalCost)
            {
            }
        }
    }
}