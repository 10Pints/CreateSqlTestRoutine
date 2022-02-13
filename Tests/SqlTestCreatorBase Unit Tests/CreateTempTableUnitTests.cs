using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Text;
using Tests.Test_Support;

namespace Tests
{
   [TestClass]
   public class CreateTempTableUnitTests : CreateSqlTestsBase
   {
      [TestMethod]
      public void TestCreateTempTable_Candidate_Get1()
      {
         Assert.IsTrue(Helper("dbo.sp_candidate_Get1", out var msg
            , new [] { "[candidate_id] INT", ",[salutation_nm] NVARCHAR(50)", ",[email] NVARCHAR(50)" }), msg); // , "Candidate", "CandidateVw"
      }

      [TestMethod]
      public void TestCreateTempTable_Person_GetAll()
      {
         Assert.IsTrue(Helper("dbo.sp_Person_GetAll", out var msg), msg); //, "Person", "PersonVw"
      }

      [TestMethod]
      public void TestCreateTempTable_sp_opsRole_get1()
      {
         Assert.IsTrue(Helper("dbo.sp_opsRole_get1", out var msg), msg); //, "OpsRole", "OpsRoleVw"
      }

      [TestMethod]
      public void TestCreateTempTable_sp_recruiter_get1()
      {
         Assert.IsTrue(Helper("dbo.sp_skill_getAll", out var msg), msg); //, "Recruiter", "RecruiterVw"
      }

      [TestMethod]
      public void TestCreateTempTable_sp_skill_getAll()
      {
         Assert.IsTrue(Helper("dbo.sp_skill_getAll", out var msg), msg); // , "Skill", "SkillVw"
      }

      /// <summary>
      /// helper rtn for CrtTempTable_Hlpr tests
      /// </summary>
      /// <param name="qTstdRtnNm"></param>
      /// <param name="msg"></param>
      /// <param name="clauses"></param>
      /// <param name="table"></param>
      /// <param name="view"></param>
      /// <returns></returns>
      protected bool Helper(string qTstdRtnNm, out string msg, string[]? clauses = null, string? table = null, string? view = null)
      {
         bool ret = false;
         var c = new TestableSqlTestCreator();

         do
         {
            if (0 != c.Init
                (
                   qTstdRtnNm : qTstdRtnNm,
                   tstNum     : 900,
                   conn_str   : ConnectionString,
                   msg        : out msg,
                   table      : table,
                   view       : view
                ))
            {
               break;
            }

            c.CrtTmpTbl(out msg);

            // run some tests
            var lines = c.SB.ToString().Split("\r\n", System.StringSplitOptions.RemoveEmptyEntries | System.StringSplitOptions.TrimEntries);

            // look for -- Create the temp table
            if(!FindClause(lines,"-- Create the temp table", out msg))
               break;

            // look for DROP TABLE IF EXISTS #tmpTbl;
            if (!FindClause(lines, "DROP TABLE IF EXISTS #tmpTbl;", out msg))
               break;

            // look for CREATE TABLE #tmpTbl
            if (!FindClause(lines, "CREATE TABLE #tmpTbl", out msg))
               break;

            // look for );
            if (!FindClause(lines, ");", out msg))
            {
               msg = "Did not find '-- );'";
               break;
            }

            // chk supplied clauses
            if(clauses != null)
               foreach (var clause in clauses)
                  if (!FindClause(lines, clause, out msg))
                     break;

            // Finally
            ret = true;
            msg = "";
         } while(false);

         if (!ret)
            DisplayScript(c.SB, $"{TestMethod}.sql");

         return ret;
      }
   }
}
