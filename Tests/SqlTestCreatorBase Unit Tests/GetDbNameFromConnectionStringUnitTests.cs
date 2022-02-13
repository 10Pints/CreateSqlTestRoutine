using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tests.Test_Support;

namespace Tests
{
   [TestClass]
   public class GetDbNameFromConnectionStringUnitTests : CreateSqlTestsBase
   {
      // POST 1: db_name found in the connection string and not null or error.
      [TestMethod]
      public void Test_Init_GetDbNameFromConnectionString_WhenEmptyConstr()
      {
         var expSvr = "DevI9";
         var expDb = "Telepat";
         var expConnStr = @$"Data Source={expSvr};Initial Catalog={expDb};Integrated Security=True";
         var c = new TestableSqlTestCreator(expConnStr);
         Assert.IsTrue(c.ConnectionString.Equals(expConnStr), @$"server not set properly - should be '{expConnStr}\r\nbut was\r\n{c.ConnectionString}");
         Assert.IsTrue(c.Server.Equals(expSvr), @$"server not set properly - should be '{expSvr}'\r\nbut was\r\n{c.Server}");
         Assert.IsTrue(c.Database.Equals(expDb), @$"database not set properly - should be '{expDb}'\r\nbut was\r\n{c.Database}");
      }
      /*
            // POST 1: db_name found in the connection string and not null or error.
            [TestMethod]
            public void GetDbNameFromConnectionString_WhenNullConstr()
            {
      #pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
               Assert.AreEqual(1350, TestableSqlTestCreator.SetServerAndDatabaseFromConnectionString(null, out var msg, out string db_name), msg);
      #pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
               Assert.IsTrue(msg.Equals("E1350: connection string is not specified"), msg);
            }

            ///   POST 2: conn_str must contain the 'Initial Catalog' key
            [TestMethod]
            public void GetDbNameFromConnectionString_Chk_ConnStrContainIInitialCatalogKey()
            {
               Assert.AreEqual(1351, TestableSqlTestCreator.SetServerAndDatabaseFromConnectionString("Data Source = DevI9\\SqlExpress; Initil Catalog = Telepat; Integrated Security = True", out var msg, out string db_name), msg);
               Assert.IsTrue(msg.Equals("E1351: Failed to extract the Initial Catalog key from the connection string"), msg);
            }
            // "Failed to extract Initial Catalog key from the connection string"
            // 
            ///   POST 3: the conn_str 'Initial Catalog' key must have a non mull or empty value
            [TestMethod]
            public void GetDbNameFromConnectionString_WhenConstr()
            {
               Assert.AreNotEqual(0, TestableSqlTestCreator.SetServerAndDatabaseFromConnectionString(null, out var msg, out string db_name), msg);
            }
      */
   }
}
