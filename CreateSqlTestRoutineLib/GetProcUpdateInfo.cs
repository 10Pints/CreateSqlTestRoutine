using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreateSqlTestRoutineLib
{
   public class GetProcUpdateInfo
   {
      public string?  obj_nm       { get; set; } = null;
      public string?  col_nm       { get; set; } = null;
      public bool?    is_selected  { get; set; } = null;
      public bool?    is_updated   { get; set; } = null;
      public bool?    is_select_all{ get; set; } = null;
      public bool?    IsProc       { get; set; } = null;
      public bool?    isTable      { get; set; } = null;
      public bool?    IsScalarFn   { get; set; } = null;
      public bool?    IsTableFn    { get; set; } = null;
      public bool?    IsInlineFn   { get; set; } = null;
      public string?  schema_nm    { get; set; } = null;
      public string?  type_nm      { get; set; } = null;
   }
}
