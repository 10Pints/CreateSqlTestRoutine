namespace CreateSqlTestRoutineLib
{
   public class ColInfo
   {
      public int?    ordinal     { get; set; }
      public string? col_nm      { get; set; }
      public int?    ty_id       { get; set; }
      public string? ty_nm       { get; set; }
      public int?    ty_len      { get; set; } 
      public bool?   is_pk       { get; set; }
      public bool?   is_nullable { get; set; }
      public bool?   is_chr_ty   { get; set; }
      public string? tbl_nm      { get; set; }
      public string? schema_nm   { get; set; }

      public ColInfo ShallowCopy()
      { 
         return (ColInfo) MemberwiseClone();
      }
   }
}