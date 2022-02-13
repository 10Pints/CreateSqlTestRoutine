using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreateSqlTestRoutineLib
{
   public class DependencyInfo
   {
      public string? referencing_name     { get; set; }
      public string? referencing_type_nm  { get; set; }
      public string? referenced_name      { get; set; }
      public string? referenced_type_nm   { get; set; }
      public int?    nest_level           { get; set; }
      public int?    referencing_id       { get; set; }
      public int?    referenced_id        { get; set; }
      public string? rtn_nm               { get; set; }

      public override string ToString()
      {
         return @$"({nest_level}): {referenced_name,-32} {referenced_type_nm}";
      }
   }
}
