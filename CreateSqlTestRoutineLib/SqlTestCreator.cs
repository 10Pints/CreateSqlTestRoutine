/*
using System.Data;
using System.Linq;
using Dapper;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using Common;
using static Common.Utils;
*/
using System.Text;
using System.Diagnostics;
using Common;
using static Common.Utils;
/*using RSS.Test;
using System;
*/
namespace CreateSqlTestRoutineLib
{
   /// <summary>
   /// This class produces boiler plate SQL code for tSQLt tests 
   /// 
   /// Design: D:\Dev\Repos\telepat.io\design\Telepat.io.EAP
   /// 
   /// It handles the current formats:
   /// "default"  - std hdr, std bdy, std ftr
   /// "Create"   - std hdr, crt bdy, std ftr
   /// "Update"   - std hdr, updt bdy, std ftr
   /// "Get1"     - std hdr, get bdy: temp table (populated from tested sp), std ftr
   /// "GetAll"   - std hdr, get bdy  populated a where clause parameter temp table, std ftr
   /// "del       - std hdr, del_bdy, std ftr"
   /// 
   /// Components:
   /// std hdr: procedure header with date, description author, starting msg
   /// std ftr: assertions, test passed bloc, commented run tests execs
   /// </summary>
   public partial class SqlTestCreator : SqlTestCreatorHlpr
   {
      #region data members

      //-------------------------------------------------------------------------

      #endregion data members
      #region public interface

      /// <summary>
      /// The only ctor
      /// </summary>
      public SqlTestCreator(string? _connectionString = null, bool? debug_mode = null)
         : base( _connectionString, debug_mode)
      {
         LoadscriptBlocs();
      }

      /// <summary>
      /// Main entry point for the basic creation method uing text substituion
      /// PRECONDITIONS: Init called
      ///                RtnType set
      /// </summary>
      /// <returns> The test function sql</returns>
      public int __CrtTestMethods
      (
           string       qTstdRtnNm
         , int          testNum
         , out string   msg
         , string?      table
         , string?      view
         , out string   hlpr_script
         , out string   mn_script
      )
      {
         int error_code = -1;
         hlpr_script    = "";
         mn_script      = "";
         msg            = "";

         do
         {
            error_code = Init(qTstdRtnNm: qTstdRtnNm, tstNum: testNum, conn_str: ConnectionString, msg: out msg, table: table, view: view);

            if (error_code != 0)
               break;

            error_code = __Hlpr_CreateScript(out hlpr_script, out msg) ? 0 : 1;
            
            if(!__Mn_CreateTestScript(out mn_script, out msg))
               break;

         } while(false);

         return error_code;
      }

      /// <summary>
      /// Creates the main test script
      /// PRCONDITION: Init() called
      /// POST if error throws exception
      /// </summary>
      /// <returns></returns>
      /// <exception cref="NotImplementedException"></exception>
      protected bool __Mn_CreateTestScript(out string script, out string msg)
      {
         bool ret = false;
         script = "";
         PreconditionChkInit("AutogenCrtMnTstScrpt");

         do
         {
            if(!OpenScriptWriter(MnScrptFile, out msg))
               break;

            Dual_CrtIntro_Bloc(TstRtnMnNm);
            Mn_Crt_TstHdr();
            Mn_Crt_Sig();
            Mn_Crt_AsBeginBloc();
            Mn_Create_HlprCall();
            Dual_Crt_WrapUp();
            script = SB.ToString();

            if(string.IsNullOrWhiteSpace(script))
            {
               msg = "script is empty";
               break;
            }

            // Finally OK so:
            ret = true;
            msg = "";
         } while (false);

         return ret;
      }

      /// <summary>
      /// 6.5: Create the As/Begin/SET NOCOUNT ON;
      /// </summary>
      /// <returns>0 if ok else error code and error msg</returns>
      protected void Mn_Crt_AsBeginBloc()
      {
         AppendLine($"AS\r\nBEGIN\r\n\tSET NOCOUNT ON;");
         AppendLine($"\tPRINT '{TstRtnMnNm} starting';\r\n");
      }

