using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using Tests.Test_Support;

namespace Tests
{
   [TestClass]
   public class CreateHelperScrpt_CandidateUnitTests : CreateSqlTestsBase
   {
      [TestMethod]
      public void Test__CreateHelperScrpt_Candidate_Create()
      {
         Assert.IsTrue(Helper("dbo.sp_candidate_create", "Candidate", out string msg), msg);
      }

      [TestMethod]
      public void TestCreateHelperScrpt_Candidate_Update()
      {
         Assert.IsTrue(Helper("dbo.sp_candidate_update", "Candidate", out string msg), msg);
      }

      [TestMethod]
      public void Test__CreateHelperScrpt_Candidate_Get1()
      {
         Assert.IsTrue(Helper("dbo.sp_Candidate_Get1", "Candidate", out string msg), msg);
      }

      [TestMethod]
      public void Test__CreateHelperScrpt_Candidate_GetAll()
      {
         Assert.IsTrue(Helper("dbo.sp_candidate_getAll", "Candidate", out string msg), msg);
      }

      [TestMethod]
      public void Test__CreateHelperScrpt_Candidate_Delete()
      {
         Assert.IsTrue(Helper("dbo.sp_Candidate_Delete", "Candidate", out string msg), msg);
      }

      protected bool Helper(string qTstdRtnNm, string table, out string msg)
      {
         bool ret = false;
         var c = new TestableSqlTestCreator();
         string script = "";

         do 
         { 
            Assert.AreEqual(0, c.Init(qTstdRtnNm, 100, conn_str: GetDefaultConnectionString(), out msg, table: table, view: "dbo.CandidateVw"), msg);
            
            if(!c.__Hlpr_CreateScript(out script, out msg))
               break;

            var lines = script.Split(new char[] { (char)10, (char)13 });

            if(0 == lines.Count())
            { 
               msg = "script contains no lines";
               break;
            }

            // check presets: line 0 = "SET ANSI_NULLS ON"
            if(!lines[0].Equals("USE Telepat"))
            {
               msg = "";
               break;
            }

            ret = true;
            msg = "";
         }while(false);

         if(!ret)
            DisplayScript(script, "Test__CreateHelperScrpt_Candidate_GetAll.sql");

         return ret;         
      }
   }
}
