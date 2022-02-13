// See https://aka.ms/new-console-template for more information

//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Threading.Tasks;
//using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
//using System.Configuration;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging; // ILogger ILoggerFactory LoggerFactoryILoggerProvider .Get<Log4NetProviderOptions>();
using Serilog;                      // ILogger 
//using Serilog.Core;
using Serilog.Events;
using CreateSqlTestRoutineLib;
using Microsoft.Extensions.DependencyInjection;
using log4net;

namespace CreateSqlTestRoutineApp
{ 
   public class Program
   {
      private static readonly ILog log = LogManager.GetLogger("ConsoleAppender");

      // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
      public void Configure(Microsoft.AspNetCore.Builder.IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
      {
         // Add these lines
         var loggingOptions = configuration?.GetSection("Log4NetCore").Get<Log4NetProviderOptions>();

         loggerFactory.AddLog4Net(loggingOptions);
         Common.Logger.InitLogger();
      }

      public static IConfigurationRoot? configuration;/// <summary>

      static void Main(string[] args)
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
         Log.Information("Creating service collection");
         ServiceCollection serviceCollection = new ServiceCollection();
         ConfigureServices(serviceCollection);

         // Create service provider
         Log.Information("Building service provider");
         IServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();

         if(configuration == null)
            throw new Exception("Failed to initialise appsettings from the appsetting.json file");

         //log.Debug("Did it again!");
         //Common.Logger.InitLogger();
         // Print connection string to demonstrate configuration object is populated
         var conn_str = configuration.GetConnectionString("Default");
         Console.WriteLine(conn_str);
         var settings_name = configuration["Settings name"];         // Get("Settings name");

         if(string.IsNullOrWhiteSpace(settings_name))
            throw new Exception("Failed to  appsettings name from the appsetting.json file");

         if(!settings_name.Equals("CreateTestApp settings"))
            throw new Exception($"Error loading appsettings configuration file, expected name to be [CreateTestApp settings] " +
"but is [{settings_name}]");


         /*
          * args: 
          * 0: tested rtn:
          * 1: test num
          * 2: description
          * 3: table
          * 4: crt_rtn
          */
         ValidateParams(
                          args
                        , tstdRtnNm  : out string tstdRtnNm
                        , testNum    : out int    testNum
                        , table      : out string table
                        , view       : out string view
                        , crt_sp     : out string crt_sp
                        );
         //               , out string rtnType
         //               , out string tstSpName


         Console.WriteLine($"SqlTestCreator tested rtn: [{tstdRtnNm}], testNum: {testNum}, table: [{table}], crt_sp: [{crt_sp}]");
         var testCreator = new SqlTestCreator();//rtnType: rtnType, test_sp: tstSpName, testedRtn: tstdRtnNm, description: description, table: table, crt_sp: crt_sp);

         var ret = testCreator.__CrtTestMethods
            (
                qTstdRtnNm: tstdRtnNm
               ,testNum: testNum
               ,msg: out var msg
               ,table: table
               ,view: view
               ,out string hlpr_script
               ,out string mn_script
            );

         File.WriteAllText($"hlpr_{testNum,3}_{tstdRtnNm}.sql", hlpr_script);
         File.WriteAllText($"test_{testNum,3}_{tstdRtnNm}_mn_script.sql", mn_script);
         Console.WriteLine( );
      }

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
         configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetParent(AppContext.BaseDirectory)?.FullName ?? "oops")
            .AddJsonFile("appsettings.json", false)
            .AddXmlFile("Tests.dll.config")
            .Build();

         // Add access to generic IConfigurationRoot
         serviceCollection.AddSingleton<IConfigurationRoot>(configuration);

         // Add app
         //serviceCollection.AddTransient<App>();
      }

      /// args: 
      ///  0: tested rtn:
      ///  1: test num
      ///  2: description
      ///  3: table
      ///  4: crt_rtn
      /// </summary>
      private static void ValidateParams(
       string[] args
      , out string tstdRtnNm
      , out int    testNum
      , out string table
      , out string view
      , out string crt_sp
      )
      {
         var cnt = args.Count();

         if (cnt < 3)
            throw new ArgumentException(GetHlpMsg("Too few arguments"));

         tstdRtnNm   = cnt > 0 ? args[0] : "tested rtn nm: <TBA>";
         testNum     = cnt > 1 ? Convert.ToInt32(args[1]) : 1;
         table       = cnt > 2 ? args[2] : "table nm: <TBA>";
         view        = cnt > 3 ? args[3] : "crt_sp  <TBA>";
         crt_sp      = cnt > 4 ? args[4] : "crt_sp  <TBA>";
      }

      private static string GetHlpMsg(string msg)
      {
         return $@"Syntax: 
CreateSqlTest.exe <tst rtn type> <test sp name> <tested rtn name> <description> [<table name>] [create sp name]

Where 
<tst rtn type> can be 1 of:
   default
   crt
   get1
   getAll
   del
";
      }
   }
}