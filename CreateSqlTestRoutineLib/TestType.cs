using Common;

namespace CreateSqlTestRoutineLib
{
   public enum TestType
   {
      [EnumAlias("Unknown")]
      Unknown = 0,
      [EnumAlias("Unknown")]
      Default,
      [EnumAlias("Unknown")]
      Create,
      [EnumAlias("Unknown")]
      Update,
      [EnumAlias("Unknown")]
      Get1,
      [EnumAlias("Unknown")]
      GetAll,
      [EnumAlias("Unknown")]
      Delete,
      [EnumAlias("Unknown")]
      Pop
   };
}
