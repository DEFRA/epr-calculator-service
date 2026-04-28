
using Microsoft.EntityFrameworkCore;
using EPR.Calculator.Service.Function.Services;

namespace EPR.Calculator.Service.Function.UnitTests.Services
{
    [TestClass]
    public class SelfManagedConsumerWasteServiceLevelsTests
    {
        [TestMethod]
        public void SingleL1_a_first()
        {
            var l1 = new SingleL1(OrgId: 1, R: 1, A: 1, G: 1, Smcw: 1);
            var result = SelfManagedConsumerWasteServiceLevels.Calculate(l1);

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(new Result(OrgId: 1, SubsidiaryId: null, Level: 1, NetR: 1, NetA: 0, NetG: 1, Residual: 0, ActionedSmcwR: 0, ActionedSmcwA: 1, ActionedSmcwG: 0), result[0]);
        }

        [TestMethod]
        public void SingleL1_r_second()
        {
            var l1 = new SingleL1(OrgId: 1, R: 1, A: 1, G: 1, Smcw: 2);
            var result = SelfManagedConsumerWasteServiceLevels.Calculate(l1);

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(new Result(OrgId: 1, SubsidiaryId: null, Level: 1, NetR: 0, NetA: 0, NetG: 1, Residual: 0, ActionedSmcwR: 1, ActionedSmcwA: 1, ActionedSmcwG: 0), result[0]);
        }

        [TestMethod]
        public void SingleL1_g_last()
        {
            var l1 = new SingleL1(OrgId: 1, R: 1, A: 1, G: 1, Smcw: 3);
            var result = SelfManagedConsumerWasteServiceLevels.Calculate(l1);

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(new Result(OrgId: 1, SubsidiaryId: null, Level: 1, NetR: 0, NetA: 0, NetG: 0, Residual: 0, ActionedSmcwR: 1, ActionedSmcwA: 1, ActionedSmcwG: 1), result[0]);
        }

        [TestMethod]
        public void SingleL1_g_collect_residual()
        {
            var l1 = new SingleL1(OrgId: 1, R: 1, A: 1, G: 1, Smcw: 4);
            var result = SelfManagedConsumerWasteServiceLevels.Calculate(l1);

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(new Result(OrgId: 1, SubsidiaryId: null, Level: 1, NetR: 0, NetA: 0, NetG: 0, Residual: 1, ActionedSmcwR: 1, ActionedSmcwA: 1, ActionedSmcwG: 1), result[0]);
        }

        [TestMethod]
        public void HC_L2s_only_reduce_A()
        {
            var l1 = new HC(
                orgId: 1,
                new List<L2>{
                    new L2(OrgId: 1, SubsidiaryId: null, R: 10, A: 10, G: 10, Smcw: 5),
                    new L2(OrgId: 1, SubsidiaryId: "2" , R: 10, A: 10, G: 10, Smcw: 5)
                }
            );
            var result = SelfManagedConsumerWasteServiceLevels.Calculate(l1);

            Assert.AreEqual(3, result.Count);
            Assert.AreEqual(new Result(OrgId: 1, SubsidiaryId: null, Level: 1, NetR: 20, NetA: 10, NetG: 20, Residual:  0, ActionedSmcwR: 0, ActionedSmcwA: 10, ActionedSmcwG: 0), result[0]);
            Assert.AreEqual(new Result(OrgId: 1, SubsidiaryId: null, Level: 2, NetR: 10, NetA:  0, NetG: 10, Residual: -5, ActionedSmcwR: 0, ActionedSmcwA: 10, ActionedSmcwG: 0), result[1]); // negative indicates smcw was borrowed
            Assert.AreEqual(new Result(OrgId: 1, SubsidiaryId: "2" , Level: 2, NetR: 10, NetA: 10, NetG: 10, Residual:  5, ActionedSmcwR: 0, ActionedSmcwA:  0, ActionedSmcwG: 0), result[2]); // positive indicates smcw is available for others
            verifyL1Matches(result[0], result.Skip(1));
        }