      /// <summary>
      ///    EXEC test.helper_020_smoke_sp_c2s_create 20, 1, 'T_Jones', Harry, 1, 'C++'   , 'Intermediate', 4;
      /// PRCONDITION: Init() called
      /// EXEC test.helper_901_sp_person_create
      ///    @inp_family_name   = NULL
      ///   ,@inp_first_name    = NULL
      ///   ,@inp_middle_name   = NULL
      ///   ,@inp_seq           = NULL
      ///   ,@inp_type_nm       = NULL
      ///   ,@inp_salutation_nm = NULL
      ///   ,@inp_email         = NULL
      ///   ,@inp_phone         = NULL
      ///   ,@inp_mobile        = NULL
      ///   ,@exp_RC            = NULL
      /// </summary>
      /// Exceptions:
      ///   PreconditionChkInit() exceptions
      ///   "E4457: primary key not found" if No primary key column found
      /// <returns></returns>
      protected void Mn_Create_HlprCall()
      {
         AppendLineDebug("-- @M1010 S: CRT_HlprCall");
         PreconditionChkInit("AgCrtMnCallHlpr");
         //PreconditionNotNull(PK_List);
         AppendLine($"\tEXEC test.{TstRtnHlprNm}");
         int[] tabstops = new int[] { 0, MaxArgLen, MaxArgLen + 20, MaxArgLen + 30 };

         // 1: the test num
         CrtParamMnRtnCrtHlprCallPrm(
              col_nm     : "tst_num"
            , needsComma : false
            , prefix     : ""
            , preTabCount: 2
            , val        : "1"
            , tabstops   : tabstops);

         if (TestType == TestType.GetAll)
         {
            Utils.Assertion((PK_List?.Count() ?? 0) > 0, "E4457: primary key not found");
            //----------------------------------------------------------------
            // 220118: Get all should query 1 row - so we need to pass the id 
            //----------------------------------------------------------------
#pragma warning disable CS8604 // Possible null reference argument.
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            CrtParamMnRtnCrtHlprCallPrm(
                 col_nm: PK_List[0].col_nm
               , needsComma: true
               , prefix: ""
               , preTabCount: 2
               , val: "1"
               , tabstops: tabstops);;
#pragma warning restore CS8602 // Dereference of a possibly null reference.
#pragma warning restore CS8604 // Possible null reference argument.
         }

         // 2: add the test method input params
         // create a line like: (,)@param_nm = null
         AppendLineDebug("-- @M1011: add tst rtn params (not outputs)");

         foreach (var pr in ParamMap)
         {
            var arg = pr.Value;

            if (arg.is_output == false)
            {
               CrtParamMnRtnCrtHlprCallPrm(
                  col_nm: arg.col_nm ?? "-",
                  needsComma: true,
                  preTabCount: 2,
                  tabstops: tabstops);
            }
         }

         // if tstd rtn is a fn returning columns then add these columns to the input params
         // so that they can be asserted when the routine is called
         if ((TestType == TestType.Default) && (TstdRtnType == RtnType.TableValuedFn))
         {
            AssertNotNull(RtnReturnedColList);

            foreach (var col in RtnReturnedColList)
            {
               CrtParamMnRtnCrtHlprCallPrm(
                    col_nm: col.col_nm ?? ""
                  //, col_ty_nm: col.col_ty_nm
                  //, is_output: col.is_output
                  , needsComma: true
                  //, ignore_output_params: true
                  //, needsOutputToken: false
                  , prefix: "inp_"
                  , preTabCount: 2
                  , tabstops: tabstops
                  //, needsEqualsNull: false
                  //, ignore: out _
                  );
            }
         }


         // 3: @exp_RC
         AppendLineDebug("-- @M1012: add exp_RC");

         CrtParamMnRtnCrtHlprCallPrm(
            col_nm: "RC",
            needsComma: true,
            prefix: "exp_",
            preTabCount: 2,
            val: "NULL",
            tabstops: tabstops);

         // 4: Get any output parmas that are not part of the key as exp_
         AppendLineDebug("-- @M1013: add the exp params");
         foreach (var pr in ExpectedParameterMap)
         {
            var param = pr.Value;
            CrtParamMnRtnCrtHlprCallPrm(
               col_nm: param.col_nm ?? "",
               needsComma: true,
               prefix: "exp_",
               preTabCount: 2,
               tabstops: tabstops
               );
         }

         AppendLineDebug("-- @M1010 E: CRT_HlprCall");
      }

      /// <summary>
      /// CREATE PROCEDURE [test].[test_020_smoke_c2s_create]
      /// </summary>
      /// <param name="sb"></param>
      /// <returns></returns>
      /// <exception cref="NotImplementedException"></exception>
      protected void Mn_Crt_Sig()
      {
         AppendLine($"CREATE PROCEDURE test.{TstRtnMnNm}");
      }

      /// <summary>
      /// 5: Create the header comment bloc
      /// </summary>
      /// <returns></returns>
      protected void Mn_Crt_TstHdr()
      {
         var cmnt_bloc = string.Format(txt_bloc_mn_tst_hdr, GetCurrentDateFormatted(), TstdRtnNm, TstdRtnDesc);
         AppendLine(cmnt_bloc);
      }

      /// <summary>
      /// Create a line like: (,)@param_nm = null
      /// PRCONDITION: Init() called
      /// </summary>
      /// <param name="param"></param>
      /// <param name="sb"></param>
      protected void CrtParamMnRtnCrtHlprCallPrm
      (
         string        col_nm,
         bool          needsComma,
         string        prefix     = "inp_",
         int           preTabCount= 1,
         string        val        = "NULL",
         int[]?        tabstops   = null
      )
      {
         if(tabstops == null)
            tabstops = new int[2] { 1, 25 };

         string preTab = new string(' ', preTabCount*3);
         string comma = needsComma ? "," : " ";
         var    spc1  = "".PadRight(tabstops[0]);
         var    line  = $"{preTab}{spc1}{comma}@{prefix}{col_nm.PadRight(tabstops[1])} = {val}";//;

         AppendLine(line);
      }

      #endregion public interface
   }
}
