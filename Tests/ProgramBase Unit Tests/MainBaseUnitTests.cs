using CreateSqlTestRoutineLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Serilog;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Tests.Test_Support;

namespace Tests
{
   [TestClass]
   public class MainBaseUnitTests : CreateSqlTestsBase
   {
      /// <summary>
      /// First test for   B018: failed to handle function returning rows - E1 (not impl)    
      /// dbo.fnGetGetProcedureUpdateCols100 =>
      ///   Error Caught exception: HandleCallFNRetRows not implemented
      ///   Error E-1: HandleCallFNRetRows not implemented
      /// </summary>
      [TestMethod]
      public void B018_hndl_fn_ret_rows_E1_not_impl_failed_intl_tst()
      { 
         string[] args = new string[] { "dbo.fnGetFnOutputCols", "100","debug:true" };
         int error_code = ProgramBase.MainBase(args, out string msg, out string hlpr_file, out string mn_tst_file);
         Assert.AreEqual(0, error_code, msg);
         Assert.AreEqual(0, msg.Length, msg);
      }
   }
}
