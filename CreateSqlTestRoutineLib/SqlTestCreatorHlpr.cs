using Common;
using RSS.Test;
using System.Text;
using static Common.Utils;

namespace CreateSqlTestRoutineLib
{
   public class SqlTestCreatorHlpr : SqlTestCreatorBase
   {

      public SqlTestCreatorHlpr(string? _connectionString = null, bool? debug_mode = null)
         : base(_connectionString, debug_mode)
      {
      }

      /// <summary>
      /// CREATE PROCEDURE [dbo].[sp_candidate_create] 
      ///   @family_name      NVARCHAR(50)
      ///  ,@seq INT
      ///  ,@email            NVARCHAR(50)          = NULL
      ///  ,@status           INT            OUTPUT
      ///  ,@key              NVARCHAR(50)   OUTPUT
      /// 
      /// no error return - always succeeds
      /// 
      /// CALLED BY:  AgCrtBlocHlprSig()
      /// Creating a line like 	[,]@[param_nm] tab   [Type}    tsb [output]
      ///                         , @key              NVARCHAR(50)   OUTPUT
      /// Uses tabs tops: tabs: 1, 4, -17, -17, -7, 
      /// 
      /// METHOD:
      /// Create Col 0: optional comma OR SPC and @ arg name  using Tab 0, 1
      /// Create Col 1  arg ty nm name using Tab 1, 2
      /// Create Col 2: optional output if specified  in using Tab 2, 3 or space
      /// Create the line by appending all the columns
      /// Append the line to string builder
      /// </summary>
      /// <param name="param"></param>
      /// <param name="needComma"></param>
      /// <param name="needOuPutToken"></param>
      /// <param name="needEqualsNull"></param>
      /// <param name="prefix"></param>
      /// <param name="tabCount"></param>
      /// <param name="tabs"></param>
      /// <param name="sb"></param>
      /// <param name="msg"></param>
      /// 
      /// <param name="ignore">sets ignore to true if (ignore_output_params == true) && (is_output == true) else false</param> 
      /// <returns> 0 ALWAYS</returns>
      protected void CrtParamLine_Hlpr
      (
         string col_nm,
         string col_ty_nm,
         bool is_output,
         bool needsComma,
         bool ignore_output_params,
         bool needsOutputToken,
         bool needsEqualsNull,
         string prefix,
         int preTabCount,
         int[] tabstops,
         out bool ignore
      )
      {
         Precondition(tabstops.Length >= 4, "CrtParamLine2: suppled tabs array must have at least 4 rising tab stops");
         Precondition(col_ty_nm != "??", "CrtParamLine2: suppled tabs array must have at least 4 rising tab stops");
         ignore = false; 
         
         // Create helper rtns don't want inp_ for an out param - they are produced by the tstd rtn
         if ((ignore_output_params == true) && (is_output == true))
         {
            ignore = true;
            return;
         }

         // preTabCount tab sz=3 and add tabstop 0
         var intial_tab = new string(' ', preTabCount * 3 + tabstops[0]);
         AppendLine(intial_tab, false);
         AppendLine(CrtPrmLn_CrtCol0_arg_nm(col_nm, tabstops[1] - tabstops[0], needsComma, prefix), false);

         // Create Col 1: arg ty nm name using Tab 1, 2
         AppendLine(CrtPrmLn_CrtCol1_type(col_ty_nm, tabstops[2] - tabstops[1]), false);

         // Create Col 2: optional output if specified in using Tab 2, 3 or space
         if (needsOutputToken)
            AppendLine(CrtPrmLn_CrtCol2_output(is_output, tabstops[3] - tabstops[2]), false);

         // Create Col 3: optional = NULL if specified in using Tab 2, 3 or space
         AppendLine(CrtPrmLn_CrtCol3_equals_null(needsEqualsNull) ?? "ooops");
      }

      /// <summary>
      /// Create Col 0: optional comma OR SPC and @ arg name  using Tab 0, 1
      ///  [,]@[prefix]seq[padding]
      /// </summary>
      /// <param name="param"></param>
      /// <param name="tabs"></param>
      /// <param name="tab_num"></param>
      /// <returns></returns>
      /// <exception cref="NotImplementedException"></exception>
      protected static string CrtPrmLn_CrtCol0_arg_nm
         (
            string col_nm,
            int padding,
            bool needsComma,
            string prefix
         )
      {
         Precondition(padding >= 0, "CrtParamLine2: suppled tab stops must be continually increasing");
         string comma = needsComma ? "," : " ";
         string col = $"{comma}@{prefix}{col_nm}".PadRight(padding);

         // Add a trailing space if needed
         if (col[col.Length - 1] != ' ')
            col += ' ';

         return col;
      }

