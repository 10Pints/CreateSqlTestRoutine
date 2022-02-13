using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using Tests.Test_Support;

namespace Tests
{
   [TestClass]
   public class __Hlpr_CreateScriptUnitTests : CreateSqlTestsBase
   {
      [TestMethod]
      public void Test_person_create()
      {
         string msg;
         var c = new TestableSqlTestCreator(ConnectionString, true);
         Assert.AreEqual(0, c.Init(qTstdRtnNm: "dbo.sp_person_create", tstNum: 900, conn_str: ConnectionString, out msg), msg);
         Assert.IsTrue(c.__Hlpr_CreateScript(out var script, out  msg), msg);
         File.WriteAllText( "script.sql", script);

         // test sql validity: run the script
         Assert.IsTrue(RunScript(script, out msg), msg);
      }
   }
}
