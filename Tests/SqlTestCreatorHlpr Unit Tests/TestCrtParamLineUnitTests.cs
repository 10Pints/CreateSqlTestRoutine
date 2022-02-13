using CreateSqlTestRoutineLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Text;
using Tests.Test_Support;

namespace Tests
{
   [TestClass]
   public class TestCrtParamLineUnitTests : CreateSqlTestsBase
   {
      [TestMethod]
      public void TestCrtParamLine()
      {
         var c = new TestableSqlTestCreator();
         string msg = "";
         Assert.AreEqual(0, c.Init("dbo.sp_candidate_create", 901, conn_str: GetDefaultConnectionString(), out msg, "Candidate", "CandidateVw"), msg);
         var pr = c.ParamMap.FirstOrDefault(x => x.Value.ordinal == 1);
         ParamInfo param = pr.Value; ; 

         c.CrtParamLine_Hlpr(
               param,
               needsComma: true,
               ignore_output_params: false,
               needsOuPutToken: true,
               needsEqualsNull: true,
               prefix: "exp_",
               preTabCount: 2,
               tabstops: new int[4] { 4, 24, 38, 40 },
               out msg
            );

         //param = c.ParamMap.FirstOrDefault(x=>x.Value.col_ordinal==9); // 
         pr = c.ParamMap.FirstOrDefault(x => x.Value.ordinal == 9);
         param = pr.Value;

         c.CrtParamLine_Hlpr(
               param,
               needsComma: true,
               ignore_output_params: false,
               needsOuPutToken: true,
               needsEqualsNull: true,
               prefix: "exp_",
               preTabCount: 2,
               tabstops: new int[4] { 4, 24, 40, 50 },
               out msg
            );

         pr = c.ParamMap.FirstOrDefault(x => x.Value.ordinal == 10);
         param = pr.Value;

         c.CrtParamLine_Hlpr(
               param,
               needsComma: true,
               ignore_output_params: false,
               needsOuPutToken: true,
               needsEqualsNull: true,
               prefix: "exp_",
               preTabCount: 2,
               tabstops: new int[4] { 4, 24, 40, 50 },
               out msg
            );

         var script = Squish(c.SB.ToString());
         var lines  = SquishScript(script);
         bool ret   = false;

         do
         { 
            if(3 != lines.Length)
            {
               msg =  $"expected 3 lines got {lines.Length}";
               break;
            }
            
            //Assert.IsTrue(ChkContains(script, " ,@exp_candidate_id INT OUTPUT = NULL", 1, out msg), msg);
            if(!ChkContains2( script, @" ,@exp_candidate_id INT OUTPUT = NULL", 1 , out msg))
               break;

            if (!ChkContains2(script, @" ,@exp_family_name NVARCHAR\(50\) = NULL", 1, out msg))
               break;
            
            if(!ChkContains2(script, @" ,@exp_candidate_id INT OUTPUT = NULL", 1, out msg))
               break;

            // Finally 
            ret = true;
         } while(false);

         if(!ret)
            DisplayScript(c.SB, "TestCrtParamLine.sql");

         Assert.IsTrue(ret, msg);
      }
   }
}
