using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using C5;

namespace CreateSqlTestRoutineLib
{
   public class CodeTidy
   {

      public CodeTidy()
      {
         Init();
      }

      /// <summary>
      /// Expect a file of a file holding method call args like:
      ///        string testedRtn
      ///      , int testNum
      ///      , out string sql
      ///      , out string error_msg
      ///      , string? table = null
      ///      , string? view = null
      ///
      /// Method:
      ///  Read all the text lines from the file
      ///  Parse to an argInfo List in order
      ///    Calculating the max type len 
      ///    
      ///  Write the args back to the same file, tidied
      ///   return the text for testing
      ///   foreach line:
      ///      start with 6 spaces (2 tabs)
      ///      print comma/sps
      ///      print the type padded to MaxLen characters
      ///      print the name
      ///   
      /// 
      /// </summary>
      /// <param name="file"></param>
      /// <returns></returns>
      /// <exception cref="NotImplementedException"></exception>
      public string TidyMethodCall(string file)
      {
         // Method:
         //  Read all the text lines form the file
         var in_lines = File.ReadAllLines(file);
         //  Parse to an argInfo List in order
         //    Calculating the  max type len 
         List<ArgInfo> argList = ParseArgs(in_lines);

         // Write the args back to the same file, tidied
         string code = WriteArgsBackToFile(argList, file);

         // return the text for testing
         return code; // new text
      }

      /// <summary>
      ///   Get the next non whitespace word
      ///   if it is a recognosed C# keyword valid in this context
      ///      
      /// </summary>
      /// <param name="in_lines"></param>
      /// <returns></returns>
      protected List<ArgInfo> ParseArgs(string[] in_lines)
      {
         List<ArgInfo> argList = new List<ArgInfo>();

         //  foreach line:
         foreach (var in_line in in_lines)
         {
            argList.Add(ParseLine(in_line));
         }

         return argList;
      }

      /// <summary>
      /// parse the arg line to a new ArgInfo structure
      /// </summary>
      /// <param name="in_line"></param>
      /// <returns></returns>
      /// <exception cref="NotImplementedException"></exception>
      protected ArgInfo ParseLine(string in_line)
      {
         var arg = new ArgInfo();
         // split the line into tokens separated by spaces;
         var words = in_line.Split(new char[] {' ',',' });
         int i = 0;

         foreach (var word in words)
         {
            // Remove the first 
            if ((i==0) && (word == ","))
               continue;

            // 
            if(IsKeyWord(word))
            { 
            }


            i++;
         }

         return arg;
      }

      protected SortedSet<string> ReservedWords { get;set;}=new SortedSet<string>();
      protected bool IsKeyWord(string word)
      {
         throw new NotImplementedException();
      }

      /// <summary>
      /// Expect input lines like:
      ///        string testedRtn
      ///      , int testNum
      ///      , out string sql
      ///      , out string error_msg
      ///      , string? table = null
      ///      , string? view = null
      ///   start with 6 spaces (2 tabs)
      ///   if Line does not have a leading comma add 2 spcs
      /// </summary>
      /// <param name="in_line"></param>
      /// <returns></returns>
      protected ArgInfo TidyLine(string in_line)
      {
         var argInfo = new ArgInfo();

         StringBuilder sb = new StringBuilder();
         in_line = in_line.TrimStart();
         // exect comma or text
         var token = ParseArg(in_line);

         if (token != ",")
            sb.Append("  ");
         else
            token = ParseArg(in_line);

         // Assertion: if here then we have the first non comma type item clause
         // it is a type item if it is either:
         //    a regognise key word
         //    starts wth an upper case letter
         // in which case add it as is
         if (IsTypeItem(token))
         { }

         return argInfo;
      }

      private string ParseArg(string in_line)
      {
         throw new NotImplementedException();
      }

      protected bool IsTypeItem(object token)
      {
         throw new NotImplementedException();
      }

      /// <summary>
      ///  Write the args back to the same file, tidied
      ///    Return the text for testing
      /// </summary>
      /// <param name="file"></param>
      /// <returns></returns>
      /// <exception cref="NotImplementedException"></exception>
      protected string WriteArgsBackToFile(List<ArgInfo> argList, string file)
      {
         StringBuilder sb = new StringBuilder();

         foreach (ArgInfo arg in argList)
         {
            sb.Append(arg.ToString());
         }

         return sb.ToString();
      }


      // Add the Keywords
      protected void Init()
      {
         PopKeywords();
      }

      protected void PopKeywords()
      {
         ReservedWords.Clear();

         ReservedWords = new SortedSet<string>() 
         {
            "bool"
            ,"int"
            ,"Dictionary"
            ,"List"
            ,"string"
            ,"StringBuilder"
            ,"[]"
            ,"?"

         };
      }
   }
}
