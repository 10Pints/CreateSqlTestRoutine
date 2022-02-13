using Common;

namespace CreateSqlTestRoutineLib
{
   public enum RtnRetType
   {
      [EnumAlias("Unknown")]
      Unknown = 0,

      [EnumAlias("SP returning no rows")]
      SpNoRows,

      [EnumAlias("SP returning rows")]
      SpRows,

      [EnumAlias("FN returning no rows")]
      FnScalar,

      [EnumAlias("FN returning rows")]
      FnRows,
   };
}
