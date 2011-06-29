using System;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Text;

namespace SqlMigrator
{
	public enum Action
	{
		Up,
		Down,
		Init
	}

	public class Options
	{
		[Description(@"Required. Database connection string e.g. 'Data Source=.\SQLEXPRESS;Initial Catalog=Tests;Integrated Security=True'")]
		public string ConnStr;

		[Description(@"Required. Path of the directory containing migration files e.g. '.\Migrations'")]
		public string MigrationsDir = Path.Combine(Environment.CurrentDirectory, "Migrations");

		[Description(@"Required. Te action to execute: Up, Down, Init")]
		public Action Action = Action.Up;

		[Description(@"If specified, script will be written to this file instead of executed against DB")]
		public string OutputScript;

		[Description(@"Encoding for migration files, default to UTF8")]
		public Encoding TextEncoding = Encoding.UTF8;
	}

	public class Program
	{
		private static readonly CommandLineParser<Options> CommandLineParser = new CommandLineParser<Options>();

		public static int Main(string[] args)
		{
			Console.WriteLine("SqlMigrator {0}", typeof(Program).Assembly.GetName().Version);
			if(args.Length == 0)
			{
				CommandLineParser.PrintHelp(Console.Out);
				return 0;
			}
			try
			{
				Options opts = CommandLineParser.Parse(args);
				IDbConnection conn = new SqlConnection(opts.ConnStr);
				var logTable = new LogTable(conn);
				var scriptBuilder = new ScriptBuilder(logTable);
				var migrationRepository = new MigrationRepository(opts.MigrationsDir, opts.TextEncoding, logTable);

				string script = null;
				if(opts.Action == Action.Up)
				{
					script = scriptBuilder.BuildUp(migrationRepository.GetPendingMigrations());
				}
				else if(opts.Action == Action.Down)
				{
					int to = 0;
					script = scriptBuilder.BuildDown(migrationRepository.GetApplyedMigrations(to, long.MaxValue));
				}
				else if(opts.Action == Action.Init)
				{
					script = logTable.BuildCreateScript();
				}

				IScriptTarget scriptTarget;
				if (string.IsNullOrWhiteSpace(opts.OutputScript))
				{
					scriptTarget = new DatabaseScriptTarget(conn);
				}
				else
				{
					scriptTarget = new FileScriptTarget(opts.OutputScript, opts.TextEncoding);
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