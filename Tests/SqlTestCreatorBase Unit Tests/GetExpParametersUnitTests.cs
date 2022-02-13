using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tests.Test_Support;

namespace Tests.SqlTestCreatorBase_Unit_Tests
{
   [TestClass]
   public class GetExpParametersUnitTests : CreateSqlTestsBase
   {
      [TestMethod]
      public void Test_sp_candidate_create()
      {
         Assert.IsTrue(Helper("dbo.sp_candidate_create", out string msg, null, null, 11), msg); // , "Candidate", "CandidateVw"
      }

      [TestMethod]
      public void Test_sp_candidate_update()
      {
         Assert.IsTrue(Helper("dbo.sp_candidate_update", out string msg, "Candidate", "CandidateVw"), msg);
      }

      [TestMethod]
      public void Test_sp_candidate_get1()
      {
         Assert.IsTrue(Helper("dbo.sp_candidate_get1", out string msg, "Candidate", "CandidateVw"), msg);
      }

      [TestMethod]
      public void Test_sp_candidate_getAll()
      {
         Assert.IsTrue(Helper("dbo.sp_candidate_getAll", out string msg, "Candidate", "CandidateVw"), msg);
      }

      [TestMethod]
      public void Test_sp_candidate_delete()
      {
         Assert.IsTrue(Helper("dbo.sp_candidate_delete", out string msg, "Candidate", "CandidateVw"), msg);
      }

      /// <summary>
      /// Common helper for this test class
      /// </summary>
      /// <param name="qTstdRtnNm"></param>
      /// <param name="tstNum"></param>
      /// <param name="exp_cnt"></param>
      /// <returns></returns>
      protected bool Helper(string qTstdRtnNm, out string msg, string? table = null, string? view = null, int? exp_prm_cnt = null)
      {
         var c = new TestableSqlTestCreator();
         bool ret = false;

         do
         {
            Assert.AreEqual(0, c.Init(
                    qTstdRtnNm: qTstdRtnNm
                   ,tstNum    : 100
                   ,conn_str  : ConnectionString
                   ,msg       : out msg
                   ,table     : table
                   , view     : view
                ), msg);

            var exp_params = c.GetExpParameters();
            var act_prm_cnt = exp_params.Count;

            if ((exp_prm_cnt != null) && (exp_prm_cnt != act_prm_cnt))
            {
               msg = $"Error: parameters count mismatch: exp {exp_prm_cnt} act: {act_prm_cnt}";
               break;
            }

            ret = true;
            msg = "";
         } while(false);

         return ret;
      }
   }
}
