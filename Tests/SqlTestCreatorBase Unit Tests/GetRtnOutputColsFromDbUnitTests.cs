using CreateSqlTestRoutineLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using Tests.Test_Support;

namespace Tests
{
   [TestClass]
   public class GetRtnOutputColsFromDbUnitTests : CreateSqlTestsBase
   {
      [TestMethod]
      public void TestGetRtnOutputColsFromDb()
      {
         var c = new TestableSqlTestCreator();
         c.SetSchema("dbo");
         c.SetTable("Candidate");

         Assert.AreEqual(0, c.GetRtnOutputColsFromDb
         (
            "dbo",
            "sp_Candidate_get1",
            out List<ParamInfo> rtnDetailInfoList,
            out var msg), msg
         );

         // smoke: 1 line
         Assert.AreEqual(11, rtnDetailInfoList.Count());
         // check presets: line 0 = "SET ANSI_NULLS ON"
         ParamInfo i = rtnDetailInfoList[0];

         Assert.IsTrue(1                  == i.ordinal         );
         Assert.IsTrue("candidate_id"     == (i.col_nm ?? "").ToLower());
         Assert.IsTrue("int"              == (i.ty_nm  ?? "").ToLower());
         Assert.IsTrue(56                 == i.ty_id          );
         Assert.IsTrue(4                  == i.ty_len         );
         Assert.IsTrue(true               == i.is_nullable    );
         Assert.IsTrue("sp_candidate_get1"== i.rtn_nm    .ToLower());
         Assert.IsTrue("sp"               == i.rtn_ty    .ToLower());
         Assert.IsTrue("dbo"              == (i.schema_nm ?? "").ToLower());
      }
   }
}
