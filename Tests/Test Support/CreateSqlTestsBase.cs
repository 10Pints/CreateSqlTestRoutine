using RSS.Test;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.Configuration;
using CreateSqlTestRoutineLib;

namespace Tests.Test_Support
{
   [TestClass]
   public class CreateSqlTestsBase : UnitTestBase
   {
      [AssemblyInitialize]
      public new static void AssemblyInitialize(TestContext context)
      {
         UnitTestBase.AssemblyInitialize(context);
         // Get the default connectionstring from the settings
         SqlTestCreatorBase.DefaultConnectionString = Configuration?.GetConnectionString("Default") ?? ""; // "" will cause a precondition expeception to file
         Common.ErrorMsgs.Init("CreateSqlTestRoutineErrorMsgs.txt");
      }

      [TestInitialize]
      public new void TestSetup()
      {
         base.TestSetup();
      }

      internal CreateSqlTestsBase()
      {
         Server = $@".\SqlExpress";
         Database = "Telepat";
      }
   }
}
