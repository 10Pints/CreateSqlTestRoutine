namespace CreateSqlTestRoutineLib
{
   public class ArgInfo
   {
      public string  Name     { get; set; }="";
      public string  Type     { get; set; }="";
      public int     Ordinal  { get; set; }=-1;

      /// <summary>
      /// start with 6 spaces (2 tabs)
      /// </summary>
      /// <returns></returns>
      public override string? ToString()
      {
         return base.ToString();
      }
   }
}