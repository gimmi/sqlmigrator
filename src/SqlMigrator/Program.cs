using System;
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
		public string ConnStr = @"Data Source=.\SQLEXPRESS;Initial Catalog=Tests;Integrated Security=True";
		public string MigrationsDir = Path.Combine(Environment.CurrentDirectory, "Migrations");
		public Action Action = Action.Up;
		public string OutputScript;
		public Encoding TextEncoding = Encoding.UTF8;
	}

	public class Program
	{
		private static readonly CommandLineParser<Options> CommandLineParser = new CommandLineParser<Options>();

		public static int Main(string[] args)
		{
			try
			{
				Options opts = CommandLineParser.Parse(args);
				IDbConnection conn = new SqlConnection(opts.ConnStr);
				var logTable = new LogTable(conn);
				var scriptBuilder = new ScriptBuilder(logTable);
				var migrationRepository = new MigrationRepository(opts.MigrationsDir, opts.TextEncoding, logTable);

				IScriptTarget scriptTarget;
				if(string.IsNullOrWhiteSpace(opts.OutputScript))
				{
					scriptTarget = new DatabaseScriptTarget(conn);
				}
				else
				{
					scriptTarget = new FileScriptTarget(opts.OutputScript, opts.TextEncoding);
				}

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