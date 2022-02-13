using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using CreateSqlTestRoutineLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tests.Test_Support;

namespace Tests.Bug_Tests
{
   [TestClass]
   public class Bug_Tests_000_050 : CreateSqlTestsBase
   {
      /// <summary>
      /// to find B004 sp_Assert_Exp_Act call wrong order
      /// [test].[sp_Assert_Exp_Act]              @tst_num, @tst_sub_num, @exp,     @act, @txt(40)
      /// was    : EXEC ut.test.sp_Assert_Exp_Act 900, @tst_num , 'RC   ', exp_RC , @act_RC          ;
      /// shld be: EXEC ut.test.sp_Assert_Exp_Act @tst_num , 1,           @exp_RC , @act_RC, 'RC' ;
      /// 
      /// code: CreateSqlTestRoutineLib\SqlTestCreatorHlpr.cs(190): SqlTestCreatorHlpr.CrtAssertLine()
      /// </summary>
      [TestMethod]
      public void B004_sp_Assert_Exp_Act_call_WrongOrder()
      {
         string msg;
         string[] args = new[] { "dbo.fnDoesSpReturnResultTable", "900", "debug:true", "CV", "CVvw" };
         int error_code = ProgramBase.MainBase(args, out msg, out string hlpr_file, out string main_tst_file);
         Assert.AreEqual(0, error_code, "ProgramBase.MainBase(args) call");
         var hlpr_script = File.ReadAllText(hlpr_file);
         var main_script = File.ReadAllText(main_tst_file);
         //Process.Start($"Notepad++.exe", $"\"{hlpr_file}\"");
         //Process.Start($"Notepad++.exe", $"\"{main_tst_file}\"");
         var lines = hlpr_script.Split("\r\n");
         string act_line;
         Assert.IsNotNull(lines, "script contains no lines");

         // Remove excess white spc for comparison
         for (int i = 0; i < lines.Count(); i++)
         {
            act_line = lines[i];
            lines[i] = Squish(act_line);
         }

         act_line = lines?.FirstOrDefault(x => x.Contains("EXEC ut.test.sp_Assert_Exp_Act")) ?? "";
         Assert.IsNotNull(act_line, "did not find EXEC ut.test.sp_Assert_Exp_Act");
         Assert.IsTrue(act_line.Contains("@act_RC"), "wrong line for RC");

         var exp_line = " EXEC ut.test.sp_Assert_Exp_Act @tst_num, 1, @exp_RC , @act_RC , 'RC';";

         Assert.IsTrue(exp_line.Equals(act_line)
            , $"\nshould be: [{exp_line}]" +
              $"\nbut was  : [{act_line}]");

         // chk we fixed B009: sp_Assert_Exp_Act call uses test_sub_num not tst_num
         Assert.IsTrue(!hlpr_script.Contains("@tst_sub_num"), "Bug #09: sp_Assert_Exp_Act call uses test_sub_num not tst_num");

         //Bug #010 : still uses @test_num
         act_line = lines?.FirstOrDefault(x => x.Contains("@test_num")) ?? "";
         Assert.IsTrue(string.IsNullOrEmpty(act_line), "Bug #010: @test_num still exists");

         // Run both tests, dont stop on first failure
         bool ret = RunScriptFile(hlpr_file, out var hlpr_scrpt_err_msgs, display: false);
         ret &= RunScriptFile(main_tst_file, out var main_scrpt_err_msgs, display: false);

         Assert.IsTrue(ret, $"Helper script errors:\r\n{hlpr_scrpt_err_msgs}\r\nmain script errors:\r\n{main_scrpt_err_msgs}");
      }

