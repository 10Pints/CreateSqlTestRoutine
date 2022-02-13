using CreateSqlTestRoutineLib;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tests.Test_Support;
using System;
using System.IO;

namespace Tests
{

   [TestClass]
   public class CreateHelperSigUnitTests : CreateSqlTestsBase
   {
      [TestMethod] public void Test_Candidate_Create_Chk_tst_num()
      {
         Assert.IsTrue(Helper( "dbo.sp_candidate_create" , "Candidate" , "dbo.CandidateVw", out string msg
            , exp_clauses: new []
         {
            " @tst_num INT",
            " ,@exp_RC INT = NULL",
            " ,@exp_RC INT = NULL"
         }
         ), msg); 
      }

      [TestMethod] public void Test_Candidate_Update()
      { 
         Assert.IsTrue(Helper("dbo.sp_candidate_update" , "Candidate" , "dbo.CandidateVw", out string msg
            , exp_clauses: new[] 
         {
            " @tst_num INT",
            " ,@exp_RC INT = NULL"
         }
         ), msg); 
      }
      
      [TestMethod] 
      public void Test_Candidate_Delete()
      {
         Assert.IsTrue(Helper("dbo.sp_candidate_update" , "Candidate" , "dbo.CandidateVw", out string msg
            , exp_clauses: new[] 
         {
            " @tst_num INT",
            " ,@exp_msg NVARCHAR(1000) = NULL"
         }), msg); 
      }

      [TestMethod] 
      public void Test_Candidate_Get1()
      { 
         Assert.IsTrue(Helper("dbo.sp_candidate_get1"   , "Candidate" , "dbo.CandidateVw", out string msg
            , exp_clauses: new[] 
         {
            " @tst_num INT",
            " ,@exp_salutation_nm NVARCHAR(50) = NULL"
         }), msg); 
      }

      [TestMethod] 
      public void Test_Candidate_GetAll()
      { 
         Assert.IsTrue(Helper("dbo.sp_Candidate_GetAll" , "Candidate" , "dbo.CandidateVw", out string msg
            , exp_clauses: new[] 
         {
            " ,@exp_RC INT = NULL"
         }), msg); 
      }

      [TestMethod] 
      public void Test_Person_Create()
      { 
         Assert.IsTrue(Helper("dbo.sp_person_create"    , "Person"    , "dbo.PersonVw"   , out string msg
            , exp_clauses: new[] 
         {
            " @tst_num INT",
            " ,@exp_RC INT = NULL"
         }), msg); 
      }

      [TestMethod] 
      public void Test_Person_Update()
      { 
         Assert.IsTrue(Helper("dbo.sp_person_update"    , "Person"    , "dbo.PersonVw"   , out string msg
            , exp_clauses: new[] 
         {
            " @tst_num INT",
            " ,@exp_RC INT = NULL"
         }), msg); 
      }

      [TestMethod] 
      public void Test_Person_Get1()
      { 
         Assert.IsTrue(Helper("dbo.sp_person_get1"      , "Person"    , "dbo.PersonVw"   , out string msg
            , exp_clauses: new[] 
         {
            " @tst_num INT",
            " ,@exp_RC INT = NULL"
         }), msg); 
      }

      [TestMethod] 
      public void Test_Person_GetAll()
      { 
         Assert.IsTrue(Helper("dbo.sp_candidate_update" , "Person"    , "dbo.PersonVw"   , out string msg
            , exp_clauses: new[] 
         {
            " @tst_num INT",
            " ,@exp_RC INT = NULL"
         }), msg); 
      }
   
      [TestMethod] 
      public void Test_Person_Delete()
      { 
         Assert.IsTrue(Helper("dbo.sp_person_delete"    , "Person"    , "dbo.PersonVw"   , out string msg
            , exp_clauses: new[] 
         {
         " ,@exp_RC INT = NULL" 
         }) , msg); 
      }

      [TestMethod] 
      public void Test_fnGetStaticDataCount()
      { 
         Assert.IsTrue(Helper("dbo.fnGetStaticDataCount", "Person"    , "dbo.PersonVw"   , out string msg
            , exp_clauses: new[] 
         {
            " @tst_num INT",
            " ,@exp_RC INT = NULL"
         }), msg); 
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="qTstdRtnNm"></param>
      /// <param name="table"></param>
      /// <param name="view"></param>
      /// <param name="msg"></param>
      /// <param name="exp_clause"></param>
      /// <returns></returns>
      protected bool Helper(string qTstdRtnNm, string table, string view, out string msg, string[]? exp_clauses = null)
      { 
         msg = "";
         var c = new TestableSqlTestCreator();

         Assert.AreEqual(0, c.Init
            (
                qTstdRtnNm : qTstdRtnNm
               ,tstNum     : 900
               ,conn_str   :  GetDefaultConnectionString()
               ,msg        : out msg
               ,table      : table
               ,view       : view
            ),
            msg
         );

         c.HlprScrptFile = Path.GetFullPath($"{TestClassName}.{TestMethod}_hlpr.sql");
         c.MnScrptFile   = Path.GetFullPath($"{TestClassName}.{TestMethod}_test.sql");

         c.Hlpr_Create_Sig_Bloc();
         var script = c.SB.ToString();

         if(exp_clauses != null)
         {
            var lines = script.Split("\r\n");

            for (int i = 0; i < lines.Length; i++)
               lines[i] = Squish(lines[i]);

            foreach (var exp_clause in exp_clauses)
            { 
               var line = Array.Find(lines, x => x.Contains(exp_clause));
               // Close down of padding to 1 space as the padding may vary

               if(line == null)
               {
                  msg = $"exp_clause [{exp_clause}] not found in script";
                  DisplayScript(c.SB, GetTestOutputFileNameSql());
                  return false;
               }
            }
         }
         else
         {
            DisplayScript(script, GetTestOutputFileNameSql());
         }

         return true;
      }
   }
}
