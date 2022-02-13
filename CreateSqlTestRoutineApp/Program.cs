using CreateSqlTestRoutineLib;

namespace CreateSqlTestRoutineApp
{
   public class Program : ProgramBase
   {
      /// <summary>
      /// args: 
      ///  0: tested rtn
      ///  1: test num
      ///  2: [table]
      ///  3: [view]

      /// </summary>
      /// <param name="args"></param>
      /// <returns></returns>
      static int Main(string[] args)
      {
         return MainBase(args, out _, out _, out _);
      }
   }
}

