using CreateSqlTestRoutineLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text;
using Tests.Test_Support;

namespace Tests
{
   [TestClass]
   public class GetCallRtnArgsUnitTests : CreateSqlTestsBase
   {

      [TestMethod]
      public void TestGetCallRtnArgs()
      {
         var crtr = new TestableSqlTestCreator();
         crtr.Init("dbo.sp_candidate_create", 100, conn_str: GetDefaultConnectionString(), out string error_msg, table: "Candidate", view: "");
         StringBuilder sb = new StringBuilder();
         crtr.GetCallRtnArgs(inlineParams: false);
         DisplayScript(sb, "TestGetCallRtnArgs.sql");
      }
   }
}
