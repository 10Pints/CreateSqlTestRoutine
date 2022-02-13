using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tests.Test_Support;

namespace Tests
{
   [TestClass]
   public class ChkLineForDescEndUnitTests : CreateSqlTestsBase
   {
      [TestMethod]
      public void TestChkLineForDescEnd()
      {
         var crtr = new TestableSqlTestCreator();
         Assert.IsTrue(crtr.ChkLineForDescEnd("CREATE PROCEDURE"));
         Assert.IsTrue(crtr.ChkLineForDescEnd("   CREATE \t \r\nPROCEDURE"));
         Assert.IsTrue(crtr.ChkLineForDescEnd(" \t \r\nCREATE PROCEDURE"));
         Assert.IsFalse(crtr.ChkLineForDescEnd(" \t \r\nCREATE \t \r\nPPROCEDURE"));
         Assert.IsTrue(crtr.ChkLineForDescEnd(" \t \r\nCREATE \t \r\nPROCEDURE"));
         Assert.IsTrue(crtr.ChkLineForDescEnd("CREATE FUNCTION"));
         Assert.IsTrue(crtr.ChkLineForDescEnd("   CREATE \t \r\nFUNCTION"));
         Assert.IsTrue(crtr.ChkLineForDescEnd(" \t \r\nCREATE FUNCTION"));
         Assert.IsTrue(crtr.ChkLineForDescEnd(" \t \r\nCREATE \t \r\nFUNCTION"));

         Assert.IsTrue(crtr.ChkLineForDescEnd("-- ====="));
         Assert.IsTrue(crtr.ChkLineForDescEnd("--====="));
         Assert.IsTrue(crtr.ChkLineForDescEnd("  --====="));
      }
   }
}
