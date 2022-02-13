using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using Tests.Test_Support;

namespace Tests
{
   [TestClass]
   public class CreateHelperScriptUnitTests : CreateSqlTestsBase
   {

      [TestMethod]
      public void Test__CreateHelperScrpt_Person_Create()
      {
         Assert.IsTrue(Helper(qTstdRtnNm: "dbo.sp_person_create", tstNum: 56, msg: out var msg, "Person", "PersonVw", out var lines), msg);

         // smoke: 1 line
         Assert.IsNotNull(lines, "unexpected: lines: null");
         Assert.IsTrue(0 < lines.Count(), "script is empty");
         // check presets: line 0 = "SET ANSI_NULLS ON"
         Assert.AreEqual("USE Telepat", lines[0], "USE db line chk failed");
      }

      [TestMethod]
      public void Test__CreateHelperScrpt_ContactDetail_Get1()
      {
         Assert.IsTrue(Helper(qTstdRtnNm: "dbo.sp_contactDetail_Get1", tstNum: 56, msg: out var msg, "ContactDetail", "ContactDetailVw", out _), msg);
      }

      [TestMethod]
      public void Test__CreateHelperScrpt_ContactDetail_GetAll()
      {
         Assert.IsTrue(Helper(qTstdRtnNm: "dbo.sp_contactDetail_GetAll", tstNum: 56, msg: out var msg, "ContactDetail", "ContactDetailVw", out _), msg);
      }

      [TestMethod]
      public void Test__CreateHelperScrpt_Skill_Delete()
      {
         Assert.IsTrue(Helper(qTstdRtnNm: "dbo.sp_skill_delete", tstNum: 56, msg: out var msg, "Skill", "SkillVw", out _), msg);
      }

      [TestMethod]
      public void Test_sp_clean_all_tables_Default()
      {
         Assert.IsTrue(Helper(qTstdRtnNm: "dbo.sp_clean_all_tables", tstNum: 901, msg: out var msg, "Candidate", "ContactDetailVw", out _), msg);
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="qTstdRtnNm"></param>
      /// <param name="tstNum"></param>
      /// <param name="msg"></param>
      /// <param name="table"></param>
      /// <param name="viewNm"></param>
      /// <param name="lines"></param>
      /// <returns></returns>
      protected bool Helper( string qTstdRtnNm, int tstNum, out string msg, string table, string? viewNm, out string[]? lines)
      { 
         lines = null;
         var c = new TestableSqlTestCreator();

         if(0 != c.Init(qTstdRtnNm: qTstdRtnNm, tstNum: tstNum, conn_str: GetDefaultConnectionString(), msg: out msg, table: table, view: viewNm))
            return false;

         if(!c.__Hlpr_CreateScript(out var script, out msg))
            return false;

         //DisplayScript(script, "Test_sp_clean_all_tables_Default.sql");

         if(true == string.IsNullOrWhiteSpace(script))
         {
            msg = $"script was not created";
            return false;
         }

         lines = script.Split(new char[] { (char)10, (char)13 });
         return true;
      }
   }
}
