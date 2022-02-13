using Microsoft.VisualStudio.TestTools.UnitTesting;
using RSS.Test;
using Serilog;

namespace Tests
{
   [TestClass]
   public class UnitTestBaseUnitTests : UnitTestBase
   {
      [TestMethod]
      public void Test_AssemblyInitialize()
      {
         Assert.IsNotNull(TestContext_);
         UnitTestBase.AssemblyInitialize(TestContext_ );
         Assert.IsNotNull(Log.Logger  , "Log.Logger is null");
         Assert.IsNotNull(TestContext_, "TestContext_ is null");
         Assert.IsFalse(string.IsNullOrWhiteSpace(GetDefaultConnectionString()), "DefaultConnectionString is null");
         Server = $@".\SqlExpress";
      }
   }
}
