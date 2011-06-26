using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;

namespace SqlMigrator
{
	public class Program
	{
		public static int Main(string[] args)
		{
			var conn = new SqlConnection("");
			var logTable = new LogTable(conn);
			var migrationFactory = new MigrationRepository(".", Encoding.UTF8, logTable);
			IScriptTarget scriptTarget = new DatabaseScriptTarget(conn);
			var scriptBuilder = new ScriptBuilder(logTable);

			IEnumerable<Migration> migrations = migrationFactory.GetPendingMigrations();
			string script = scriptBuilder.BuildUp(migrations);
			try
			{
				scriptTarget.Execute(script);
			}
			catch(Exception e)
			{
				Console.WriteLine(e);
				return 1;
			}
			return 0;
		}
	}
}