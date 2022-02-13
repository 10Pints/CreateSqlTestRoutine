using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreateSqlTestRoutineLib
{
   public enum RtnType
   {
      [EnumAlias("Unknown")]
      Unknown = 0,

      [EnumAlias("SQL_STORED_PROCEDURE")]
      Sp,

      [EnumAlias("SQL_TABLE_VALUED_FUNCTION")]
      TableValuedFn,

      [EnumAlias("SQL_SCALAR_FUNCTION")]
      ScalarFn
   }
}
