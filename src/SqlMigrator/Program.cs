using System;
using System.Data;
using System.Data.SqlClient;

namespace SqlMigrator
{
	public class Program
	{
		public static int Main(string[] args)
		{
			try
			{
				var commandLineParser = new CommandLineParser<Options>();
				Console.WriteLine("SqlMigrator {0}", typeof(Program).Assembly.GetName().Version);
				if(args.Length == 0)
				{
					Console.WriteLine("Command line parameters:");
					commandLineParser.PrintHelp(Console.Out);
					return 0;
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
				return 0;
			}
			catch(Exception e)
			{
				Console.WriteLine(e);
				return 1;
			}
		}
	}
}