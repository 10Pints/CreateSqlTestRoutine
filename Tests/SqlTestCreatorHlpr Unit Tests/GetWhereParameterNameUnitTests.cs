using CreateSqlTestRoutineLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tests.Test_Support;

namespace Tests
{
   [TestClass]
   public class GetWhereParameterNameUnitTests : CreateSqlTestsBase
   {
      [TestMethod]
      public void Test_GetWhereParameterName_For_sp_candidate_delete()
      {
         TestableSqlTestCreator c = new TestableSqlTestCreator();
         Assert.AreEqual(0, c.Init("dbo.sp_candidate_delete", 1, ConnectionString, out var msg, "Candidate", "CandidateVw"), msg);
         Assert.IsNotNull(c.PK_List);

         KeyInfo pk = c.PK_List[0];
         Assert.IsNotNull(pk);
         var col_nm = pk.col_nm;
         Assert.IsNotNull(col_nm);
         c.GetWhereParameterName(col_nm, out string where_param_nm);
         Assert.IsTrue(where_param_nm.Equals("@act_candidate_id"), "pk_inp_var_nm mismatch");
      }
   }
}
