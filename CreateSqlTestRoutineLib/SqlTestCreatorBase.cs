using Common;
using Dapper;
using Microsoft.Extensions.Configuration;
using System.Threading;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Text.RegularExpressions;
using static Common.Utils;
using Serilog;
using System.Diagnostics;
using System.Linq;

namespace CreateSqlTestRoutineLib
{
   public class SqlTestCreatorBase
   {
      protected static IConfigurationRoot? Configuration { get; set; }

      //---------------------- Text blocks stored in files ----------------------

      protected string txt_bloc_call_tstd_rtn_cmnt = "";
      protected string txt_bloc_crt_bdy = "";
      protected string txt_bloc_crt_tmp_tbl = "";
      protected string txt_bloc_get_bdy = "";
      protected string txt_bloc_del_bdy = "";
      protected string txt_bloc_hlpr_cmnt_hdr = "";
      protected string txt_bloc_hlpr_hdr = "";
      protected string txt_bloc_mn_crt_wrap_up_rtn = "";
      protected string txt_bloc_mn_tst_hdr = "";
      protected string txt_bloc_pre_sets = "";
      protected string txt_bloc_rtn_hdr = "";
      protected string txt_bloc_std_bdy = "";
      protected string txt_bloc_std_ftr = "";
      protected string txt_bloc_strgy_crt = "";
      protected string txt_bloc_strgy_del = "";
      protected string txt_bloc_strgy_dflt = "";
      protected string txt_bloc_strgy_get_all = "";
      protected string txt_bloc_strgy_get1 = "";
      protected string txt_bloc_strgy_updt = "";
      public TestType         TestType          { get; protected set; }
      public RtnType?         TstdRtnType       { get; private set; }
      public RtnRetType       RtnRetType        { get; private set; }
      public string           Script            => SB.ToString();

      private string _conn_str = "";
      public string           ConnectionString
      {
         get
         {
            Precondition(false == String.IsNullOrWhiteSpace(_conn_str), "E5726: the connection string is not specified");
            return _default_conn_str;
         }

         protected set
         {
            Precondition(false == String.IsNullOrWhiteSpace(value), "E5727: the connection string is not specified");
            _conn_str = value ?? "";
         }
      }

      private static string _default_conn_str = "";
      public static string DefaultConnectionString
      {
         get
         {
            Precondition(false == String.IsNullOrWhiteSpace(_default_conn_str), "E1009: the default connection string is not specified"); // E1009
            return _default_conn_str;
         }

         set
         {
            Precondition(false == String.IsNullOrWhiteSpace(value), "E5725: the default connection string is not specified");
            _default_conn_str = value ?? "";
         }
      }

      public int              TstNum            { get; protected set; } = 0;
      public bool             IsInitialised     { get; protected set; } = false;
      public string           GetDbName         { get; protected set; } = "";
      public string?          CrtSp             { get; protected set; } = null;
      public string?          CrtParams         { get; protected set; } = null;
                                                
      public string?          TstHlprDesc       { get; protected set; } = null;
      
      private string? _table = null;
      public string?          Table             
      {
         get
         {
            //Precondition(false == String.IsNullOrWhiteSpace(_table), "E5720: the table is not specified");
            return _table;
         }

         protected set
         {
            Precondition(false == String.IsNullOrWhiteSpace(value), "E5721: the table is not specified");
            _table = value;
         }
      }

      private string? _view = "";
      public string? View 
      { 
         get => _view;

         protected set
         {
            Precondition(false == String.IsNullOrWhiteSpace(value), "E5723: the view is not specified");
            _view = value;
         }
      }

      private List<KeyInfo>? _pk_list = null;
      public List<KeyInfo>?  PK_List           
      { 
         get => _pk_list;

         protected set
         {
            Precondition(value!=null, "E5728: the primary key fields list is not specified");
            _pk_list = value;
            IsPkListPopulated = true;
         }
      }

      public string           TstdRtnNm         { get; protected set; } = "";
      public string           QualTstdRtnNm     => String.IsNullOrWhiteSpace(Schema) 
                                                   ? TstdRtnNm 
                                                   : $"{Schema}.{TstdRtnNm}";

      public string           Database          { get; protected set; } = "";
      public string           Server            { get; protected set; } = "";
      public string           HlprScrptFile     { get; set;           } = "";
      public string           MnScrptFile       { get; set;           } = "";
      public string?          Schema            { get; protected set; } = "";
      public object?          QryParams         { get; protected set; } = null;
      public string           TstRtnMnNm        { get; protected set; } = "";
      public string           TstRtnHlprNm      { get; protected set; } = "";
      public bool IsParamMapPopulated           { get; protected set; } = false;
      public bool IsRtnReturnedColListPopulated { get; protected set; } = false;
      public bool IsOutArgsMapPopulated         { get; protected set; } = false;
      public int              MaxArgLen         { get; protected set; } = 0;
      public string?          TstdRtnDesc       { get; protected set; }
      public List<string>?    TableList         { get; protected set; }
      public List<string>?    ViewList          { get; protected set; }
      public List<ParamInfo>  ParamInfoList     { get; protected set; } = new();
      public string           CurrentScriptFile { get; private set; } = "";

      public Dictionary<string, ParamInfo> OutArgsMap          { get; private set;    } = new();
      public Dictionary<string, ParamInfo> ParamMap            { get; private set;    } = new();
      public List<RtnDetail>?               RtnDetailInfoList  { get; protected set;  }
      public List<ParamInfo>?               RtnReturnedColList { get; protected set;  }
      public Dictionary<string, ParamInfo> ExpectedParameterMap{ get; protected set;  } = new();
      public List<ColInfo>? TableCols                          { get; private set; }
      public bool IsExpectedParameterMapPopulated              { get; private set; }

      private StreamWriter? _sw;


      protected StreamWriter SW 
      {
         get
         {
            if(_sw == null)
               throw new Exception("E5562: sql script writer not initialised");

            return _sw;
         }
      }

      /// <summary>
      /// main script builder
      /// </summary>
      private StringBuilder _sb = new StringBuilder();

      /// <summary>
      /// 
      /// </summary>
      public StringBuilder SB => _sb;

      /// <summary>
      /// 
      /// </summary>
      public bool IsPkListPopulated { get; private set; } = false;

      /// <summary>
      /// 
      /// </summary>
      public bool DebugMode { get; set; } = false;

      /// <summary>
      /// 
      /// </summary>
      public List<GetProcUpdateInfo>? SpUpdatedCols { get; private set; } = null;

      /// <summary>
      /// Open the script writer
      /// PRECONDITION path specified
      /// </summary>
      /// <param name="filePath"></param>
      /// <param name="msg"></param>
      /// <returns></returns>
      protected bool OpenScriptWriter(string filePath, out string msg)
      {
         bool ret = false;
         msg = "";
         CurrentScriptFile = "";
         Precondition(!string.IsNullOrWhiteSpace(filePath), "E4587: OpenScriptWriter failed: file path is not specified");
         filePath = Path.GetFullPath(filePath);
         Logger.LogS($"opening script writer: File: [{filePath}]");
         Console.WriteLine($"opening script writer: File: [{filePath}]");
         int count = 0;

         // try 3 times - to wait for file locks
         do
         { 
            try
            { 
               if(IsScriptWriterIsOpen())
                  CloseScriptWriter();

               _sb = new StringBuilder();
               _sw = new(filePath, append: false);
               _sw.AutoFlush = true;
               CurrentScriptFile = filePath;
               ret = true;
               msg = "";
               break;
            }catch(Exception e)
            {
               Logger.LogException(e);
               count++;
               _sw?.Close();

               if (count>3)
               {
                  msg = $"failed to open script file: {filePath} \r\n{e.Message}";
                  break;
               }

               filePath = $"{Path.GetFileName(filePath)}.{GetTimestamp(true)}.{Path.GetExtension(filePath)}";
            }

            // finally
            ret = true;
            msg = "";
         } while((ret == false) && (count<4));

         return ret;
      }

      protected bool IsScriptWriterIsOpen()
      { 
         return (_sw?.BaseStream != null);
      }

      protected void CloseScriptWriter()
      {
         _sw?.Close();
         _sb.Clear();
      }

      /// <summary>
      /// Main script writeting i/f - writes the line with a linefeed at the end
      /// Writes to the file AND to the internal StringBuilder _sw.
      /// 
      /// The file buffer is flushed after each write
      /// </summary>
      /// <param name="line">the line to write</param>
      protected void AppendLine(string line = "", bool lnFeed = true)
      {
         if (!IsScriptWriterIsOpen())
            if(!OpenScriptWriter(CurrentScriptFile, out var msg))
               throw new Exception(msg);

         if(lnFeed)
         { 
            _sw?.WriteLine(line);
            _sb.AppendLine(line);
         }
         else
         {
            _sw?.Write(line);
            _sb.Append(line);
         }
      }

      /// <summary>
      /// 220129: optionally include/remove trace comments in script
      /// </summary>
      /// <param name="line"></param>
      /// <param name="lnFeed"></param>
      protected void AppendLineDebug(string line, bool lnFeed = true)
      {
         if(DebugMode == true)
            AppendLine( line, lnFeed);
      }


      protected static bool NeedsTable(TestType templateType)
      {
         return ((templateType == TestType.Create)
              || (templateType == TestType.Update)
              || (templateType == TestType.Get1)
              || (templateType == TestType.GetAll));
      }

      /// <summary>
      /// Main cstr sets teh  connections string, server and database if _connectionString supplied
      /// </summary>
      /// <param name="_connectionString"></param>
      public SqlTestCreatorBase(string? _connectionString = null, bool? debug_mode = null)
      {
         if (_connectionString != null)
         {
            Precondition(!string.IsNullOrWhiteSpace(_connectionString), "The connection string is configured");
            SetConnectionStringProperties(_connectionString);
         }

         if(debug_mode != null)
            DebugMode = debug_mode ?? false;
      }

      /// <summary>
      ///  
      /// POST CONDITIONS:
      /// EITHER:
      /// (
      ///  IsInitialised and the following state is set
      ///   P01: Schema
      ///   P02: TstdRtnNm 
      ///   P03: TstRtnMnNm
      ///   P04: TstRtnHlprNm
      ///   P05: TestType
      ///   P06: Description
      ///   P07: ArgList
      ///   P08: MaxArgLen
      ///   P09: TstdRtnDesc
      ///   P10: MaxArgLen: if args then must be > 0
      ///   P11: primary key found
      ///   P12: DbName must be specified
      ///   P13: IsParamMapPopulated must be set
      ///   P14: IsRtnOutputColInfoListPopulated must be set
      ///   )
      ///  OR IsInitialised = false
      /// </summary>
      /// <param name="msg"></param>
      /// <returns></returns>
      protected int ChkInitPostConditions(out string msg)
      {
         int error_code = -1;
         IsInitialised = false;
         msg = "";

         do
         {
            error_code = ChkInitPostConditions_1(out msg);

            if (error_code != 0)
               break;

            error_code = ChkInitPostConditions_2(out msg);

            if (error_code != 0)
               break;

            error_code = ChkInitPostConditions_3(out msg);

            if (error_code != 0)
               break;

            // Finally
            IsInitialised = true;
            error_code = 0;
         } while (false);

         if((error_code != 0) && (!msg.Contains($"E{error_code}:")))
            msg = $"E{error_code}: {msg}";

         return error_code;
      }

