using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using CreateSqlTestRoutineLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RSS.Test;
using Tests.Test_Support;

namespace Tests.SqlTestCreatorBase_Unit_Tests
{
   [TestClass]
   public class GetObjTableAndViewFromDbUnitTests : CreateSqlTestsBase
   {
      [TestMethod]
      public void Test_sp_candidate_delete_Exp_1()
      {
         Assert.IsTrue(Helper( "dbo", "sp_candidate_delete", out string msg, exp_cnt: 1, items: new[] { "Person" }), msg); // , t: "candidate", v: "candidateVw"
      }

      [TestMethod]
      public void Test_sp_candidate_create()
      {
         Assert.IsTrue(Helper("dbo", "sp_candidate_create", out string msg, exp_cnt: 9, items: new[] 
         { 
             "Candidate"
            ,"fnGetCandidateStatusIdFromName" 
            ,"fnGetPersonKey"
            ,"sp_person_create"
            ,"CandidateStatus"
            ,"Person"
            ,"PersonType"
            ,"Salutation"
            }), msg); // , t: null, v: "candidateVw"
      }

      [TestMethod]
      public void Test_sp_candidate_get1()
      {
         Assert.IsTrue(Helper("dbo", "sp_candidate_Get1", out string msg, exp_cnt: 10, items: new[] { "CandidateVw" }), msg); // , t: "candidate", v: "candidateVw"
      }

      [TestMethod]
      public void Test_sp_candidate_getAll()
      {
         Assert.IsTrue(Helper("dbo", "sp_candidate_GetAll", out string msg, exp_cnt: 9, items: new[] { "CandidateVw" }), msg); // , t: "candidate", v: "candidateVw"
      }

      [TestMethod]
      [ExpectedException(typeof(Exception))]
      public void Test_sp_unknow_rtn()
      {
         Assert.IsTrue(Helper("dbo", "sp_unknow_rtn", out string msg, exp_ec: 7254), msg); // , t: "candidate", v: "candidateVw"
      }

      [TestMethod]
      public void Test_tvf_fnCandidate_Get1()
      {
         Assert.IsTrue(Helper("dbo", "fnCandidate_Get1", out string msg, exp_cnt: 9, items: new[] { "CandidateVw" }), msg); // , t: "candidate", v: "candidateVw"
      }

      [TestMethod]
      public void Test_scalarf_fnGetDataCount()
      {
         Assert.IsTrue(Helper("dbo", "fnGetDataCount", out string msg, exp_cnt: 17, 
            items: new[] 
            { "PersonType", "Ops2OpsRole","ContactDetailType", "Candidate", "SkillLevel", "Salutation" 
            }), msg); // , t: "candidate", v: "candidateVw"
      }

      private bool Helper(
           string schemaNm, string objNm
         , out string msg, int? exp_cnt = null
         , string? t = null, string? v = null
         , string[]? items = null, int? exp_ec = null)
      {
         bool ret = false;
         bool cont = true;
         int error_code = -1; // not set
         TestableSqlTestCreator c = new();
         List<DependencyInfo> list = new();

         do
         { 
            error_code = c.Init($"{schemaNm}.{objNm}", 1, ConnectionString, out msg, t, v);

            if(error_code != 0)
               break;

            error_code = c.GetRtnDependencies(objNm, out list, out msg);

            if (error_code != 0)
               break;

            if(exp_cnt != null)
            {
               var act_cnt = list.Count;

               if (exp_cnt != act_cnt)
               { 
                  msg = $"Error: count mismatch: exp: {exp_cnt} act: {act_cnt}";
                  Console.WriteLine($"\r\n{msg}\r\n");

                  for (int i=0; i<list.Count;i++)
                  {
                     //var dep = list[i];
                     Console.WriteLine($"[{i}]: {list[i]}");
                  }

                     break;
               }
            }

            if(items != null)
            {
               foreach(string item in items)
               {
                  var tv = list.Find(x=>(x?.referenced_name ?? "").Equals(item, StringComparison.OrdinalIgnoreCase));

                  if(tv == null)
                  {
                     cont = false;
                     msg = $"failed to find {item} in the returned list";
                     break;
                  }
               }

               if(!cont)
                  break;
            }

            // finally
            ret = true;
         } while(false);

         // expected failures
         if(exp_ec != null)
            if(exp_ec == error_code)
               ret = true;

         Utils.Postcondition(((ret == true) &&(msg.Length==0)) || ((ret == false) &&(msg.Length != 0)), $"");
         return ret;
      }
   }
}
