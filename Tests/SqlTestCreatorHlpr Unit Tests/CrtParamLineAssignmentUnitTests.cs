using CreateSqlTestRoutineLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using Tests.Test_Support;

namespace Tests
{
   [TestClass]
   public class CrtParamLineAssignmentUnitTests : CreateSqlTestsBase
   {

      /// <summary>
      /// ALTER FUNCTION [dbo].[fnDoesSpReturnResultTable] (@schema_nm NVARCHAR(20), @sp_nm NVARCHAR(100) )
      /// expects: 
      /// @inp_schema_nm
      /// ,@inp_sp_nm

      /// </summary>
      [TestMethod]
      public void Test_CrtParamLineAssignment_fnDoesSpReturnResultTable()
      {
         Assert.IsTrue (Init_1_off("dbo.fnDoesSpReturnResultTable", out TestableSqlTestCreator tst_crtr, out StringBuilder sb, out string msg),$"Test Init failed: {msg}");

         Assert.IsTrue(Helper(
               tst_crtr       : tst_crtr,
               param_nm       : "@schema_nm",
               needsComma     : false,
               inlineParams   : false,
               exp_rc         : 1,
               sb             : sb,
               exp_line       : " @inp_schema_nm\r\n",
               act_line       : out var act_line,
               msg            : out msg), msg);

         Assert.IsTrue(Helper(
               tst_crtr       : tst_crtr,
               param_nm       : "@sp_nm",
               needsComma     : true,
               inlineParams   : false,
               exp_rc         : 1,
               sb             : sb,
               exp_line       : " ,@inp_sp_nm\r\n",
               act_line       : out act_line,
               msg            : out msg,
               exp_ln_cnt     : 2,
               exp_ln_map     : new () {{0, " @inp_schema_nm" } , { 1, " ,@inp_sp_nm" } }
               ), msg);
      }

      /// <summary>
      /// Expect 
      /// ALTER FUNCTION [dbo].[fnGetCandidatesMatchingSkills](
      ///    @skill_id        -- NVARCHAR(50)
      ///   ,@level_val       -- INT = NULL
      ///   ,@experience      -- INT  = NULL)
      ///
      /// </summary>
      [TestMethod]
      public void Test_CrtParamLineAssignment_fnGetCandidatesMatchingSkills()
      {
         Assert.IsTrue(Init_1_off("dbo.fnGetCandidatesMatchingSkills", out TestableSqlTestCreator tst_crtr, out StringBuilder sb, out string msg), $"Test Init failed: {msg}");

         Assert.IsTrue(Helper(
               tst_crtr    : tst_crtr,
               param_nm    : "@skill_id",
               needsComma  : false,
               inlineParams: false,
               exp_rc      : 1,
               sb          : sb,
               exp_line    : " @inp_skill_id\r\n",
               act_line    : out var act_line,
               msg         : out msg), msg);

         Assert.IsTrue(Helper(
               tst_crtr    : tst_crtr,
               param_nm    : "@level_val",
               needsComma  : true,
               inlineParams: false,
               exp_rc      : 1,
               sb          : sb,
               exp_line    : " ,@inp_level_val\r\n",
               act_line    : out _,
               msg         : out msg), msg);

         Assert.IsTrue(Helper(
               tst_crtr    : tst_crtr,
               param_nm    : "@experience",
               needsComma  : true,
               inlineParams: false,
               exp_rc      : 1,
               sb          : sb,
               exp_line    : " ,@inp_experience\r\n",
               act_line    : out _,
               exp_ln_cnt  : 3,
               exp_ln_map  : new () {{0, " @inp_skill_id" }, { 1, " ,@inp_level_val" }, { 2, " ,@inp_experience" } },
               msg         : out msg), msg);
      }

      [TestMethod]
      public void Test_CrtParamLineAssignment_sp_candidate_create()
      {
         string act_line;
         Assert.IsTrue(Init_1_off("dbo.sp_candidate_create", out TestableSqlTestCreator tst_crtr, out StringBuilder sb, out string msg), $"Test Init failed: {msg}");

         Assert.IsTrue( Helper(
               tst_crtr       : tst_crtr,
               param_nm       : "@family_name",
               needsComma     : false, 
               inlineParams   : true,
               exp_rc         : 1, 
               sb             : sb,
               exp_line       : " @family_name = @inp_family_name",
               act_line       : out act_line,
               msg            : out msg), msg);

         // "exp: '[@inp_skill_id, @inp_level_val, @inp_experience]' act: [@family_name = @inp_family_name ,@candidate_id = @act_candidate_id OUTPUT]"
         Assert.IsTrue(Helper(
               tst_crtr       : tst_crtr,
               param_nm       : "@candidate_id", // OUTPUT
               needsComma     : true,
               inlineParams   : true,
               exp_rc         : 1,
               sb             : sb,
               exp_line       : " ,@candidate_id = @act_candidate_id OUTPUT",
               act_line       : out act_line,
               msg            : out msg,
               exp_ln_cnt     : 1,
               exp_ln_map: new() { { 0, " @family_name = @inp_family_name ,@candidate_id = @act_candidate_id OUTPUT" } }
               ), msg);
      }

