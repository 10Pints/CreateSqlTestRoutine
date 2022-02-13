using CreateSqlTestRoutineLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using Tests.Test_Support;
#pragma warning disable CS8602 // Dereference of a possibly null reference.

namespace Tests
{
   [TestClass]
   public class GetPK_ListFromDbUnitTests : CreateSqlTestsBase
   {
      [TestMethod]
      public void TestGetPK_ListFromDb()
      {
         var c = new TestableSqlTestCreator();
         c.SetSchema("dbo");
         c.SetTable("Candidate");
         List<KeyInfo> pk_fields = c.GetPK_ListFromDb();

         // smoke: 1 line
         Assert.AreEqual(1, pk_fields.Count());

         // check presets: line 0 = "SET ANSI_NULLS ON"
         string exp = "candidate_id";
         string act = pk_fields[0]?.col_nm ?? "";
         Assert.IsTrue(exp.Equals(act));
      }

      [TestMethod]
      public void Test_fnGetPK_Fields()
      {
         var c = new TestableSqlTestCreator();
         c.SetSchema("dbo");
         c.SetTable("ContactDetail");
         List<KeyInfo> list = c.GetPK_ListFromDb();
         Assert.AreEqual(1, list.Count());
         KeyInfo pk = list[0];
         Assert.IsTrue(pk.col_nm.Equals("contactDetail_id"));
         Assert.IsTrue(pk.ty_nm.Equals("int"));
         Assert.IsTrue(pk.key_nm.Equals("PK_ContactDetail"));
         Assert.IsTrue(pk.key_ty.Equals(1));
         Assert.IsTrue(pk.ordinal.Equals(1));
         Assert.IsTrue(pk.schema_nm.Equals("dbo"));
         Assert.IsTrue(pk.tbl_nm.Equals("ContactDetail"));
      }
   }
}
#pragma warning restore CS8602 // Dereference of a possibly null reference.
