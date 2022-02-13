using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text;
using Tests.Test_Support;

namespace Tests
{
   [TestClass]
   public class Hlpr_Crt_Decl_BlocUnitTests : CreateSqlTestsBase
   {
      /// <summary>
      /// Bug 011: fnDoesSpReturnResultTable test is creating and tesing for inp params as well as exp params 
      /// - should only test for exp params 
      /// 220122: RULE for creating exp parameters : only test input params if a create or update type test (, and not a function: functions cannot write to tables: side effect not allowed)
      /// </summary>
      [TestMethod]
      public void Test_Default()
      {
         TestableSqlTestCreator c = new TestableSqlTestCreator();
         Assert.AreEqual( 0, c.Init("dbo.fnDoesSpReturnResultTable", 1, ConnectionString, out var msg, "Candidate", "CandidateVw"), msg);
         c.Hlpr_Create_Declare_Bloc();
         DisplayScript(c.SB, "Hlpr_Crt_Decl_BlocUnitTests_Test_Default.sql");
         var script = c.SB.ToString();
         Assert.IsFalse(script.Contains("@act_schema_nm"), "still contains @act_schema_nm");
         Assert.IsFalse(script.Contains("@act_sp_nm "   ), "still contains @act_sp_nm ");
      }
   }
}
