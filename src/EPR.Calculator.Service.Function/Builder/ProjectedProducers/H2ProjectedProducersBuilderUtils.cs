using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Builder.ProjectedProducers;

public static class H2ProjectedProducersBuilderUtils
{
    public static List<CalcResultH2ProjectedProducer> GetProjectedProducers(List<ProducerReportedMaterialsForSubmissionPeriod> reportedMaterials, ImmutableArray<MaterialDto> materials)
    {
        return reportedMaterials.Select(rm => new CalcResultH2ProjectedProducer
        {
            ProducerId = rm.ProducerId,
            SubsidiaryId = rm.SubsidiaryId,
            Level = string.Empty, // Level will be set later when subtotals are added
            SubmissionPeriodCode = rm.SubmissionPeriod,
            H2ProjectedTonnageByMaterial = GetProjectedTonnages(materials, rm.ReportedMaterials)
        }).ToList();
    }

    private static Dictionary<string, CalcResultH2ProjectedProducerMaterialTonnage> GetProjectedTonnages(ImmutableArray<MaterialDto> materials, List<ProducerReportedMaterial> reportedMaterials)
    {
        return materials.ToDictionary(m => m.Code, m => GetProjectedTonnage(m, reportedMaterials.Where(rm => rm.MaterialId == m.Id).ToList()));
    }

    private static CalcResultH2ProjectedProducerMaterialTonnage GetProjectedTonnage(MaterialDto material, List<ProducerReportedMaterial> reportedMaterials)
    {
        RAMTonnage GetProjectedRam(RAMTonnage tonnage, decimal tonnageWithoutRam) =>
            new()
            {
                Tonnage = tonnage.Tonnage,
                RedTonnage = tonnage.RedTonnage + tonnageWithoutRam,
                AmberTonnage = tonnage.AmberTonnage,
                GreenTonnage = tonnage.GreenTonnage,
                RedMedicalTonnage = tonnage.RedMedicalTonnage,
                AmberMedicalTonnage = tonnage.AmberMedicalTonnage,
                GreenMedicalTonnage = tonnage.GreenMedicalTonnage
            };

        var householdRAMTonnage = CalcResultProjectedProducersBuilder.GetRAMTonnage(PackagingTypes.Household, reportedMaterials);
        var publicBinRAMTonnage = CalcResultProjectedProducersBuilder.GetRAMTonnage(PackagingTypes.PublicBin, reportedMaterials);
        var hdcRAMTonnage = material.Code == MaterialCodes.Glass ? CalcResultProjectedProducersBuilder.GetRAMTonnage(PackagingTypes.HouseholdDrinksContainers, reportedMaterials) : null;
        var hhWithoutRam = CalcResultProjectedProducersBuilder.TonnageWithoutRAM(householdRAMTonnage);
        var pbWithoutRam = CalcResultProjectedProducersBuilder.TonnageWithoutRAM(publicBinRAMTonnage);
        decimal? hdcWithoutRam = hdcRAMTonnage != null ? CalcResultProjectedProducersBuilder.TonnageWithoutRAM(hdcRAMTonnage) : null;

        return new CalcResultH2ProjectedProducerMaterialTonnage
        {
            HouseholdRAMTonnage = householdRAMTonnage,
            PublicBinRAMTonnage = publicBinRAMTonnage,
            HouseholdDrinksContainerRAMTonnage = hdcRAMTonnage,
            HouseholdTonnageWithoutRAM = hhWithoutRam,
            PublicBinTonnageWithoutRAM = pbWithoutRam,
            HouseholdDrinksContainerTonnageWithoutRAM = hdcWithoutRam,
            ProjectedHouseholdRAMTonnage = GetProjectedRam(householdRAMTonnage, hhWithoutRam),
            ProjectedPublicBinRAMTonnage = GetProjectedRam(publicBinRAMTonnage, pbWithoutRam),
            ProjectedHouseholdDrinksContainerRAMTonnage = hdcRAMTonnage != null ? GetProjectedRam(hdcRAMTonnage, hdcWithoutRam!.Value) : null,
            TotalTonnage = householdRAMTonnage.Tonnage + publicBinRAMTonnage.Tonnage + (hdcRAMTonnage?.Tonnage ?? 0)
        };
    }

