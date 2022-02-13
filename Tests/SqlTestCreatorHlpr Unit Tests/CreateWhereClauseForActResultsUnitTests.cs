using CreateSqlTestRoutineLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using Tests.Test_Support;

namespace Tests
{
   [TestClass]
   public class CreateWhereClauseForActResultsUnitTests : CreateSqlTestsBase
   {
      [TestMethod]
      public void TestCreateWhereClause_For_Person_Create()
      {
         Assert.IsTrue( Helper(qTstdRtnNm: "dbo.sp_person_create", table:"Person", view:"PersonVw", exp_cnt: 2, msg: out var msg, clauses: new[] { " WHERE [person_id] = @inp_person_id;" }), msg);
      }

      [TestMethod]
      public void TestCreateWhereClause_For_Candidate_Update()
      {
         Assert.IsTrue(Helper(qTstdRtnNm: "dbo.sp_Candidate_Update", table: "Candidate", view: "CandidateVw", exp_cnt: 2, msg: out var msg, clauses: new[] { " WHERE [candidate_id] = @inp_candidate_id;" }), msg);
      }

      [TestMethod]
      public void TestCreateWhereClause_For_Recruiter_Get1()
      {
         Assert.IsTrue(Helper(qTstdRtnNm: "dbo.sp_Recruiter_Get1", table: "Recruiter", view: "RecruiterVw", exp_cnt: 2, msg: out var msg, clauses: new[] { " WHERE [recruiter_id] = @inp_recruiter_id;" }), msg);
      }

      /// <summary>
      /// GetAll tests dont have a where clause ??
      /// </summary>
      [TestMethod]
      public void TestCreateWhereClause_For_Candidate_sp_OpsRole_GetAll()
      {
         /*var c = new TestableSqlTestCreator();
         Assert.AreEqual(0, c.Init(qTstdRtnNm: "dbo.sp_OpsRole_GetAll", tstNum: 900, conn_str: ConnectionString, out var msg, "OpsRole", "OpsRoleVw"), msg);
         c.Hlpr_CreateWhereClauseForActResults(out var script);
         Assert.AreNotEqual(0, script.Length);*/
         Assert.IsTrue(Helper(qTstdRtnNm: "dbo.sp_OpsRole_GetAll", table: "OpsRole", view: "OpsRoleVw", exp_cnt: 2, msg: out string msg, clauses: new []{" WHERE", "[opsRole_id] = @inp_opsRole_id;" }), msg);
      }

      /// <summary>
      /// No where clause should be created for Delete tests
      /// </summary>
      [TestMethod]
      public void TestCreateWhereClause_For_ContactDetail_Delete()
      {
         Assert.IsTrue(Helper(qTstdRtnNm: "dbo.sp_ContactDetail_Delete", table: "ContactDetail", view: "ContactDetailVw", exp_cnt: 0, msg: out var msg, clauses: new[] { " WHERE [contactDetail_id] = @inp_contactDetail_id;" }), msg);
      }

      /// <summary>
      /// TemplateType.Default
      /// </summary>
      [TestMethod]
      public void TestCreateWhereClause_For_sp_clean_all_tables_Type_Default()
      {
         Assert.IsTrue(Helper(qTstdRtnNm: "dbo.sp_clean_all_tables", table: "Skill", view: "SkillVw", exp_cnt: 2, msg: out var msg, clauses: new[] { " WHERE [skill_id] = @inp_skill_id;" }), msg);
      }

      /// <summary>
      /// TemplateType.Unknown
      /// </summary>
      [TestMethod]
      [ExpectedException(typeof(Exception))]
      public void TestCreateWhereClause_For_sp_clean_all_tables_Type_Unknown()
      {
         string msg;
         string script = "";
         var c = new TestableSqlTestCreator();
         Assert.AreEqual(0,  c.Init(qTstdRtnNm: "dbo.sp_sys_rtn_vw", tstNum: 900, conn_str: ConnectionString, out msg, "Skill", "SkillVw"), msg);
         c.SetTemplateType (TestType.Unknown);

         script = c.Hlpr_CreateWhereClauseForActResults();
     }

      protected bool Helper( string qTstdRtnNm, string table, string view, int exp_cnt, out string msg, string[]? clauses = null)
      {
         msg = "";
         var c = new TestableSqlTestCreator();

         if (0 != c.Init(qTstdRtnNm: qTstdRtnNm, tstNum: 900, conn_str: ConnectionString, out msg, table, view))
            return false;

         var script = c.Hlpr_CreateWhereClauseForActResults();

         // Get the script and remove any debug comments
         var lines = SquishLines(RemoveDebugComments(script.Split(new char[] { (char)10, (char)13 }, StringSplitOptions.RemoveEmptyEntries)));
         var act_cnt = lines.Count();

         if (exp_cnt != act_cnt)
         {
            msg = $"Line count mismatch: exp: {exp_cnt}, act: {act_cnt}";
            return false;
         }

         if (clauses != null)
            foreach (var clause in clauses)
               if (!FindClause(lines, clause, out msg))
                  break;

         return true;
      }
   }
}
