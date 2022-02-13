using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CreateSqlTestRoutineLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tests.Test_Support;

namespace Tests
{
   [TestClass]
   public class GetRtnDetailsFromDbUnitTests : CreateSqlTestsBase
   {
      [TestMethod]
      public void TestGetRtnDetailsFromDb()
      {
         var c = new TestableSqlTestCreator();
         c.SetSchema("dbo");
         c.SetTable("Candidate");
         Assert.AreEqual(0, c.GetRtnDetailsFromDb("dbo", "sp_Candidate_get1", out List<RtnDetail> rtnDetailInfo, out var msg), msg);

         // smoke: 1 line
         Assert.AreEqual(1, rtnDetailInfo.Count());
         // check presets: line 0 = "SET ANSI_NULLS ON"
         string exp = "dbo";
         string act = rtnDetailInfo[0]?.schema_nm ?? "";
         Assert.IsTrue(exp.Equals(act));
         exp = "sp_Candidate_Get1";
         act = rtnDetailInfo[0]?.rtn_nm ?? "";
         Assert.IsTrue(exp.Equals(act));
      }
   }
}
