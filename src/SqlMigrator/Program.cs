using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;

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
			var commandLineParser = new CommandLineParser<Options>();
			log.WriteLine("SqlMigrator {0}", typeof(Program).Assembly.GetName().Version);
			if(args.Length == 0)
			{
				log.WriteLine("Command line parameters:");
				commandLineParser.PrintHelp(log);
				return;
			}
			Options opts = commandLineParser.Parse(args);
			IDbConnection conn = new SqlConnection(opts.ConnStr);
			var logTable = new LogTable(conn);
			var scriptBuilder = new ScriptBuilder(logTable);
			var migrationRepository = new MigrationRepository(opts.MigrationsDir, opts.TextEncoding, logTable);

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
				script = logTable.BuildCreateScript();
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