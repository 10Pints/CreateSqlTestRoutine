// See https://aka.ms/new-console-template for more information
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging; 
using Serilog;
using Serilog.Events;
using Microsoft.Extensions.DependencyInjection;

namespace CreateSqlTestRoutineLib
{
   public class ProgramBase
   {
      protected static string ConnectionString { get;set; } = "";


      public static IConfigurationRoot? Configuration { get; set; }

      /// <summary>
      /// Called by int CreateSqlTestRoutineApp.Program.Main(string[] args)
      /// args: 
      ///  0: tested rtn
      ///  1: test num
      ///  2: table
      ///  3: view
      /// </summary>
      /// <param name="args"></param>
      /// <param name="error_msg"></param>
      /// <param name="hlpr_file"></param>
      /// <param name="mn_tst_file"></param>
      /// <returns></returns>
      public static int MainBase(string[] args, out string error_msg, out string hlpr_file, out string mn_tst_file)
      {
         int error_code = -1;
         error_msg            = "";
         hlpr_file      = "";
         mn_tst_file    = "";

         try
         {
            do
            {
               error_code = Init(args  
                              , error_msg : out        error_msg
                              , tstdRtnNm : out string tstdRtnNm
                              , testNum   : out int    testNum
                              , table     : out string? table
                              , view      : out string? view
                              , debug_mode: out bool?   debug_mode
                        );

               if (error_code != 0)
                  break;

               var testCreator = new SqlTestCreator(ConnectionString, debug_mode);

               error_code = testCreator.__CrtTestMethods
                  (
                      qTstdRtnNm    : tstdRtnNm
                     , testNum      : testNum
                     , msg          : out error_msg
                     , table        : table 
                     , view         : view
                     , hlpr_script  : out string hlpr_script
                     , mn_script    : out string mn_script
                  );

               if(error_code != 0)
                  break;

               hlpr_file   = Path.GetFullPath($"hlpr_{testNum,3:000}_{tstdRtnNm}.sql");
               mn_tst_file = Path.GetFullPath($"test_{testNum,3:000}_{tstdRtnNm}_mn_script.sql");

               Log.Information($"Writing hlpr file: {hlpr_file}");
               Log.Information($"Writing main test file  file: {mn_tst_file}");

               File.WriteAllText(hlpr_file  , hlpr_script);
               File.WriteAllText(mn_tst_file, mn_script);

               if (!testCreator.RunScript(hlpr_script, out error_msg, display: false))
               {
                  error_code = 3650;
                  Log.Error($"E{3650}: the generated helper script failed to compile: {hlpr_file}\r\n{error_msg}"); 
                  break;
               }

               if (!testCreator.RunScript(mn_script, out error_msg, display: false))
               {
                  error_code = 3651;
                  Log.Error($"E{3650}: the generated main test script failed to compile: {mn_tst_file}\r\n{error_msg}");
                  break;
               }

               // Finally
               error_code = 0;
            } while(false);
         }
         catch (Exception e)
         {
            error_msg = e.Message;
            Log.Error($"Caught exception: {error_msg}");
            error_code = -1;
         }

         if(error_code != 0)
         {
            error_msg = $"E{error_code}: {error_msg}";
            Log.Error(GetHlpMsg( error_msg));
         }

         return error_code;
      }

