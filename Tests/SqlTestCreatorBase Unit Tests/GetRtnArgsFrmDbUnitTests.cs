using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CreateSqlTestRoutineLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tests.Test_Support;

namespace Tests
{
   [TestClass]
   public class GetRtnArgsFrmDbUnitTests : CreateSqlTestsBase
   {
      [TestMethod]
      public void TestCrtVarNm()
      {
         StringBuilder sb = new StringBuilder();
         var c = new TestableSqlTestCreator();
         Assert.AreEqual(0,  c.Init("dbo.sp_candidate_create", 100, conn_str: ConnectionString, out var error_msg, table: "Candidate", view: "CandidateVw"), error_msg);
         Dictionary<string, ParamInfo> args = c.GetRtnParamsFromDb(c.Schema, c.TstdRtnNm) ?? new Dictionary<string, ParamInfo>();

         bool needsComma = false;

         foreach(var pr in args)
         {
            ParamInfo p = pr.Value;
            sb.AppendLine(c.CrtVarNm(p.col_nm ?? "", true, p.is_output, needsComma));
            needsComma = true;
         }

         DisplayScript(sb, "TestCrtVarNm.sql");

         // Detailed test: check parameter nm does not have an @
         ParamInfo arg = args["first_name"];
         Assert.IsTrue(arg.ordinal == 2, "ordinal");
         Assert.IsTrue((arg.col_nm ?? "").Equals("first_name"), "col_nm"); // check parameter nm does not have an @
         Assert.IsTrue(arg.ty_id == 231);
      }

      /// <summary>
      /// 
      /// </summary>
      [TestMethod]
      public void TestGetRtnArgsFrmDb_WhenNoArgs()
      {
         var crtr = new TestableSqlTestCreator();
         Dictionary<string, ParamInfo>? param_list = crtr.GetRtnParamsFromDb("dbo", "sp_contactDetail_GetAll");
         Assert.IsNotNull(param_list);
         Assert.IsTrue(param_list.Count == 0);

         StringBuilder sb = new StringBuilder();
         sb.AppendLine(ParamInfo.GetHdr());

         foreach (var param in param_list)
            sb.AppendLine(param.Value.ToString());

         var script = sb.ToString();
         DisplayScript(script, "TestGetRtnArgsFrmDb.sql");
      }
   }
}
