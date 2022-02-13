using CreateSqlTestRoutineLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Tests.Test_Support;

namespace Tests
{
   [TestClass]
   public class CreateAssertBlocUnitTests : CreateSqlTestsBase
   {
      /*
       signatire: [dbo].[sp_candidate_create] 
	 @family_name        NVARCHAR(50)            
	,@first_name         NVARCHAR(50) 
	,@middle_name        NVARCHAR(50) 
	,@seq                INT          
	,@salutation_nm      NVARCHAR(50)
   ,@email              NVARCHAR(50)
   ,@phone              NVARCHAR(25)
   ,@mobile             NVARCHAR(25)
   ,@candidate_id       INT            OUTPUT 
   ,@status_id          INT            OUTPUT
   ,@key                NVARCHAR(50)   OUTPUT
   ,@msg                NVARCHAR(100)  OUTPUT
AS

   expected asserts
	EXEC ut.test.sp_Assert_Exp_Act @tst_num,  1, @exp_RC              , @act_RC              , 'RC';               1  OK

	EXEC ut.test.sp_Assert_Exp_Act @tst_num,  2, @inp_family_name     , @act_family_name     , 'family_name';      2  OK
	EXEC ut.test.sp_Assert_Exp_Act @tst_num,  3, @inp_first_name      , @act_first_name      , 'first_name';       3  OK
	EXEC ut.test.sp_Assert_Exp_Act @tst_num,  4, @inp_middle_name     , @act_middle_name     , 'middle_name';      4  OK
	EXEC ut.test.sp_Assert_Exp_Act @tst_num,  5, @inp_seq             , @act_seq             , 'seq';              5  OK
	EXEC ut.test.sp_Assert_Exp_Act @tst_num,  6, @inp_salutation_nm   , @act_salutation_nm   , 'salutation_nm';    6  OK
	EXEC ut.test.sp_Assert_Exp_Act @tst_num,  7, @inp_email           , @act_email           , 'email';            7  OK
	EXEC ut.test.sp_Assert_Exp_Act @tst_num,  8, @inp_phone           , @act_phone           , 'phone';            8  OK
	EXEC ut.test.sp_Assert_Exp_Act @tst_num,  9, @inp_mobile          , @act_mobile          , 'mobile';           9  OK
                                                                                                                  
   testable outputs:                                                                                              
                                                                                                                  
	EXEC ut.test.sp_Assert_Exp_Act @tst_num, 11, @exp_status_id       , @act_status_id       , 'status_id';        10 ER
	EXEC ut.test.sp_Assert_Exp_Act @tst_num, 12, @exp_key             , @act_key             , 'key';              11 ER
	EXEC ut.test.sp_Assert_Exp_Act @tst_num, 13, @exp_msg             , @act_msg             , 'msg';              12 ER
                                                                                                                  
   do not expect:
      
	EXEC ut.test.sp_Assert_Exp_Act @tst_num, 10, @inp_candidate_id    , @act_candidate_id    , 'candidate_id';     
	EXEC ut.test.sp_Assert_Exp_Act @tst_num, 12, @inp_key             , @act_key             , 'key';              
	EXEC ut.test.sp_Assert_Exp_Act @tst_num, 13, @inp_msg             , @act_msg             , 'msg';   
      
   Current bug output:

	EXEC ut.test.sp_Assert_Exp_Act @tst_num,  1, @exp_RC              , @act_RC              , 'RC';               OK
	EXEC ut.test.sp_Assert_Exp_Act @tst_num,  2, @inp_family_name     , @act_family_name     , 'family_name';      OK
	EXEC ut.test.sp_Assert_Exp_Act @tst_num,  3, @inp_first_name      , @act_first_name      , 'first_name';       OK
	EXEC ut.test.sp_Assert_Exp_Act @tst_num,  4, @inp_middle_name     , @act_middle_name     , 'middle_name';      OK
	EXEC ut.test.sp_Assert_Exp_Act @tst_num,  5, @inp_seq             , @act_seq             , 'seq';              OK
	EXEC ut.test.sp_Assert_Exp_Act @tst_num,  6, @inp_salutation_nm   , @act_salutation_nm   , 'salutation_nm';    OK
	EXEC ut.test.sp_Assert_Exp_Act @tst_num,  7, @inp_email           , @act_email           , 'email';            OK
	EXEC ut.test.sp_Assert_Exp_Act @tst_num,  8, @inp_phone           , @act_phone           , 'phone';            OK
	EXEC ut.test.sp_Assert_Exp_Act @tst_num,  9, @inp_mobile          , @act_mobile          , 'mobile';           OK

	EXEC ut.test.sp_Assert_Exp_Act @tst_num, 10, @inp_candidate_id    , @act_candidate_id    , 'candidate_id';     ER should not exist
	EXEC ut.test.sp_Assert_Exp_Act @tst_num, 11, @inp_status_id       , @act_status_id       , 'status_id';        ER @inp_status_id should be @exp_status_id
	EXEC ut.test.sp_Assert_Exp_Act @tst_num, 12, @inp_key             , @act_key             , 'key';              ER @inp_key       should be @exp_key
	EXEC ut.test.sp_Assert_Exp_Act @tst_num, 13, @inp_msg             , @act_msg             , 'msg';              ER @inp_msg       should be @exp_msg

   Fixes:
      1: get rid of candidate_id as it is is key col;
      2: outputs not part of key should be compared  @exp / @act

*/
      [TestMethod] public void Test_CreateAssertBloc_Candidate_Create() { Assert.IsTrue(Helper(qTstdRtnNm: "dbo.sp_candidate_create", tstNum: 100, msg: out string msg, exp_chks: 12), msg);}
      [TestMethod] public void Test_CreateAssertBloc_Candidate_Delete() { Assert.IsTrue(Helper(qTstdRtnNm: "dbo.sp_candidate_delete", tstNum: 100, msg: out string msg, exp_chks: 2, table: "Candidate"), msg);}
      [TestMethod] public void Test_CreateAssertBloc_Candidate_Update() { Assert.IsTrue(Helper(qTstdRtnNm: "dbo.sp_candidate_update", tstNum: 100, msg: out string msg, table: "Candidate", viewNm: "dbo.CandidateVw", exp_chks: 11), msg);}

