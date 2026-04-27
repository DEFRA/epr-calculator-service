using Microsoft.EntityFrameworkCore;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.API.Data;
using EPR.Calculator.Service.Function.Mappers;
using EPR.Calculator.Service.Function.Builder.ProjectedProducers;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.Enums;

namespace EPR.Calculator.Service.Function.Services
{
    public record ReportedData(
        decimal Total,
        decimal R,
        decimal A,
        decimal G,
        decimal RM,
        decimal AM,
        decimal GM,
        decimal Smcw
    );

    public record MaterialSubmission(
        MaterialDetail Material,
        string SubmissionPeriod,
        ReportedData HH,
        ReportedData PB,
        ReportedData HDC
    );

    public interface ProducerData
    {
        public int OrgId { get; }
        public List<MaterialSubmission> MaterialSubmissions { get; }
    }
    public interface L1 : ProducerData;

    public record SingleL1(
        int OrgId,
        List<MaterialSubmission> MaterialSubmissions
    ) : L1;

    public record HC : L1
    {
        public int OrgId { get; }
        public List<MaterialSubmission> MaterialSubmissions { get; }

        public List<L2> L2s { get; }
        public HC(int orgId, List<L2> l2s)
        {
            this.OrgId = orgId;
            this.L2s   = l2s;
            this.MaterialSubmissions    = new List<MaterialSubmission>();//l2s.Sum(x => x.hh);// TODO make ReportedData a Monoid (Summable)
        }
    }

    public record L2(
        int OrgId,
        string? SubsidiaryId,
        List<MaterialSubmission> MaterialSubmissions
    ) : ProducerData;

    public interface ILevelledProducerService
    {
        Task<List<L1>> GetProducers(int runId, List<MaterialDetail> materials);
    }

    /*
    public class ProducerReportedMaterialsForSubmissionPeriod
    {
        public int ProducerId { get; }
        public string? SubsidiaryId { get; }
        public string SubmissionPeriod { get; }
        public List<ProducerReportedMaterial> ReportedMaterials { get; }

        public ProducerReportedMaterialsForSubmissionPeriod(int producerId, string? subsidiaryId, string submissionPeriod, List<ProducerReportedMaterial> reportedMaterials)
        {
            ProducerId = producerId;
            SubsidiaryId = subsidiaryId;
            SubmissionPeriod = submissionPeriod;
            ReportedMaterials = reportedMaterials;
        }
    }*/


    public class LevelledProducerService : ILevelledProducerService
    {
        private readonly ApplicationDBContext dbContext;

        public LevelledProducerService(IDbContextFactory<ApplicationDBContext> context)
        {
            this.dbContext = context.CreateDbContext();
        }