      /// POST CONDITIONS:
      /// EITHER:
      /// (
      ///  ret = true AND and the following state is set
      /// POST 1: ConnectionString set
      /// POST 2: Db name set
      /// POST 3: Schema set
      /// POST 4: TstNum set
      /// POST 5: TstdRtnNm set
      /// )
      ///  OR ((ret != 0) AND (error msg set))
      protected int ChkInitPostConditions_1(out string msg)
      {
         int error_code = -1;

         do
         {
            error_code = Validate(QualTstdRtnNm, TstNum, out msg);

            if (error_code != 0)
               break;

            // POST 1: ConnectionString set
            if (string.IsNullOrWhiteSpace(ConnectionString))
            {
               error_code = 1121;
               msg = "P01 violation: ConnectionsString is not specified.";
               break;
            }

            // POST 2: Db name set
            if (string.IsNullOrWhiteSpace(Database))
            {
               error_code = 1122;
               msg = "P02 violation: the Database name is not specified.";
               break;
            }

            // POST 3: Schema set
            if (string.IsNullOrWhiteSpace(Schema))
            {
               error_code = 1123;
               msg = "P03 violation: Schema is not specified.";
               break;
            }

            // POST 4: TstNum set
            if (string.IsNullOrWhiteSpace(TstdRtnNm))
            {
               error_code = 1124;
               msg = "P04 violation: the test number is not specified.";
               break;
            }

            // POST 5: TstdRtnNm set
            if (TestType == TestType.Unknown)
            {
               error_code = 1125;
               msg = $"P05 violation: Could not determine the test type for {QualTstdRtnNm}";
               break;
            }

            // Finally:
            msg = "";
            error_code = 0;
         } while (false);

         // Postconditions:
         // Either ((ret = 0 AND msg is "") OR (ret != 0 AND error msg set))
         Postcondition((error_code != -1) && ((error_code == 0) && (msg.Length == 0)) || ((error_code != 0) && (!string.IsNullOrWhiteSpace(msg))), "error");
         return error_code;
      }

      /// <summary>
      /// POST CONDITIONS:
      /// EITHER:
      /// (
      ///  ret = true AND and the following state is set
      ///  IsInitialised and the following state is set
      ///   P04: TstRtnHlprNm
      ///   P05: TestType
      ///   P06: Description
      ///   P07: ArgList
      ///   P08: MaxArgLen
      /// )
      ///  OR ret = false
      ///  
      /// Checks the following:
      ///   TstdRtnNm, 
      /// 
      /// </summary>
      /// <param name="msg"></param>
      /// <returns></returns>
      protected int ChkInitPostConditions_2(out string msg)
      {
         int error_code = -1;

         do
         {
            // P04: TstRtnHlprNm
            if (string.IsNullOrWhiteSpace(TstdRtnNm))
            {
               msg = "The main test name was not determined.";
               error_code = 1123;
               break;
            }

            // P05: TestType
            if (TestType == TestType.Unknown)
            {
               msg = "The helper TestType was not determined.";
               error_code = 1124;
               break;
            }

            // P06: Description
            if (string.IsNullOrWhiteSpace(TstHlprDesc))
            {
               msg = "The routine description was not found";
               error_code = 1125;
               break;
            }

            // P07: ArgList
            if (string.IsNullOrWhiteSpace(TstHlprDesc))
            {
               msg = "The test description was not created";
               error_code = 1126;
               break;
            }

            // P08: MaxArgLen
            // MaxArgLen: if args then must be > 0
            if (ParamMap.Count() > 0 && (MaxArgLen <= 0))
            {
               msg = "Max arg len not calculated";
               error_code = 1127;
               break;
            }

            // Finally:
            msg = "";
            error_code = 0;
         } while (false);

         // Postconditions:
         // Either ((ret = 0 AND msg is "") OR (ret != 0 AND error msg set))
         Postcondition((error_code != -1) && ((error_code == 0) && (msg.Length == 0)) || ((error_code != 0) && (!string.IsNullOrWhiteSpace(msg))), "error");
         return error_code;
      }