      /// <summary>
      /// This test checks that CrtParamLineAssignment handles a non existent parameter
      /// ALTER PROCEDURE [dbo].[sp_cv_getAll]
      ///    @candidate_id INT
      ///   , @name NVARCHAR(50) = null
      /// </summary>
      [TestMethod]
      public void Test_CrtParamLineAssignment_sp_cv_getAll()
      {
         Assert.IsTrue(Init_1_off("dbo.sp_cv_getAll", out TestableSqlTestCreator tst_crtr, out StringBuilder sb, out string msg), $"Test Init failed: {msg}");

         Assert.IsTrue( Helper(
               tst_crtr: tst_crtr,
               param_nm: "@candidate_id",
               needsComma: false,
               inlineParams: false,
               exp_rc: 1,
               sb: sb,
               exp_line: " @candidate_id = @inp_candidate_id\r\n",
               act_line: out _,
               msg: out msg), msg);

         Assert.IsTrue( Helper(
               tst_crtr: tst_crtr,
               param_nm: "@candidate_id",
               needsComma: false,
               inlineParams: false,
               exp_rc: 1,
               sb: sb,
               exp_line: " @candidate_id = @inp_candidate_id\r\n",
               act_line: out _,
               msg: out msg), msg);
      }

      protected bool Helper(
           TestableSqlTestCreator tst_crtr
         , string       param_nm
         , bool         needsComma
         , bool         inlineParams
         , int?         exp_rc
         , StringBuilder sb
         , string?      exp_line
         , out string   act_line
         , out string   msg
         , int?         exp_ln_cnt             = null
         , Dictionary<int, string>? exp_ln_map = null
         )
      {
         bool ret = false;
         msg      = "";

         do
         {
            var param_nm_raw = param_nm.TrimStart('@');
            ParamInfo p = tst_crtr.ParamMap[param_nm_raw];
            int act_rc = tst_crtr.CrtParamLineAssignment(p.col_nm ?? "", p.is_output, needsComma, inlineParams, out act_line);
            sb.Append(act_line);

            if ((exp_rc != null) && (exp_rc != act_rc))
            {
               msg = $"Error: rc: exp: {exp_rc} act: {act_rc}";
               break;
            }

            if ((exp_line != null))
            {
               // Close down of padding to 1 space as the padding may vary
               var act_line_squished = Squish(act_line);

               if (exp_line != act_line_squished)
               {
                  msg = $"Error: line: exp: [{exp_line}] act: [{act_line_squished}]";
                  break;
               }
            }

            var lines = SquishScript(sb);

            if(exp_ln_cnt != null)
               if(exp_ln_cnt != lines.Length)
               { 
                  msg = $"Expected {exp_ln_cnt} lines, but got {lines.Length}";
                  break;
               }

            if(!CheckLines(lines, exp_ln_map, out msg))
               break;

            // Finally
            ret = true;
            msg = "";
         } while(false);

         if(!ret)
            DisplayScript(sb, $"{TestClass}.{TestMethod}.sql");

         return ret;
      }

      /// <summary>
      /// single line check
      /// </summary>
      /// <param name="act"></param>
      /// <param name="expLnMap"></param>
      /// <param name="msg">error msg if failed a test</param>
      /// <returns>true if either expLnMap not specified or exp/act line mismatch</returns>
      private bool CheckLines(string[] act_lines, Dictionary<int, string>? exp_ln_map, out string msg)
      {
         msg = "";

         if (exp_ln_map == null)
            return true;

         foreach (var pr in exp_ln_map)
         {
            var act = act_lines[pr.Key];
            var exp = pr.Value;

            if (!act.Equals(exp))
            {
               msg = $"exp: '[{exp}]' act: [{act}]";
               return false;
            }
         }

         return true;
      }

      /// <summary>
      /// Performs 1 off init for a test
      /// </summary>
      /// <param name="qTstdRtnNm"></param>
      /// <param name="tst_crtr"></param>
      /// <param name="sb"></param>
      /// <param name="msg"></param>
      /// <returns></returns>
      protected bool Init_1_off(string qTstdRtnNm, out TestableSqlTestCreator tst_crtr, out StringBuilder sb, out string msg)
      {
         tst_crtr = new TestableSqlTestCreator();
         sb = new StringBuilder();

         int error_code = tst_crtr.Init
                                (
                                 qTstdRtnNm: qTstdRtnNm,
                                 tstNum: 100,
                                 conn_str: ConnectionString,
                                 msg: out msg
                                );
         if (error_code != 0)
            return false;

         // Finally
         return true;
      }
   }
}
