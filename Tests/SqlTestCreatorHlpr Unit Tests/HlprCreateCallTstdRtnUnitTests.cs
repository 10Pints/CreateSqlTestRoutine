using CreateSqlTestRoutineLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Text;
using Tests.Test_Support;

namespace Tests
{
   [TestClass]
   public class HlprCreateCallTstdRtnUnitTests : CreateSqlTestsBase
   {
      [TestMethod]
      public void Test_CallTstdRtn_Bloc_Candidate_Create()
      {
         Assert.IsTrue(Helper(qTstdRtnNm: "dbo.sp_candidate_create", tstNum: 100, table: "Candidate", viewNm: "dbo.CandidateVw", out var msg), msg);
      }
 
      [TestMethod]
      public void Test_CallTstdRtn_Bloc_Candidate_Get1()
      {
         Assert.IsTrue(Helper(qTstdRtnNm: "dbo.sp_candidate_get1", tstNum: 100, table: "Candidate", viewNm: "dbo.CandidateVw", out var msg), msg);
      }

      [TestMethod]
      public void Test_CallTstdRtn_Bloc_Person_GetAll()
      {
         Assert.IsTrue(Helper(qTstdRtnNm: "dbo.sp_person_getAll", tstNum: 100, table: "Person", viewNm: "dbo.PersonVw", out var msg), msg);
      }

      [TestMethod]
      public void Test_CallTstdRtn_Bloc_fnDoesSpReturnResultTable()
      {
         Assert.IsTrue(Helper(qTstdRtnNm: "dbo.fnDoesSpReturnResultTable", tstNum: 100, table: "Person", viewNm: "dbo.PersonVw", out var msg), msg);
      }

      protected bool Helper(string qTstdRtnNm, int tstNum, string table, string viewNm, out string msg)
      {
         var c = new TestableSqlTestCreator();
         bool ret = false;

         do 
         { 
            if(0 != c.Init(qTstdRtnNm: qTstdRtnNm, tstNum: tstNum, conn_str: GetDefaultConnectionString(), table: table, view: viewNm, msg: out msg))
               break;

            if (!c.Hlpr_Crt_CallTstdRtn_Bloc(out msg))
               break;

            var lines = c.SB.ToString().Split("\r\n");

            // smoke: 1 line
            if(0 == lines.Count())
            { 
               msg = "Script is empty";
               break;
            }

            ret = true;
         } while(false);

         if (ret == false)
            DisplayScript(c.SB, $"HlprCreateCallTstdRtnUnitTests_{TestMethod}.sql");

         return ret;
      }
   }
}