    public static CalcResultH2ProjectedProducer CreateParentProducer(CalcResultH2ProjectedProducer p)
    {
        return new CalcResultH2ProjectedProducer
        {
            ProducerId = p.ProducerId,
            SubsidiaryId = null,
            Level = CommonConstants.LevelOne.ToString(),
            SubmissionPeriodCode = p.SubmissionPeriodCode,
            IsSubtotal = true,
            H2ProjectedTonnageByMaterial = p.H2ProjectedTonnageByMaterial
        };
    }

    public static CalcResultH2ProjectedProducer SumProducerGroupTonnages(IEnumerable<CalcResultH2ProjectedProducer> prodGroup)
    {
        var producer = prodGroup.First();
        var sumRam = (string matKey, Func<CalcResultProjectedProducerMaterialTonnage, RAMTonnage?> tonnageFunc) =>
            CalcResultProjectedProducersBuilder.SumRAMTonnages(prodGroup.Cast<ICalcResultProjectedProducer>().ToList(), matKey, tonnageFunc);

        return new CalcResultH2ProjectedProducer
        {
            ProducerId = producer.ProducerId,
            SubsidiaryId = null,
            Level = CommonConstants.LevelOne.ToString(),
            SubmissionPeriodCode = producer.SubmissionPeriodCode,
            IsSubtotal = true,
            H2ProjectedTonnageByMaterial = producer.H2ProjectedTonnageByMaterial.ToDictionary(
                kvp => kvp.Key,
                kvp => new CalcResultH2ProjectedProducerMaterialTonnage
                {
                    HouseholdRAMTonnage = sumRam(kvp.Key, p => p.HouseholdRAMTonnage),
                    PublicBinRAMTonnage = sumRam(kvp.Key, p => p.PublicBinRAMTonnage),
                    HouseholdDrinksContainerRAMTonnage = kvp.Key == MaterialCodes.Glass ? sumRam(kvp.Key, p => p.HouseholdDrinksContainerRAMTonnage) : null,
                    HouseholdTonnageWithoutRAM = prodGroup.Sum(p => p.H2ProjectedTonnageByMaterial[kvp.Key].HouseholdTonnageWithoutRAM),
                    PublicBinTonnageWithoutRAM = prodGroup.Sum(p => p.H2ProjectedTonnageByMaterial[kvp.Key].PublicBinTonnageWithoutRAM),
                    HouseholdDrinksContainerTonnageWithoutRAM = kvp.Key == MaterialCodes.Glass ? prodGroup.Sum(p => p.H2ProjectedTonnageByMaterial[kvp.Key].HouseholdDrinksContainerTonnageWithoutRAM ?? 0) : null,
                    ProjectedHouseholdRAMTonnage = sumRam(kvp.Key, p => p.ProjectedHouseholdRAMTonnage),
                    ProjectedPublicBinRAMTonnage = sumRam(kvp.Key, p => p.ProjectedPublicBinRAMTonnage),
                    ProjectedHouseholdDrinksContainerRAMTonnage = kvp.Key == MaterialCodes.Glass ? sumRam(kvp.Key, p => p.ProjectedHouseholdDrinksContainerRAMTonnage) : null,
                    TotalTonnage = prodGroup.Sum(p => p.H2ProjectedTonnageByMaterial[kvp.Key].TotalTonnage)
                })
        };
    }


    public static ProjectedProducersHeaders GetProjectedProducerHeaders(ImmutableArray<MaterialDto> materials)
    {
        return new ProjectedProducersHeaders
        {
            TitleHeader = new ProjectedProducersHeader
            {
                Name = CalcResultProjectedProducersHeaders.H2ProjectedProducers,
                ColumnIndex = 1
            },
            MaterialBreakdownHeaders = GetMaterialsBreakdownHeader(materials),
            ColumnHeaders = GetColumnHeaders(materials)
        };
    }

    private static List<ProjectedProducersHeader> GetMaterialsBreakdownHeader(ImmutableArray<MaterialDto> materials)
    {
        var materialsBreakdownHeaders = new List<ProjectedProducersHeader>();
        var columnIndex = GetInitialColumnHeaders().Count + 1;

        foreach (var material in materials)
        {
            materialsBreakdownHeaders.Add(new ProjectedProducersHeader
            {
                Name = $"{material.Name} Breakdown",
                ColumnIndex = columnIndex
            });

            var materialHeaderCount = GetMaterialColumnHeaders().Count + GetPostFixColumnHeaders().Count;

            columnIndex = material.Code == MaterialCodes.Glass
                ? columnIndex + materialHeaderCount + GetGlassColumnHeaders().Count
                : columnIndex + materialHeaderCount;
        }

        return materialsBreakdownHeaders;
    }

