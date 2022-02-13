using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CreateSqlTestRoutineLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tests.Test_Support;
#pragma warning disable CS8602 // Dereference of a possibly null reference.

namespace Tests
{
   [TestClass]
   public class GetPrmDetailsFromDbUnitTests : CreateSqlTestsBase
   {
      [TestMethod]
      public void TestGetPrmDetailsFromDb()
      {
         var c = new TestableSqlTestCreator();
  
         Assert.AreEqual(0, c.GetPrmDetailsFromDb
         (
            "dbo",
            "sp_Candidate_create",
            out List<ParamInfo> paramInfoList,
            out var msg), msg
         );

         // smoke: 1 line
         Assert.AreEqual(12, paramInfoList.Count());
         // check presets: line 0 = "SET ANSI_NULLS ON"
         ParamInfo i = paramInfoList[0];

         Assert.IsTrue(i.ordinal             == 1);
         Assert.IsTrue(i.col_nm.ToLower()    == "@family_name", i.col_nm);
         Assert.IsTrue(i.ty_nm.ToLower()     == "nvarchar(50)", i.ty_nm);
         Assert.IsTrue(i.ty_len              == 50);
         Assert.IsTrue(i.is_pk               == false);
         Assert.IsTrue(i.is_output           == false);
         Assert.IsTrue(i.schema_nm.ToLower() == "dbo");
         Assert.IsTrue(i.rtn_nm.ToLower()    == "sp_candidate_create");
         Assert.IsTrue(i.rtn_ty.ToLower()    == "sql_stored_procedure");

         i = paramInfoList[3];
         Assert.IsTrue(i.ordinal             == 4);
         Assert.IsTrue(i.col_nm.ToLower()    == "@seq");
         Assert.IsTrue(i.ty_nm.ToLower()     == "int");
         Assert.IsTrue(i.ty_len == 4);

         i = paramInfoList[8];
         Assert.IsTrue(i.ordinal             == 9);
         Assert.IsTrue(i.col_nm.ToLower()    == "@candidate_id");
         Assert.IsTrue(i.ty_nm.ToLower()     == "int");
         Assert.IsTrue(i.is_pk               == true);
         Assert.IsTrue(i.is_output           == true);

         i = paramInfoList[9];
         Assert.IsTrue(i.ordinal == 10);
         Assert.IsTrue(i.col_nm.ToLower()    == "@status_id");
         Assert.IsTrue(i.ty_nm.ToLower()      == "int");
         Assert.IsTrue(i.is_pk               == false);
         Assert.IsTrue(i.is_output           == true);

         i = paramInfoList[10];
         Assert.IsTrue(i.ordinal == 11);
         Assert.IsTrue(i.col_nm.ToLower()    == "@key");
         Assert.IsTrue(i.is_pk               == false);
         Assert.IsTrue(i.is_output           == true);

         i = paramInfoList[11];
         Assert.IsTrue(i.ordinal == 12);
         Assert.IsTrue(i.col_nm.ToLower()    == "@msg");
         Assert.IsTrue(i.is_pk               == false);
         Assert.IsTrue(i.is_output           == true);
      }
   }
}
#pragma warning restore CS8602 // Dereference of a possibly null reference.
