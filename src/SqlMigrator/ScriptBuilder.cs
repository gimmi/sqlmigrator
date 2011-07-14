using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SqlMigrator
{
	public class ScriptBuilder
	{
		private readonly IDatabase _database;
		private readonly TextWriter _log;

		public ScriptBuilder(IDatabase database, TextWriter log)
		{
			_database = database;
			_log = log;
		}

		private StringBuilder CreateStringBuilder()
		{
			var ret = new StringBuilder();
			if(!_database.MigrationsTableExists())
			{
				_log.WriteLine("Adding migrations table creation to script");
				ret.AppendLine("-- Migrations table creation")
					.Append(_database.BuildCreateScript())
					.AppendLine(_database.GetStatementDelimiter());
			}
			return ret;
		}

		public string BuildUp(IEnumerable<Migration> migrations, int count)
		{
			var ret = CreateStringBuilder();
			foreach(Migration migration in migrations.OrderBy(m => m.Id).Take(count))
			{
				_log.WriteLine("Adding migration {0} to script", migration);
				ret.AppendFormat("-- Migration {0}", migration).AppendLine()
					.AppendLine(migration.Up)
					.Append(_database.BuildInsertScript(migration))
					.AppendLine(_database.GetStatementDelimiter());
			}
			return ret.ToString();
		}

		public string BuildDown(IEnumerable<Migration> migrations, int count)
		{
			var ret = CreateStringBuilder();
			foreach (Migration migration in migrations.OrderByDescending(m => m.Id).Take(count))
			{
				_log.WriteLine("Adding migration {0} to script", migration);
				ret.AppendFormat("-- Migration {0}", migration).AppendLine()
					.AppendLine(migration.Down)
					.Append(_database.BuildDeleteScript(migration))
					.AppendLine(_database.GetStatementDelimiter());
			}
			return ret.ToString();
		}
	}
}