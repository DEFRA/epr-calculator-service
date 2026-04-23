using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.Service.Function.Services
{
    public interface ProducerData
    {
        public int OrgId { get; }
        public decimal R { get; }
        public decimal A { get; }
        public decimal G { get; }
        public decimal Smcw { get; }
    }
    public interface L1 : ProducerData;

    public record SingleL1(int OrgId, decimal R, decimal A, decimal G, decimal Smcw) : L1;

    public record HC : L1
    {
        public int OrgId { get; }
        public List<L2> L2s { get; }
        public decimal R { get; }
        public decimal A { get; }
        public decimal G { get; }
        public decimal Smcw { get; }

        public HC(int orgId, List<L2> l2s)
        {
            this.OrgId = orgId;
            this.L2s   = l2s;
            this.R     = l2s.Sum(x => x.R);
            this.A     = l2s.Sum(x => x.A);
            this.G     = l2s.Sum(x => x.G);
            this.Smcw  = l2s.Sum(x => x.Smcw);
        }
    }

    public record L2(int OrgId, int? SubsidiaryId, decimal R, decimal G, decimal A, decimal Smcw) : ProducerData;

    public record Result(
        int OrgId,
        int? SubsidiaryId,
        string Level,
        decimal NetR,
        decimal NetA,
        decimal NetG,
        decimal Residual,
        decimal ActionedSmcwR,
        decimal ActionedSmcwA,
        decimal ActionedSmcwG
    );

    public interface ISelfManagedConsumerWasteServiceLevels
    {
        List<Result> Calculate(L1 l1);
    }

    public class SelfManagedConsumerWasteServiceLevels: ISelfManagedConsumerWasteServiceLevels
    {
        public List<Result> Calculate(L1 l1)
        {
            return l1 switch
            {
                SingleL1 sl1 => new List<Result> { UpdateSingleL1(sl1) },
                HC       hc  => UpdateHC(hc),
                _            => throw new ArgumentException($"Unsupported L1 type {l1.GetType}")
            };
        }

        private Result UpdateSingleL1(L1 l1)
        {
            var (netA, actionedA, nextSmcwA     ) = ApplySubSmcw(l1.A, l1.Smcw);
            var (netR, actionedR, nextSmcwR     ) = ApplySubSmcw(l1.R, nextSmcwA);
            var (netG, actionedG, nextAvailableG) = ApplySubSmcw(l1.G, nextSmcwR);

            return new Result(
                OrgId: l1.OrgId,
                SubsidiaryId: null,
                Level: "L1",
                NetR: netR,
                NetA: netA,
                NetG: netG,
                Residual: l1.Smcw - (actionedA + actionedR + actionedG),
                ActionedSmcwR: actionedR,
                ActionedSmcwA: actionedA,
                ActionedSmcwG: actionedG
            );
        }

        private List<Result> UpdateHC(HC hc)
        {
            var hcResult = UpdateSingleL1(hc);
            var availableA = hc.A - hcResult.NetA;
            var availableR = hc.R - hcResult.NetR;
            var availableG = hc.G - hcResult.NetG;
            var l2Results = ProcessL2s(hc.L2s, availableA, availableR, availableG);
            return new List<Result> { hcResult }.Concat(l2Results.results).ToList();
        }


        private static (decimal residualA, decimal residualR, decimal residualG, List<Result> results)
            ProcessL2s(List<L2> l2s, decimal availableA, decimal availableR, decimal availableG)
        {
            decimal currentA = availableA;
            decimal currentR = availableR;
            decimal currentG = availableG;
            var results = new List<Result>();

            foreach (var sub in l2s)
            {
                var (netA, actionedA, nextAvailableA) = ApplySubSmcw(sub.A, currentA);
                var (netR, actionedR, nextAvailableR) = ApplySubSmcw(sub.R, currentR);
                var (netG, actionedG, nextAvailableG) = ApplySubSmcw(sub.G, currentG);

                results.Add(new Result(
                    OrgId : sub.OrgId,
                    SubsidiaryId : sub.SubsidiaryId,
                    Level : "L2",
                    NetR : netR,
                    NetA : netA,
                    NetG :  netG,
                    Residual: sub.Smcw - (actionedA + actionedR + actionedG),
                    ActionedSmcwR: actionedR,
                    ActionedSmcwA: actionedA,
                    ActionedSmcwG: actionedG
                ));

                currentA = nextAvailableA;
                currentR = nextAvailableR;
                currentG = nextAvailableG;
            }

            return (currentA, currentR, currentG, results);
        }

        private static (decimal net, decimal used, decimal nextAvailable) ApplySubSmcw(decimal t, decimal smcw)
        {
            var net = Math.Max(0, t - smcw);
            var used = t - net;
            var nextAvailable = smcw - used;
            return (net, used, nextAvailable);
        }
    }
}
