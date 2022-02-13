using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text;
using Tests.Test_Support;

namespace Tests
{
   /// <summary>
   ///   tstd rtn summary: 
   ///      Hlpr_Create_Declare_BlocForActuals(int[] tabstops):
   ///         Creates the @act_  declaration bloc
   ///         returns: count of exp params
   /// </summary>
   [TestClass]
   public class CreateActDeclsUnitTests : CreateSqlTestsBase
   {
      [TestMethod]
      public void Test_sp_candidate_Create()
      {
         Assert.IsTrue(Helper("dbo.sp_candidate_Create", 12, out string msg), msg);
      }

      [TestMethod]
      public void Test_sp_candidate_Get1()
      {
         Assert.IsTrue(Helper("dbo.sp_candidate_Get1", 11, out string msg), msg);
      }

      [TestMethod]
      public void Test_sp_candidate_GetAll()
      {
         Assert.IsTrue(Helper("dbo.sp_candidate_GetAll", 11, out string msg), msg);
      }

      protected bool Helper( string qTstdRtnNm, int exp_cnt, out string msg)
      {
         msg = "";
         var c = new TestableSqlTestCreator();
         var tabstops = new int[] { 0, 20, 35, 45 };

         if(0 != c.Init(
                qTstdRtnNm: qTstdRtnNm,
                tstNum: 900, conn_str: ConnectionString,
                msg: out msg,
                table: "C2S",
                view: "dbo.C2SVw"
             ))
         {
            return false;
         }

         var act_cnt = c.Hlpr_Create_Declare_BlocForActuals(tabstops);
         var script = c.SB.ToString();

         if(exp_cnt != act_cnt)
         {
            msg = $"exp count: {exp_cnt}  act count: {act_cnt}";
            DisplayScript(c.SB, "CreateActDeclsUnitTests_Candidate_Create.sql");
            return false;
         };

         return true;
      }
   }
}
