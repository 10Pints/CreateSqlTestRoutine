using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreateSqlTestRoutineLib
{
   public class RtnDetail
   {
      public string     schema_nm       { get; set; } = "";
      public string     rtn_nm          { get; set; } = "";
      public string     rtn_ty          { get; set; } = "";
      public int        rtn_ret_ty_code { get; set; } = -1;
      public string     rtn_ret_ty_nm   { get; set; } = "";
      public string     data_type       { get; set; } = "";
      public int        char_max_len    { get; set; }
      public string     ty_nm           { get; set; } = "";
      public string     ty_code         { get; set; } = "";
      public int        type_id         { get; set; } = -1;
      public DateTime?  created         { get; set; } = null;
      public DateTime?  last_altered    { get; set; } = null;
   }
}
