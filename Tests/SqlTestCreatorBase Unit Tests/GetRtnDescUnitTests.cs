using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Tests.Test_Support;

namespace Tests
{
   [TestClass]
   public class GetRtnDescUnitTests : CreateSqlTestsBase
   {
      /// <summary>
      /// Smoke test
      /// </summary>
      [TestMethod]
      public void TestGetRtnDesc()
      {
         var exp = @"Rec: Create Candidate 
-- SP name format: dbo.sp_[table]_[fn]
-- where fn: GetAll, Get, Insert, Update, Delete
--
-- DESIGN:
--    x
--
-- TESTS:
--    test_022_smoke_sp_candidate_create
--";
         var exp_len = exp.Length;
         var crtr = new TestableSqlTestCreator();
         var desc = crtr.GetRtnDesc("sp_candidate_create") ?? "";
         var desc_len = desc.Length;

         DisplayScript(desc, "TestGetRtnDesc.sql");

         Assert.IsFalse(string.IsNullOrWhiteSpace(desc));
         Assert.AreNotEqual(0, desc?.Length ?? 0);
         Assert.AreEqual(exp_len, desc_len);
         Assert.IsTrue(exp.Equals(desc, StringComparison.OrdinalIgnoreCase));
      }
   }
}