        public async Task<List<L1>> GetProducers(int runId, List<MaterialDetail> materials)
        {
            return (await GetProducers2(runId))
                .GroupBy(e => e.ProducerId)
                .Select(e =>
                {
                    var bySub = e.GroupBy(e => e.SubsidiaryId);
                    if (bySub.Count() == 1 && bySub.First().Key == null)
                    {
                        List<MaterialSubmission> materialSubmissions =
                            bySub.SelectMany(f =>
                            {
                                return f.SelectMany(g =>
                                {
                                    var x = g.ReportedMaterials.Select( h =>
                                        new {
                                        PackagingType = h.PackagingType,
                                        MaterialId    = h.MaterialId,
                                        data          = new ReportedData(
                                            Total: h.PackagingTonnage,
                                            R    : h.PackagingTonnageRed ?? 0m,
                                            A    : h.PackagingTonnageAmber ?? 0m,
                                            G    : h.PackagingTonnageGreen ?? 0m,
                                            RM   : h.PackagingTonnageRedMedical ?? 0m,
                                            AM   : h.PackagingTonnageAmberMedical ?? 0m,
                                            GM   : h.PackagingTonnageGreenMedical ?? 0m,
                                            Smcw : 0m // h.Smcw ?? 0m
                                        )
                                        }
                                    );
                                    List<MaterialSubmission> y = x.GroupBy(e => e.MaterialId)
                                    .Select( e =>
                                        new MaterialSubmission(
                                            Material : materials.Find(m => m.Id == e.Key),
                                            SubmissionPeriod : g.SubmissionPeriod,
                                            HH  : e.First(f => f.PackagingType == "HH").data,
                                            PB  : e.First(f => f.PackagingType == "PB").data,
                                            HDC : e.First(f => f.PackagingType == "HDC").data
                                        )
                                    ).ToList();
                                    return y;
                                });
                            }).ToList();

                        return (L1) new SingleL1(
                          OrgId: e.Key,
                          MaterialSubmissions: materialSubmissions
                        );
                    } else
                    {
                        // TODO
                        return null;
                    }
                }).ToList();
        }

/*
        public static List<ProducerData> ToProducerData(List<ProducerReportedMaterialsForSubmissionPeriod> reported)
        {
            // flatten all reported materials with producer context
            var flat = reported
                .SelectMany(p => p.ReportedMaterials.Select(m => new
                {
                    ProducerId = p.ProducerId,
                    SubmissionPeriod = m.SubmissionPeriod ?? p.SubmissionPeriod,
                    Material = m.Material,           // MaterialDetail assumed inside m.Material
                    PackagingType = m.PackagingType,
                    PackagingTonnage = m.PackagingTonnage,
                    PackagingTonnageRed = m.PackagingTonnageRed ?? 0m,
                    PackagingTonnageAmber = m.PackagingTonnageAmber ?? 0m,
                    PackagingTonnageGreen = m.PackagingTonnageGreen ?? 0m,
                    PackagingTonnageRedMedical = m.PackagingTonnageRedMedical ?? 0m,
                    PackagingTonnageAmberMedical = m.PackagingTonnageAmberMedical ?? 0m,
                    PackagingTonnageGreenMedical = m.PackagingTonnageGreenMedical ?? 0m,
                    //Smcw = m.Smcw ?? 0m
                }));

            // group by producer and material+submission period
            var grouped = flat
                .GroupBy(x => new { x.ProducerId, Material = x.Material, x.SubmissionPeriod })
                .Select(g =>
                {
                    // helper to pick values for a packaging type (HH, PB, HC)
                    ReportedData MakeReportedData(string pkgType)
                    {
                        var entry = g.FirstOrDefault(e => string.Equals(e.PackagingType, pkgType, StringComparison.OrdinalIgnoreCase));
                        if (entry == null)
                            return new ReportedData(0m, 0m, 0m, 0m, 0m, 0m, 0m);

                        return new ReportedData(
                            R: entry.PackagingTonnageRed,
                            A: entry.PackagingTonnageAmber,
                            G: entry.PackagingTonnageGreen,
                            RM: entry.PackagingTonnageRedMedical,
                            AM: entry.PackagingTonnageAmberMedical,
                            GM: entry.PackagingTonnageGreenMedical,
                            Smcw: entry.Smcw
                        );
                    }

                    var hh = MakeReportedData("HH");
                    var pb = MakeReportedData("PB");
                    var hdc = MakeReportedData("HC"); // you said HC maps to HDC

                    var materialSubmission = new MaterialSubmission(
                        Material: g.Key.Material!, // cast/adapt to your MaterialDetail type
                        SubmissionPeriod: g.Key.SubmissionPeriod,
                        HH: hh,
                        PB: pb,
                        HDC: hdc
                    );

                    return new
                    {
                        ProducerId = g.Key.ProducerId,
                        MaterialSubmission = materialSubmission
                    };
                });

            // assemble per-producer lists
            var result = grouped
                .GroupBy(x => x.ProducerId)
                .Select(pg => new ProducerData(
                    OrgId: pg.Key,
                    MaterialSubmissions: pg.Select(x => x.MaterialSubmission).ToList()
                ))
                .ToList();

            return result;
        }*/


        private async Task<IEnumerable<ProducerReportedMaterialsForSubmissionPeriod>> GetProducers2(int runId)
        {
            return await (
                from run in dbContext.CalculatorRuns.AsNoTracking()
                join pd in dbContext.ProducerDetail.AsNoTracking() on run.Id equals pd.CalculatorRunId
                join prm in dbContext.ProducerReportedMaterial.AsNoTracking() on pd.Id equals prm.ProducerDetailId
                where pd.CalculatorRunId == runId
                group prm by new { pd.ProducerId, pd.SubsidiaryId, prm.SubmissionPeriod } into prms
                select new ProducerReportedMaterialsForSubmissionPeriod(prms.Key.ProducerId, prms.Key.SubsidiaryId, prms.Key.SubmissionPeriod, prms.ToList())
            ).ToListAsync();
        }
    }
}