      [TestMethod] public void Test_CreateAssertBloc_Candidate_Get1  () { Assert.IsTrue(Helper(qTstdRtnNm: "dbo.sp_Candidate_get1"  , tstNum: 100, msg: out string msg
         , clauses: new[] 
         { 
            "EXEC ut.test.sp_Assert_Exp_Act @tst_num, 1, @exp_RC , @act_RC , 'RC';" 
            ,"sp_Assert_Exp_Act @tst_num, 2, @exp_key , @act_key , 'key';"
         }, exp_chks: 11), msg);
      }

      [TestMethod] public void Test_CreateAssertBloc_Candidate_GetAll() { Assert.IsTrue(Helper(qTstdRtnNm: "dbo.sp_candidate_getAll", tstNum: 100, msg: out string msg, table: "Candidate", viewNm: "dbo.Candidate"  , exp_chks: 11), msg);}
      [TestMethod] public void Test_CreateAssertBloc_Recruiter_Get1  () { Assert.IsTrue(Helper(qTstdRtnNm: "dbo.sp_recruiter_get1"  , tstNum: 100, msg: out string msg, exp_chks: 12), msg);}
      [TestMethod] public void Test_CreateAssertBloc_Person_Create   () { Assert.IsTrue(Helper(qTstdRtnNm: "dbo.sp_person_Create"   , tstNum: 100, msg: out string msg, table: "Person"   , viewNm: "dbo.PersonVw"   , exp_chks: 12), msg);}
      [TestMethod] public void Test_CreateAssertBloc_Person_GetAll   () { Assert.IsTrue(Helper(qTstdRtnNm: "dbo.sp_person_getAll"   , tstNum: 100, msg: out string msg, table: "Person"   , viewNm: "dbo.PersonVw"   , exp_chks: 15), msg);}

      protected bool Helper(string qTstdRtnNm, int tstNum, out string msg, int exp_chks, string[]? clauses = null, string? table=null, string? viewNm=null)
      {
         var c = new TestableSqlTestCreator();
         
         if(c.Init(qTstdRtnNm: qTstdRtnNm, tstNum: tstNum, conn_str: ConnectionString, msg: out msg, table: table, view: viewNm) != 0)
            return false;

         c.Hlpr_Crt_AssertBloc();

         string file = Path.GetFullPath($"Hlpr_Crt_AssertBloc_{TestMethod}_sql");
         var lines = File.ReadAllLines(file);

         // Reduce padding to minimum
         for(int i = 0; i< lines.Length; i++)
            lines[i] = Squish(lines[i]);

         var script =string.Join("\r\n", lines);
         var Matches = Regex.Matches(script, @".*test\.sp_Assert_Exp_Act.*", RegexOptions.Multiline | RegexOptions.IgnoreCase);

         if(Matches.Count != exp_chks)
         {
            msg = $"*** Error Assert count chk failed: exp: {exp_chks} act: {Matches.Count}";
            c.AppendLine(msg);
            DisplayScript(c.SB, file);
            return false;
         }

         // chk supplied clauses
         if (clauses != null)
            foreach (var clause in clauses)
               if (!FindClause(lines, clause, out msg))
               {
                  c.AppendLine(msg);
                  DisplayScript(c.SB, file);
                  return false;
               }

         // Finally
         msg = "";
         return true;
      }

      [TestInitialize]
      public new void TestSetup()
      {
         base.TestSetup();       
      }
   }
}