      /// <summary>
      /// to find B009 sp_Assert_Exp_Act call uses test_sub_num not tst_num
      /// </summary>
      [TestMethod]
      public void B009_sp_Assert_Exp_Act_uses_test_sub_num_not_tst_num()
      {
         string msg;
         string[] args = new[] { "dbo.fnDoesSpReturnResultTable", "900", "CV", "CVvw" };
         int ret = ProgramBase.MainBase(args, out msg, out string hlpr_file, out string mn_tst_file);
         Assert.AreEqual(0, ret, "ProgramBase.MainBase(args) call");
         var hlpr_script = File.ReadAllText(hlpr_file);
         var mn_script = File.ReadAllText(mn_tst_file);
         //Process.Start($"Notepad++.exe", $"\"{hlpr_file}\"");
         //Process.Start($"Notepad++.exe", $"\"{mn_tst_file}\"");
         Assert.IsFalse(hlpr_script.Contains("@tst_sub_num"), "@tst_sub_num still exists");
      }

      /// <summary>
      ///       /// <summary>
      /// Bug 011: fnDoesSpReturnResultTable test is creating and testing for inp params 
      /// as well as exp params 
      /// - should only test for exp params 
      /// 
      /// This test checks the test rows
      /// </summary>
      [TestMethod]
      public void B011_fnDoesSpReturnResultTableTestIsTstingInpPrms()
      {
         string msg;
         string[] args = new[] { "dbo.fnDoesSpReturnResultTable", "900", "CV", "CVvw" };
         int ret = ProgramBase.MainBase(args, out msg, out string hlpr_file, out string mn_tst_file);
         Assert.AreEqual(0, ret, "ProgramBase.MainBase(args) call");
         var hlpr_script = File.ReadAllText(hlpr_file);
         var main_script = File.ReadAllText(mn_tst_file);
         //Process.Start($"Notepad++.exe", $"\"{hlpr_file}\"");
         //Process.Start($"Notepad++.exe", $"\"{mn_tst_file}\"");
         Assert.IsFalse(hlpr_script.Contains("@tst_sub_num"), "@tst_sub_num still exists");
         Match match = Regex.Match(hlpr_script, "ut.test.sp_Assert_Exp_Act.*@inp_schema_nm");
         Assert.IsFalse(match.Success, "@inp_schema_nm check still exists");
         match = Regex.Match(main_script, "ut.test.sp_Assert_Exp_Act.*@inp_sp_nm");
         Assert.IsFalse(match.Success, "@inp_sp_nm check still exists");

         Assert.IsTrue(RunScriptFile(hlpr_file, out msg), msg);
         File.Delete("sqlcmd.txt");
      }

      /// <summary>
      /// Bug: sp_pop_country 80 ->  E7254: E7254: caught exception initialising SQL Test creator: 
      ///   Index was out of range. Must be non-negative and less than the size of the collection. (Parameter 'index')
      /// 
      /// </summary>
      [TestMethod]
      public void B015_sp_pop_country_Index_out_of_range()
      {
         string msg;
         string[] args = new[] { "dbo.sp_pop_country", "80", "debug=true" };
         int ret = ProgramBase.MainBase(args, out msg, out string hlpr_file, out string mn_tst_file);
         Assert.IsTrue(RunScriptFile(hlpr_file  , out msg), msg);
         Process.Start($"Notepad++.exe", $"\"{hlpr_file}\"");
         Assert.IsTrue(RunScriptFile(mn_tst_file, out msg), msg);
      }

      /// <summary>
      /// @H0070 S: Crt_Decl_Bloc
      /// </summary>
      [TestMethod]
      public void B016_helper_fnFormatDate_shld_decl_rc_NVARChar_not_int()
      {
         string msg;
         string[] args = new[] { "dbo.fnFormatDate", "88" }; // , "debug=true"
         int ret = ProgramBase.MainBase(args, out msg, out string hlpr_file, out string mn_tst_file);

         DisplayScriptFile(hlpr_file);
         DisplayScriptFile(mn_tst_file);
         var script = File.ReadAllText(hlpr_file);
         var lines = SquishScript(script);
         var found = FindClause(lines, "@act_RC INT", out msg);
         Assert.IsFalse(found, "@act_RC INT still exists in script - but it should not exist");
         found = FindClause(lines, "@act_RC NVARCHAR", out msg);
         Assert.IsTrue(found, "@act_RC NVARCHAR does not exist in the script - but it should exist");
         Assert.AreEqual(0, ret, msg);
      }

