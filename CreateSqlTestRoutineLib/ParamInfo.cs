using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreateSqlTestRoutineLib
{
   public class ParamInfo : ColInfo
   {
      public bool   is_output  { get; set; } = false;  // true if it is an output parameter
      public bool   is_input   { get; set; } = false;  // true if the parameter exists in param map, false if not
      public string rtn_nm      { get; set; } = "";
      public string rtn_ty      { get; set; } = "";

      public override string ToString()
      {
         return $"ordinal: {ordinal}   col_nm: {col_nm}   col_ty_nm: {ty_nm}   col_ty_id: {ty_id}" +
            $"   col_len: {ty_len}   is_pk: {is_pk}   is_output: {is_output}   schema_nm: {schema_nm}" +
            $"   rtn_nm: {rtn_nm}   rtn_ty: {rtn_ty}";
      }

      public static string GetHdr()
      {
         return "ordinal\tcol_nm\tty_nm\tty_id" +
            $"\tcol_len\tis_pk\tis_output\tschema_nm\trtn_nm\trtn_ty";
      }      
   }
}
