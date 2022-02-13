using CreateSqlTestRoutineLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tests.Test_Support;

namespace Tests
{
   [TestClass]
   public class SqlTestCreatorBaseInitTests : CreateSqlTestsBase
   {

      [TestMethod]
      public void TestCreateMnTestName()
      {
         var crtr = new TestableSqlTestCreator();
         Assert.IsTrue("test_123_sp_cv_Get1".Equals(crtr.CrtMnTestName("sp_cv_Get1", 123)));
         Assert.IsTrue("test_092_sp_cv_Get1".Equals(crtr.CrtMnTestName("sp_cv_Get1", 92)));
         Assert.IsTrue("test_007_sp_cv_Get1".Equals(crtr.CrtMnTestName("sp_cv_Get1", 7)));
      }

      [TestMethod]
      public void TestCrtHlprTstNm()
      {
         var crtr = new TestableSqlTestCreator();
         Assert.IsTrue("helper_001_sp_cv_Get1".Equals(crtr.CrtHlprTstNm("sp_cv_Get1", 1)));
         Assert.IsTrue("helper_023_sp_cv_Get1".Equals(crtr.CrtHlprTstNm("sp_cv_Get1", 23)));
         Assert.IsTrue("helper_543_sp_cv_Get1".Equals(crtr.CrtHlprTstNm("sp_cv_Get1", 543)));
      }


      [TestMethod]
      public void TestDetermineTestTemplateType()
      {
         var c = new TestableSqlTestCreator();
         Assert.AreEqual(0, c.Init("dbo.sp_cv_Get1", 100, ConnectionString, out var msg), msg);

         Assert.AreEqual(TestType.Default, c.DetermineTestType("sp_cv_Get1_"));
         Assert.AreEqual(TestType.Get1,    c.DetermineTestType("sp_cv_Get1"));
         Assert.AreEqual(TestType.GetAll,  c.DetermineTestType("sp_cv_GetAll"));
         Assert.AreEqual(TestType.Update,  c.DetermineTestType("sp_cv_Update"));
         Assert.AreEqual(TestType.Create,  c.DetermineTestType("sp_cv_Create"));
         Assert.AreEqual(TestType.Delete,  c.DetermineTestType("sp_cv_Delete"));
      }

      [TestMethod]
      public void TestValidate()
      {
         string error_msg = "";
         var c = new TestableSqlTestCreator();
         Assert.AreEqual(0,    c.Validate("sp_cv_Get1",       1, error_msg: out error_msg), error_msg);
         Assert.AreEqual(0,    c.Validate("sp_cv_Get1",       2, error_msg: out error_msg), error_msg);
         Assert.IsTrue(error_msg.Equals(""), error_msg);
         Assert.AreEqual(1200, c.Validate("",                 3, error_msg: out error_msg), error_msg);
         Assert.IsTrue(error_msg.Equals("E1200: the tested routine must be specified"), error_msg);
         Assert.AreEqual(1200,    c.Validate(null,            4, error_msg: out error_msg), error_msg);
         Assert.IsTrue(error_msg.Equals("E1200: the tested routine must be specified"), error_msg);
         Assert.AreEqual(1201,    c.Validate("sp_cv_Get1",    0, error_msg: out error_msg), error_msg);
         Assert.IsTrue(error_msg.Equals("E1201: the test number must be > 0"), error_msg);
         Assert.AreEqual(1201,    c.Validate("sp_cv_Get1",   -1, error_msg: out error_msg), error_msg);
         Assert.IsTrue(error_msg.Equals("E1201: the test number must be > 0"), error_msg);
         Assert.AreEqual(1202,    c.Validate("sp_cv_Get1", 1000, error_msg: out error_msg), error_msg);
         Assert.IsTrue(error_msg.Equals("E1202: the test number must be < 1000"), error_msg);
      }

      [TestMethod]
      public void TestNeedTable()
      {
         Assert.IsFalse(TestableSqlTestCreator.NeedsTable(TestType.Default));
         Assert.IsTrue (TestableSqlTestCreator.NeedsTable(TestType.Get1));
         Assert.IsTrue (TestableSqlTestCreator.NeedsTable(TestType.GetAll));
         Assert.IsTrue (TestableSqlTestCreator.NeedsTable(TestType.Create));
         Assert.IsTrue (TestableSqlTestCreator.NeedsTable(TestType.Update));
         Assert.IsFalse(TestableSqlTestCreator.NeedsTable(TestType.Unknown));
         Assert.IsFalse(TestableSqlTestCreator.NeedsTable(TestType.Default));
         Assert.IsFalse(TestableSqlTestCreator.NeedsTable(TestType.Default));
      }

      [TestMethod]
      public void TestChkInitPostConditions()
      {
         string error_msg;
         var c = new TestableSqlTestCreator(GetDefaultConnectionString());
         Assert.AreEqual(1200, c.ChkInitPostConditions(out error_msg), error_msg); // Validation fails
         Assert.IsTrue(error_msg.Equals("E1200: the tested routine must be specified"), error_msg);
         c.SetSchema("dbo");
         Assert.AreEqual(1201, c.ChkInitPostConditions(out error_msg), error_msg);    // Validation fails
         Assert.IsTrue(error_msg.Equals("E1201: the test number must be > 0"), error_msg);
         c.SetTstNum(1);
         //c.SetConnectionString = GetDefaultConnectionString();
         c.SetSchema("");
         c.SetTstdRtnNm("MyTstdRtnName");
         Assert.AreEqual(1123, c.ChkInitPostConditions(out error_msg), error_msg);    // Validation fails: E1122: P02 violation: the Database name is not specified.
         Assert.IsTrue(error_msg.Equals("E1123: P03 violation: Schema is not specified."), error_msg);
         c.SetDbName("Telepat");
         c.SetTstdRtnNm("abc");
         c.SetSchema("dbo");
         Assert.AreEqual(1125, c.ChkInitPostConditions(out error_msg), error_msg);   // Validation succeeds but ChkInitPostConditions should still fail
         Assert.IsTrue( error_msg.Equals("E1125: P05 violation: Could not determine the test type for dbo.abc"), error_msg);

         c.SetTstdRtnNm("sp_person_create");
         Assert.AreEqual(1125, c.ChkInitPostConditions(out error_msg), error_msg);   // Validation succeeds but ChkInitPostConditions should still fail
         Assert.IsTrue(error_msg.Contains("E1125: P05 violation: Could not determine the test type for "), error_msg);
      }
   }
}