      /// <summary>
      /// 220213: Create and Delete DO NOT require a temporary table
      /// and use it based on the input parameters and the expected non pk outputs
      /// </summary>
      [TestMethod]
      public void B017_sp_person_create_ShouldUseTmpTable()
      {
         var c = new TestableSqlTestCreator( ConnectionString, debug_mode: true);
         Assert.AreEqual(0, c.Init(qTstdRtnNm: "dbo.sp_person_create", tstNum: 900, conn_str: ConnectionString, out var msg), msg);

         // Specific rtn test  - the method that creates the tmptable part of the scripy
         var ret = c.CrtTmpTbl( out msg);
         var script = c.Script;
         var lines = SquishScript(script);
         Assert.IsFalse(FindClause(lines, "CREATE TABLE #tmpTbl", out msg), msg);

         // More General Test - the main routine that creates teh helper script - to capture any environments issues
         Assert.IsTrue(c.__Hlpr_CreateScript(out script, out msg), msg);
         lines = SquishScript(script);
         Assert.IsFalse(FindClause(lines, "CREATE TABLE #tmpTbl", out msg), msg);

         // more tests: create temp table has rules on when it should create a temp table
         Assert.AreEqual(0, c.Init(qTstdRtnNm: "dbo.sp_candidate_delete", tstNum: 900, conn_str: ConnectionString, out msg), msg);
         Assert.IsTrue(c.__Hlpr_CreateScript(out script, out msg), msg);
         lines = SquishScript(script);
         Assert.IsFalse(FindClause(lines, "CREATE TABLE #tmpTbl", out msg), msg);

         Assert.AreEqual(0, c.Init(qTstdRtnNm: "dbo.sp_person_get1", tstNum: 900, conn_str: ConnectionString, out msg), msg);
         Assert.IsTrue(c.__Hlpr_CreateScript(out script, out msg), msg);
         lines = SquishScript(script);
         Assert.IsTrue(FindClause(lines, "CREATE TABLE #tmpTbl", out msg), msg);

         Assert.AreEqual(0, c.Init(qTstdRtnNm: "dbo.sp_person_getAll", tstNum: 900, conn_str: ConnectionString, out msg), msg);
         Assert.IsTrue(c.__Hlpr_CreateScript(out script, out msg), msg);
         lines = SquishScript(script);
         Assert.IsTrue(FindClause(lines, "CREATE TABLE #tmpTbl", out msg), msg);

         Assert.AreEqual(0, c.Init(qTstdRtnNm: "dbo.sp_class_creator", tstNum: 900, conn_str: ConnectionString, out msg), msg);
         Assert.IsTrue(c.__Hlpr_CreateScript(out script, out msg), msg);
         lines = SquishScript(script);
         Assert.IsTrue(FindClause(lines, "CREATE TABLE #tmpTbl", out msg), msg);
      }

      /// <summary>
      /// dbo.fnGetProcUpdateCols 100 =>
      ///   Error Caught exception: HandleCallFNRetRows not implemented
      ///   Error E-1: HandleCallFNRetRows not implemented
      /// </summary>
      [TestMethod]
      public void B018_hndl_fn_ret_rows_E1_not_impl_failed_detailed()
      {
         var c = new TestableSqlTestCreator(ConnectionString, debug_mode: true);
         Assert.AreEqual(0, c.Init(qTstdRtnNm: "dbo.fnGetFnOutputCols", tstNum: 100, conn_str: ConnectionString, out var msg), msg);
         Assert.IsTrue(c.__Hlpr_CreateScript(out var script, out msg), msg);
         Assert.IsTrue(RunScript(script, out msg), msg);
         Assert.IsTrue(c.__Mn_CreateTestScript(out script, out msg), msg);
         Assert.IsTrue(RunScript(script, out msg), msg);
      }
   }
}