        [TestMethod]
        public void HC_L2s_share_SMCW_with_others_to_reduce_A_to_0_then_reduce_R()
        {
            var l1 = new HC(
                orgId: 1,
                new List<L2>{
                    new L2(OrgId: 1, SubsidiaryId: null, R: 10, A: 10, G: 10, Smcw:  5),
                    new L2(OrgId: 1, SubsidiaryId: "2" , R: 10, A: 10, G: 10, Smcw: 25)
                }
            );
            var result = SelfManagedConsumerWasteServiceLevels.Calculate(l1);

            Assert.AreEqual(3, result.Count);
            Assert.AreEqual(new Result(OrgId: 1, SubsidiaryId: null, Level: 1, NetR: 10, NetA: 0, NetG: 20, Residual:   0, ActionedSmcwR: 10, ActionedSmcwA: 20, ActionedSmcwG: 0), result[0]);
            Assert.AreEqual(new Result(OrgId: 1, SubsidiaryId: null, Level: 2, NetR:  0, NetA: 0, NetG: 10, Residual: -15, ActionedSmcwR: 10, ActionedSmcwA: 10, ActionedSmcwG: 0), result[1]); // negative indicates smcw was borrowed
            Assert.AreEqual(new Result(OrgId: 1, SubsidiaryId: "2" , Level: 2, NetR: 10, NetA: 0, NetG: 10, Residual:  15, ActionedSmcwR:  0, ActionedSmcwA: 10, ActionedSmcwG: 0), result[2]); // positive indicates smcw is available for others
            verifyL1Matches(result[0], result.Skip(1));
        }

        [TestMethod]
        public void HC_Both_A_and_R_are_reduced_to_0_by_sharing_then_reduces_G()
        {
            var l1 = new HC(
                orgId: 1,
                new List<L2>{
                    new L2(OrgId: 1, SubsidiaryId: null, R: 10, A: 10, G: 10, Smcw: 45),
                    new L2(OrgId: 1, SubsidiaryId: "2" , R: 30, A: 10, G: 20, Smcw: 30)
                }
            );
            var result = SelfManagedConsumerWasteServiceLevels.Calculate(l1);

            Assert.AreEqual(3, result.Count);
            Assert.AreEqual(new Result(OrgId: 1, SubsidiaryId: null, Level: 1, NetR: 0, NetA: 0, NetG: 15, Residual:   0, ActionedSmcwR: 40, ActionedSmcwA: 20, ActionedSmcwG: 15), result[0]);
            Assert.AreEqual(new Result(OrgId: 1, SubsidiaryId: null, Level: 2, NetR: 0, NetA: 0, NetG:  0, Residual:  15, ActionedSmcwR: 10, ActionedSmcwA: 10, ActionedSmcwG: 10), result[1]);
            Assert.AreEqual(new Result(OrgId: 1, SubsidiaryId: "2" , Level: 2, NetR: 0, NetA: 0, NetG: 15, Residual: -15, ActionedSmcwR: 30, ActionedSmcwA: 10, ActionedSmcwG:  5), result[2]);
            verifyL1Matches(result[0], result.Skip(1));
        }

        [TestMethod]
        public void HC_A_R_and_G_are_reduced_to_0_by_sharing()
        {
            var l1 = new HC(
                orgId: 1,
                new List<L2>{
                    new L2(OrgId: 1, SubsidiaryId: null, R: 10, A: 10, G: 10, Smcw: 70),
                    new L2(OrgId: 1, SubsidiaryId: "2" , R: 30, A: 10, G: 20, Smcw: 30)
                }
            );
            var result = SelfManagedConsumerWasteServiceLevels.Calculate(l1);

            Assert.AreEqual(3, result.Count);
            Assert.AreEqual(new Result(OrgId: 1, SubsidiaryId: null, Level: 1, NetR: 0, NetA: 0, NetG: 0, Residual:  10, ActionedSmcwR: 40, ActionedSmcwA: 20, ActionedSmcwG: 30), result[0]);
            Assert.AreEqual(new Result(OrgId: 1, SubsidiaryId: null, Level: 2, NetR: 0, NetA: 0, NetG: 0, Residual:  40, ActionedSmcwR: 10, ActionedSmcwA: 10, ActionedSmcwG: 10), result[1]);
            Assert.AreEqual(new Result(OrgId: 1, SubsidiaryId: "2" , Level: 2, NetR: 0, NetA: 0, NetG: 0, Residual: -30, ActionedSmcwR: 30, ActionedSmcwA: 10, ActionedSmcwG: 20), result[2]);
            verifyL1Matches(result[0], result.Skip(1));
        }

        private void verifyL1Matches(Result l1, IEnumerable<Result> l2s)
        {
            Assert.AreEqual(1, l1.Level);
            foreach (var l2 in l2s)
            {
               Assert.AreEqual(2, l2.Level);
            }
            Assert.AreEqual(l1.NetR, l2s.Sum(x => x.NetR));
            Assert.AreEqual(l1.NetA, l2s.Sum(x => x.NetA));
            Assert.AreEqual(l1.NetG, l2s.Sum(x => x.NetG));
            Assert.AreEqual(l1.Residual, l2s.Sum(x => x.Residual));
            Assert.AreEqual(l1.ActionedSmcwR, l2s.Sum(x => x.ActionedSmcwR));
            Assert.AreEqual(l1.ActionedSmcwA, l2s.Sum(x => x.ActionedSmcwA));
            Assert.AreEqual(l1.ActionedSmcwG, l2s.Sum(x => x.ActionedSmcwG));
        }
    }
}