    private static List<ProjectedProducersHeader> GetColumnHeaders(ImmutableArray<MaterialDto> materials)
    {
        var columnHeaders = new List<ProjectedProducersHeader>();

        columnHeaders.AddRange(GetInitialColumnHeaders());

        foreach (var material in materials.Select(m => m.Code))
        {
            columnHeaders.AddRange(GetMaterialColumnHeaders());

            if (material == MaterialCodes.Glass) columnHeaders.AddRange(GetGlassColumnHeaders());

            columnHeaders.AddRange(GetPostFixColumnHeaders());
        }

        return columnHeaders;
    }

    private static List<ProjectedProducersHeader> GetInitialColumnHeaders()
    {
        return new List<ProjectedProducersHeader>
        {
            new() { Name = CalcResultProjectedProducersHeaders.ProducerId },
            new() { Name = CalcResultProjectedProducersHeaders.SubsidiaryId },
            new() { Name = CalcResultProjectedProducersHeaders.Level },
            new() { Name = CalcResultProjectedProducersHeaders.SubmissionPeriodCode }
        };
    }

    private static List<ProjectedProducersHeader> GetPostFixColumnHeaders()
    {
        return new List<ProjectedProducersHeader>
        {
            new() { Name = CalcResultProjectedProducersHeaders.TotalTonnage }
        };
    }


    private static List<ProjectedProducersHeader> GetMaterialColumnHeaders()
    {
        return new List<ProjectedProducersHeader>
        {
            new() { Name = CalcResultProjectedProducersHeaders.HouseholdPackagingTonnage },
            new() { Name = CalcResultProjectedProducersHeaders.HouseholdRedTonnage },
            new() { Name = CalcResultProjectedProducersHeaders.HouseholdAmberTonnage },
            new() { Name = CalcResultProjectedProducersHeaders.HouseholdGreenTonnage },
            new() { Name = CalcResultProjectedProducersHeaders.HouseholdRedMedicalTonnage },
            new() { Name = CalcResultProjectedProducersHeaders.HouseholdAmberMedicalTonnage },
            new() { Name = CalcResultProjectedProducersHeaders.HouseholdGreenMedicalTonnage },
            new() { Name = CalcResultProjectedProducersHeaders.HouseholdTonnageWithoutRAMDefaultedToRed },
            new() { Name = CalcResultProjectedProducersHeaders.PublicBinPackagingTonnage },
            new() { Name = CalcResultProjectedProducersHeaders.PublicBinRedTonnage },
            new() { Name = CalcResultProjectedProducersHeaders.PublicBinAmberTonnage },
            new() { Name = CalcResultProjectedProducersHeaders.PublicBinGreenTonnage },
            new() { Name = CalcResultProjectedProducersHeaders.PublicBinRedMedicalTonnage },
            new() { Name = CalcResultProjectedProducersHeaders.PublicBinAmberMedicalTonnage },
            new() { Name = CalcResultProjectedProducersHeaders.PublicBinGreenMedicalTonnage },
            new() { Name = CalcResultProjectedProducersHeaders.PublicBinTonnageWithoutRAMDefaultedToRed }
        };
    }

    private static List<ProjectedProducersHeader> GetGlassColumnHeaders()
    {
        return new List<ProjectedProducersHeader>
        {
            new() { Name = CalcResultProjectedProducersHeaders.HouseholdDrinksContainersPackagingTonnage },
            new() { Name = CalcResultProjectedProducersHeaders.HouseholdDrinksContainersRedTonnage },
            new() { Name = CalcResultProjectedProducersHeaders.HouseholdDrinksContainersAmberTonnage },
            new() { Name = CalcResultProjectedProducersHeaders.HouseholdDrinksContainersGreenTonnage },
            new() { Name = CalcResultProjectedProducersHeaders.HouseholdDrinksContainersRedMedicalTonnage },
            new() { Name = CalcResultProjectedProducersHeaders.HouseholdDrinksContainersAmberMedicalTonnage },
            new() { Name = CalcResultProjectedProducersHeaders.HouseholdDrinksContainersGreenMedicalTonnage },
            new() { Name = CalcResultProjectedProducersHeaders.HouseholdDrinksContainersTonnageWithoutRAMDefaultedToRed }
        };
    }
}
