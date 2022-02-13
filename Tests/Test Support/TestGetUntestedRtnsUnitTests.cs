using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using Tests.Test_Support;

namespace Tests
{
   [TestClass]
   public class TestGetUntestedRtnsUnitTests : CreateSqlTestsBase
   {
      /// <summary>
      /// Smoke test
      /// </summary>
      [TestMethod]
      public void TestGetUntestedRtns()
      {
         var c = new TestableSqlTestCreator();
         var untstdRtns = c.GetUntestedRtns();
         Assert.IsNotNull(untstdRtns);
         Assert.AreEqual(untstdRtns.Count, untstdRtns.Distinct().Count());
      }
   }
}
