using System;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;

namespace SqlMigrator
{
	public class Program
	{
		public static int Main(string[] args)
		{
			try
			{
				Main(args, Console.Out);
				return 0;
			}
			catch(Exception e)
			{
				Console.WriteLine(e);
				return 1;
			}
		}

		public static void Main(string[] args, TextWriter log)
		{
			log.WriteLine("{0} {1}", typeof(Program).Assembly.GetCustomAttributes(typeof(AssemblyProductAttribute), false).Cast<AssemblyProductAttribute>().Single().Product, typeof(Program).Assembly.GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute), false).Cast<AssemblyInformationalVersionAttribute>().Single().InformationalVersion);
			log.WriteLine(typeof(Program).Assembly.GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false).Cast<AssemblyDescriptionAttribute>().Single().Description);
			log.WriteLine(typeof(Program).Assembly.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false).Cast<AssemblyCopyrightAttribute>().Single().Copyright);

			var commandLineParser = new CommandLineParser<Options>();
			if (args.Length == 0)
			{
				log.WriteLine("Command line parameters:");
				commandLineParser.PrintHelp(log);
				return;
			}
			Options opts = commandLineParser.Parse(args);
			IDbConnection conn = new SqlConnection(opts.ConnStr);
			var db = new Database(conn);
			var scriptBuilder = new ScriptBuilder(db);
			var migrationRepository = new MigrationRepository(opts.MigrationsDir, opts.TextEncoding, db);

			string script = null;
			if(opts.Action == Action.Up)
			{
				script = scriptBuilder.BuildUp(migrationRepository.GetPendingMigrations(), opts.Count);
			}
			else if(opts.Action == Action.Down)
			{
				script = scriptBuilder.BuildDown(migrationRepository.GetApplyedMigrations(), opts.Count);
			}
			else if(opts.Action == Action.Init)
			{
				script = db.BuildCreateScript();
			}

			IScriptTarget scriptTarget;
			if(string.IsNullOrWhiteSpace(opts.OutputFile))
			{
				scriptTarget = new DatabaseScriptTarget(conn);
			}
			else
			{
				scriptTarget = new FileScriptTarget(opts.OutputFile, opts.TextEncoding);
			}
			scriptTarget.Execute(script);
		}
	}
}