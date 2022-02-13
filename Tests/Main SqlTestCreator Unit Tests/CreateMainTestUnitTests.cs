using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using Tests.Test_Support;

namespace Tests
{
   [TestClass]
   public class CreateMainTestUnitTests : CreateSqlTestsBase
   {

      [TestMethod]
      public void Test__CreateMainTestScript_Person()
      {
         Assert.IsTrue(Helper("dbo.sp_person_create", 901, out string msg, out string[]? lines), msg); // , "Person", "PersonVw"
         Assert.IsTrue((lines?.Count() ?? 0) > 20, "script.Length > 400 test");
      }

      [TestMethod]
      public void TestAutogenCrtMnTstScrpt_candidate_create()
      {
         Assert.IsTrue(Helper("dbo.sp_candidate_create", 100, out string msg, out string[]? lines), msg); // , "Candidate", "CandidateVw"
      }

      [TestMethod]
      public void TestAutogenCrtMnTstScrpt_candidate_update()
      {
         Assert.IsTrue(Helper("dbo.sp_candidate_update", 100, out string msg, out string[]? lines), msg); // , "Candidate", "CandidateVw"
      }

      [TestMethod]
      public void TestAutogenCrtMnTstScrpt_candidate_delete()
      {
         Assert.IsTrue(Helper("dbo.sp_candidate_delete", 100, out string msg, out string[]? lines), msg); //, "Candidate", "CandidateVw"
      }

      [TestMethod]
      public void TestAutogenCrtMnTstScrpt_candidate_get1()
      {
         Assert.IsTrue(Helper("dbo.sp_candidate_get1", 100, out string msg, out string[]? lines), msg); //, "Candidate", "CandidateVw"
      }

      [TestMethod]
      public void TestAutogenCrtMnTstScrpt_candidate_getAll()
      {
         Assert.IsTrue(Helper("dbo.sp_candidate_getAll", 100, out string msg, out string[]? lines), msg); // , "Candidate", "CandidateVw"
      }

      protected bool Helper( string qTstdRtnNm, int tstNum, out string msg, out string[]? lines, string? table=null, string? view = null)
      {
         bool ret       = false;
         lines          = null;
         var c          = new TestableSqlTestCreator();
         string script  = "";

         do
         {          
            if(0 != c.Init(qTstdRtnNm, tstNum, conn_str: ConnectionString, out msg, table, view))
               break;

            if(!c.__Mn_CreateTestScript(out script, out msg))
               break;

            lines = script.Split(new char[] { (char)10, (char)13 });

            // smoke: 1 line
            if(0 >= lines.Count())
            { 
               msg = "script is empty";
               break;
            }

            if(!lines[0].Equals("USE Telepat", StringComparison.OrdinalIgnoreCase))
            {
               msg = "script first line should be 'USE Telepat'";
               break;
            }

            // Finally
            msg = "";
            ret = true; 
         } while(false);

         if(!ret)
            DisplayScript(script, $"{TestClassName}_{TestMethod}.sql");

         return ret;
      }
   }
}