      /// <summary>
      /// Creates the helper test script for the following routine types:
      ///   Create, Update, Get1, GetAll, Delete, Default
      ///  
      /// Algorithm:
      ///   Checks rtn type then
      ///   Delegates all to  _CreateTestHlprScript
      ///   
      /// PRECONDITION : Init called
      /// POSTCONDITION: if error throws exception
      /// </summary>
      /// <param name="tstdRtnNm"></param>
      /// <param name="tstNum"></param>
      /// <returns>helper script</returns>
      protected bool __Hlpr_CreateScript(out string script, out string msg)
      {
         bool ret = false;
         msg    = "";
         script = "";
         PreconditionChkInit("__CreateHelperScrpt");
         Precondition(TestType != TestType.Unknown, "Unexpected scenario: TestType.Unknown");

         do
         { 
            if(!OpenScriptWriter(HlprScrptFile, out  msg))
               break;

            //           Precondition                  message
            Precondition(IsInitialised == true, "_CreateTestHlprScript precondition: Init must be called first");

            //--------------------------------------
            // ASSERTION: args known
            //--------------------------------------

            script = "";

            switch (TestType)
            {
               default          : ret = CreateTestHlprScriptGeneral(out script, out msg); break;
               case TestType.Pop: ret = CreateTestHlprScriptPop    (out script, out msg); break;
            }

         } while (false);

         return ret;
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="script"></param>
      /// <param name="msg"></param>
      /// <returns></returns>
      protected bool CreateTestHlprScriptGeneral(out string script, out string msg)
      {
         bool ret = false;
         msg = "";

         do
         {
            // 1: Create the intro script bloc
            // returns 0 (OK always)
            Dual_CrtIntro_Bloc(TstRtnHlprNm);

            // 2: Create the header comment bloc
            Hlpr_Crt_Hdr_Bloc();

            // 3: Create the helper test strategy
            CrtStrategyHdr();

            // 4: Create the signature bloc
            Hlpr_Create_Sig_Bloc();

            // 4: Create the As/Begin/SET NOCOUNT ON;
            Hlpr_Crt_AsBegin_Bloc();

            // 5: Create the Declare bloc
            Hlpr_Create_Declare_Bloc();

            // Currently only using a tmp table for Get1 and GetAll
            // could use for create and update ??
            // CreateTempTable checks if the tmp table is wanted by checking the routine type
            // returns true if RtnReturnedColList has columns false otherwise
            // 220130: B017_sp_person_create_ShouldUseTmpTable(): if ((TestType == TestType.Get1) || (TestType == TestType.Get1) || (TestType == TestType.GetAll))

            // 6: Create a temporary table if rtn type specifies one
            // 220204: creates will not return the updated columns - but they can be found
            // by running a select @act= ... from table where pk matches
            CrtTmpTbl(out msg);
            //script = SB.ToString();
            // 7: Call the tested rtn
            if (!Hlpr_Crt_CallTstdRtn_Bloc(out msg))
               break;

            //script = SB.ToString();
            // 8: Get the ACT resuts
            GetActResults_Bloc();
            //script = SB.ToString();
            // 9: Check all ACTs match EXPs
            Hlpr_Crt_AssertBloc();
            //script = SB.ToString();
            // 10: Wrap up rtn end
            Dual_Crt_WrapUp();

            // Finally
            msg = "";
            ret = true;
         } while (false);

         // 10: Return the script
         script = SB.ToString();
         return ret;
      }

      /// Pop routines in Telepat are hard coded - take no parameters
      /// There are 2 kinds of pop routines:
      /// 1: simple - 1 procedure populates 1 country - can be tested as follows
      /// 2: complex like dbo.sp_pop_all_static_data that populate many SD tables - currently cannot be tested automatically.
      /// 
      /// So we are implementing type 1: single table pop routines here
      /// 
      /// The test helper should be able to check a specific row based on either the PK or all the row
      /// so the additional arguments are:
      /// inp_PK, exp_values as per the table all optional 
      /// 
      /// Parameter bloc: 
      ///   input args: 
      /// 	pk,
      /// 
      ///    expected args: 
      /// 	exp RC INT
      ///   	all table cols bar pk all optional (= null)
      /// 
      /// Declare bloc:
      ///   @act_ vars 1 per col type as per table spec
      /// 
      /// Select bloc:
      /// 
      /// SELECT
      /// 	 @act_col_1 = col_1
      /// 	,@act_col_2 = col_2
      /// FROM <tbl>
      /// WHERE pk matches
      /// 
      /// Assert bloc:
      /// for each non null exp parameter run the exp/act check
      /// 
      /// Wrap up
      /// 
      protected bool CreateTestHlprScriptPop(out string script, out string msg)
      {
         bool ret = false;
         msg = "";

         do
         {
            // 1: Create the intro script bloc
            // returns 0 (OK always)
            Dual_CrtIntro_Bloc(TstRtnHlprNm);

            // 2: Create the header comment bloc
            Hlpr_Crt_Hdr_Bloc();

            // 3: Create the helper test strategy
            CrtStrategyHdr();

            // 4: Create the signature/Parameter bloc Hlpr_Create_Sig_Bloc_Pop
            Hlpr_Create_Sig_Bloc_Pop();

            // 5: Create the As/Begin/SET NOCOUNT ON;
            Hlpr_Crt_AsBegin_Bloc();

            // 6: Create the Declare bloc
            Hlpr_Create_Declare_Bloc();

            // Currently only using a tmp table for Get1 and GetAll
            // could use for create and update ??
            // CreateTempTable checks if the tmp table is wanted by checking the routine type
            // returns true if RtnReturnedColList has columns false otherwise
            // 220130: B017_sp_person_create_ShouldUseTmpTable(): if ((TestType == TestType.Get1) || (TestType == TestType.Get1) || (TestType == TestType.GetAll))

            // 7: Create a temporary table if rtn type is not Delete or unknown
            // 220204: creates will not return the updated columns - but they can be found and a table created for them
            //CrtTmpTbl(out msg);

            // 8: Call the tested rtn
            if (!Hlpr_Crt_CallTstdRtn_Bloc(out msg))
               break;

            // 9: Get the ACT resuts
            GetActResults_Bloc();

            // 10: Check all ACTs match EXPs
            Hlpr_Crt_AssertBloc();

            // 11: Wrap up rtn end
            Dual_Crt_WrapUp();

            // 13: Finally
            ret = true;
         } while (false);

         // 12: Return the script
         script = SB.ToString();
         return ret;
      }


      /// <summary>
      /// Create Col 1: arg ty nm name using Tab 1, 2
      /// LIKE [NVARCHAR(50) padding]
      /// </summary>
      /// <param name="param"></param>
      /// <param name="tabs"></param>
      /// <param name="tab_num"></param>
      /// <returns></returns>
      /// <exception cref="NotImplementedException"></exception>
      protected static string CrtPrmLn_CrtCol1_type(string col_ty_nm, int padding)
      {
         Precondition(padding >= 0, "CrtParamLine2: suppled tab stops must be continually increasing");
         string col = (col_ty_nm ?? "").PadRight(padding);

         // Add a trailing space if needed
         if (col[col.Length - 1] != ' ')
            col += ' ';

         return col.ToUpper();
      }

      /// <summary>
      /// Create Col 2: optional output if specified in using Tab 2, 3 or space
      /// ,@key              NVARCHAR(50)   OUTPUT
      /// </summary>
      /// <param name="param"></param>
      /// <param name="tabs"></param>
      /// <param name="tab_num"></param>
      /// <returns></returns>
      /// <exception cref="NotImplementedException"></exception>
      protected static string CrtPrmLn_CrtCol2_output(bool is_output, int padding)
      {
         Precondition(padding >= 0, "CrtParamLine2: suppled tab stops must be continually increasing");
         string output = is_output ? "OUTPUT" : "";
         string col = output.PadRight(padding);

         // Add a trailing space if needed
         if (col[col.Length - 1] != ' ')
            col += ' ';

         return col;
      }

      /// <summary>
      /// Create Col 3: optional = NULL if specified in using Tab 2, 3 or space
      /// , @key              NVARCHAR(50)   OUTPUT
      /// 
      /// Note no padding 
      /// </summary>
      /// <param name="param"></param>
      /// <param name="tabs"></param>
      /// <param name="tab_num"></param>
      /// <returns></returns>
      /// <exception cref="NotImplementedException"></exception>
      protected static string? CrtPrmLn_CrtCol3_equals_null(bool needsEqualsNull)
      {
         string col = needsEqualsNull ? "= NULL" : "";
         return col;
      }

      /// <summary>
      /// Create a call like EXEC @rc =  <qualified rtn nm> (args) 
      /// </summary>
      /// <returns></returns>
      /// <exception cref="NotImplementedException"></exception>
      protected void Call_TstdRtn_Sp_RetNoRows_Hlpr()
      {
         AppendLine($"\tEXEC @act_rc = {QualTstdRtnNm} ");
         GetCallRtnArgs(inlineParams: false);
      }

      /// <summary>
      /// EXEC tSQLt.AssertEquals @inp_family_name        , @act_family_name;
      ///             sb.AppendLine($"-- EXEC ut.test.sp_Assert_Exp_Act 1,2, @exp_RC, @act_RC, 'RC';");
      /// sb.AppendLine($"-- EXEC ut.test.sp_Assert_Exp_Act 1,2, @exp_RC, @act_RC, 'RC';");
      /// </summary>
      /// <param name="param"></param>
      /// <returns></returns>
      protected void CrtAssertLine(string? _arg, string prefix, ref int tst_sub_num)
      {
         string script = "";
         string arg = _arg ?? "";
         var argP = arg.PadRight(MaxArgLen-2);

         var line = $"\tEXEC ut.test.sp_Assert_Exp_Act @tst_num, {tst_sub_num, 2}, @{prefix}_{argP}, @act_{argP}, '{arg}';";
         AppendLine(line);
         script = SB.ToString();
         tst_sub_num++;
      }

      /// <summary>
      /// Create <rtn ty> [<shema>].[<tst_rtn_nm>] no error return - always succeeds
      /// </summary>
      /// <param name="sb"></param>
      /// <returns>no error return - always succeeds</returns>
      protected void CrtCreateRtnLine()
      {
         AppendLine($@"CREATE PROCEDURE [test].[{TstRtnHlprNm}]");
      }

      /// <summary>
      /// This is used to create the parameters for the tested rtn call
      /// 
      /// PRECONDITIONS: none
      /// 
      /// POSTCONDITIONS:
      ///   line = 
      ///      if IsScalarReturnCol then "";
      ///      if needsComma then ",...."
      ///      if inlineParams then paramA, paramB, paramC
      ///      if is fn then "<whitespace>{comma}@{prefix}_{arg.col_nm}" where prefix = is_output ? "act" : "inp"
      /// </summary>
      /// <param name="arg"></param>
      /// <param name="needsComma"></param>
      /// <param name="inlineParams"></param>
      /// <param name="line"></param>
      /// <returns>1 if added a line, 0 if not</returns>
      protected int CrtParamLineAssignment(
                     string col_nm,
                     bool is_output,
                     bool needsComma,
                     bool inlineParams,
                     out string line
         )
      {
         var NL = inlineParams ? "" : "\r\n";
         line = "";
         
         // Don't script the output parmater with no name from a scalar
         if (IsScalarReturnCol(col_nm))
            return 0;

         var    paramNm       = CrtVarNm(col_nm: col_nm, isParameter: true, is_output: is_output, needsComma: needsComma).PadRight(MaxArgLen + 4);
         var    output_phrase = is_output ? " OUTPUT" : "";
         var    prefix        = is_output ? "act" : "inp";
         string argVal;

         // If is a function
         if(IsFn())
         {
            var comma = needsComma ? "," : " ";
            argVal = $"{comma}@{prefix}_{col_nm}";
            line = $"\t\t{argVal}{NL}"; // $"\t\t{argVal}{output_phrase}";
         }
         else
         {
            var argNm = col_nm;

            // Only pad if output
            if(is_output)
               argNm = argNm.PadRight(MaxArgLen + 4);

            argVal = $"@{prefix}_{argNm}";
            line = $"\t\t{paramNm} = {argVal}{output_phrase}{NL}";
         }
         
         return 1;
      }

      /// <summary>
      /// Scalar functions return the scalar value as a column with no name
      /// </summary>
      /// <param name="arg"></param>
      /// <returns></returns>
      protected bool IsScalarReturnCol(string col_nm)
      {
         //return ((arg.is_output == true) && (RtnRetType == RtnRetType.FnScalar) && (arg.col_nm.Equals("")));
         return col_nm.Equals("");
      }

      /// <summary>
      /// Creates the name for variable or parameter
      /// based on the RtnArg row
      /// parameters   : @[arg.name]
      /// input  values: @inp_[arg.name]
      /// output values: @act_[arg.name]
      /// 
      /// The inp/act is decided from the ArgTable row (out_sym)
      /// </summary>
      /// <param name="param"></param>
      /// <param name="needsPrefix"></param>
      /// <param name="needsComma"></param>
      /// <returns></returns>
      protected string CrtVarNm(string col_nm, bool isParameter, bool is_output, bool needsComma) // ParamInfo param, 
      {
         var padVar = isParameter ? MaxArgLen : MaxArgLen + 4;
         var comma = needsComma ? ',' : ' ';
         var prefix = isParameter ? "" : (is_output ? "act_" : "inp_");
         var argName = $"{comma}@{prefix}{col_nm?.PadRight(padVar)}";
         return argName;
      }

      /// <summary>
      /// ---------------------------------------------
      /// -- Call the tested routine like
      /// for each arg
      /// ---------------------------------------------
      /// EXEC @act_rc = sp_c2s_create
      ///     @candidate_id = @inp_candidate_id
      ///    ,@skill_id     = @inp_skill_id
      ///    ,@skillLevel_id= @inp_skillLevel_id
      ///    ,@experience   = @inp_experience;
      /// 
      /// PRECONDITION: RtnArg populated
      /// </summary>
      /// <returns> always returns 0 (OK)/returns>
      protected void GetCallRtnArgs( bool inlineParams)
      {
         AppendLineDebug("-- @0010 S: HCRT_GetCallRtnArgs");
         bool needsComma = false;
         string line = "";

         foreach (var pr in ParamMap)
         {
            ParamInfo p = pr.Value;

            // CrtParamLineAssignment returns 1 if added a line, 0 if not
            if (CrtParamLineAssignment(p.col_nm ?? "", p.is_output, needsComma, inlineParams, out line) == 1)
            {
               AppendLine(line, false);
               needsComma = true;
            }
         }

         if(inlineParams)
            AppendLine();

         AppendLineDebug("-- @0010 E: HCRT_GetCallRtnArgs");
      }

      /// <summary>
      /// 6.5: Create the As/Begin/SET NOCOUNT ON;
      /// </summary>
      /// <returns></returns>
      protected void Hlpr_Crt_AsBegin_Bloc()
      {
         AppendLineDebug("-- @0020 S: HCRT_AsBegBloc");
         AppendLine($"AS");
         AppendLine($"BEGIN");
         AppendLine($"\tSET NOCOUNT ON;");
         AppendLine($"\tPRINT CONCAT('{TstRtnHlprNm}, test:', @tst_num, ' starting');\r\n");
         AppendLineDebug("-- @0020 E HCRT_AsBegBloc");
      }

      /// <summary>
      /// 
      ///      13: Assert all ACTs match EXPs
      /// 
      /// PRECONDITIONS: IsInitialised = true
      ///
      ///  ------------------------------
      ///  -- Chk the update
      ///  ------------------------------
      ///  EXEC tSQLt.AssertEquals @inp_family_name        , @act_family_name;
      ///  EXEC tSQLt.AssertEquals @inp_first_name         , @act_first_name;
      ///  EXEC tSQLt.AssertEquals @inp_middle_name        , @act_middle_name;
      ///  EXEC tSQLt.AssertEquals @inp_salutation_nm      , @act_salutation_nm;
      ///  EXEC tSQLt.AssertEquals @inp_candidateStatus_nm , @act_candidateStatus_nm;
      ///
      /// </summary>
      /// <param name="args"></param>
      /// <returns></returns>
      protected void Hlpr_Crt_AssertBloc()
      {
         AppendLineDebug("-- @0030 S: HCRT_AssertBloc");
         PreconditionChkInit("AgCrtBlocAssertActMtchExp");
         string   line        = "";
         int      tst_sub_num =  1;

         // Revise maxArgLen to include the output cols
         UpdateMaxArgLenForOutputCols();

         do
         {
            line = "\r\n\r\n" +
"\t-------------------------------------\r\n" +
"\t-- Run tests\r\n" +
"\t-------------------------------------";

            AppendLine(line);

            // RC exp/Act
            CrtAssertLine("RC", "exp", ref tst_sub_num);

            // 3: check the inputs and returned cols
            AddAssertOutputs( ref tst_sub_num);

            // if here then tests passed
            AppendLine($"\r\n\tPRINT CONCAT('{TstRtnHlprNm}, test:', @tst_num, ' passed');");
         } while (false);

         AppendLineDebug("-- @0030 E: HCRT_AssertBloc");
      }

      /// <summary>
      /// asserts: check the inputs and returned cols bar any PK cols
      /// </summary>
      /// <param name="sb"></param>
      /// <param name="msg"></param>
      /// <returns></returns>
      protected void AddAssertOutputs( ref int tst_sub_num)
      {
         // 220207: When checking Create/Update procs compare inp/exp
         string prefix = 
            TestType == TestType.Create ? "inp" :
            TestType == TestType.Update ? "inp" : "act";

         // 3: output cols bar any PK cols -- exp / act
         foreach ( var pr in ExpectedParameterMap)
         {
            ParamInfo p = pr.Value;
            AssertNotNull(p.is_output, "E5499: null parameter is output value");

            // Skip any pk fields
            if(p.is_pk ?? false)
               continue;

            //prefix = (v?.is_input  ?? false) ? "inp" : "exp";
            prefix = GetPrefix(p);
            CrtAssertLine(pr.Value.col_nm, prefix, ref tst_sub_num);
         }
      }

      /// <summary>
      /// Rules
      ///   R1: if output                  then prefix=exp
      ///   R2: if testtype in get, getall then prefix=exp
      ///   R3: else inp
      /// </summary>
      /// <param name="p"></param>
      /// <returns></returns>
      /// <exception cref="NotImplementedException"></exception>
      protected string GetPrefix(ParamInfo p)
      {
         // R3: else inp
         string prefix = "inp";

         do
         { 
            //   R1: if output                  then prefix=exp
            if(p.is_output)
            {
               prefix ="exp";
               break;
            }

            //   R2: if testtype in get, getall then prefix=exp
            switch(TestType)
            {
               case TestType.Get1:
               case TestType.GetAll:
                  prefix = "exp";
                  break;
            }
         } while(false);

         return prefix;
      }

      /// <summary>
      /// 10: Create the Call the tested rtn
      /// 
      /// If the test is a get1 or GetAll then this will createa tmp table
      /// PRECONDITIONS: IsInitialised = true
      /// 
      /// TESTS: 
      /// HlprCreateCallTstdRtnUnitTests 
      /// Bug_Tests_000_050.B017_sp_person_create_ShouldUseTmpTable()
      /// 
      /// </summary>
      /// <param name="args"></param>
      /// <returns>true if RtnReturnedColList has columns false otherwise</returns>
      protected bool Hlpr_Crt_CallTstdRtn_Bloc(out string msg)
      {
         AppendLineDebug("-- @0060 S");
         PreconditionChkInit("Hlpr_Crt_CallTstdRtn_Bloc");

 /*        // Currently only using a tmp table for Get1 and GetAll
         // could use for create and update ??
         // CreateTempTable checks if the tmp table is wanted by checking the routine type
         // returns true if RtnReturnedColList has columns false otherwise
         // 220130: B017_sp_person_create_ShouldUseTmpTable(): if ((TestType == TestType.Get1) || (TestType == TestType.Get1) || (TestType == TestType.GetAll))

         switch(TestType)
         { 
            case TestType.Get1:
            case TestType.GetAll:
            case TestType.Create:
            case TestType.Update:
            case TestType.Default:
               if (!CrtTmpTblHlprForRetCols(out msg))
                  return false;
            break;
         }*/

         // Add the 'Call the tested routine' comment bloc
         AppendLine(txt_bloc_call_tstd_rtn_cmnt);

         switch (RtnRetType)
         {
            default:
            case RtnRetType.Unknown:
               {
                  throw new ArgumentException( $"Hlpr_Crt_CallTstdRtn_Bloc(): unrecognised rtn type {RtnRetType}");
               }

            case RtnRetType.SpNoRows:
               Call_TstdRtn_Sp_RetNoRows_Hlpr();
               break;

            case RtnRetType.SpRows:
               Call_TstdRtn_Sp_RetRows_Hlpr();
               break;

            case RtnRetType.FnScalar:
               Call_TstdRtn_Fn_RetNoRows_Hlpr();
               break;

            case RtnRetType.FnRows:
               Call_TstdRtn_Fn_RetRows_Hlpr();
               break;
         }

         // if a create then check the new row exists
         // check the row exists
         if (TestType == TestType.Create)
            CreationCheck_Hlpr();

         AppendLineDebug("-- @0060 E");
         msg = "";
         return true;
      }

      /// <summary>
      /// 7: Create the Declare bloc
      /// 
      /// There should be declared:
      ///   1 @act_RC INT  
      ///   for each @exp_param: 1 act_ variable 
      ///   
      /// PRECODITION: IsInitialised = true
      /// 
      /// METHOD:
      ///   need a section for the ACT parameters
      ///   Not the PK - that never changes
      ///   
      ///   Declare:
      ///      1 @act_RC INT  
      ///      for each @exp_param: 1 act_ variable ( see the mn create call rtn:Mn_Create_CallHlpr)
      /// </summary>
      /// <returns>0 always</returns>
      /// <exception cref="NotImplementedException"></exception>
      protected void Hlpr_Create_Declare_Bloc()
      {
         AppendLineDebug("-- @H0070 S: Crt_Decl_Bloc");
         var tabstops = new int[] { 0, 20, 35, 45 };
         PreconditionChkInit("AgCrtBlocDecl");
         string line = "\tDECLARE";
         AppendLine(line);

         // 220129: Bug 016: helper for fnFormatDate RC decl should be char not int
         // if a stored proc then RC type is INT
         // if a scalar fn then RC type should be the FN return type 
         var rc_col_ty_nm = "INT";

         if(IsScalarFn())
            rc_col_ty_nm = GetScalarFnReturnType();

         // no error return - always succeeds
         CrtParamLine_Hlpr(
           col_nm: "RC",
           col_ty_nm: rc_col_ty_nm,
           is_output: false,
           needsComma: false,
           ignore_output_params: false,
           needsOutputToken: false,
           needsEqualsNull: false,
           prefix: "act_",
           preTabCount: 2,
           tabstops: tabstops,
           ignore: out bool ignore
           );

         // Create the Act params for the input params
         Hlpr_Create_Declare_BlocForActuals( tabstops);

         line = "\r\n" +
"\t-------------------------------------\r\n" +
"\t-- Setup\r\n" +
"\t-------------------------------------";
         AppendLine(line);
         AppendLineDebug("-- @H0070 E");
      }

      /// <summary>
      /// Applies to Scalar Functions
      /// </summary>
      /// <returns></returns>
      private string GetScalarFnReturnType()
      {
         //return TstdRtnType?.ToString() ?? "";
         AssertNotNull(RtnDetailInfoList);
         Assertion(RtnDetailInfoList.Count == 1);
         var rtn_detail = RtnDetailInfoList[0];
         var ty_nm = rtn_detail.data_type;

         if(ty_nm.Equals("NVARCHAR", StringComparison.OrdinalIgnoreCase))
         {
            // get the char max  len
            ty_nm = $"{ty_nm}({rtn_detail.char_max_len})";
         }

         return ty_nm;
      }

      private bool IsScalarFn()
      {
         return (TstdRtnType == RtnType.ScalarFn);
      }

      /// <summary>
      /// Creates the @act_  declaration bloc
      /// </summary>
      /// <param name="tabstops"></param>
      /// <param name="sb"></param>
      /// <returns>count of exp params</returns>
      protected int Hlpr_Create_Declare_BlocForActuals(int[] tabstops)
      {
         int n = 0;
         string script;
         AppendLineDebug("-- @H0090 S: Crt_Decl Bloc For Acts");

         // Create the Act params
         foreach (var pr in ExpectedParameterMap)
         {
            var col_nm = pr.Key;
            var p = pr.Value;

            // if a scalar fn then ignore the output parameter with no name
            // as we got this covered by Exp TC
            if (col_nm.Equals(""))
               continue;

            // In this scenario we do the outputs later
            if(p.is_output == true)
               continue;

            n++;
            CrtParamLine_Hlpr
                        (
                             col_nm              : col_nm
                           , col_ty_nm           : p.ty_nm ?? ""
                           , is_output           : p.is_output
                           , needsComma          : true
                           , ignore_output_params: false
                           , needsOutputToken    : false
                           , needsEqualsNull     : false
                           , prefix              : "act_"
                           , preTabCount         : 2
                           , tabstops            : tabstops
                           , ignore              : out _
                        );
         }

         script = SB.ToString();
         AppendLineDebug("-- @H0091");

         foreach (var pr in OutArgsMap)
         {
            var col_nm = pr.Key;
            var p = pr.Value;

            // Don't repeat parameter
            if (ExpectedParameterMap.ContainsKey(col_nm) && (p.is_output == false))
               continue;

            n++;
            CrtParamLine_Hlpr
                        (
                             col_nm: col_nm
                           , col_ty_nm: p.ty_nm ?? ""
                           , is_output: p.is_output
                           , needsComma: true
                           , ignore_output_params: false
                           , needsOutputToken: false
                           , needsEqualsNull: false
                           , prefix: "act_"
                           , preTabCount: 2
                           , tabstops: tabstops
                           , ignore: out _
                        );
         }

         script = SB.ToString();
         AppendLineDebug("-- @H0092");

         // if tstd rtn is a fn returning columns then add these columns to the input params
         // so that they can be asserted when the routine is called
         if ((TestType == TestType.Default) && (TstdRtnType == RtnType.TableValuedFn))
         {
            AssertNotNull(RtnReturnedColList);

            foreach(var col in RtnReturnedColList)
            {
               AssertNotNull(col.col_nm);
               AssertNotNull(col.ty_nm);

               if (ExpectedParameterMap.ContainsKey(col.col_nm))
                  continue;

               n++;
               CrtParamLine_Hlpr
                  (
                        col_nm: col.col_nm
                     , col_ty_nm: col.ty_nm
                     , is_output: col.is_output
                     , needsComma: true
                     , ignore_output_params: false
                     , needsOutputToken: false
                     , needsEqualsNull: false
                     , prefix: "act_"
                     , preTabCount: 2
                     , tabstops: tabstops
                     , ignore: out _
                  );
            }
         }

         AppendLineDebug("-- @H0090 E");
         script = SB.ToString();
         return n;
      }

      /// <summary>
      /// 5: Create the header comment bloc
      /// </summary>
      protected void Hlpr_Crt_Hdr_Bloc()
      {
         var cmnt_bloc = string.Format(txt_bloc_hlpr_cmnt_hdr, GetCurrentDateFormatted(), TstdRtnNm, TstdRtnDesc);
         AppendLine(cmnt_bloc.TrimEnd());
      }

      /// <summary>
      /// 6: Create the signature bloc
      /// 
      /// CREATE PROCEDURE [dbo].[sp_candidate_create] 
      ///   @family_name      NVARCHAR(50)
      ///  ,@first_name       NVARCHAR(50)
      ///  ,@middle_name      NVARCHAR(50)
      ///  ,@seq INT
      ///  ,@salutation_nm    NVARCHAR(50)
      ///  ,@email            NVARCHAR(50)
      ///  ,@phone            NVARCHAR(25)
      ///  ,@mobile           NVARCHAR(25)
      ///  ,@candidate_id     INT            OUTPUT 
      ///  ,@status           INT            OUTPUT
      ///  ,@key              NVARCHAR(50)   OUTPUT
      ///  ,@msg              NVARCHAR(50)   OUTPUT
      /// 
      /// PRECONDITIONS: IsInitialised = true
      /// 
      /// Exceptions:
      ///   PreconditionChkInit() exceptions
      ///   "E4456: primary key not found" if No primary key column found
      /// </summary>
      /// <param name="args"></param>
      protected void Hlpr_Create_Sig_Bloc()
      {
         AppendLineDebug("-- @H0110 S Create_Sig_Bloc");
         PreconditionChkInit("AgCrtBlocHlprSig");
         int[] tabstops = new int[] { 0, 20, 38, 40 };

         do
         {
            // Create the Create Rtn ty rtn nm line
            CrtCreateRtnLine();

            //------------------------------------------------------
            // Add the @tst_num   INT (we know the  test num) always succeeds
            //------------------------------------------------------
            CrtParamLine_Hlpr(
               col_nm: "tst_num ",
               col_ty_nm: "INT",
               is_output: false,
               needsComma: false,
               ignore_output_params: true,
               needsOutputToken: false,
               prefix: "",
               preTabCount: 1,
               tabstops: tabstops,
               needsEqualsNull: false,
               ignore: out _);

            if(TestType==TestType.GetAll)
            {
               if (PK_List == null)
                  throw new Exception("E5731: the primary key fields list is not specified");

               Assertion(PK_List.Count() > 0, "E4456: primary key not found");
               var pk_param_nm = PK_List[0].col_nm;
               AssertNotNull(pk_param_nm);

               //----------------------------------------------------------------
               // 220118: Get all should query 1 row - so we need to pass the id 
               //----------------------------------------------------------------
               CrtParamLine_Hlpr(
                  col_nm               : pk_param_nm,
                  col_ty_nm            : "INT",
                  is_output            : false,
                  needsComma           : true,
                  ignore_output_params : true,
                  needsOutputToken     : false,
                  prefix               : "inp_",
                  preTabCount          : 1,
                  tabstops             : tabstops,
                  needsEqualsNull      : false,
                  ignore               : out _);
            }

            // Add the input parameters prefixed @inp_ 
            AddInpParams(tabstops);

            //--------------------------------------------
            // Add the output cols as exp_
            // 	,@exp_rc             INT          = NULL
            //--------------------------------------------
            //bool needsComma = true;

            // Add the expected parameters prefixed @inp_
            AddExpParams(tabstops);
            AddOutParams(tabstops);
         } while (false);

         AppendLineDebug("-- @0110 E: Hlpr_Create_Sig_Bloc");
      }

      /// <summary>
      ///  Parameter bloc: 
      ///   input args: pk,
      ///   expected args: 
      /// 	   exp RC INT
      ///   	all table cols bar pk all optional (= null)
      /// </summary>
      protected void Hlpr_Create_Sig_Bloc_Pop()
      {
         int[] tabstops = new int[] { 0, 20, 38, 40 };
         AppendLineDebug("-- @H0210 S Create_Sig_Bloc_pop");
         //   input args: pk,
         //    expected args: 
         // 	exp RC INT
         //   	all table cols bar pk all optional (= null)
         do
         {
            // Create the Create Rtn ty rtn nm line
            CrtCreateRtnLine();

            //------------------------------------------------------
            // Add the @tst_num   INT (we know the  test num) always succeeds
            //------------------------------------------------------
            CrtParamLine_Hlpr(
               col_nm: "tst_num ",
               col_ty_nm: "INT",
               is_output: false,
               needsComma: false,
               ignore_output_params: true,
               needsOutputToken: false,
               prefix: "",
               preTabCount: 1,
               tabstops: tabstops,
               needsEqualsNull: false,
               ignore: out _);

            CrtParamLine_Hlpr(
               col_nm: "RC ",
               col_ty_nm: "INT",
               is_output: false,
               needsComma: true,
               ignore_output_params: true,
               needsOutputToken: false,
               prefix: "exp_",
               preTabCount: 1,
               tabstops: tabstops,
               needsEqualsNull: false,
               ignore: out _);

            // Add the input parameters prefixed @inp_ 
            // the input parameters are the search key
            // the exp parms are the column values in the table row given by the search key
            AddInpParamsPop(tabstops);

            //--------------------------------------------
            // Add the output cols as exp_
            // 	,@exp_rc             INT          = NULL
            //--------------------------------------------
            //bool needsComma = true;

            // Add the expected parameters prefixed @inp_
            AddExpParamsPop(tabstops);
            AddOutParams(tabstops);
         } while (false);

         var script = SB.ToString();

         AppendLineDebug("-- @H0210 E Create_Sig_Bloc_pop");
      }
      /// <summary>
      /// Add the input parameters prefixed @inp_
      ///     @inp_<pk_column> <pk_type>
      /// </summary>
      /// <param name="tabstops"></param>
      /// <param name="sb"></param>
      /// <returns>no error return: always succeeds</returns>
      /// <exception cref="Exception"></exception>
      protected void AddInpParams(int[] tabstops)
      {
         AppendLineDebug("-- @0120 S: AddInpParams");

         foreach (var pr in ParamMap)
         {
            var p = pr.Value;

            // if the tstd rtn is a function returning a scalar (no rows)
            // then the scalar is the first parameter has no name and is marked as output
            // in this case this is the act output @act_RC which has an exp_RC defined later.
            // So we need to get the type of the parameter now
            if (p.is_output == true) 
               continue;

            // no error return = always succeeds
            AssertNotNull(p.col_nm);
            AssertNotNull(p.ty_nm);

            CrtParamLine_Hlpr(
                 col_nm: p.col_nm
               , col_ty_nm: p.ty_nm
               , is_output: p.is_output
               , needsComma: true
               , ignore_output_params: true
               , needsOutputToken: false
               , prefix: "inp_"
               , preTabCount: 1
               , tabstops: tabstops
               , needsEqualsNull: false
               , ignore: out _
               );
         } // end for

         // if tstd rtn is a fn returning columns then add these columns to the input params
         // so that they can be asserted when the routine is called
         if((TestType == TestType.Default) && (TstdRtnType == RtnType.TableValuedFn))
         {
            AssertNotNull(RtnReturnedColList);

            foreach(var p in RtnReturnedColList)
            {
               AssertNotNull(p.col_nm);
               AssertNotNull(p.ty_nm);

               // 220213: duplicate returned col in param map
               if(ParamMap.ContainsKey(p.col_nm))
                  continue;

               CrtParamLine_Hlpr(
                    col_nm              : p.col_nm
                  , col_ty_nm           : p.ty_nm
                  , is_output           : p.is_output
                  , needsComma          : true
                  , ignore_output_params: true
                  , needsOutputToken    : false
                  , prefix              : "inp_"
                  , preTabCount         : 1
                  , tabstops            : tabstops
                  , needsEqualsNull     : true
                  , ignore              : out _
                  );
            }
         }

         AppendLineDebug("-- @0120 E: AddInpParams");
      }

      /// <summary>
      /// Alternative method to AddInpParams for populate type routines
      /// Add the input parameters prefixed @inp_ 
      /// the input parameters are the search key
      /// the exp parms are the column values in the table row given by the search key
      /// </summary>
      /// <param name="tabstops"></param>
      /// <exception cref="NotImplementedException"></exception>
      protected void AddInpParamsPop(int[] tabstops)
      {
         // Add the search key
         AddSearchKeyInpParams(tabstops);
      }

      /// <summary>
      /// Like id=@id
      /// foreach col in the  pk
      /// </summary>
      /// <exception cref="NotImplementedException"></exception>
      protected void AddSearchKeyInpParams(int[] tabstops)
      {
         PreconditionNotNull(PK_List);

         foreach (KeyInfo col in PK_List)
         {
            AssertNotNull(col.col_nm);
            AssertNotNull(col.ty_nm);

            CrtParamLine_Hlpr(
               col_nm              : col.col_nm,
               col_ty_nm           : col.ty_nm  ?? "??",
               is_output           : false,
               needsComma          : true,
               ignore_output_params: true,
               needsOutputToken    : false,
               prefix              : "inp_",
               preTabCount         : 1,
               tabstops            : tabstops,
               needsEqualsNull     : false,
               ignore              : out _
               );
         }

         var script = SB.ToString();
      }

      /// <summary>
      /// Adds the populated table columns to the exp paramslist
      /// 
      /// PRECONDITIONS:
      ///   TableCols not null AND contains at least 1 column
      ///   
      /// POSTCONDITIONS
      ///   exp columns added to the signature
      /// 
      /// </summary>
      /// <param name="tabstops"></param>
      protected void AddExpParamsPop(int[] tabstops)
      {
         PreconditionNotNull(TableCols);
         Precondition(TableCols.Count > 0);

         // foreach col in table being populated add an input param
         foreach (ColInfo col in TableCols)
         {
            AssertNotNull(col);
            AssertNotNull(col.col_nm);
            AssertNotNull(col.ty_nm);

            if (!(col.is_pk ?? false))
               CrtParamLine_Hlpr(
                    col_nm              : col.col_nm
                  , col_ty_nm           : col.ty_nm
                  , is_output           : false
                  , needsComma          : true
                  , ignore_output_params: true
                  , needsOutputToken    : false
                  , prefix              : "exp_"
                  , preTabCount         : 1
                  , tabstops            : tabstops
                  , needsEqualsNull     : true
                  , ignore              : out _
                  );
         }
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="tabstops"></param>
      /// <param name="sb"></param>
      /// <returns>no error return = always succeeds</returns>
      protected void AddInpParams_(int[] tabstops)
      {
         AppendLineDebug("-- @0130 S");

         foreach (var pr in ParamMap)
         {
            var param = pr.Value;
            string expRcTy;

            // if the tstd rtn is a function returning a scalar (no rows)
            // then the scalar is the first parameter has no name and is marked as output
            // in this case this is the act output @act_RC which has an exp_RC defined later.
            // So we need to get the type of the parameter now
            if ((param.is_output == true) && (RtnRetType == RtnRetType.FnScalar))
               expRcTy = param.ty_nm ?? "";

            // no error return = always succeeds
            CrtParamLine_Hlpr(
                 col_nm: param.col_nm   ?? ""
               , col_ty_nm: param.ty_nm ?? ""
               , is_output: param.is_output
               , needsComma: true
               , ignore_output_params: true
               , needsOutputToken: false
               , prefix: "inp_"
               , preTabCount: 1
               , tabstops: tabstops
               , needsEqualsNull: false
               , ignore: out _
               );
         } // end for

         AppendLineDebug("-- @0130 E");
      }

      /// <summary>
      /// Add the expected parameters 
      /// </summary>
      /// <param name="tabstops"></param>
      /// <param name="sb"></param>
      /// <returns>no error return - always succeeds</returns>
      protected void AddExpParams(int[] tabstops)
      {
         AppendLineDebug("-- @0140 S: AddExpParams");
         bool needsComma = true;
         bool ignore;

         // 220129: Bug 016: helper for fnFormatDate RC decl should be char not int
         // if a stored proc then RC type is INT
         // if a scalar fn then RC type should be the FN return type 
         var rc_col_ty_nm = "INT";

         if (IsScalarFn())
            rc_col_ty_nm = GetScalarFnReturnType();

         // add the exp_rc
         CrtParamLine_Hlpr(
              col_nm: "RC"
            , col_ty_nm: rc_col_ty_nm
            , is_output: false
            , needsComma: needsComma
            , ignore_output_params: false
            , needsOutputToken: false
            , prefix: "exp_"
            , preTabCount: 1
            , tabstops: tabstops
            , needsEqualsNull: true
            , ignore: out ignore
            );

         // Get the exp_ for the out put cols 
         // 220207: Create does not need exp (duplicate of inp)
         if(TestType != TestType.Create)
         {
            foreach (var pr in ExpectedParameterMap)
            {
               ParamInfo output_param = pr.Value;

               // ignore output parameters for now - they will be added 
               // by @0150 S: AddOutParams
               if (output_param.is_output == false)
               CrtParamLine_Hlpr(
                    col_nm: output_param.col_nm   ?? ""
                  , col_ty_nm: output_param.ty_nm ?? ""
                  , is_output: output_param.is_output
                  , needsComma: needsComma
                  , ignore_output_params: false
                  , needsOutputToken: false
                  , prefix: "exp_"
                  , preTabCount: 1
                  , tabstops: tabstops
                  , needsEqualsNull: true
                  , ignore: out ignore
                  );
            }
         }

         AppendLineDebug("-- @0140 E: AddExpParams");
      }

      /// <summary>
      /// Get any output params that are not part of the key as exp_ no error return - always succeeds
      /// </summary>
      /// <param name="tabstops"></param>
      /// <param name="sb"></param>
      /// <returns>no error return - always succeeds</returns>
      protected void AddOutParams(int[] tabstops)
      {
         AppendLineDebug("-- @0150 S: AddOutParams");

         foreach (var pr in ParamMap)
         {
            var param = pr.Value;
            var col_nm = param.col_nm;

            // If the tested rtn is a scalar fn then ignore columns with no name
            if ((param.is_output == false) ||
               (param.is_pk      == true ) ||
               (RtnRetType       == RtnRetType.FnScalar && (col_nm?.Equals("") ?? false)))
               continue;

            CrtParamLine_Hlpr
            (
                 col_nm: col_nm ?? ""
               , col_ty_nm: param.ty_nm ?? ""
               , is_output: param.is_output
               , needsComma: true
               , ignore_output_params: false
               , needsOutputToken: false
               , prefix: "exp_"
               , preTabCount: 1
               , tabstops: tabstops
               , needsEqualsNull: true
               , ignore: out _
            );
         }

         AppendLineDebug("-- @0150 E: AddOutParams");
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="sb"></param>
      /// <exception cref="NotImplementedException"></exception>
      protected void CreationCheck_Hlpr()
      {
         AppendLineDebug("-- @0160 S: CreationCheck");

         if (TestType == TestType.Create)
         {
            if(PK_List != null)
            { 
               string pkActVarNm = GetPKVarName("act_");
               AppendLine($"\r\n\t-- CREATION Check");
               AppendLine($"\tIF {pkActVarNm} IS NULL");
               AppendLine($"\t\tTHROW 60524, 'Failed to create the {Table}', 1;");
            }
            else
               AppendLineDebug("-- @0160: PK not defined");
         }

         AppendLineDebug("-- @0160 E");
      }

      /// <summary>
      /// 
      /// </summary>
      protected void CrtTstHlprHdr()
      {
         AppendLine(string.Format(txt_bloc_hlpr_hdr, GetCurrentDateFormatted(), TstdRtnNm, TstHlprDesc));
      }

      protected string GetCurrentDateFormatted()
      {
         return DateTime.Now.ToString("dd-MMM-yyyy");
      }

      protected string Hlpr_Crt_FromClauseForActResults()
      {
         Precondition(!string.IsNullOrEmpty(View), "Hlpr_Crt_FromClauseForActResults():View not specified");
         return $"\tFROM {View}";
      }

      /// <summary>
      /// 11: Get the ACT resuts
      /// PRECONDITIONS: IsInitialised = true
      /*
   SELECT
	    @act_<col_nm >            = <col_nm >
      ...
   FROM <Vw>
   WHERE <pk_id> =@inp_<pk_id>;
*/
      /// </summary>
      /// <param name="args"></param>
      /// <returns></returns>
      protected void GetActResults_Bloc()
      {
         Precondition(TestType != TestType.Unknown, "Unexpected scenario: TestType.Unknown");
         AppendLineDebug("-- @0170 S: Crt_GetActResults_Bloc");

         // Won't be able to retrieve changes as the row should have been deleted
         if ((TestType == TestType.Delete) || (TestType == TestType.Unknown))
            return;

         if (RtnRetType == RtnRetType.FnScalar)
            return;

         // if no output cols or no detected updated columns then we cannot check much
         if ((RtnRetType == RtnRetType.SpNoRows) && ((SpUpdatedCols?.Count ?? 0)== 0) && (ExpectedParameterMap.Count==0))
            return;

         //
         // ASSERTION: if here then resutls are returned
         //
         string line = @"

      ------------------------------
      -- Get the actuals  @171
      ------------------------------

      SELECT";

         AppendLine(line);

         // @act_<col_nm > = <col_nm >
         // Don't want out params
         AddActParamsToSelect();
         AppendLineDebug("-- @0170 E");
      }

      /// <summary>
      /// Adds the actual params to the 
      ///  SELECT 
      ///   @act_<col_nm > = <col_nm >   <---- adds these lines
      ///  FROM xxxVw
      ///  WHERE PK matches inp pk flds
      ///  
      /// There should be 1 act for each exp_ param
      /// </summary>
      /// <param name="sb"></param>
      /// <param name="msg"></param>
      protected void AddActParamsToSelect()
      {
         Precondition(TestType != TestType.Unknown, "Unexpected scenario: TestType.Unknown");
         string comma = " ";
         AppendLineDebug("-- @0180 S");

         // Get the max arg len for the expected.act params
         int max_len = 0;

         foreach (var pr in ExpectedParameterMap)
         {
            if(pr.Key.Length > max_len)
               max_len = pr.Key.Length;
         }

         max_len+= 1;

         foreach (var pr in ExpectedParameterMap)
         {
            var param = pr.Value;

            if(!param.is_output)
            { 
               AppendLine($"\t\t{comma}@act_{param.col_nm?.PadRight(max_len)}= [{param.col_nm}]");
               comma = ",";
            }
         }
               
         // TLW: CrtParamMnRtnCrtHlprCallPrm
         if((TstdRtnType == RtnType.TableValuedFn) && (TestType==TestType.Default))
         {
            AssertNotNull(RtnReturnedColList);

            // Add the fn returned cols
            foreach (var col in RtnReturnedColList)
            {
               AppendLine($"\t\t{comma}@act_{col.col_nm?.PadRight(max_len)}= [{col.col_nm}]");
               comma = ",";
            }
         }

         // Create the From Clause
         AppendLine(Hlpr_Crt_FromClauseForActResults());

         // Create the Where Clause - Delete (and Unknown) have no where clause
         switch (TestType)
         {
            case TestType.Delete:
            case TestType.Unknown:
               break;

           default:
               Hlpr_CreateWhereClauseForActResults();
               break;
         }
           
         AppendLineDebug("-- @0180 E");
      }

      /// <summary>
      /// Create a line like: $"\tWHERE person_id = act_person_id;"
      /// andd stores it in the StringBuilder SB
      /// 
      /// NOTE: GetAll type routines do not need a where clause ??
      ///   ... unless we decide to pass one in to test
      /// 
      /// PRECONDITIONS:
      ///   211229: currently only handles single column PK
      ///   arguments must contain the PK column(s):
      ///   msg:  $"5600: internal error could not find any primary key columns in the arguments for the tested routine: [{Schema}].[{TstdRtnNm}]"
      ///   
      ///   220122: PK_List != null
      /// </summary>
      /// <returns>true if there needs to be a where clause:
      ///   true  if: TemplateType.Create, Get1, Update, Delete, 
      ///   false if: TemplateType.GetAll, or TemplateType.UNKNOWN type
      ///   </returns>
      protected string Hlpr_CreateWhereClauseForActResults()
      {
         Precondition(TestType != TestType.Unknown, "Unexpected scenario: TestType.Unknown");

         // Only relevant in certain scenarios:
         if (TestType == TestType.Delete)
               return "";

         // 220122: PRECONDITION PK_List != null
         PreconditionNotNull(PK_List, "E5729: the primary key fields list is not specified");
         Precondition(PK_List.Count > 0);

         AppendLineDebug("-- @0190 S");
         var whereClause = $"\tWHERE\r\n";

         // 211229: assume single column pk
         // 220201: dont assume single P key field 
         // for each field in the PK: add a criterion row in the where clause  
         for (int i=0; i<PK_List.Count; i++)
            whereClause = AddKeyFieldToWhereClause(whereClause, PK_List[i], i > 0);

         // remove the last /r/n as AppendLine will add another
         whereClause = whereClause.TrimEnd();
         AppendLine(whereClause);
         AppendLineDebug("-- @0190 E");
         return whereClause;
      }

      /// <summary>
      /// add a criterion row in the where clausee,g,:
      ///   id = 6
      /// 
      /// PRECONDITIONS:
      ///   
      /// </summary>
      /// <param name="whereClause"></param>
      /// <param name="keyInfo"></param>
      /// <exception cref="NotImplementedException"></exception>
      protected string AddKeyFieldToWhereClause(string whereClause, KeyInfo keyInfo, bool needAnd)
      {
         Precondition(TestType != TestType.Unknown, "Unexpected scenario: TestType.Unknown");
         // Get pk field name
         var pk_col_nm = $"{keyInfo.col_nm}";

         // Get the pk input variable name
         ParamInfo? pkParam = null;

         if (ParamMap.ContainsKey(pk_col_nm))
            pkParam = ParamMap[pk_col_nm];


         // all updates, gets, get all, delete should have a pk field in the input
         //if (pkParam?.is_pk ?? false)
         //   pk_inp_var_nm = $"@act_{pkParam.col_nm}";
         //else
         //   GetWhereParameterName(pkParam?.col_nm ?? "", out pk_inp_var_nm);
         string pk_inp_var_nm = keyInfo.col_nm ?? "???";

         // Get the pk input variable name - for Create type routines
         // the newly created id is returned in the @RC 

         // target field changes depending on the test type is Get1 or GetAll
         string tgt_pk_var_nm = "";
         var prefix = (TestType == TestType.Create) ? "act" : "inp";

         switch (TestType)
         {
            case TestType.Get1:
               tgt_pk_var_nm = pkParam?.col_nm ?? "oops!";
               break;

            default:
               tgt_pk_var_nm = pk_col_nm;
               break;
         }

         string and = needAnd ? "and" : "   ";
         whereClause += $"\t{and} [{pk_col_nm}] = @{prefix}_{tgt_pk_var_nm}\r\n";
         return whereClause;
      }

      /// <summary>
      /// Creates the parameter name like @person_id
      /// </summary>
      /// <param name="pkParam"></param>
      /// <param name="whereClause"></param>
      /// <param name="where_param_nm"></param>
      /// <param name="msg"></param>
      /// 
      /// <returns> pk_inp_var_nm with a leading @ like @person_id </returns>
      protected void GetWhereParameterName(string col_nm, out string where_param_nm)
      {
         Precondition(TestType != TestType.Unknown, "Unexpected scenario: TestType.Unknown");
         where_param_nm  = "";

         // msg = $"5600: internal error could not find any primary key columns in the arguments for the tested routine: [{Schema}].[{TstdRtnNm}]";
         // will happend with create type rtns as the returned pk is arbitary
         // 211229: current conventions is: return the pk in SP return code (@act_rc) 
         // all updates, gets, get all, delete routines should have only 1 arg: the pk field
         switch (TestType)
         {
            case TestType.Get1:
            case TestType.Update:
            case TestType.Delete:
               Assertion(ParamMap.Count() > 0,
                  "all updates, gets, get all, delete routines the first 1 arg: the pk field");

               // person_id = @person_id;
               where_param_nm = $"@act_{col_nm}";
               break;

            case TestType.Create:
               where_param_nm = "@act_rc";
               break;

            case TestType.GetAll:
               break;

            case TestType.Default: // default test type: do nothing
               break;
         }
      }

      protected void ScriptFnParameters(bool inlineParams)
      {
         GetCallRtnArgs(inlineParams);
      }

      /// <summary>
      /// Creates the helper test script for the following routine types:
      ///   Create, Update, Get1, GetAll, Delete, Default
      ///  
      /// Algorithm:
      ///   Checks rtn type then
      ///   Delegates all to  _CreateTestHlprScript
      ///   
      /// PRECONDITION : Init called, test type known
      /// 
      /// Creates the Helper script
      /// returns true if no issues
      /// </summary>
      /*protected bool _CreateTestHlprScript(out string script, out string msg)
      {  
         //           Precondition                  message
         Precondition(IsInitialised == true       , "_CreateTestHlprScript precondition: Init must be called first");

         //--------------------------------------
         // ASSERTION: args known
         //--------------------------------------

         bool ret = false;
         script = "";

         switch(TestType)
         {
            default          : ret = CreateTestHlprScriptGeneral(out script, out msg); break;
            case TestType.Pop: ret = CreateTestHlprScriptPop    (out script, out msg); break;
         }

         return ret;
      }*/

      /*protected bool CreateTestHlprScriptGeneral(out string script, out string msg)
      {
         bool ret = false;
         msg = "";

         do
         {
            // 1: Create the intro script bloc
            // returns 0 (OK always)
            Dual_CrtIntro_Bloc(TstRtnHlprNm);

            // 2: Create the header comment bloc
            Hlpr_Crt_Hdr_Bloc();

            // 3: Create the helper test strategy
            CrtStrategyHdr();

            // 4: Create the signature bloc
            Hlpr_Create_Sig_Bloc();

            // 4: Create the As/Begin/SET NOCOUNT ON;
            Hlpr_Crt_AsBegin_Bloc();

            // 5: Create the Declare bloc
            Hlpr_Create_Declare_Bloc();

            // Currently only using a tmp table for Get1 and GetAll
            // could use for create and update ??
            // CreateTempTable checks if the tmp table is wanted by checking the routine type
            // returns true if RtnReturnedColList has columns false otherwise
            // 220130: B017_sp_person_create_ShouldUseTmpTable(): if ((TestType == TestType.Get1) || (TestType == TestType.Get1) || (TestType == TestType.GetAll))

            // 6: Create a temporary table if rtn type is not Delete or unknown
            // 220204: creates will not return the updated columns - but they can be found and a table created for them
            CrtTmpTbl(out msg);

            // 7: Call the tested rtn
            if (!Hlpr_Crt_CallTstdRtn_Bloc(out msg))
               break;

            // 8: Get the ACT resuts
            Hlpr_Crt_GetActResults_Bloc();

            // 9: Check all ACTs match EXPs
            Hlpr_Crt_AssertBloc();

            // 10: Wrap up rtn end
            Dual_Crt_WrapUp();

            // Finally
            msg = "";
            ret = true;
         } while (false);

         // 10: Return the script
         script = SB.ToString();
         return ret;
      }*/

      /// Pop routines in Telepat are hard coded - take no parameters
      /// There are 2 kinds of pop routines:
      /// 1: simple - 1 procedure populates 1 country - can be tested as follows
      /// 2: complex like dbo.sp_pop_all_static_data that populate many SD tables - currently cannot be tested automatically.
      /// 
      /// So we are implementing type 1: single table pop routines here
      /// 
      /// The test helper should be able to check a specific row based on either the PK or all the row
      /// so the additional arguments are:
      /// inp_PK, exp_values as per the table all optional 
      /// 
      /// Parameter bloc: 
      ///   input args: 
      /// 	pk,
      /// 
      ///    expected args: 
      /// 	exp RC INT
      ///   	all table cols bar pk all optional (= null)
      /// 
      /// Declare bloc:
      ///   @act_ vars 1 per col type as per table spec
      /// 
      /// Select bloc:
      /// 
      /// SELECT
      /// 	 @act_col_1 = col_1
      /// 	,@act_col_2 = col_2
      /// FROM <tbl>
      /// WHERE pk matches
      /// 
      /// Assert bloc:
      /// for each non null exp parameter run the exp/act check
      /// 
      /// Wrap up
      /// 
      /*private bool CreateTestHlprScriptPop(out string script, out string msg)
      {
         bool ret = false;
         msg = "";

         do
         {
            // 1: Create the intro script bloc
            // returns 0 (OK always)
            Dual_CrtIntro_Bloc(TstRtnHlprNm);

            // 2: Create the header comment bloc
            Hlpr_Crt_Hdr_Bloc();

            // 3: Create the helper test strategy
            CrtStrategyHdr();

            // 4: Create the signature/Parameter bloc Hlpr_Create_Sig_Bloc_Pop
            Hlpr_Create_Sig_Bloc_Pop();

            // 5: Create the As/Begin/SET NOCOUNT ON;
            Hlpr_Crt_AsBegin_Bloc();

            // 6: Create the Declare bloc
            Hlpr_Create_Declare_Bloc();

            // Currently only using a tmp table for Get1 and GetAll
            // could use for create and update ??
            // CreateTempTable checks if the tmp table is wanted by checking the routine type
            // returns true if RtnReturnedColList has columns false otherwise
            // 220130: B017_sp_person_create_ShouldUseTmpTable(): if ((TestType == TestType.Get1) || (TestType == TestType.Get1) || (TestType == TestType.GetAll))

            // 7: Create a temporary table if rtn type is not Delete or unknown
            // 220204: creates will not return the updated columns - but they can be found and a table created for them
            //CrtTmpTbl(out msg);

            // 8: Call the tested rtn
            if (!Hlpr_Crt_CallTstdRtn_Bloc(out msg))
               break;

            // 9: Get the ACT resuts
            Hlpr_Crt_GetActResults_Bloc();

            // 10: Check all ACTs match EXPs
            Hlpr_Crt_AssertBloc();

            // 11: Wrap up rtn end
            Dual_Crt_WrapUp();

            // 13: Finally
            ret = true;
         } while (false);

         // 12: Return the script
         script = SB.ToString();
         return ret;
      }*/

      /// <summary>
      /// 
      /// </summary>
      /// <param name="sb"></param>
      /// <param name="msg"></param>
      /// <returns></returns>
      protected void Call_TstdRtn_Fn_RetNoRows_Hlpr()
      {
         //inline params if only 1 or 2
         bool inlineParams = (this.ParamMap.Count < 3);
         var NL = inlineParams ? "" : "\r\n";
         AppendLine($"\tSELECT @act_RC={Schema}.{TstdRtnNm}({NL}");
         // Add parameters 
         ScriptFnParameters(inlineParams);
         AppendLine(");");
      }

      /// <summary>
      /// '  INSERT INTO #tmpTbl select * FROM {QualTstdRtnNm}({args}); "
      /// </summary>
      /// <exception cref="NotImplementedException"></exception>
      protected void Call_TstdRtn_Fn_RetRows_Hlpr()
      {
         //inline params if only 1 or 2
         bool inlineParams = (this.ParamMap.Count < 3);
         var NL = inlineParams ? "" : "\r\n";
         AppendLine($"\tINSERT INTO #tmpTbl SELECT * FROM {QualTstdRtnNm} ({NL}");
         ScriptFnParameters(inlineParams);
         AppendLine($"\t\t\t)");
      }

      /// <summary>
      /// '  INSERT INTO #tmpTbl EXEC @act_rc = {QualTstdRtnNm}; "
      /// </summary>
      protected void Call_TstdRtn_Sp_RetRows_Hlpr()
      {
         AppendLine($"\tINSERT INTO #tmpTbl EXEC @act_rc = {QualTstdRtnNm} ");
         GetCallRtnArgs(inlineParams: false);
      }
   }
}