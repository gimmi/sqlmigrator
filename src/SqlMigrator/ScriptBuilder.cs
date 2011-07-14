using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SqlMigrator
{
	public class ScriptBuilder
	{
		private readonly IDatabase _database;

		public ScriptBuilder(IDatabase database)
		{
			_database = database;
		}

		private StringBuilder CreateStringBuilder()
		{
			var ret = new StringBuilder();
			if(!_database.MigrationsTableExists())
			{
				ret.AppendLine("-- Migrations table creation")
					.AppendLine(_database.BuildCreateScript());
			}
			return ret;
		}

		public string BuildUp(IEnumerable<Migration> migrations, int count)
		{
			var ret = CreateStringBuilder();
			foreach(Migration migration in migrations.OrderBy(m => m.Id).Take(count))
			{
				ret.AppendFormat("-- Migration {0}", migration).AppendLine()
					.AppendLine(migration.Up)
					.AppendLine(_database.BuildInsertScript(migration));
			}
			return ret.ToString();
		}

		public string BuildDown(IEnumerable<Migration> migrations, int count)
		{
			var ret = CreateStringBuilder();
			foreach (Migration migration in migrations.OrderByDescending(m => m.Id).Take(count))
			{
				ret.AppendFormat("-- Migration {0}", migration).AppendLine()
					.AppendLine(migration.Down)
					.AppendLine(_database.BuildDeleteScript(migration));
			}
			return ret.ToString();
		}
	}
}