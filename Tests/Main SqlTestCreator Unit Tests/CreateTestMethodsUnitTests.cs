using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using System.IO;
using Tests.Test_Support;

namespace Tests
{
   [TestClass()]
   public class CreateTestMethodsUnitTests : CreateSqlTestsBase
   {
      [TestMethod] public void TestCreateTestMethods_fnGetStaticDataCount() { Assert.IsTrue(Helper("dbo.fnGetStaticDataCount", 501, out var msg), msg); }
      [TestMethod] public void TestCreateTestMethods_Person_Create()        { Assert.IsTrue(Helper("dbo.sp_person_create",     502, out var msg), msg); }
      [TestMethod] public void TestCreateTestMethods_Person_Delete()        { Assert.IsTrue(Helper("dbo.sp_person_Delete",     503, out var msg), msg); }
      [TestMethod] public void TestCreateTestMethods_Person_Get1()          { Assert.IsTrue(Helper("dbo.sp_person_Get1",       504, out var msg), msg); }
      [TestMethod] public void TestCreateTestMethods_Person_GetAll()        { Assert.IsTrue(Helper("dbo.sp_person_GetAll",     505, out var msg), msg); }
      [TestMethod] public void TestCreateTestMethods_Person_Update()        { Assert.IsTrue(Helper("dbo.sp_person_Update"    , 506, out var msg), msg); }

      protected bool Helper(string qtstdRtn, int tstNum, out string error_msg, string? table=null, string? view=null)
      {
         var c = new TestableSqlTestCreator(ConnectionString, true);

         if (0 != c.__CrtTestMethods(qtstdRtn, tstNum, out error_msg, table, view, out var hlpr_script, out var mn_script))
            return false;

         var hlprScriptFile = $"{TestMethod}_hlpr_script.sql";
         var mainScriptFile = $"{TestMethod}_main_script.sql";

         File.WriteAllText(hlprScriptFile, hlpr_script);
         File.WriteAllText(mainScriptFile, mn_script  );

         // Run both tests, don't stop on first failure
         bool ret  = RunScriptFile(hlprScriptFile, out var hlpr_scrpt_err_msgs, display: false);
              ret &= RunScriptFile(mainScriptFile, out var main_scrpt_err_msgs, display: false);

         //Process.Start("Notepad++.exe", hlprScriptFile);
         //Process.Start("Notepad++.exe", mainScriptFile);

         error_msg = (ret == true) ? "" : $"Helper script errors:\r\n{hlpr_scrpt_err_msgs}\r\nmain script errors:\r\n{main_scrpt_err_msgs}";
         return ret;
      }
   }
}
