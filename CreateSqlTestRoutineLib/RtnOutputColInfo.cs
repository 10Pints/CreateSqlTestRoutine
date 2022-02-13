using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreateSqlTestRoutineLib
{
   public class RtnOutputColInfo
   {
      public int?    ordinal     { get; set; }
      public string? col_nm      { get; set; }
      public string? ty_nm       { get; set; }
      public int?    type_id     { get; set; }
      public int?    len         { get; set; }
      public bool?   is_nullable { get; set; }
      public bool?   is_char_ty  { get; set; }
      public string? rtn_nm      { get; set; }
      public string? rtn_ty      { get; set; }
      public string? schema_nm   { get; set; }
      public string? db_nm       { get; set; }
   }
}






