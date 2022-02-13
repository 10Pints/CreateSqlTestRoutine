using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RSS.Test;
using Tests.Test_Support;

namespace Tests
{
   [TestClass]
   public class GetOutArgsUnitTests : CreateSqlTestsBase
   {
      [TestMethod]
      public void Test_GetOutArgs_For_sp_candidate_delete_Exp_1()
      {
         TestableSqlTestCreator c = new TestableSqlTestCreator();
         Assert.AreEqual(0, c.Init("dbo.sp_candidate_delete", 1, ConnectionString, out var msg, "Candidate", "CandidateVw"), msg);
         Assert.AreEqual(1, c.OutArgsMap.Count(), "Expected 1 OutArg");
      }

      [TestMethod]
      public void Test_GetOutArgs_For_sp_Candidate_Get1_Exp_0()
      {
         TestableSqlTestCreator c = new TestableSqlTestCreator();
         Assert.AreEqual(0, c.Init("dbo.sp_Candidate_Get1", 1, ConnectionString, out var msg, "Candidate", "CandidateVw"), msg);
         Assert.AreEqual(0, c.OutArgsMap.Count(), "Expected 0 OutArg");
      }

      [TestMethod]
      public void Test_GetOutArgs_For_tv_fnGetCandidateSkills_Exp_0()
      {
         TestableSqlTestCreator c = new TestableSqlTestCreator();
         Assert.AreEqual(0, c.Init("dbo.fnGetCandidateSkills", 1, ConnectionString, out var msg, "Candidate", "CandidateVw"), msg);
         Assert.AreEqual(0, c.OutArgsMap.Count(), "Expected 0 OutArg");
      }

      [TestMethod]
      public void Test_GetOutArgs_For_scalar_fnGetPersonKey_Exp_0()
      {
         TestableSqlTestCreator c = new TestableSqlTestCreator();
         Assert.AreEqual(0, c.Init("dbo.fnGetCandidateSkills", 1, ConnectionString, out var msg, "Candidate", "CandidateVw"), msg);
         Assert.AreEqual(0, c.OutArgsMap.Count(), "Expected 0 OutArg");
      }

      /// <summary>
      /// Detailed check sp returns 4 outout params
      /// </summary>
      [TestMethod]
      public void Test_GetOutArgs_For_sp_candidate_create_Exp_4()
      {
         TestableSqlTestCreator c = new TestableSqlTestCreator();
         Assert.AreEqual(0, c.Init("dbo.sp_candidate_create", 1, ConnectionString, out var msg, "Candidate", "CandidateVw"), msg);
         Assert.AreEqual(4, c.OutArgsMap.Count(), "Expected 0 OutArg");
         var p = c.OutArgsMap["key"];
         Assert.IsTrue(p.is_output);
         p = c.OutArgsMap["status_id"];
         Assert.IsTrue(p.is_output);
         p = c.OutArgsMap["candidate_id"];
         Assert.IsTrue(p.is_output);
         p = c.OutArgsMap["msg"];
         Assert.IsTrue(p.is_output);
      }

   }
}
