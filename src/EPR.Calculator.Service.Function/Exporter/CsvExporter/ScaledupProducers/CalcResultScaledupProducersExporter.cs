using System.Text;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Enums;
using EPR.Calculator.Service.Function.Misc;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Exporter.CsvExporter.ScaledupProducers
{
    public interface ICalcResultScaledupProducersExporter
    {
        public void Export(
            CalcResultScaledupProducers calcResultScaledupProducers,
            IImmutableList<MaterialDetail> materials,
            bool showTotal,
            StringBuilder stringBuilder
        );
    }

    public class CalcResultScaledupProducersExporter : ICalcResultScaledupProducersExporter
    {
        private const int MaterialsBreakdownHeaderInitialColumnIndex = 10;
        private const int MaterialsBreakdownHeaderIncrementalColumnIndex = 10;


        public void Export(
            CalcResultScaledupProducers calcResultScaledupProducers,
            IImmutableList<MaterialDetail> materials,
            bool showTotal,
            StringBuilder stringBuilder
        )
        {
            // Add empty lines
            stringBuilder.AppendLine();
            stringBuilder.AppendLine();

            // Add headers
            PrepareScaledupProducersHeader(materials, stringBuilder);

            // Add data
            if (calcResultScaledupProducers.ScaledupProducers?.Any() == true)
            {
                foreach (var producer in calcResultScaledupProducers.ScaledupProducers)
                    producer.ScaledupProducerTonnageByMaterial = GetTonnages(producer.PomData, materials);

                AppendScaledupProducers(calcResultScaledupProducers.ScaledupProducers, materials, stringBuilder);
                if (showTotal)
                {
                    AppendOverallTotalRow(GetOverallTotalRow(calcResultScaledupProducers.ScaledupProducers, materials), materials, stringBuilder);
                }
            }
            else
            {
                stringBuilder.AppendLine(CsvSanitiser.SanitiseData(CalcResultScaledupProducerHeaders.NoScaledupProducers));
            }
        }

        private static void AppendScaledupProducers(IEnumerable<CalcResultScaledupProducer> producers, IReadOnlyCollection<MaterialDetail> materials, StringBuilder csvContent)
        {
            foreach (var producer in producers)
            {
                csvContent.Append(CsvSanitiser.SanitiseData(producer.ProducerId));
                csvContent.Append(CsvSanitiser.SanitiseData(producer.SubsidiaryId));
                csvContent.Append(CsvSanitiser.SanitiseData(producer.ProducerName));
                csvContent.Append(CsvSanitiser.SanitiseData(producer.TradingName));
                csvContent.Append(CsvSanitiser.SanitiseData(producer.Level));
                csvContent.Append(CsvSanitiser.SanitiseData(producer.SubmissionPeriodCode));
                csvContent.Append(CsvSanitiser.SanitiseData(producer.DaysInSubmissionPeriod != -1 ? producer.DaysInSubmissionPeriod.ToString() : string.Empty));
                csvContent.Append(CsvSanitiser.SanitiseData(producer.DaysInWholePeriod != -1 ? producer.DaysInWholePeriod.ToString() : string.Empty));
                csvContent.Append(CsvSanitiser.SanitiseData(producer.ScaleupFactor == -1 ? CommonConstants.Totals : producer.ScaleupFactor.ToString()));

                AppendScaledupProducerTonnageByMaterial(csvContent, materials, producer);

                csvContent.AppendLine();
            }
        }

        private static void AppendOverallTotalRow(CalcResultScaledupProducer totalProducer, IReadOnlyCollection<MaterialDetail> materials, StringBuilder csvContent)
        {
            csvContent.Append(new string(CommonConstants.CsvFileDelimiter[0], 8));
            csvContent.Append(CsvSanitiser.SanitiseData(CommonConstants.Totals));
            AppendScaledupProducerTonnageByMaterial(csvContent, materials, totalProducer);
            csvContent.AppendLine();
        }

        private static void AppendScaledupProducerTonnageByMaterial(StringBuilder csvContent, IReadOnlyCollection<MaterialDetail> materials, CalcResultScaledupProducer producer)
        {
            // Iterate the materials rather than the dictionary, so the data columns always
            // line up with the headers, which are also generated from the materials.
            foreach (var material in materials)
            {
                // Per producer rows are keyed by material code, the overall total row by material name.
                if (!producer.ScaledupProducerTonnageByMaterial.TryGetValue(material.Code, out var tonnage)
                    && !producer.ScaledupProducerTonnageByMaterial.TryGetValue(material.Name, out tonnage))
                {
                    continue;
                }

                var isGlass = material.Code == MaterialCodes.Glass;

                csvContent.Append(CsvSanitiser.SanitiseData(tonnage.ReportedHouseholdPackagingWasteTonnage, DecimalPlaces.Three, DecimalFormats.F3));
                csvContent.Append(CsvSanitiser.SanitiseData(tonnage.ReportedPublicBinTonnage, DecimalPlaces.Three, DecimalFormats.F3));

                if (isGlass)
                {
                    csvContent.Append(CsvSanitiser.SanitiseData(tonnage.HouseholdDrinksContainersTonnageGlass, DecimalPlaces.Three, DecimalFormats.F3));
                }

                csvContent.Append(CsvSanitiser.SanitiseData(tonnage.TotalReportedTonnage, DecimalPlaces.Three, DecimalFormats.F3));
                csvContent.Append(CsvSanitiser.SanitiseData(tonnage.ReportedSelfManagedConsumerWasteTonnage, DecimalPlaces.Three, DecimalFormats.F3));
                csvContent.Append(CsvSanitiser.SanitiseData(tonnage.NetReportedTonnage, DecimalPlaces.Three, DecimalFormats.F3));
                csvContent.Append(CsvSanitiser.SanitiseData(tonnage.ScaledupReportedHouseholdPackagingWasteTonnage, DecimalPlaces.Three, DecimalFormats.F3));
                csvContent.Append(CsvSanitiser.SanitiseData(tonnage.ScaledupReportedPublicBinTonnage, DecimalPlaces.Three, DecimalFormats.F3));

                if (isGlass)
                {
                    csvContent.Append(CsvSanitiser.SanitiseData(tonnage.ScaledupHouseholdDrinksContainersTonnageGlass, DecimalPlaces.Three, DecimalFormats.F3));
                }

                csvContent.Append(CsvSanitiser.SanitiseData(tonnage.ScaledupTotalReportedTonnage, DecimalPlaces.Three, DecimalFormats.F3));
                csvContent.Append(CsvSanitiser.SanitiseData(tonnage.ScaledupReportedSelfManagedConsumerWasteTonnage, DecimalPlaces.Three, DecimalFormats.F3));
                csvContent.Append(CsvSanitiser.SanitiseData(tonnage.ScaledupNetReportedTonnage, DecimalPlaces.Three, DecimalFormats.F3));
            }
        }

        public static CalcResultScaledupProducer GetOverallTotalRow(
            IEnumerable<CalcResultScaledupProducer> producers,
            IEnumerable<MaterialDetail> materials
        )
        {
            var dict = ImmutableDictionary.CreateBuilder<string, CalcResultScaledupProducerTonnage>();

            var allMaterialDict = producers
                .Where(x => !x.IsSubtotalRow)
                .Select(x => x.ScaledupProducerTonnageByMaterial)
                .ToImmutableList();

            foreach (var material in materials)
            {
                var materialValues = allMaterialDict.Where(x => x.ContainsKey(material.Code)).Select(x => x[material.Code]).ToList();

                var totalRow = new CalcResultScaledupProducerTonnage
                {
                    ReportedHouseholdPackagingWasteTonnage = materialValues.Sum(x => x.ReportedHouseholdPackagingWasteTonnage),
                    ReportedPublicBinTonnage = materialValues.Sum(x => x.ReportedPublicBinTonnage),
                    HouseholdDrinksContainersTonnageGlass = material.Code == MaterialCodes.Glass ? materialValues.Sum(x => x.HouseholdDrinksContainersTonnageGlass) : 0,
                    TotalReportedTonnage = materialValues.Sum(x => x.TotalReportedTonnage),
                    ReportedSelfManagedConsumerWasteTonnage = materialValues.Sum(x => x.ReportedSelfManagedConsumerWasteTonnage),
                    NetReportedTonnage = materialValues.Sum(x => x.NetReportedTonnage),
                    ScaledupReportedHouseholdPackagingWasteTonnage = materialValues.Sum(x => x.ScaledupReportedHouseholdPackagingWasteTonnage),
                    ScaledupReportedPublicBinTonnage = materialValues.Sum(x => x.ScaledupReportedPublicBinTonnage),
                    ScaledupHouseholdDrinksContainersTonnageGlass = material.Code == MaterialCodes.Glass ? materialValues.Sum(x => x.ScaledupHouseholdDrinksContainersTonnageGlass) : 0,
                    ScaledupTotalReportedTonnage = materialValues.Sum(x => x.ScaledupTotalReportedTonnage),
                    ScaledupReportedSelfManagedConsumerWasteTonnage = materialValues.Sum(x => x.ScaledupReportedSelfManagedConsumerWasteTonnage),
                    ScaledupNetReportedTonnage = materialValues.Sum(x => x.ScaledupNetReportedTonnage)
                };

                dict.Add(material.Name, totalRow);
            }

            return new CalcResultScaledupProducer
            {
                Level = string.Empty,
                SubmissionPeriodCode = string.Empty,
                ProducerId = 0,
                SubsidiaryId = null,
                ProducerName = null,
                TradingName = null,
                IsSubtotalRow = false,
                DaysInSubmissionPeriod = 0,
                DaysInWholePeriod = 0,
                ScaleupFactor = 0,
                ScaledupProducerTonnageByMaterial  = dict.ToImmutableDictionary()
            };
        }

        private static void PrepareScaledupProducersHeader(IImmutableList<MaterialDetail> materials, StringBuilder csvContent)
        {
            // Add scaledup producer header
            csvContent.AppendLine(CsvSanitiser.SanitiseData(CalcResultScaledupProducerHeaders.ScaledupProducers));
            csvContent.AppendLine();

            // Add material breakdown header
            WriteScaledupProducersSecondaryHeaders(GetMaterialsBreakdownHeader(materials), csvContent);

            // Add column header
            WriteScaledupProducersColumnHeaders(GetColumnHeaders(materials), csvContent);
            csvContent.AppendLine();
        }

        private sealed class CalcResultScaledupProducerHeader
        {
            required public string Name { get; set; }

            public int ColumnIndex { get; set; }
        }

        private static void WriteScaledupProducersSecondaryHeaders(IReadOnlyCollection<CalcResultScaledupProducerHeader> headers, StringBuilder csvContent)
        {
            var maxColumnSize = headers.MaxBy(h => h.ColumnIndex)?.ColumnIndex ?? throw new ArgumentException("No headers specified");

            var headerRows = new string[maxColumnSize];
            foreach (var item in headers)
            {
                headerRows[item.ColumnIndex - 1] = item.Name;
            }

            var headerRow = string.Join("", headerRows.Select(x => CsvSanitiser.SanitiseData(x)));
            csvContent.AppendLine(headerRow);
        }

        private static void WriteScaledupProducersColumnHeaders(IReadOnlyCollection<CalcResultScaledupProducerHeader> columnHeaders, StringBuilder csvContent)
        {
            foreach (var item in columnHeaders)
            {
                csvContent.Append(CsvSanitiser.SanitiseData(item.Name));
            }
        }

        private static ImmutableList<CalcResultScaledupProducerHeader> GetMaterialsBreakdownHeader(IEnumerable<MaterialDetail> materials)
        {
            var materialsBreakdownHeaders = ImmutableList.CreateBuilder<CalcResultScaledupProducerHeader>();
            var columnIndex = MaterialsBreakdownHeaderInitialColumnIndex;

            materialsBreakdownHeaders.Add(new CalcResultScaledupProducerHeader
            {
                Name = CalcResultScaledupProducerHeaders.EachSubmissionForTheYear,
                ColumnIndex = 1,
            });

            foreach (var material in materials)
            {
                materialsBreakdownHeaders.Add(new CalcResultScaledupProducerHeader
                {
                    Name = $"{material.Name} Breakdown",
                    ColumnIndex = columnIndex,
                });

                columnIndex = material.Code == MaterialCodes.Glass
                    ? columnIndex + MaterialsBreakdownHeaderIncrementalColumnIndex + 2
                    : columnIndex + MaterialsBreakdownHeaderIncrementalColumnIndex;
            }

            return materialsBreakdownHeaders.ToImmutable();
        }

        private static ImmutableList<CalcResultScaledupProducerHeader> GetColumnHeaders(IReadOnlyCollection<MaterialDetail> materials)
        {
            var columnHeaders = ImmutableList.CreateBuilder<CalcResultScaledupProducerHeader>();

            columnHeaders.AddRange([
                new CalcResultScaledupProducerHeader { Name = CalcResultScaledupProducerHeaders.ProducerId },
                new CalcResultScaledupProducerHeader { Name = CalcResultScaledupProducerHeaders.SubsidiaryId },
                new CalcResultScaledupProducerHeader { Name = CalcResultScaledupProducerHeaders.ProducerOrSubsidiaryName },
                new CalcResultScaledupProducerHeader { Name = CalcResultScaledupProducerHeaders.TradingName },
                new CalcResultScaledupProducerHeader { Name = CalcResultScaledupProducerHeaders.Level },
                new CalcResultScaledupProducerHeader { Name = CalcResultScaledupProducerHeaders.SubmissionPeriodCode },
                new CalcResultScaledupProducerHeader { Name = CalcResultScaledupProducerHeaders.DaysInSubmissionPeriod },
                new CalcResultScaledupProducerHeader { Name = CalcResultScaledupProducerHeaders.DaysInWholePeriod },
                new CalcResultScaledupProducerHeader { Name = CalcResultScaledupProducerHeaders.ScaleupFactor }
            ]);

            foreach (var material in materials)
            {
                var columnHeadersList = new List<CalcResultScaledupProducerHeader>
                {
                    new CalcResultScaledupProducerHeader { Name = CalcResultScaledupProducerHeaders.HouseholdPackagingWasteTonnage },
                    new CalcResultScaledupProducerHeader { Name = CalcResultScaledupProducerHeaders.PublicBinTonnage },
                    new CalcResultScaledupProducerHeader { Name = CalcResultScaledupProducerHeaders.TotalTonnage },
                    new CalcResultScaledupProducerHeader { Name = CalcResultScaledupProducerHeaders.SelfManagedConsumerWasteTonnage },
                    new CalcResultScaledupProducerHeader { Name = CalcResultScaledupProducerHeaders.NetTonnage },
                    new CalcResultScaledupProducerHeader { Name = CalcResultScaledupProducerHeaders.ScaledupHouseholdPackagingWasteTonnage },
                    new CalcResultScaledupProducerHeader { Name = CalcResultScaledupProducerHeaders.ScaledupPublicBinTonnage },
                    new CalcResultScaledupProducerHeader { Name = CalcResultScaledupProducerHeaders.ScaledupTotalTonnage },
                    new CalcResultScaledupProducerHeader { Name = CalcResultScaledupProducerHeaders.ScaledupSelfManagedConsumerWasteTonnage },
                    new CalcResultScaledupProducerHeader { Name = CalcResultScaledupProducerHeaders.ScaledupNetTonnage },
                };

                if (material.Code == MaterialCodes.Glass)
                {
                    columnHeadersList.Insert(2, new CalcResultScaledupProducerHeader { Name = CalcResultScaledupProducerHeaders.HouseholdDrinksContainersTonnageGlass });
                    columnHeadersList.Insert(8, new CalcResultScaledupProducerHeader { Name = CalcResultScaledupProducerHeaders.ScaledupHouseholdDrinksContainersTonnageGlass });
                }

                columnHeaders.AddRange(columnHeadersList);
            }

            return columnHeaders.ToImmutable();
        }

        public static ImmutableDictionary<string, CalcResultScaledupProducerTonnage> GetTonnages(
            IReadOnlyCollection<ScaledupPomEntry> pomData,
            IReadOnlyCollection<MaterialDetail> materials
        )
        {
            var scaledupProducerTonnages = ImmutableDictionary.CreateBuilder<string, CalcResultScaledupProducerTonnage>();

            foreach (var material in materials)
            {
                var materialPomData = pomData.Where(e => e.MaterialId == material.Id).ToImmutableList();

                var hh  = materialPomData.SingleOrDefault(e => e.PackagingType == PackagingTypes.Household) ?? ScaledupPomEntry.Zero;
                var pb  = materialPomData.SingleOrDefault(e => e.PackagingType == PackagingTypes.PublicBin) ?? ScaledupPomEntry.Zero;
                var cw  = materialPomData.SingleOrDefault(e => e.PackagingType == PackagingTypes.ConsumerWaste) ?? ScaledupPomEntry.Zero;
                var hdc = materialPomData.SingleOrDefault(e => material.Code == MaterialCodes.Glass && e.PackagingType == PackagingTypes.HouseholdDrinksContainers) ?? ScaledupPomEntry.Zero;

                var totalReportedTonnage         = hh.Tonnage + pb.Tonnage + hdc.Tonnage;
                var netReportedTonnage           = totalReportedTonnage - cw.Tonnage;
                var scaledupTotalReportedTonnage = hh.ScaledTonnage + pb.ScaledTonnage + hdc.ScaledTonnage;
                var scaledupNetReportedTonnage   = scaledupTotalReportedTonnage - cw.ScaledTonnage;

                scaledupProducerTonnages.Add(material.Code, new CalcResultScaledupProducerTonnage
                {
                    ReportedHouseholdPackagingWasteTonnage          = hh.Tonnage,
                    ReportedPublicBinTonnage                        = pb.Tonnage,
                    ReportedSelfManagedConsumerWasteTonnage         = cw.Tonnage,
                    HouseholdDrinksContainersTonnageGlass           = hdc.Tonnage,
                    ScaledupReportedHouseholdPackagingWasteTonnage  = hh.ScaledTonnage,
                    ScaledupReportedPublicBinTonnage                = pb.ScaledTonnage,
                    ScaledupReportedSelfManagedConsumerWasteTonnage = cw.ScaledTonnage,
                    ScaledupHouseholdDrinksContainersTonnageGlass   = hdc.ScaledTonnage,
                    TotalReportedTonnage                            = totalReportedTonnage,
                    NetReportedTonnage                              = netReportedTonnage,
                    ScaledupTotalReportedTonnage                    = scaledupTotalReportedTonnage,
                    ScaledupNetReportedTonnage                      = scaledupNetReportedTonnage
                });
            }

            return scaledupProducerTonnages.ToImmutable();
        }
    }
}
