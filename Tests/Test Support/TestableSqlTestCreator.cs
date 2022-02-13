using CreateSqlTestRoutineLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Tests
{
   /// <summary>
   /// Testing interface exposing protected items to be tested
   /// </summary>
   public class TestableSqlTestCreator : SqlTestCreator
   {
      public TestableSqlTestCreator(string? connectionString = null, bool ? debug_mode = null)
         : base(connectionString, debug_mode)
      {
      }

      public new void SetConnectionStringProperties(string? conn_str)
      {
         base.SetConnectionStringProperties(conn_str);
      }

      public void SetDbName(string dbName)
      {
         Database = dbName;
      }
         
      public new bool ChkLineForDescEnd(string line)
      {
         return base.ChkLineForDescEnd( line);
      }

      public new List<string>? GetUntestedRtns()
      {
         return base.GetUntestedRtns();
      }
         
      public new string? GetRtnDesc(string rtnName)
      {
         return base.GetRtnDesc(rtnName);
      }

      public new Dictionary<string, ParamInfo>? GetRtnParamsFromDb( string? schema_nm, string rtn_nm)
      {
         return base.GetRtnParamsFromDb(schema_nm, rtn_nm);
      }

      public new List<KeyInfo> GetPK_ListFromDb()
      {
         return base.GetPK_ListFromDb();
      }

      public new static bool NeedsTable( TestType templateType)
      {
         return SqlTestCreator.NeedsTable(templateType);
      }

      public new TestType DetermineTestType( string rtnName)
      {
         return base.DetermineTestType( rtnName);
      }

      public new string CrtMnTestName( string testedRtn, int testNum)
      {
         return base.CrtMnTestName( testedRtn,  testNum);
      }

      public new string CrtHlprTstNm(string testedRtn, int testNum)
      {
         return base.CrtHlprTstNm(testedRtn, testNum);
      }

      public new int ChkInitPostConditions(out string msg)
      {
         return base.ChkInitPostConditions(out msg);
      }

      public new bool __Mn_CreateTestScript(out string script, out string msg)
      {
         return base.__Mn_CreateTestScript(out script, out msg); 
      }

      public new bool __Hlpr_CreateScript(out string script, out string msg)
      {
         return base.__Hlpr_CreateScript(out script, out msg); 
      }

     /* public new bool _CreateTestHlprScript(out string script, out string msg)
      {
         return base._CreateTestHlprScript(out script, out msg);
      }*/

      public void SetSchema(string value)
      {
         Schema = value;
      }

      public void SetTstdRtnNm(string value)
      {
         TstdRtnNm = value;
      }

      public void SetTable (string value) 
      {
         Table = value;
      }

      public new int GetPrmDetailsFromDb(string schema, string tstdRtnNm, out List<ParamInfo> list,  out string msg)
      {
         return base.GetPrmDetailsFromDb( schema, tstdRtnNm, out list, out msg);
      }

      public new int GetRtnDetailsFromDb(string schema, string tstdRtnNm, out List<RtnDetail> rtnDetailInfoList, out string msg)
      {
         return base.GetRtnDetailsFromDb( schema, tstdRtnNm, out rtnDetailInfoList, out msg);
      }

      public new int GetRtnOutputColsFromDb(string schema, string tstdRtnNm, out List<ParamInfo> list, out string msg)
      {
         return base.GetRtnOutputColsFromDb( schema, tstdRtnNm, out list, out msg);
      }

      public new void GetWhereParameterName(string col_nm, out string pk_inp_var_nm)
      {
         base.GetWhereParameterName(col_nm, out pk_inp_var_nm);
      }

      public new bool CrtTmpTbl(out string msg)
      {
         return base.CrtTmpTbl(out msg);
      }

      /// <summary>
      /// Always succeeds: no error return
      /// </summary>
      /// <param name="param"></param>
      /// <param name="needsComma"></param>
      /// <param name="ignore_output_params"></param>
      /// <param name="needsOuPutToken"></param>
      /// <param name="needsEqualsNull"></param>
      /// <param name="prefix"></param>
      /// <param name="preTabCount"></param>
      /// <param name="tabstops"></param>
      /// <param name="sb"></param>
      /// <param name="msg"></param>
      /// <returns>Always succeeds: no error return</returns>
      public void  CrtParamLine_Hlpr
      (
         ParamInfo param,
         bool needsComma,
         bool ignore_output_params,
         bool needsOuPutToken,
         bool needsEqualsNull,
         string prefix,
         int preTabCount,
         int[] tabstops,
         out string msg
      )
      {
         msg = "";

         CrtParamLine_Hlpr
         (
             col_nm              : param.col_nm ?? ""
            ,col_ty_nm           : param.ty_nm ?? ""
            ,is_output           : param.is_output
            ,needsComma          : needsComma
            ,ignore_output_params: ignore_output_params
            ,needsOutputToken     : needsOuPutToken
            ,needsEqualsNull     : needsEqualsNull
            ,prefix              : prefix
            ,preTabCount         : preTabCount
            ,tabstops            : tabstops
            ,ignore              : out bool ignore
         );
      }
   
      public new string Hlpr_CreateWhereClauseForActResults()
      {
         return base.Hlpr_CreateWhereClauseForActResults();
      }

      public void SetTemplateType(TestType testType)
      {
         TestType = testType;
      }

      public void SetTstNum(int testNum)
      {
         TstNum= testNum;
      }
      
      public new int Validate(string? testedRtn, int testNum, out string error_msg)
      {
         return base.Validate(testedRtn, testNum,  out error_msg);
      }

      public new int CrtParamLineAssignment(string col_nm, bool is_output, bool needsComma, bool inlineParams, out string line )
      {
         return base.CrtParamLineAssignment(col_nm, is_output, needsComma, inlineParams, out line);
      }

      public new void Hlpr_Create_Declare_Bloc()
      {
         base.Hlpr_Create_Declare_Bloc();
      }

      public new int GetOutArgsFromDb(out Dictionary<string, ParamInfo> outArgs, out string error_msg)
      {
         return base.GetOutArgsFromDb(out outArgs, out error_msg); 
      }

      public new int Hlpr_Create_Declare_BlocForActuals(int[] tabstops)
      {
         return base.Hlpr_Create_Declare_BlocForActuals(tabstops);
      }

      public new void Mn_Create_HlprCall()
      {
         base.Mn_Create_HlprCall();
      }

      public new Dictionary<string, ParamInfo> GetExpParameters()
      {
         return base.GetExpParameters();
      }

      public new void Call_TstdRtn_Sp_RetNoRows_Hlpr()
      {
         base.Call_TstdRtn_Sp_RetNoRows_Hlpr();
      }
      public new void CrtAssertLine(string? _arg, string prefix, ref int tst_sub_num)
      {
         base.CrtAssertLine( _arg, prefix, ref tst_sub_num);
      }
      public new void CrtCreateRtnLine()
      {
         base.CrtCreateRtnLine();
      }
      public new string CrtVarNm(string col_nm, bool isParameter, bool is_output, bool needsComma)
      {
         return base.CrtVarNm(col_nm, isParameter, is_output, needsComma);
      }

      public new void GetCallRtnArgs(bool inlineParams)
      {
         base.GetCallRtnArgs(inlineParams);
      }

      public new void Hlpr_Crt_AsBegin_Bloc()
      {
         base.Hlpr_Crt_AsBegin_Bloc();
      }

      public new void Hlpr_Crt_AssertBloc()
      {
         base.Hlpr_Crt_AssertBloc();
      }

      public new bool Hlpr_Crt_CallTstdRtn_Bloc(out string msg)
      {
         return base.Hlpr_Crt_CallTstdRtn_Bloc(out msg);
      }

      public new void Hlpr_Create_Sig_Bloc()
      {
         base.Hlpr_Create_Sig_Bloc();
      }

      public new void Hlpr_Crt_Hdr_Bloc()
      {
         base.Hlpr_Crt_Hdr_Bloc();
      }

      public new int GetRtnDependencies(string rtnNm, out List<DependencyInfo> list, out string error_msg)
      {
         return base.GetRtnDependencies(rtnNm, out list, out error_msg);
      }

      public new void AppendLine(string line = "", bool lnFeed = true)
      {
         base.AppendLine(line, lnFeed);
      }
   }
}
