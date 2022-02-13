using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CreateSqlTestRoutineLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RSS.Test;
using Tests.Test_Support;

namespace Tests
{
   [TestClass]
   public class MnRtnCrtHlprCallUnitTests : CreateSqlTestsBase
   {
      [TestMethod]
      public void Test_Candidate_Create()
      {
         var c = new TestableSqlTestCreator();

         Assert.AreEqual(0,
            c.Init
            (
               qTstdRtnNm: "dbo.sp_candidate_create",
               tstNum: 900, conn_str: ConnectionString,
               msg: out string msg,
               table: "Candidate",
               view: "dbo.CandidateVw"
            ),
            msg
         );

         c.Mn_Create_HlprCall();
         //DisplayScript(c.SB, "Test_MnRtnCrtHlprCall_Candidate_Create.sql");
         var script = c.SB.ToString();
         var lines = RemoveDebugComments( script.Split("\r\n"));
         var line = lines[1];
         // Close down of padding to 1 space as the padding may vary
         var line_depadded = Squish(line);
         Assert.IsTrue(line_depadded.Equals(" @tst_num = 1"), $"exp: [ @tst_num = 1] act: [{line_depadded}]");
      }
   }
}