      /// <summary>
      /// POSTCONDITIONS:
      /// Either:
      /// (
      ///   P09: TstdRtnDesc
      ///   P10: MaxArgLen: if args or expected values then MaxArgLen must be > 0
      ///   P11: primary key found
      ///   P12: DbName must be specified
      ///   P13: IsParamMapPopulated must be set
      ///   P14: IsRtnOutputColInfoListPopulated must be set
      ///   AND ret = 0 AND msg = ""
      ///  )
      ///  OR ((ret != 0) AND (error msg set))
      ///  
      /// Checks the following:
      ///   ViewNm, TestType, PK_List, DbName, IsParamMapPopulated, IsRtnOutputColInfoListPopulated
      ///   
      /// </summary>
      /// <param name="msg"></param>
      /// <returns></returns>
      protected int ChkInitPostConditions_3(out string msg)
      {
         int error_code = -1;

         do
         { 
            // P09: TstdRtnDesc
            if (((TestType == TestType.Create) || (TestType == TestType.Update)) && string.IsNullOrWhiteSpace(View) )
            {
               msg = "Must specify a view for update or create tests";
               error_code = 1128;
               break;
            }

            // P10: MaxArgLen: if args then must be > 0
            if(((ParamMap.Count()>0) || (ExpectedParameterMap.Count() > 0)) && (MaxArgLen==0))
            {
               msg = "MaxArgLen not calculated";
               error_code = 1129;
               break;
            }

            //   P11: primary key set
            //if ((PK_List?.Count() ?? 0) == 0)
            //{
            //   msg = "Primary key not set";
            //   error_code = 1129;
            //   break;
            //}

            //   P12: DbName must be specified
            if (string.IsNullOrWhiteSpace(Database))
            {
               msg = "Database not specified";
               error_code = 1130;
               break;
            }

            //   P13: IsParamMapPopulated must be set
            if (IsParamMapPopulated == false)
            {
               msg = "ParamMap population failed";
               error_code = 1131;
               break;
            }

            //   P14: IsRtnOutputColInfoListPopulated must be set
            if (IsRtnReturnedColListPopulated == false)
            {
               msg = "RtnOutputColInfoListP population failed";
               error_code = 1132;
               break;
            }

            // Finally:
            msg = "";
            error_code = 0;
         } while(false);

         // Postconditions:
         // Either ((ret = 0 AND msg is "") OR (ret != 0 AND error msg set))
         Postcondition((error_code != -1) && ((error_code == 0) && (msg.Length==0)) || ((error_code != 0) && (!string.IsNullOrWhiteSpace(msg )) ), "error");
         return error_code;
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="msg"></param>
      /// <returns></returns>
      protected int ChkInitPostConditions_4(out string msg)
      {
         int error_code = -1;

         do
         {
            // Assert postconditions
            //   POST 1: ExpectedParameterMap pop and IsExpectedParameterMapPopulated set
            if(IsExpectedParameterMapPopulated == false)
            {
               error_code = 2001;
               msg = $"E{error_code}: POSTCONDITION 1 violation: ExpectedParameterMap pop and IsExpectedParameterMapPopulated set";
               break;
            }

            //   POST 2: MaxArgLen set
            if(MaxArgLen == -1)
            {
               error_code = 2002;
               msg = $"E{error_code}: POSTCONDITION 1 violation: ExpectedParameterMap pop and IsExpectedParameterMapPopulated set";
               break;
            }

            //   POST 3: HlprScrptFile
            if(string.IsNullOrWhiteSpace(HlprScrptFile))
            {
               msg = $"E{2003}: POSTCONDITION POSTCONDITION 3 violation: the helper script file path is not set";
               break;
            }

            //   POST 4: MnScrptFile
            if (string.IsNullOrWhiteSpace(MnScrptFile))
            {
               msg = $"E{2004}: POSTCONDITION 4 violation: the main script file path is not set";
               break;
            }

            //   POST 5: CurrentScriptFile
            if (string.IsNullOrWhiteSpace(CurrentScriptFile))
            {
               msg = $"E{2005}: POSTCONDITION 5 violation: the current script f file path is not set";
               break;
            }

            //   POST 6: script wrier open
            if(!IsScriptWriterIsOpen())
            {
               msg = "E{2001}: POSTCONDITION 6 violation: the script writer is not open";
               break;
            }

            // finally: OK
            error_code = 0;
            msg = "";
         } while(false);

         return error_code;
      }

      /// from there look for a line like:
      /// "-- DESIGN:\r\n"
      /// "-- TESTS:\r\n"
      /// --.*=====*
      /// "^CREATE
      protected bool ChkLineForDescEnd(string line)
      {
         bool ret = true;

         do
         {
            if (ChkLineForMatch(line, @"^\s*--\s*====="))
               break;

            if (ChkLineForMatch(line, @"^\s*create\s+procedure"))
               break;

            if (ChkLineForMatch(line, @"^\s*create\s+function"))
               break;

            // If here then n match
            ret = false;
         } while (false);

         return ret;
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="line"></param>
      /// <param name="condition"></param>
      /// <returns></returns>
      protected bool ChkLineForMatch(string line, string condition)
      {
         Match myMatch = Regex.Match(line.ToLower(), condition);
         return myMatch.Success;
      }

      /// <summary>
      /// sp_cv_Get1 -> helper_123_sp_cv_Get1
      /// 
      /// PRECONDITIONS:
      ///   testedRtn and testNum have been validated
      /// </summary>
      /// <param name="testedRtn"></param>
      /// <param name="testNum"></param>
      /// <returns></returns>
      protected string CrtHlprTstNm(string testedRtn, int testNum)
      {
         var rtnNm = $"helper_{testNum:D3}_{testedRtn}";
         return rtnNm;
      }

      /// <summary>
      /// sp_cv_Get1 -> test_123_sp_cv_Get1
      ///   testedRtn and testNum have been validated
      /// </summary>
      /// <param name="testedRtn"></param>
      /// <param name="testNum"></param>
      /// <returns></returns>
      protected string CrtMnTestName(string testedRtn, int testNum)
      {
         return $"test_{testNum:D3}_{testedRtn}";
      }


      /// <summary>
      /// Like: test helper for [tstdrtn] tests
      /// </summary>
      /// <returns></returns>
      protected string CrtTstHlpDesc()
      {
         Assertion(!string.IsNullOrWhiteSpace(TstdRtnNm));
         return $@"test helper for {TstdRtnNm} tests";
      }


      /// <summary>
      /// 3: Determine the template type (create, update, get1, getall, update, delete, pop)
      /// 
      /// Population routine name format: sp_pop_|tblnm|
      /// 
      /// All decides a type - either 1 of (create, update, get1, getall, update, delete, pop) or default
      /// 
      /// 
      /// PRECONDITION: tables list populated
      /// POSTCONDTION TestType.Unkown type is NOT returned
      /// </summary>
      /// <param name="rtnName"></param>
      /// <returns></returns>
      protected TestType DetermineTestType(string rtnName)
      {
         PreconditionNotNull(TableList, "DetermineTestTemplateType() precondition: TableList must be populated");
         rtnName = rtnName.ToLower();
         var parts = rtnName.Split('_', StringSplitOptions.RemoveEmptyEntries);
         bool isPopTable=false;
         
         if( rtnName.Contains("_pop_"))
         {
            // expect remainder to be an existing table nm
            if( parts.Length > 2 )
            {
               if (TableList.Find(x=>x.Equals(parts[2], StringComparison.OrdinalIgnoreCase))!= null)
                  isPopTable = true;
            }
         }

         var testType =
            rtnName.Substring(rtnName.Length - 6) == "create" ? TestType.Create :
            rtnName.Substring(rtnName.Length - 4) == "get1"   ? TestType.Get1   :
            rtnName.Substring(rtnName.Length - 6) == "getall" ? TestType.GetAll :
            rtnName.Substring(rtnName.Length - 6) == "update" ? TestType.Update :
            rtnName.Substring(rtnName.Length - 6) == "delete" ? TestType.Delete :
            (rtnName.Contains("_pop_") && isPopTable)         ? TestType.Pop    :
            TestType.Default;

         Postcondition(testType != TestType.Unknown, "TestType.Unkown type is NOT returned");
         return testType;
      }

      /// <summary>
      /// 4: Create the intro script bloc
      /// called by main and hlper creates
      /// </summary>
      /// <returns>0 (OK always)</returns>
      protected void Dual_CrtIntro_Bloc(string tstRtnNm)
      {
         AppendLine(string.Format(txt_bloc_pre_sets, Database, tstRtnNm));
      }
      /// <summary>
      /// 
      /// PRECONDITIONS: Init called
      /// </summary>
      /// <returns></returns>
      protected void Dual_Crt_WrapUp()
      {
         AppendLineDebug("-- @M1020 S: CRT_WrapUp");
         AppendLine(@$"END

/*
-----------------------------------------------------------
EXEC ut.dbo.sp_set_debug_mode 1
EXEC tSQLt.Run 'test.{TstRtnMnNm}'
-----------------------------------------------------------
EXEC test.sp_run_all_tests
EXEC test.{TstRtnHlprNm}
-----------------------------------------------------------
*/
GO
");
         AppendLineDebug("-- @M1020 E: CRT_WrapUp");
      }

      /// <summary>
      /// DESC: creates the PK_list
      /// Normally the PK is taken from the table,
      /// how ever if the table is a temporaty table then 
      /// PRECONDITIONS: table known
      /// </summary>
      /// <exception cref="NotImplementedException"></exception>
      private void CreatePK_List()
      {
         Precondition(!string.IsNullOrEmpty(Table));

         if(Table == "#tmptbl")
            PopPKListFromRtnReturnedColList();
         else 
            PK_List = GetPK_ListFromDb();
      }

      /// <summary>
      /// PRECONDITION: table and schema nms set
      /// </summary>
      protected List<KeyInfo> GetPK_ListFromDb()
      {
         var msg = " GetPK_ListFromDb(): {0} must be specified";
         Precondition(!string.IsNullOrWhiteSpace(Schema), string.Format(msg, "Schema"));//"GetPK_ListFromDb(): Schema must be specified");
         Precondition(!string.IsNullOrWhiteSpace(Table) , string.Format(msg, "Table" ));//"GetPK_ListFromDb(): Table must be specified");
         List<KeyInfo> pkList = new List<KeyInfo>();

         using (IDbConnection conn = new SqlConnection(@"Data Source=DevI9\SqlExpress;Initial Catalog=Telepat;Integrated Security=True"))
         {
            var values = new { schema_nm = Schema, table_nm = Table };

            pkList = conn.Query<KeyInfo>("SELECT * FROM fnGetPK_Fields(@schema_nm, @table_nm)",
               values, commandType: CommandType.Text).ToList();
         }

         return pkList;
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="qual_rtn_nm"></param>
      /// <returns></returns>
      protected List<RtnDetail> GetRoutineDetailsFromDb(string schema_nm, string tstd_rtn_nm)
      {
         Precondition(!(string.IsNullOrWhiteSpace(schema_nm) || string.IsNullOrWhiteSpace(tstd_rtn_nm)));
         List<RtnDetail> items;

         using (IDbConnection conn = new SqlConnection(@"Data Source=DevI9\SqlExpress;Initial Catalog=Telepat;Integrated Security=True"))
         {
            items = conn.Query<RtnDetail>("SELECT * FROM dbo.fnGetRtnDetails(@schema_nm, @rtn_nm)", new { @schema_nm = schema_nm, @rtn_nm = tstd_rtn_nm }, commandType: CommandType.Text).ToList();
         }

         return items;
      }

      /// <summary>
      /// Gets the list of columns for a table from the database
      /// </summary>
      /// <returns></returns>
      protected List<ColInfo>? GetTableColsFrmDb(string schema_nm, string table_nm)
      {
         Precondition(!(string.IsNullOrWhiteSpace(Schema) || string.IsNullOrWhiteSpace(Table)));
         List<ColInfo> cols;

         //  using (IDbConnection conn = new SqlConnection(ConnectionString))
         using (IDbConnection conn = new SqlConnection(@"Data Source=DevI9\SqlExpress;Initial Catalog=Telepat;Integrated Security=True"))
         {
            cols = conn.Query<ColInfo>("SELECT * FROM dbo.fnGetTableCols(@schema_nm, @table_nm)", new { @schema_nm = schema_nm, table_nm = table_nm }, commandType: CommandType.Text).ToList();
         }

         return cols;
      }


      /// <summary>
      /// DESC: returns the param map for the given routine
      /// 
      /// METHOD:
      /// 1: Get the rtn parameters from the database
      /// 2: trim the leading @
      /// 
      /// <param name="rtnName">[qualifed] rtn name - does not have to be qualified 
      ///  if the name is unique accross schemas</param>
      ///  
      /// returns the param map for the given routine
      /// 
      /// POSTCONDITIONS: parmeter info populated correctly
      /// </summary
      protected Dictionary<string, ParamInfo>? GetRtnParamsFromDb(string? schema_nm, string rtn_nm)
      {
         PreconditionNotNull(Schema);
         Dictionary<string, ParamInfo> paramMap = new Dictionary<string, ParamInfo>();

         using (IDbConnection conn =
            new SqlConnection(@"Data Source=DevI9\SqlExpress;Initial Catalog=Telepat;Integrated Security=True"))
         {
            var values = new { schema_nm = schema_nm, rtn_nm = rtn_nm };
            var param_list = conn.Query<ParamInfo>("select * from fnGetRtnParams(@schema_nm, @rtn_nm)", values, commandType: CommandType.Text).ToList();

            foreach (var p in param_list)
            {
               AssertNotNull(p);
               AssertNotNull(p.ordinal);
               AssertNotNull(p.col_nm);
               AssertNotNull(p.is_chr_ty);
               AssertNotNull(p.is_nullable);
               AssertNotNull(p.is_output);
               AssertNotNull(p.is_pk);
               AssertNotNull(p.ordinal);
               AssertNotNull(p.rtn_nm);
               AssertNotNull(p.rtn_ty);
               AssertNotNull(p.schema_nm);

               Assertion(p.tbl_nm == null);

               AssertNotNull(p.ty_id);
               AssertNotNull(p.ty_len);
               AssertNotNull(p.ty_nm);
               p.col_nm = (p.col_nm ?? "").TrimStart('@');
               paramMap.Add(p.col_nm, p);
            }
         }

         return paramMap;
      }


      /// <summary>
      /// Gets the column (and table) names for the given view
      /// </summary>
      /// <param name="schema_nm"></param>
      /// <param name="view_nm"></param>
      /// <returns></returns>
      protected List<ViewColInfo> GetViewColsFromDb(string schema_nm, string view_nm)
      {
         List <ViewColInfo> list;

         using (IDbConnection conn =
            new SqlConnection(@"Data Source=DevI9\SqlExpress;Initial Catalog=Telepat;Integrated Security=True"))
         {
            var values = new { schema_nm = schema_nm, vw_nm = view_nm };
            list = conn.Query<ViewColInfo>("select * from fnGetViewCols(@schema_nm, @vw_nm)", values, commandType: CommandType.Text).ToList();
         }

         return list;
      }

      protected string? GetRtnDesc(string rtnName)
      {
         List<string>? lines = null;

         using (IDbConnection conn = new SqlConnection(@"Data Source=DevI9\SqlExpress;Initial Catalog=Telepat;Integrated Security=True"))
         {
            var values = new { objname = rtnName };
            lines = conn.Query<string>("sp_helptext", values, commandType: CommandType.StoredProcedure).ToList();
         }

         int stLine = 0;
         // Look for ^-- Description
         foreach (var line in lines)
         {
            if (ChkLineForMatch(line, @"^\s*--\s*description"))
               break;

            stLine++;
         }

         // 220129: May be no description?
         if(stLine >= lines.Count)
            return null;

         lines.RemoveRange(0, stLine);
         var s = lines[0];
         // remove the --\s*description
         s = Regex.Replace(s, @"^\s*--\s*[D,d]escription:\s*", "");
         // trim the end whitespace
         // s = s.TrimEnd();
         lines[0] = s;

         int endLine = 0;
         int cnt = lines.Count();

         foreach (var line in lines)
         {
            if (ChkLineForDescEnd(line))
               break;

            endLine++;
         }

         // backup endLine 1 line
         // endLine--;
         lines.RemoveRange(endLine, lines.Count() - endLine);

         // get the  text after it
         var sql = string.Join(@"", lines);
         return sql.Trim();
      }

      /// <summary>
      /// 
      /// </summary>
      /// <returns></returns>
      protected List<string>? GetUntestedRtns()
      {
         List<string>? unTestedRoutinesList = null;

         using (IDbConnection conn = new SqlConnection(@"Data Source=DevI9\SqlExpress;Initial Catalog=Telepat;Integrated Security=True"))
         {
            unTestedRoutinesList = conn.Query<string>("test.sp_get_untested_rtns", commandType: CommandType.StoredProcedure).ToList();
         }

         return unTestedRoutinesList;
      }

      /// <summary>
      ///  resp: all init
      ///  
      /// Method:
      /// 0: Validate inputs
      /// 1: Get the rtn details from the Db
      /// 2: Get the parameter details from the Db
      /// 3: Determine the test helper rtn name and store
      /// 4: Determine the template type
      /// 5: Check post conditions
      /// 
      /// POST CONDITIONS:
      ///  IsInitialised and the following state is set
      ///   P01: Schema
      ///   P02: TstdRtnNm 
      ///   P03: TstRtnMnNm
      ///   P04: TstRtnHlprNm
      ///   P05: TemplateType
      ///   P06: Description
      ///   P07: ArgList
      ///   P08: MaxArgLen
      ///   P09: TstdRtnDesc
      ///   P10: MaxArgLen: if args then must be > 0
      ///   P11: primary key found
      ///   P12: DbName must be specified
      ///   
      ///   OR IsInitialised = false
      /// </summary>
      /// <param name="qualTestedRtn">schema.testedRtnName</param>
      /// <param name="testNum"></param>
      /// <param name="description"></param>
      /// <param name="error_msg"></param>
      /// <returns></returns>
      public int Init
      (
          string     qTstdRtnNm
         ,int        tstNum
         ,string     conn_str
         ,out string msg
         ,string?    table = null
         ,string?    view  = null
      )
      {
         int error_code = -1;
         IsInitialised = false;

         try
         {
            do 
            {
               // Sets: ConnectionsString, DbName, Schema, TstNum, TstdRtnNm, Table, ViewNm
               error_code = Init1( qTstdRtnNm, tstNum, table, view, out msg, conn_str);

               if (error_code != 0)
                  break;

               // Sets TableList, Table, RtnDetailInfoList, ParamInfoList
               error_code = Init2(out msg);

               if (error_code != 0)
                  break;

               // Sets: TestType, RtnRetType, TstRtnMnNm, TstRtnHlprNm, TstdRtnDesc
               // , TstHlprDesc, ParamMap, IsParamMapPopulated, IsOutArgsMapPopulated
               error_code = Init3(out msg);

               if (error_code != 0)
                  break;

               // Sets: 1: ExpectedParameterMap, MaxArgLen, HlprScrptFile, MnScrptFile, CurrentScriptFile, script writer open
               if(!Init4(out msg)) // no checks so no return value
                  break;

               error_code = ChkInitPostConditions(out msg);
            } while(false);
         }
         catch (Exception e)
         {
            error_code = 7254;
            msg = $"E{error_code}: caught exception initialising SQL Test creator: {e.Message}";
         }

         return error_code;
      }

      /// <summary>
      /// Validates the imputs: qTstdRtnNm, int tstNum, string? viewNm, string conn_str
      /// Sets: ConnectionsString, DbName, Schema, TstNum
      ///       , TstdRtnNm, TableList, Table, ViewNm
      ///   
      /// POSTCONDITIONS
      ///   POST  1: ConnectionString set
      ///   POST  2: Server set
      ///   POST  3: Database set
      ///   POST  4: Schema set
      ///   POST  5: TstNum set
      ///   POST  6: TstdRtnNm set
      ///   POST  7: test type is set and not unkown
      ///   POST  8: either (table is known and PK_List populated) or both (table and PK are null)
      ///   POST  9: if the test type is Crt, Update, Get1, Get all, Delete then PK is needed, if no state is changed (fn or sp not updating) then do not need PK
      ///   POST 10: RtnOutputColInfoList pop
      ///   Post 11: either (table and view) are set or both (table and view) are NOT set
      ///   Post 12: TableList pop
      ///   Post 13: ViewList pop
      ///   
      /// </summary>
      /// <param name="qTstdRtnNm"></param>
      /// <param name="tstNum"></param>
      /// <param name="viewNm"></param>
      /// <param name="conn_str"></param>
      /// <param name="msg"></param>
      /// <returns></returns>
      private int Init1(string qTstdRtnNm,
         int         tstNum,
         string?     table,
         string?     view,
         out string  msg,
         string?     conn_str = null)
      {
         //msg = "";
         // 0: Validate inputs
         int error_code = Validate(qTstdRtnNm, tstNum, out msg);

         if (error_code != 0)
            return error_code;

         // This is a set up chk: DefaultConnectionString getter Will throw if DefaultConnectionString is not set
         //var default_conn_str = DefaultConnectionString;

         SetConnectionStringProperties(conn_str);

         var bits = qTstdRtnNm.Split(new[] { '.' });

         if (bits.Length != 2)
         {
            error_code = 1101;
            msg = $"E{error_code} the tested routine name should be qualified like <schema>.<rtn>";
            return error_code;
         }

         Schema    = bits[0];
         TstdRtnNm = bits[1];

         TstNum    = tstNum;
         TstdRtnType = GetRtnType(TstdRtnNm);
         TableList = GetTableListFromDb();
         ViewList  = GetViewListFromDb();

         // 4: Determine the template type
         TestType = DetermineTestType(TstdRtnNm);

         // 3: RtnOutputColInfoList: Get the output table details from the Db 
         error_code = PopRtnOutputColInfoList(Schema, TstdRtnNm, out msg);

         if (error_code != 0)
            return error_code;

         RtnRetType = DetermineRtnRetType();
         SetTableAndView( table, view);

         //-----------------------------------------------------------------------------------------------------------------
         // Chk post conditions
         //-----------------------------------------------------------------------------------------------------------------

         // POST 1: ConnectionsString set
         Postcondition(!string.IsNullOrWhiteSpace(ConnectionString), "E3120: connection string is not specified");
         // POST 2: Db name set
         Postcondition(!string.IsNullOrWhiteSpace(Database), "E3121: Db name is not set");
         // POST 3: Server set
         Postcondition(!string.IsNullOrWhiteSpace(Server), "E3122: Db name is not set");
         // POST 4: Schema set
         Postcondition(!string.IsNullOrWhiteSpace(Schema), "E3123: Schema name is not set");
         // POST 5: TstNum set
         Postcondition(TstNum>0, "E3124: TstNum is too small");
         // POST 7: test type is set and not unkown
         Postcondition(TestType != TestType.Unknown, "E3126: test type is set and not unkown");
         // POST 8: either (table is known and PK_List populated) or (table and PK are null)
         Postcondition((((!string.IsNullOrWhiteSpace(Table)) && (PK_List != null) && ((PK_List?.Count ?? -1) > 0)) || ((string.IsNullOrWhiteSpace(Table)) &&(PK_List == null))), "E3128: either (table must be known and PK_List populated) or bothe (table and PK are null)");


         // POST 9: if the test type is Crt, Update, Get1, Get all, Delete then PK is needed, if no state is changed (fn or sp not updating) then do not need PK
         Postcondition(
            (
               string.IsNullOrWhiteSpace(Table) && ((TestType==TestType.Default))
            ) 
            ||
            (
               (!string.IsNullOrWhiteSpace(Table)) && ((PK_List != null) && ((PK_List?.Count ?? -1) > 0))
            )
            , "E3127: PK_List not populated");

         // POST 10: RtnOutputColInfoList pop
         Postcondition(RtnReturnedColList != null, "E3128: RtnReturnedColList is not populated");

         // Post 11: either (table and view) are set or both (table and view) are NOT set
         Postcondition( (TestType==TestType.Default)  ||
            ((!string.IsNullOrWhiteSpace(Table) && (!string.IsNullOrWhiteSpace(View))) ||
                        (string.IsNullOrWhiteSpace(Table) && string.IsNullOrWhiteSpace(View)) 
                       ), "E3129: either default test type or ((table and view) are set or both (table and view) are NOT set)");

         // Post 12: TableList pop
         Postcondition((TableList != null) && (TableList.Count > 0), "E3131: TableList should have been populated");
         // Post 13: ViewList pop
         Postcondition((TableList != null) && (TableList.Count > 0), "E3132: ViewList should have been populated");
         return error_code;
      }

      /// <summary>
      ///   Sets the following properties:
      ///      ConnectionString,
      ///      Server,
      ///      Ddatabase 
      ///   from the connection string.
      /// 
      /// PRECONDITIONS:
      ///   conn_str valid
      ///   
      /// POSTCONDITIONS:
      ///   POST 1: ConnectionString set or exception thrown
      ///   POST 2: Database found in the connection string, set and not null or exception thrown
      ///   POST 3: Server   found in the connection string, set and not null or exception thrown
      /// </summary>
      /// <param name="conn_str"></param>
      /// <param name="error_msg"></param>
      /// <returns></returns>
      protected void SetConnectionStringProperties(string? conn_str)
      {
         if(conn_str != null)
         { 
            ConnectionString = conn_str;
            int error_code = -1;
            string msg = "";
            var database_nm = "";
            var srvr_nm = "";

            string db_key = "Initial Catalog";

            do
            {
               var bits = ConnectionString.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
               Dictionary<string, string> dictionary = bits.ToDictionary(s => s.Split('=')[0], s => s.Split('=')[1]);

               if ((dictionary.Count() == 0) || (!dictionary.ContainsKey(db_key)))
               {
                  error_code = 1351;
                  msg = $"Failed to extract the {db_key} key from the connection string";
                  break;
               }

               database_nm = dictionary["Initial Catalog"];
               srvr_nm = dictionary["Data Source"];

               if (string.IsNullOrWhiteSpace(database_nm))
               {
                  error_code = 1352;
                  msg = $"The database name is not spcified properly in the connection";
                  break;
               }

               if (string.IsNullOrWhiteSpace(srvr_nm))
               {
                  error_code = 1352;
                  msg = $"The server name is not spcified properly in the connection";
                  break;
               }

               // Finally
               Database = database_nm;
               Server = srvr_nm;
               error_code = 0;
            } while (false);

            if (error_code != 0)
               msg = $"E{error_code}: {msg}";

            // POST 1: ConnectionString set or exception thrown
            Postcondition(string.IsNullOrWhiteSpace(ConnectionString ) == false, msg);
            // POST 2: Database found in the connection string, set and not null or exception thrown
            Postcondition(string.IsNullOrWhiteSpace(Database         ) == false, msg);
            // POST 3: Server   found in the connection string, set and not null or exception thrown
            Postcondition(string.IsNullOrWhiteSpace(Server           ) == false, msg);
         }
      }

      /// <summary>
      /// Sets the Table 
      /// Getting the table name from the database or rtn name if not specified as a parametere
      /// 
      /// PRECONDITIONS:
      ///   PRE 1: schema pop
      ///   
      /// POSTCONDITIONS:
      /// POST 1: if table is known then the view must be set
      /// POST 2: if table is known then the table column information must be known
      /// 
      /// </summary>
      /// <param name="table"></param>
      /// <exception cref="Exception"></exception>
      private void SetTableAndView(string? table, string? view)
      {
         // PRECONDITIONS:
         PreconditionNotNull(Schema);

         // returns 0 if OK, 1 if the tested rtn is not a stored proc
         bool ret = GetTableAndView(ref table, ref view, out int error_code, out var msg);
         
         if(ret == true)
         {
            if(!string.IsNullOrWhiteSpace(table))
            {
               Table = table;
               CreatePK_List();
               TableCols = GetTableColsFrmDb(Schema, table);
            }

            if (view == null)
               view = table;

            if (!string.IsNullOrWhiteSpace(view))
               View = view;
         }
         else
         {
            //throw new Exception($"E{error_code} there was an error setting the  table, error: {msg}");
         }

         if (NeedsTable(TestType) && Table == null)
         {
            error_code = 1103;
            msg = "the table must be specified for this type of test";
            throw new Exception($"E{error_code}: {msg}");
         }

         // Chk postconditions
         Postcondition((table == null) || (!string.IsNullOrWhiteSpace(view)), "E6661: Post condition 1: if table is known then the view must be set");
         Postcondition((table == null) || ((TableCols != null) && (TableCols.Count>0)), "E6662: Post condition 2: if table is known then the table column information must be known");
      }

      /// <summary>
      /// Uses convention to set the View nm from teh table nm
      /// The required view should be [table_nm}Vw
      /// </summary>
      /// <param name="view"></param>
      protected string? GetView(string? view)
      {
         if (view == null)
         {
            // Get the view from the table name if known
            if(Table != null)
            {
               view = $"{Table}Vw";
            }
         }
         else 
         {
            // may need to remove schema qualifier
            if(view.Contains('.'))
               view = view.Substring(view.IndexOf('.')+1);
         }

         if (view != null)
         {
            // check the view exists
            // precondition View List pop
            PreconditionNotNull(ViewList, "E1235: view list should have been populated");

            if (!ViewList.Contains(view))
               view = null;
         }

         return view;
      }

      /// <summary>
      /// Sets RtnDetailInfoList, ParamInfoList, TstdRtnType
      /// </summary>
      /// <param name="table"></param>
      /// <param name="error_msg"></param>
      /// <returns></returns>
      private int Init2(out string error_msg)
      {
         PreconditionNotNull(Schema);
         // 1: Get the rtn details from the Db
         int error_code = GetRtnDetailsFromDb(Schema, TstdRtnNm, out List<RtnDetail> rtnDetailInfoList, out error_msg);

         if (error_code != 0)
            return error_code;

         RtnDetailInfoList = rtnDetailInfoList;

         // 2: Get the parameter details from the Db
         error_code = GetPrmDetailsFromDb(Schema, TstdRtnNm, out List<ParamInfo> paramInfoList, out error_msg);
         ParamInfoList = paramInfoList;

         Postcondition(
            ((TstdRtnType != null) && (error_msg.Length == 0)) ||
            ((TstdRtnType == null) && (error_msg.Length > 0)),
            "E5010: ParamMap is not Populated ");

         return error_code;
      }

      /// <summary>
      /// Sets: TestType, RtnRetType, TstRtnMnNm, TstRtnHlprNm, 
      ///       TstdRtnDesc, TstHlprDesc, ParamMap
      ///
      /// Post condition chks
      ///   Post 1:(((IsParamMapPopulated   == true) &&(error_code == 0) &&(msg.Length ==0)) || ((error_code > 0) && (msg.Length> 0)), "E5010: ParamMap is not Populated ");
      ///   Post 2:(((IsOutArgsMapPopulated == true) &&(error_code == 0) &&(msg.Length ==0)) || ((error_code > 0) && (msg.Length> 0)), "E5010: OutArgsMap is not Populated";
      /// </summary>
      /// <param name="msg"></param>
      /// <returns></returns>
      private int Init3(out string msg)
      {
         int error_code = 0;
         PreconditionNotNull(Schema);

         do
         {
            IsParamMapPopulated = false;
            IsOutArgsMapPopulated = false;
            // 3: Get the output table details from the Db
            //error_code = PopRtnOutputColInfoList(Schema, TstdRtnNm, out msg);
            //
            //if (error_code != 0)
            //   break;


            //error_code = DetermineRtnRetType(out var _rtnRetType, out msg);
            //
            //if (error_code != 0)
            //   break;
            //
            //RtnRetType = _rtnRetType;

            // Dont always need a pk list (which table) - crt and update will

            // 3: Determine the test helper rtn name and store
            TstRtnMnNm  = CrtMnTestName(TstdRtnNm, TstNum);
            TstRtnHlprNm= CrtHlprTstNm(TstdRtnNm, TstNum);
            TstdRtnDesc = GetRtnDesc(QualTstdRtnNm);
            TstHlprDesc = CrtTstHlpDesc();
            ParamMap    = GetRtnParamsFromDb(Schema, TstdRtnNm) ?? new Dictionary<string, ParamInfo>();
            IsParamMapPopulated = true;

            error_code = GetOutArgsFromDb(out Dictionary<string, ParamInfo> outArgs, out msg);

            if (error_code != 0)
               break;

            OutArgsMap = outArgs;
            IsOutArgsMapPopulated = true;
            error_code = 0;
         } while (false);

         // Post condition chks
         Postcondition(((IsParamMapPopulated   == true) && (error_code == 0) && (msg.Length == 0)) || ((error_code > 0) && (msg.Length > 0)), "E5010: ParamMap is not Populated ");
         Postcondition(((IsOutArgsMapPopulated == true) && (error_code == 0) && (msg.Length == 0)) || ((error_code > 0) && (msg.Length > 0)), "E5011: OutArgsMap is not Populated");
         return error_code;
      }

      /// <summary>
      /// Sets the following:
      ///   1: ExpectedParameterMap
      ///   2: MaxArgLen
      ///   3: HlprScrptFile
      ///   4: MnScrptFile
      ///   5: CurrentScriptFile
      ///   6: script writer is open
      /// PRECONDITIONS: ParamMap pop, PKList pop
      /// 
      /// POST CONDITIONS: 
      ///   POST 1: ExpectedParameterMap pop and IsExpectedParameterMapPopulated set
      ///   POST 2: MaxArgLen set
      ///   POST 3: HlprScrptFile
      ///   POST 4: MnScrptFile
      ///   POST 5: CurrentScriptFile
      ///   POST 6: script wrier open
      /// </summary>
      private bool Init4(out string msg)
      {
         bool ret = false;
         // PRECONDITIONS: ParamMap pop, PKList pop
         Precondition(IsParamMapPopulated == true, "ParamMap not Populated");

         do
         {
            IsExpectedParameterMapPopulated = false; // initally
            MaxArgLen = -1;

            ExpectedParameterMap = GetExpParameters();
            //   POST 2: ExpectedParameterMap pop and IsExpectedParameterMapPopulated set
            IsExpectedParameterMapPopulated = true;
            MaxArgLen = 0;

            // Get the  max len of args
            foreach (var pr in ParamMap)
            {
               var param = pr.Value;

               if ((param?.col_nm?.Length ?? 0) > MaxArgLen)
                  MaxArgLen = param?.col_nm?.Length ?? MaxArgLen;
            }

            foreach (var pr in ExpectedParameterMap)
            {
               var param = pr.Value;

               if ((param?.col_nm?.Length ?? 0) > MaxArgLen)
                  MaxArgLen = param?.col_nm?.Length ?? MaxArgLen;
            }

            // Allow for @<prefix>_  5 chrs
            MaxArgLen += 5;

            // Default file = helpr ??
            HlprScrptFile = Path.GetFullPath($"{Database}.{Schema}.{TstdRtnNm}.{GetTimestamp(true)}_hlpr.sql");
            MnScrptFile = Path.GetFullPath($"{Database}.{Schema}.{TstdRtnNm}.{GetTimestamp(true)}_test.sql");
            CurrentScriptFile = HlprScrptFile;

            if(!OpenScriptWriter(CurrentScriptFile, out msg))
               break;

            ret = true;
         } while(false);

         // Assert postconditions
         //   POST 1: ExpectedParameterMap pop and IsExpectedParameterMapPopulated set
         Postcondition(IsExpectedParameterMapPopulated == true, "POSTCONDITION 1 violation: ExpectedParameterMap pop and IsExpectedParameterMapPopulated set");
         //   POST 2: MaxArgLen set
         Postcondition(MaxArgLen > -1, "POSTCONDITION 2 violation: ExpectedParameterMap pop and IsExpectedParameterMapPopulated set");
         //   POST 3: HlprScrptFile
         Postcondition(!string.IsNullOrWhiteSpace(HlprScrptFile), "POSTCONDITION 3 violation: HlprScrptFile not set");
         //   POST 4: MnScrptFile
         Postcondition(!string.IsNullOrWhiteSpace(MnScrptFile), "POSTCONDITION 4 violation: MnScrptFile not set");
         //   POST 5: CurrentScriptFile
         Postcondition(!string.IsNullOrWhiteSpace(CurrentScriptFile), "POSTCONDITION 5 violation: MnScrptFile not set");
         //   POST 6: script wrier open
         Postcondition(IsScriptWriterIsOpen(), "POSTCONDITION 5 violation: the script writer is not open");
         return ret;
      }

      /// <summary>
      /// Gets the table - or null
      /// if the table or view are specified on entry then they are checked for existence, and if exist leave as is, else set to null if not exist
      /// If the view cannot be found but the table is known then the view should be the tble
      /// </summary>
      /// <param name="table"></param>
      /// <param name="error_code"></param>
      /// <param name="error_msg"></param>
      /// <returns></returns>
      protected bool GetTableAndView(ref string? table, ref string? view, out int error_code, out string error_msg)
      {
         error_code = -1;
         error_msg  = "";
         PreconditionNotNull(TableList, "Table list should be populated at this stage");

         do
         {
            // If the table is not specified - deduce it from the name and the table dependencies
            if(string.IsNullOrWhiteSpace(table))
            {
               error_code = GetTableAndViewFromRtnDependencies( TstdRtnNm, out table, ref view, out error_msg);

               // Table not found in rtn dependencies - for example:
               // Candidate delete uses person table and cascade delete not the Candidate table
               // in which case try to deduce it from the name
               switch(error_code)
               {
                  case 0: // found table
                     break;

                  case 1:
                     throw new NotImplementedException("caase 1");
                  //break;

                  case 2:
                     if(!GetTblNmFrmRtnNm(TstdRtnNm, out table))
                     {
                        // doesnt matter if we dont need the table
                        if(NeedsTable(TestType))
                        { 
                           error_code = 1246;
                           error_msg = $"Failed to find the table for the tested routine: {TstdRtnNm} ";
                           return false;
                        }
                     }

                     break; // ??

                  case 1246: // msg: "Failed to find the table for the tested routine: fnGetProcUpdateCols "
                     // if this is a default test type we need a tmp table
                     if(TestType==TestType.Default)
                     {
                        table = null;//"#tmptbl";
                        view = "#tmptbl";
                        error_code = 0;
                        error_msg = "";
                        break;
                     }
                     else
                     {
                        table = null;
                        return false;
                     }
               }
            }

            if (table == null)
            { 
               if (TestType != TestType.Default)
               {
                  error_code = 1;
                  error_msg = $"W{error_code} table not found for the tested rtn {TstdRtnNm}";
                  //view = null;
                  return false;
               }
            }

            if (table != null)
            {
               // Assertion: table not null

               // Check deduced table name and get the case right at the same time
               var _tableNm = table;

               if (table != "#tmptbl")
                  table = TableList.Find(x => x.Equals(_tableNm, StringComparison.OrdinalIgnoreCase));

               if (table == null)
               {
                  error_code = 1103;
                  error_msg = $"E{error_code} table not found in database";
                  view = null;
                  return false;
               }

               // Assertion: if here then table is known and exists in the db
               if (view == null)
               {
                  // get from naming convention based on table
                  view = $"{table}Vw";
                  var _view = view;

                  // check the view exists
                  AssertNotNull(ViewList);
                  view = ViewList.Find(x => x.Equals(_view, StringComparison.OrdinalIgnoreCase));

                  // if the standard named view {table}Vw does not exist so use the table
                  // which we know exists
                  if (view == null)
                     view = table;
               }
            }

            error_msg  = "";
            error_code = 0;
         } while (false);

         return error_code==0;
      }

      /// <summary>
      /// 1: table: tries to set the table from the SQL routine dependencies
      /// 2: viwe:  if the view is not specified tries to set the view from the SQL routine dependencies
      /// if the view
      /// Chases the SQL dependency tree for the table and or view
      /// Expects sp named like sp_candidate_delete : sp_[tablename]_[crt or update, get, del]
      /// PRECONDITIONS: 
      ///   PRE 1: only works for stored procedures
      ///   PRE 2: TstdRtnType set
      /// 
      /// METHOD:
      ///   Check type is a stored procedure
      ///   Check test type  is a crt, update, get, del
      ///   Get the candidate table name from the rtn dependencies (recursively)
      /// </summary>
      /// <param name="tstdRtnNm"></param>
      /// <returns>
      ///   0 if found exact match and table param = table nm
      ///   1 if rtn is not a strored proc
      ///   2 if table not found in the rtn dependencies
      /// </returns>
      protected int GetTableAndViewFromRtnDependencies(string tstdRtnNm, out string? table, ref string? view, out string msg)
      {
         PreconditionNotNull(TstdRtnType, "TstdRtnType not set");
         PreconditionNotNull(TableList  , "TableList not set");
         Precondition(RtnRetType != RtnRetType.Unknown, "Routine return type is not known");

         int error_code = -1;
         bool tbl_found = false;
         table = null;

         do
         {
            // PRE 1: only works for stored procedures
            if(( TstdRtnType != RtnType.Sp) && 
               (TstdRtnType != RtnType.ScalarFn) && 
               (TstdRtnType != RtnType.TableValuedFn))
            {
               error_code = 1;
               msg = "tested routine is not a stored procedure";
               break;
            }

            if(!GetTblNmFrmRtnNm(tstdRtnNm, out var exp_table_nm))
            {
               if(!(IsFn() && (RtnRetType==RtnRetType.FnScalar) && (TestType==TestType.Default)))
               { 
                  error_code = 1246;
                  msg = $"Failed to find the table for the tested routine: {TstdRtnNm} ";
                  break;
               }
            }

            error_code = GetRtnDependencies(tstdRtnNm, out List<DependencyInfo> dependencies, out msg);

            if (error_code != 0)
               break;

            List<DependencyInfo> candidateTables = new List<DependencyInfo>();
            List<DependencyInfo> candidateViews  = new List<DependencyInfo>();

            foreach (DependencyInfo tv in dependencies)
            {
               if(tv.referenced_type_nm == "USER_TABLE")
               {
                  candidateTables.Add(tv);
                  AssertNotNull(tv.referenced_name, "????");

                  // exact match?
                  if((tv.referenced_name).Equals(exp_table_nm, StringComparison.OrdinalIgnoreCase))
                  {
                     table = tv.referenced_name;
                     tbl_found = true;
                     //break;        
                  }

                  continue;
               }

               if (tv.referenced_type_nm == "VIEW")
               {
                  candidateViews.Add(tv);
                  continue;
               }
            }

            // If we can find the table then we can look for the View by naming convention viw=<tbl>Vw
            if ((view ==null) && (tbl_found == true))
            {
               var expVwNm = $"{table}Vw";
               var dep = candidateViews.Find(x=>(x?.referenced_name ?? "").Equals(expVwNm, StringComparison.OrdinalIgnoreCase));

               if(dep!= null)
                  view = dep.referenced_name;

               error_code = 0;
               msg = "";
               //break;
            }

            // ASSERTION We have the candidateTables and candidateViews, and maybe the table and view 
            var n = candidateTables.Count;

            if(table == null)
            {
               error_code = 2;
               msg = "table not found from rtn dependencies";
               break;
            }

            // finally
            if (!TableList.Contains(table, StringComparer.OrdinalIgnoreCase))
            {
               error_code = 1103;
               msg = $"Failed to find the table: {table} for the tested routine: {TstdRtnNm} ";
               table = null;
               break;
            }

            error_code = 0;
            msg = "";
         }while(false);

         return error_code;
      }

      /// <summary>
      /// Tries to determine the table name from the rtn name only
      /// Expects sp named like sp_candidate_delete : sp_[tablename]_[crt or update, get, del]
      /// </summary>
      /// <param name="rtn_nm"></param>
      /// <returns>table name or "" if rtn_nm is not in correct format</returns>
      protected bool GetTblNmFrmRtnNm(string rtn_nm, out string? table_nm)
      {
         bool ret = false;
         PreconditionNotNull(TableList);
         // Take off the op type
         // find the last _
         var cnt = rtn_nm.Occurrences("_");
         table_nm = null;

         do
         { 
            if (cnt < 1)
               return false;

            var parts = rtn_nm.Split("_", StringSplitOptions.RemoveEmptyEntries).ToList();

            // remove the sp_ or fn prefix
            if(parts[0].ToLower() == "sp")
               parts.RemoveAt(0);

            if(parts[0].Substring(0, 2) == "fn")
               parts[0]= parts[0].Substring(2);

            foreach (var part in parts)
            {
               var actTable = TableList.Find(x => x.Equals(part, StringComparison.OrdinalIgnoreCase));

               if (actTable != null)
               {
                  table_nm = part;
                  ret = true;
                  break;
               }
            }

            if(ret == true)
               break; // founs

            string table = rtn_nm.Reverse();
            var p = table.IndexOf('_');

            if (p == -1)
               return false;

            p   = table.Length - p-1;
            table = rtn_nm.Substring(0, p);

            // op type removed tmp like 'sp_candidate' or 'sp_candidate_detail'

            p = table.IndexOf('_');
            table_nm = table.Substring(p+1);
            ret = true;
         } while(false);

         return ret;
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="rtnNm"></param>
      /// <returns></returns>
      RtnType GetRtnType(string rtnNm)
      {
         RtnType rtnType = RtnType.Unknown;
         var sql = $"SELECT type_desc FROM sys.objects where[name] = '{rtnNm}';";

         using (IDbConnection conn = new SqlConnection(ConnectionString))//@"Data Source=DevI9\SqlExpress;Initial Catalog=Telepat;Integrated Security=True"))
         {
            var list = conn.Query<string>(sql: sql, commandType: CommandType.Text).ToList();

            switch(list.Count)
            { 
               case 0:  throw new Exception($"E1234: The routine {rtnNm} was not found in the database");
               case 1:  break; // OK
               default: throw new Exception($"E1235: Internal error: the routine {rtnNm} has a duplicate the database???");
            }

            rtnType = list[0].FindEnumByAliasExact<RtnType>();
         }

         return rtnType;
      }

      /// <summary>
      /// Gets the list of out args from the Param map 
      /// 
      /// PRECONDITIONS: PRE 1: ParamMap pop
      /// </summary>
      /// <param name="outArgs"></param>
      /// <param name="error_msg"></param>
      /// <returns>0 if OK, 1048 and msg="ParamMap is not populated" if PRE 1 false</returns>
      /// <exception cref="NotImplementedException"></exception>
      protected int GetOutArgsFromDb(out Dictionary<string, ParamInfo> outArgs, out string error_msg)
      {
         int error_code = 0;
         outArgs = new Dictionary<string, ParamInfo>();

         if (IsParamMapPopulated == false)
         { 
            error_msg = "ParamMap is not populated";
            return 1048;
         }

         foreach( KeyValuePair<string, ParamInfo> pr in ParamMap)
         {
            ParamInfo p = pr.Value;

            if((p.is_output == true) && ((p.col_nm?.Length ?? 0)>0))
               outArgs.Add(pr.Key, p);
         }

         error_msg = "";
         return error_code;
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="conn_str"></param>
      /// <param name="error_msg"></param>
      /// <returns></returns>
      public static int ValidateConnectionString(string? conn_str, out string db_name, out string error_msg)
      {
         int error_code = -1;
         error_msg = "";
         string db_key = "Initial Catalog";
         db_name = "";

         do
         {
            if (string.IsNullOrWhiteSpace(conn_str))
            {
               error_code = 1350;
               error_msg = "connection string is not specified";
               break;
            }

            var bits = conn_str.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            Dictionary<string, string> dictionary = bits.ToDictionary(s => s.Split('=')[0], s => s.Split('=')[1]);

            if ((dictionary.Count() == 0) || (!dictionary.ContainsKey(db_key)))
            {
               error_code = 1351;
               error_msg = $"Failed to extract the {db_key} key from the connection string";
               break;
            }

            db_name = dictionary["Initial Catalog"];

            if (string.IsNullOrWhiteSpace(db_name))
            {
               error_code = 1352;
               error_msg = $"The database name is not spcified properly in the connection";
               break;
            }

            // Finally
            error_code = 0;
         } while (false);

         if(error_code !=0)
            error_msg = $"E{error_code}: {error_msg}";

         return error_code;
      }

      /// <summary>
      /// Gets the list of expected parameters to test
      /// expected parameters = 
      ///   1: input params  including the output params minus the PK cols - they are usually auto generated and will vary and should not be altered
      ///   2: rtn returned cols
      /// 
      /// 220122: RULE for creating exp parameters : only test input params if a create or update type test (, and not a function: functions cannot write to tables: side effect not allowed)
      ///
      /// Ignore any input param that is part of the search key (primary key)
      /// 
      /// These are used in the following places:
      ///   1: creating @exp_ parameters in helper call signature from the main test
      ///   2: creating the helper signature
      ///   3: creating the corresponding @act_ variable declarions
      ///   
      /// PRECONDITIONS: Param map, PkList, RtnOutputColInfoList, output param list populated
      /// 
      /// </summary>
      protected Dictionary<string, ParamInfo> GetExpParameters()
      {
         var notPopMsg = "not populated";
         // PRECONDITION checks: Param map, PkList, RtnOutputColInfoList, output param list populated
         Precondition(IsRtnReturnedColListPopulated== true, $"RtnReturnedColList {notPopMsg}");
         Precondition(IsOutArgsMapPopulated        == true, $"OutArgsMap {notPopMsg}");
         PreconditionNotNull(RtnReturnedColList, $"E5728: the RtnReturnedColList {notPopMsg}");

         Dictionary<string, ParamInfo> expParamMap = new();

         // 1: input params 
         // Get the inp_ (input parameter) cols for the assertions, these will be inp / act checks
         foreach( var pr in ParamMap)
         {
            ParamInfo param = (ParamInfo)pr.Value.ShallowCopy();// new ParamInfo(pr.Value);
            // Ignore any param that is part of the search key (primary key)
            var col_nm = param.col_nm;
            param.is_input = true;

            // Dont want the PK input columns
            if ( PK_List?.FirstOrDefault(x => x.col_nm == col_nm) != null)
               continue;

            // Skip the internal scalar fn return value 
            if(string.IsNullOrEmpty(col_nm))
               continue;

            // 220122: RULE for creating exp parameters : only test input params if either output OR is a create or update type test (, and not a function: functions cannot write to tables: side effect not allowed)
            if((param.is_output == false) && (TestType != TestType.Create) && (TestType != TestType.Update))
               continue;

            expParamMap.Add(col_nm, param);
         }

         // 2: the exp_ returned cols for the assertions, these will be exp / act checks
         foreach ( ParamInfo param in RtnReturnedColList)
         {
            // Ignore any param that is part of the search key (primary key)
            var col_nm = param.col_nm;

            if( PK_List?.FirstOrDefault(x => x.col_nm == col_nm) != null)
               continue;

            if((col_nm != null) && ( !expParamMap.ContainsKey(col_nm ?? "")))
               expParamMap.Add(col_nm ?? "??", param);
         }

         return expParamMap;
#pragma warning restore CS8604 // Possible null reference argument.
      }


      /// <summary>
      /// Get the output table details from the Db
      /// 
      /// POSTCONDITIONS:
      ///   POST 1: either: (RtnOutputColInfoList populated with 0 or more items or error_code != 0 amd msg is ""), and msg = "E5721: PopRtnOutputColInfoList failed"
      /// </summary>
      /// <returns></returns>
      protected int PopRtnOutputColInfoList(string schema, string tstdRtnNm, out string error_msg)
      {
         error_msg = "";
         IsRtnReturnedColListPopulated = false;
         int error_code = -1;

         do
         {
            Precondition(!string.IsNullOrWhiteSpace(schema)   , $"PopRtnOutputColInfoList(): schema must be set");
            Precondition(!string.IsNullOrWhiteSpace(tstdRtnNm), $"PopRtnOutputColInfoList(): tstdRtnNm must be set");

            error_code = GetRtnOutputColsFromDb(schema, tstdRtnNm, out var rtnReturnedColList, out error_msg);

            if (error_code != 0)
               break;

            RtnReturnedColList = rtnReturnedColList;
            IsRtnReturnedColListPopulated = true;
         } while (false);

         // POST 1: RtnOutputColInfoList populated with 0 or more items or error_code != 0, and msg not empty
         Postcondition(((IsRtnReturnedColListPopulated == true) && (error_msg.Length == 0)) ||((error_code != 0) && (string.IsNullOrWhiteSpace(error_msg) ==false)), "E5721: PopRtnOutputColInfoList failed");
         return error_code;
      }

      public string[] ErrorMsgs = new string[5];

      /// <summary>
      /// Standard validate
      /// </summary>
      /// <param name="testedRtn"></param>
      /// <param name="testNum"></param>
      /// <param name="description"></param>
      /// <param name="error_msg"></param>
      /// Error messags:
      ///  Condition:                        msg
      ///  IsNullOrWhiteSpace(testedRtn)     "the tested routine must be specified"
      ///  testNum <= 0                      "the test number must be > 0"
      ///  testNum > 999                     "the test number must be < 1000"
      ///  
      /// Connection string validation (see the Connection string validation specification
      /// <returns>true if Validation succeeds, false and an error msg oterwise</returns>
      protected int Validate(string? testedRtn, int testNum, out string error_msg)
      {
         int error_code = 0;
         error_msg = "";

         do
         {
            if (string.IsNullOrWhiteSpace(testedRtn))
            {
               error_code = 1200;
               error_msg = $"the tested routine must be specified";
               break;
            }

            if (testNum <= 0)
            {
               error_code = 1201;
               error_msg = $"the test number must be > 0";
               break;
            }

            if (testNum > 999)
            {
               error_code = 1202;
               error_msg = $"the test number must be < 1000";
               break;
            }

         } while (false);

         if(error_code != 0)
            error_msg = $"E{error_code}: {error_msg}";

         return error_code;
      }

      /// <summary>
      // file:D:\Dev\Repos\C#\Tools\CreateSqlTestRoutine\Tests\Bug_Tests\Bug_Tests_000_050.cs
      ///
      ///  Create a temporary table if the rtn type specifies one
      ///  Create and Delete DO NOT require a temporary table
      ///  220204: creates do not return the created columns - but they can be found by using a select for updated cols 
      ///  220210: create the tmp table from the view not the table
      ///  
      /// PRECONDITIONS:
      /// RtnOutputColInfoList pop
      /// </summary>
      /// <param name="sb"></param>
      /// <param name="rtnRetType"></param>
      /// <exception cref="NotImplementedException"></exception>
      /// <returns>true if a tmp table is needed, false otherwise</returns>
      /// <see cref = "DetermineRtnRetType" />
      protected bool CrtTmpTbl(out string msg)
      {
         switch(TestType)
         {
            case TestType.Delete:
            case TestType.Create:
               msg = "";
               return false;
         }

         AppendLineDebug("-- @0040 S: CRTTMPTBL4RETCOLS");
         PreconditionNotNull(RtnReturnedColList,    $"E5728: the RtnReturnedColList notPopMsg");
         Precondition(TestType != TestType.Unknown, $"E5729: TestType = TestType.Unknown in CrtTmpTblHlprForRetCols()");
         msg = "";

         // Don't need a tmp table for this type
         if (TestType == TestType.Delete)
         {
            AppendLineDebug("-- @0040 tmp table not needed: TestType == TestType.Delete");
            return false;
         }

         // Unwanted scenarios: return ok
         if (RtnReturnedColList.Count != 0)
         {
            CrtTmpTblFromRetColList();
            // ASSERTION: creating a temporary table is relevant for this tested routine
         }
         else
         {
            // Scenario: no returned cols, AND TestType != TestType.Delete
            // if this is a scalar fn - dont need a tmp table
            if(TestType==TestType.Default && RtnRetType == RtnRetType.FnScalar)
            {
               AppendLineDebug("-- @0040 tmp table not needed: TestType == TestType.Default && RtnRetType == RtnRetType.FnScalar");
               return false;
            }

            // Create / UpdateCrtTmpTblFromUpdatedCols
            CrtTmpTblFromView();
         }

         // wrap up: add  ");<NL>"
         AppendLine($"\t);");

         // Check the view - should be #tmptable ?? - may be null if there is no table or view specified or relevant to the tstd rtn
         //Table = "#tmpTbl";
         //View  = "#tmpTbl";

         // Table and PK list may not be defined in this scenario either
         if (string.IsNullOrEmpty(Table))
            PopPKListFromRtnReturnedColList();// "#tmpTbl", out var hasPk);

         msg = "";
         AppendLineDebug("-- @0040 E: CRTTMPTBL4RETCOLS");
         return true;
      }

      /// <summary>
      /// 220210: create the tmp table from the view not the table
      /// </summary>
      protected void CrtTmpTblFromView()
      {
         PreconditionNotNull(Schema);
         string comma = " ";
         // Get the output columns for the tstd rtn
         AppendLine(txt_bloc_crt_tmp_tbl);

         // Get the columns for the view
         PreconditionNotNull(View);
         var cols = GetViewColsFromDb(Schema, View);

         foreach (ViewColInfo col in cols)
         {
            if(col.tbl_nm.Equals(Table, StringComparison.OrdinalIgnoreCase))
               ScrptTmpTblCol(col.data_type, col.col_nm, ref comma);
         }
      }

      /// <summary>
      /// Use dbo.fnGetProcUpdateCols(@schema_nm, @sp_nm)
      /// </summary>
      protected void CrtTmpTblFromUpdatedCols()
      {
         // Get the updated columns for the tstd rtn
         SpUpdatedCols = GetSpUpdatedCols();

         // Script the table from the col_list:
         ScriptTmpTable(SpUpdatedCols);
      }

      /// <summary>
      /// scripts teh tmp table from the updated cols list
      /// </summary>
      /// <param name="spUpdatedCols"></param>
      private void ScriptTmpTable(List<GetProcUpdateInfo> spUpdatedCols)
      {
         AppendLine(txt_bloc_crt_tmp_tbl);
         string comma = " ";
         PreconditionNotNull(SpUpdatedCols);

         PreconditionNotNull(PK_List);

         // Add PK cols to the tmp table script
         AddPkColsToTmpTblScrpt(ref comma);

         foreach (GetProcUpdateInfo colInfo in SpUpdatedCols)
            ScrptTmpTblCol(colInfo?.type_nm, colInfo?.col_nm, ref comma);
      }

      /// <summary>
      /// 
      /// </summary>
      protected void AddPkColsToTmpTblScrpt( ref string comma)
      {
         PreconditionNotNull(PK_List);

         // Add PK cols to the tmp table script
         foreach (var col in PK_List)
            ScrptTmpTblCol(col?.ty_nm, col?.col_nm, ref comma);
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="ty_nm"></param>
      /// <param name="col_nm"></param>
      /// <param name="comma"></param>
      protected void ScrptTmpTblCol(string? ty_nm, string? col_nm, ref string comma)
      {
         if (ty_nm  == null) ty_nm  = "????";
         if (col_nm == null) col_nm = "????";
         col_nm = $"[{col_nm}]";
         // Add a line like: [,][< fld_nm >]< tabbing >< type[with len] >
         AppendLine($"\t\t{comma}{col_nm.PadRight(17)} {ty_nm}");
         comma = ",";
      }

      /// <summary>
      /// Gets the updated cols list for the stored proc
      /// PRECONDITIONS: myst be a stroed proc
      /// </summary>
      /// <returns></returns>
      private List<GetProcUpdateInfo> GetSpUpdatedCols()
      {
         Precondition(TstdRtnType == RtnType.Sp, "GetSpUpdatedCols(): the tested routine must be a stored procedure");
         PreconditionNotNull(Schema);
         // Add the this proc as the root
         List<Tuple<string, string>> parentProcs = new List<Tuple<string, string>>() { new Tuple<string, string>(Schema, TstdRtnNm) };
         List<GetProcUpdateInfo> updated_col_list = new();
         var rtnNm = TstdRtnNm;

         do
         {
            // Recursively(for each child create sp) get the column names that are created / updated
            // EXEC sp_get_proc_update_cols 'dbo', 'sp_candidate_update' 
            // look for any child procedures that might update columns
            /*
            obj_nm            col_nm               is_sel is_upd  is_sel_all  IsProc IsTbl IsSclrFn IsTblFn IsInlnFn
            Candidate         NULL                 1      1       0           0      1     0        0       0
            Candidate         candidate_id         1      0       0           0      1     0        0       0
            Candidate         candidateStatus_id   0      1       0           0      1     0        0       0
            FnGetSalIdFrmNm   NULL                 0      0       0           0      0     1        0       0
            sp_person_update  NULL                 0      0       0           1      0     0        0       0 */
            List<Tuple<string, string>> child_procs = new List<Tuple<string, string>>();

            foreach (var pr in parentProcs)
            {
               var items = GetSpUpdatedCols(pr.Item1, pr.Item2);

               // for each updated column: add it to the updated col list
               foreach (var item in items)
               {
                  if ((!string.IsNullOrWhiteSpace(item.col_nm)) &&(item.is_updated == true))
                     if(null == updated_col_list.Find(x=>Equals(x.col_nm, StringComparison.OrdinalIgnoreCase)))
                        updated_col_list.Add(item);

                  if (item.IsProc == true)
                  {
                     AssertNotNull(item.schema_nm, "E8987: null GetProcUpdateInfo schema_nm");
                     AssertNotNull(item.obj_nm   , "E8987: null GetProcUpdateInfo obj_nm");
                     child_procs.Add(new Tuple<string, string>(item.schema_nm, item.obj_nm));
                  }
               }
            }

            parentProcs = child_procs;
            // Repeat till no more child procs
         } while (parentProcs.Count > 0);

         return updated_col_list;
      }

      protected List<GetProcUpdateInfo> GetSpUpdatedCols(string schemaNm, string tstdRtnNm)
      {
         List<GetProcUpdateInfo>? list = null;

         using (IDbConnection conn = new SqlConnection(@"Data Source=DevI9\SqlExpress;Initial Catalog=Telepat;Integrated Security=True"))
         {
            var values = new { schema_nm = schemaNm, rtn_nm = tstdRtnNm };
            list = conn.Query<GetProcUpdateInfo>("dbo.sp_get_proc_update_cols @schema_nm, @rtn_nm", values, commandType: CommandType.Text).ToList();
         }

         return list;
      }

      /// <summary>
      /// 
      /// </summary>
      /// <returns>true if hasPK</returns>
      protected bool CrtTmpTblFromRetColList()
      {
         PreconditionNotNull(RtnReturnedColList);
         // Get the output columns for the tstd rtn
         AppendLine(txt_bloc_crt_tmp_tbl);

         string comma = " ";
         bool hasPK = false;

         // for each result column:
         foreach (var colInfo in RtnReturnedColList)
         {
            string ty_nm = colInfo.ty_nm ?? "??";
            ty_nm = ty_nm.ToUpper();
            string col_nm = colInfo.col_nm ?? "??";
            col_nm = $"[{col_nm}]";

            // May need to set up PK later if a tmp table
            if (colInfo.is_pk == true)
               hasPK = true;

            if (colInfo.is_chr_ty ?? false)
               ty_nm = $"{ty_nm}({colInfo.ty_len})";

            // Add a line like: [,][< fld_nm >]< tabbing >< type[with len] >
            AppendLine($"\t\t{comma}{col_nm.PadRight(17)} {ty_nm}");
            comma = ",";
         }

         return hasPK;
      }

      /// <summary>
      /// Populates the PK List from the rtn returned cols
      /// 
      /// Scenario: 
      ///   TestType: default
      ///   No table or View defined -> #tmptbl
      ///   
      /// </summary>
      /// <param name="table">table name e.g. #tmpTbl </param>
      /// <param name="hasPK"></param>
      protected void PopPKListFromRtnReturnedColList()// string table
      {
         var msg = " should be specified at this point (PopPKListFromRtnReturnedColList)";
         PreconditionNotNull(RtnReturnedColList, "RtnReturnedColList" + msg);

         bool hasPK = RtnReturnedColList.Find(x => x.is_pk == true) != null;

         if (PK_List == null)
            PK_List = new List<KeyInfo>();

         AssertNotNull(PK_List, "PK_List" + msg);

         PK_List.Clear();
         int i = 1;

         // Setup the PK List
         foreach (var colInfo in RtnReturnedColList)
         {
            // Either we know the PK exists in the tmp table and we add only the PK fileds 
            // or we add all fields
            if (((hasPK == true) && (colInfo.is_pk == true)) || (hasPK == false))
            {
               KeyInfo keyInfo = new KeyInfo()
               {
                  ordinal   = i++,
                  schema_nm = Schema,
                  key_nm    = "????",
                  tbl_nm    = Table,
                  col_nm    = colInfo.col_nm ?? "",
                  ty_nm     = colInfo.ty_nm
               };

               PK_List.Add(keyInfo);
            }
         }
      }

      /// <summary>
      /// uses the update values
      /// </summary>
      /// <param name="msg"></param>
      /// <returns></returns>
      protected bool CrtTmpTblHlprForCrtUpdt(out string msg)
      {
         bool ret = false;
         msg = "";

         return ret;
      }

      /// <summary>
      /// Deduces the tested routine return type enum
      /// 
      /// POSTCONDITIONS:
      ///   Post 1: condition: rtnRetType != RtnRetType.Unknown.   action:  Throws amsg: "E1301: failed to deduce the routine return type for {TstdRtnNm}"
      ///   Post 2: condition: if GetRoutineDetailsFromDb() fails. action: msg: "E1300: Unable to Get [{Schema}].[{TstdRtnNm}] routine details from the database"
      ///   Post 2: if the stored RtnRetType value is not a RtnRetType enum value. action: Throws a NotSupportedException 
      /// </summary>
      /// <returns></returns>
      /// <exception cref="Exception"></exception>
      protected RtnRetType DetermineRtnRetType()
      {
         PreconditionNotNull(Schema);
         var rtnRetType = RtnRetType.Unknown;

         do
         { 
            // Determine the routine type (there are 4 types): e.g:
            // sp returning rows or no rows
            // fn returning rows or a scalar
            RtnDetail? item = GetRoutineDetailsFromDb(Schema, TstdRtnNm).FirstOrDefault();

            if (item == null)
               throw new Exception($"E1300: Unable to Get [{Schema}].[{TstdRtnNm}] routine details from the database");

            string rtnRetTypeNm = item?.rtn_ret_ty_nm ?? "";
            rtnRetType = rtnRetTypeNm.FindEnumByAliasExact<RtnRetType>(bThrowIfNotFound: true);
         } while(false);

         // Postcondition checks
         // Post 1: condition: rtnRetType != RtnRetType.Unknown, msg: "E1300: failed to deduce the routine return type for {TstdRtnNm}"
         Postcondition(rtnRetType != RtnRetType.Unknown, $"E1301: failed to deduce the routine return type for {TstdRtnNm}");
         return rtnRetType;
      }

      protected string GetPKVarName(string prefix)
      {
         if (PK_List == null)
            throw new Exception("E5730: the primary key fields list is not specified");

         var line = $"@{prefix}{PK_List[0].col_nm}";
         return line;
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="schema"></param>
      /// <param name="tstdRtnNm"></param>
      /// <param name="list"></param>
      /// <param name="msg"></param>
      /// <returns></returns>
      protected int GetPrmDetailsFromDb
      (
         string              schema,
         string              tstdRtnNm,
         out List<ParamInfo> list,
         out string          msg
      )
      {
         Precondition(!(string.IsNullOrWhiteSpace(schema) || string.IsNullOrWhiteSpace(tstdRtnNm)));
         list = new List<ParamInfo>();
         msg = "";

         using (IDbConnection conn = new SqlConnection(@"Data Source=DevI9\SqlExpress;Initial Catalog=Telepat;Integrated Security=True"))
         {
            var values = new { schema_nm = schema, rtn_nm = tstdRtnNm };
            list = conn.Query<ParamInfo>("SELECT * FROM dbo.fnGetRtnParams(@schema_nm, @rtn_nm)", values, commandType: CommandType.Text).ToList();
         }

         return 0;
      }

      /// <summary>
      /// Gets the primary Key columns for the given table
      /// </summary>
      /// <param name="schemaNm"></param>
      /// <param name="tblNm"></param>
      /// <returns></returns>
      protected List<string> GetTablePrimaryKeyColsFromDb
      (
         string schemaNm,
         string tblNm
      )
      {
         Precondition(!(string.IsNullOrWhiteSpace(schemaNm) || string.IsNullOrWhiteSpace(tblNm)));
         var list = new List<string>();

         using (IDbConnection conn = new SqlConnection(@"Data Source=DevI9\SqlExpress;Initial Catalog=Telepat;Integrated Security=True"))
         {
            var values = new { schema_nm = schemaNm, tblNm = tblNm };
            list = conn.Query<string>("SELECT * FROM dbo.fnGetPrimaryKeyCols(@schema_nm, @rtn_nm)", values, commandType: CommandType.Text).ToList();
         }

         return list;
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="schemaNm"></param>
      /// <param name="tstdRtnNm"></param>
      /// <param name="list"></param>
      /// <param name="error_msg"></param>
      /// <returns></returns>
      protected int GetRtnDetailsFromDb(string schemaNm, string rtnNm, out List<RtnDetail> list, out string error_msg)
      {
         int error_code = -1;
         Precondition(!(string.IsNullOrWhiteSpace(schemaNm) || string.IsNullOrWhiteSpace(rtnNm)));
         list = new List<RtnDetail>();

         do
         { 
            using (IDbConnection conn = new SqlConnection(@"Data Source=DevI9\SqlExpress;Initial Catalog=Telepat;Integrated Security=True"))
            {
               var values = new { schema_nm = schemaNm, rtn_nm = rtnNm };
               list = conn.Query<RtnDetail>("SELECT * FROM dbo.fnGetRtnDetails(@schema_nm, @rtn_nm)", values, commandType: CommandType.Text).ToList();
            }

            if (list == null)
            {
               error_code = 1400;
               error_msg = $"E{error_code}: dbo.fnGetRtnDetails(schema_nm: {schemaNm}, tstdRtnNm: {rtnNm}) failed to get any rows";
               break;
            }

            error_code = 0;
            error_msg = "";
         } while (false);

         return error_code;
      }

      protected int GetRtnDependencies(string objNm, out List<DependencyInfo> list, out string error_msg)
      {
         int error_code = 0;
         error_msg = "";
         // fnGetObjTableAndView   
         using (IDbConnection conn = new SqlConnection(@"Data Source=DevI9\SqlExpress;Initial Catalog=Telepat;Integrated Security=True"))
         {
            var values = new { rtn_nm = objNm };
            list = conn.Query<DependencyInfo>("SELECT * FROM dbo.fnGetDependencies(@rtn_nm)", values, commandType: CommandType.Text).ToList();
         }

         if(list == null)
         {
            error_code = 1401;
            error_msg = $"E{error_code}: dbo.fnGetObjTableAndView({objNm}) failed to get any rows";
         }

         return error_code;
      }

      /// <summary>
      /// Gets the routine output details form teh database if the rtn returns rows
      /// </summary>
      /// <param name="schema"></param>
      /// <param name="tstdRtnNm"></param>
      /// <param name="list"></param>
      /// <param name="error_msg"></param>
      /// <returns></returns>
      protected int GetRtnOutputColsFromDb(string schemaNm, string tstdRtnNm, out List<ParamInfo> list, out string error_msg)
      {
         Precondition(!(string.IsNullOrWhiteSpace(schemaNm) || string.IsNullOrWhiteSpace(tstdRtnNm)));
         int error_code = 0;
         list = new List<ParamInfo>();
         error_msg = "";

         using (IDbConnection conn = new SqlConnection(@"Data Source=DevI9\SqlExpress;Initial Catalog=Telepat;Integrated Security=True"))
         {
            var values = new { schema_nm = schemaNm, rtn_nm = tstdRtnNm };
            list = conn.Query<ParamInfo>("SELECT * FROM dbo.fnGetRtnOutputCols(@schema_nm, @rtn_nm)", values, commandType: CommandType.Text).ToList();
         }

         if (list == null)
         {
            error_code = 1402;
            error_msg = $"E{error_code}: dbo.fnGetRtnOutputCols(schema_nm: {schemaNm}, tstdRtnNm: {tstdRtnNm}) failed to get any rows";
         }

         return error_code;
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="tableList"></param>
      /// <param name="error_msg"></param>
      /// <returns></returns>
      /// <exception cref="NotImplementedException"></exception>
      protected List<string> GetTableListFromDb()
      {
         var list = new List<string>();

         using (IDbConnection conn = new SqlConnection(@"Data Source=DevI9\SqlExpress;Initial Catalog=Telepat;Integrated Security=True"))
         {
            list = conn.Query<string>("SELECT TABLE_NAME FROM dbo.ListDboTablesVw", commandType: CommandType.Text).ToList();
         }

         if(list.Count() == 0)
            throw new Exception($"E1450: failed to get the table list from the databse");

         return list;
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="viewList"></param>
      /// <param name="error_msg"></param>
      /// <returns></returns>
      /// <exception cref="NotImplementedException"></exception>
      protected List<string> GetViewListFromDb()
      {
         var list = new List<string>();

         using (IDbConnection conn = new SqlConnection(@"Data Source=DevI9\SqlExpress;Initial Catalog=Telepat;Integrated Security=True"))
         {
            list = conn.Query<string>("SELECT [TABLE_NAME] FROM [Telepat].[INFORMATION_SCHEMA].[VIEWS]WHERE TABLE_SCHEMA = 'dbo';", commandType: CommandType.Text).ToList();
         }

         if (list.Count() == 0)
            throw new Exception($"E1451: failed to get the view list from the databse");

         return list;
      }

      protected int UpdateMaxArgLenForOutputCols()
      {
         PreconditionNotNull(RtnReturnedColList, "E5728: the RtnReturnedColList {notPopMsg}");

         foreach (var col in RtnReturnedColList)
         {
            var len = (col.col_nm?.Length ?? 0) + 5;

            if (len > MaxArgLen)
               MaxArgLen = len;
         }

         return MaxArgLen;
      }

      /// <summary>
      /// Responsible for loading all scripts
      /// </summary>
      protected void LoadscriptBlocs()
      {
         var dir = @".\Components\";
         // standard template blocs
         txt_bloc_call_tstd_rtn_cmnt= File.ReadAllText(dir + "call_tstd_rtn_cmnt.txt");
         txt_bloc_crt_bdy           = File.ReadAllText(dir + "crt_bdy.txt");
         txt_bloc_crt_tmp_tbl       = File.ReadAllText(dir + "crt_tmp_tbl.txt");
         txt_bloc_del_bdy           = File.ReadAllText(dir + "del_bdy.txt");
         txt_bloc_get_bdy           = File.ReadAllText(dir + "get_bdy.txt");
         txt_bloc_hlpr_cmnt_hdr     = File.ReadAllText(dir + "hlpr_cmnt_hdr.txt");
         txt_bloc_hlpr_hdr          = File.ReadAllText(dir + "hlpr_hdr.txt");
         txt_bloc_mn_crt_wrap_up_rtn= File.ReadAllText(dir + "mn_crt_wrap_up_rtn.txt");
         txt_bloc_mn_tst_hdr        = File.ReadAllText(dir + "mn_tst_hdr.txt");
         txt_bloc_pre_sets          = File.ReadAllText(dir + "pre_sets.txt");
         txt_bloc_rtn_hdr           = File.ReadAllText(dir + "rtn_hdr.txt");
         txt_bloc_std_bdy           = File.ReadAllText(dir + "std_bdy.txt");
         txt_bloc_std_ftr           = File.ReadAllText(dir + "std_ftr.txt");
         txt_bloc_strgy_crt         = File.ReadAllText(dir + "strgy_crt.txt");
         txt_bloc_strgy_del         = File.ReadAllText(dir + "strgy_del.txt");
         txt_bloc_strgy_dflt        = File.ReadAllText(dir + "strgy_dflt.txt");
         txt_bloc_strgy_get_all     = File.ReadAllText(dir + "strgy_get_all.txt");
         txt_bloc_strgy_get1        = File.ReadAllText(dir + "strgy_get1.txt");
         txt_bloc_strgy_updt        = File.ReadAllText(dir + "strgy_Updt.txt");
      }
      /// <summary>
      /// Precondition chk
      /// </summary>
      /// <param name="rtn"></param>
      protected void PreconditionChkInit(string rtn)
      {
         Precondition(IsInitialised, $"SqlTestCreator must be initialised before {rtn}() is called");
      }

      /// <summary>
      /// Returns true if a function, false if a stored procedure
      /// </summary>
      /// <returns>true if a function, false if a stored procedure</returns>
      public bool IsFn()
      {
         return ((RtnRetType == RtnRetType.FnScalar) || (RtnRetType == RtnRetType.FnRows));
      }


      /// <summary>
      /// 
      /// </summary>
      /// <param name="script"></param>
      /// <param name="error_msg"></param>
      /// <param name="display"></param>
      /// <param name="database"></param>
      /// <param name="server"></param>
      /// <returns></returns>
      public bool RunScript(string script, out string error_msg, bool display = false)
      {
         var sqlFile = "script.sql";
         File.WriteAllText(sqlFile, script);
         return RunScriptFile(sqlFile, out error_msg, display);
      }

      /// <summary>
      /// Runs a script agains a sql srver
      ///
      /// PRECONDITIONS:
      ///   the following must be specified:
      ///      Connectionstring
      ///      Server
      ///      Database
      ///      sqlFile
      ///      
      /// </summary>
      /// <param name="sqlFile"></param>
      /// <param name="error_msg"></param>
      /// <param name="database"></param>
      /// <param name="server"></param>
      /// <returns></returns>
      public bool RunScriptFile(string sqlFile, out string error_msg, bool display = false)
      {
         // PRECONDITION checks:
         Precondition(!string.IsNullOrWhiteSpace(ConnectionString), "Connectionstring not set");
         Precondition(!string.IsNullOrWhiteSpace(Server)          , "Server not set");
         Precondition(!string.IsNullOrWhiteSpace(Database)        , "Database not set");
         Precondition(!string.IsNullOrWhiteSpace(sqlFile)         , "Runscript(sqlFile): sqlFile not specified");

         sqlFile = Path.GetFullPath(sqlFile);

         error_msg = "";

         var script = File.ReadAllText(sqlFile);
         var cmdErrorFile = "sqlcmd.out.txt";

         if (script.Length == 0)
         {
            error_msg = $"E1020: the sql script is empty";
            return false;
         }

         File.Delete(cmdErrorFile);
         var paramaters = $"-S {Server} -d {Database} -i \"{sqlFile}\" -j -m 1 -o {cmdErrorFile}";
         Log.Information($"Sqlcmd {paramaters}]");

         // Run the script against the database
         Process p = new Process();
         p.StartInfo.FileName = "SqlCmd.exe";
         p.StartInfo.RedirectStandardOutput = true;
         p.StartInfo.RedirectStandardError = true;
         p.StartInfo.UseShellExecute = false;
         p.StartInfo.Arguments = paramaters;
         p.Start();
         string stdout = p.StandardOutput.ReadToEnd();
         string stderr = p.StandardError.ReadToEnd();
         p.WaitForExit(2000);

         if (p.HasExited == false)
         {
            error_msg = "E1021 Sqlcmd still running";
            return false;
         }

         if (File.Exists(cmdErrorFile) == false)
         {
            // oops!
            error_msg = $"E1022: the sqlcmd output file: [{cmdErrorFile}] does not exist - there was an internal error running the script";
            return false;
         }

         string[] lines = File.ReadAllLines(cmdErrorFile);

         bool ret = lines.Count() == 0;

         // if error running script then display the errors file
         if (ret == false)
         {
            var msgs = $"{string.Join("\r\n", lines)} ";
            error_msg = $"/* E1027: there was an error running the script [{sqlFile}] against the database.\r\nErrors:\r\n{msgs}\r\n*/";
            File.AppendAllText(sqlFile, error_msg);
            var updated_lines = File.ReadAllLines(sqlFile);
         }

         // Display the script for checking if asked or an error 
         if ((display == true) || (ret == false))
            Process.Start("Notepad++.exe", sqlFile);

         return ret;
      }


      /// <summary>
      /// Creates the strategy hdr based on the RtnType
      /// </summary>
      /// <param name="sb"></param>
      /// <exception cref="NotImplementedException"></exception>
      protected void CrtStrategyHdr()
      {
         var bloc = "";

         switch (TestType)
         {
            case TestType.Default: bloc = txt_bloc_strgy_dflt; break;
            case TestType.Create: bloc = txt_bloc_strgy_crt; break;
            case TestType.Get1: bloc = txt_bloc_strgy_get1; break;
            case TestType.GetAll: bloc = txt_bloc_strgy_get_all; break;
            case TestType.Update: bloc = txt_bloc_strgy_updt; break;
            case TestType.Delete: bloc = txt_bloc_strgy_del; break;
            case TestType.Pop: bloc = txt_bloc_strgy_del; break;
            default: throw new Exception("E 5531: unexpected test type Creating the Strategy Header");
         }

         AppendLine(bloc);
      }

   }
}