      /// <summary>
      /// <summary>
      /// args: 
      ///  0: tested rtn : tstdRtnNm
      ///  1: test num   : testNum
      ///  2: description: table
      ///  3: table      : view
      /// </summary>
      /// </summary>
      /// <param name="args"></param>
      /// <param name="error_msg"></param>
      /// <param name="tstdRtnNm"></param>
      /// <param name="testNum"></param>
      /// <param name="table"></param>
      /// <param name="view"></param>
      /// <returns>0 if ok or non zero error code if error</returns>
      static int Init(string[] args
                           , out string  error_msg
                           , out string  tstdRtnNm
                           , out int     testNum
                           , out string? table
                           , out string? view
                           , out bool?   debug_mode
                     )
      {
         int error_code = -1;
         error_msg = "";
         tstdRtnNm = "";
         testNum   = 0;
         table     = null;
         view      = null;
         debug_mode= null;

         do
         {
            Log.Logger = new LoggerConfiguration()
                              .WriteTo.File("CreateSqlTestRoutine.log", rollingInterval: RollingInterval.Day)
                              .Enrich.FromLogContext()
                              .MinimumLevel.Debug()
                              .WriteTo.Console(
                                 LogEventLevel.Verbose,
                                 "{NewLine}{Timestamp:HH:mm:ss} [{Level}] ({CorrelationToken}) {Message}{NewLine}{Exception}")
                              .CreateLogger();

            // Create service collection
            string msg_ = "";

            foreach (var arg in args)
               msg_ += $"{arg} ";

            Log.Information($"starting, args: {msg_}");
            ServiceCollection serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);

            // Create service provider
            IServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();

            if (Configuration == null)
            {
               error_code = 1000;
               error_msg = $"{error_code}: Failed to initialise appsettings from the appsetting.json file";
               break;
            }

            ConnectionString = Configuration.GetConnectionString("Default");
            SqlTestCreatorBase.DefaultConnectionString = Configuration?.GetConnectionString("Default") ?? ""; // "" will cause a precondition expeception to file

            Log.Information($"connection string: {ConnectionString}");
            var settings_name = Configuration?["Settings name"] ?? "";

            if (string.IsNullOrWhiteSpace(settings_name))
            {
               error_code = 1001;
               error_msg = $"{error_code}: Failed to get the appsettings name from the appsetting.json file";
               break;
            }

            /*
             * args: 
             * 0: tested rtn:
             * 1: test num
             * 2: description
             * 3: table
             * 4: crt_rtn
             */
            error_code = ValidateParams(
                             args
                           , tstdRtnNm : out tstdRtnNm
                           , testNum   : out testNum
                           , table     : out table
                           , view      : out view
                           , msg       : out error_msg
                           , debug_mode: out debug_mode
                           );

            if( error_code != 0)
               break;

            Log.Information($"SqlTestCreator tested rtn: [{tstdRtnNm}], testNum: {testNum}, table: [{table}], view: [{view}]");

            // Finally 
            error_code = 0;
         } while(false);

         return error_code;
      }

      // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
      public void Configure(Microsoft.AspNetCore.Builder.IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
      {
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="serviceCollection"></param>
      static void ConfigureServices(IServiceCollection serviceCollection)
      {
         // Add logging
         serviceCollection.AddSingleton(LoggerFactory.Create(builder =>
         {
            builder
               .AddSerilog(dispose: true);
         }));

         serviceCollection.AddLogging();

         // Build configuration
         Configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetParent(AppContext.BaseDirectory)?.FullName ?? "oops")
            .AddJsonFile("appsettings.json", false)
            //.AddXmlFile("Tests.dll.config")
            .Build();

         // Add access to generic IConfigurationRoot
         serviceCollection.AddSingleton<IConfigurationRoot>(Configuration);

         // Add app
         //serviceCollection.AddTransient<App>();
      }

      /// <summary>
      /// args: 
      ///  0: tested rtn
      ///  1: test num
      ///  2: table
      ///  3: view
      /// </summary>
      private static int ValidateParams(
        string[]     args
      , out string   tstdRtnNm
      , out int      testNum
      , out string?  table
      , out string?  view
      , out bool?    debug_mode
      , out string   msg
      )
      {
         int error_code = -1;
         tstdRtnNm      = "";
         testNum        = 0;
         table          = null;
         view           = null;
         msg            = "";
         debug_mode     = false;

         do
         { 
            var cnt = args.Count();
            msg = "";

            if (cnt < 2)
            {
               error_code = 1024;
               msg = "To few parameters";
               break;
            }

            if(cnt > 0)
               tstdRtnNm = args[0];
            if (cnt > 1)
               testNum   = Convert.ToInt32(args[1]);

            if (cnt > 2)
            {
               var i = 2;
               var arg = args[i];

               if(arg.Contains("debug"))
               {
                  debug_mode = true;
                  i++;
               }

               if (cnt > i)
                  table = args[i];

               if (cnt > ++i)
                  view = args[i];
            }

            if (tstdRtnNm == "")
            {
               error_code = 1040;
               msg = "parameter 1: the tested routne qualified name must be specified";
               break;
            }

            if (testNum == 0)
            {
               error_code = 1041;
               msg = "parameter 2: the test number must be specified";
               break;
            }

            error_code = 0;
         } while (false);

         return error_code;
      }

      /// <summary>
      /// Main help message
      /// </summary>
      /// <param name="msg"></param>
      /// <returns></returns>
      private static string GetHlpMsg(string msg)
      {
         var msg_ = msg.Length>0 ? $"{msg}" : "";

         return $@"{msg_}

Syntax: 
CreateSqlTestRoutineApp.exe <tested rtn name> <test num> [<table name>] [view name]";
      }
   }
}