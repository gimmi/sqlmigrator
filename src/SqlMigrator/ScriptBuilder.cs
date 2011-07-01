using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SqlMigrator
{
	public class ScriptBuilder
	{
		private readonly ILogTable _logTable;

		public ScriptBuilder(ILogTable logTable)
		{
			_logTable = logTable;
		}

		public string BuildUp(IEnumerable<Migration> migrations, int count)
		{
			var ret = new StringBuilder();
			foreach(Migration migration in migrations.OrderBy(m => m.Id).Take(count))
			{
				ret.AppendFormat("-- Migration {0}", migration).AppendLine()
					.AppendLine(migration.Up)
					.AppendLine(_logTable.BuildInsertScript(migration));
			}
			return ret.ToString();
		}

		public string BuildDown(IEnumerable<Migration> migrations, int count)
		{
			var ret = new StringBuilder();
			foreach (Migration migration in migrations.OrderByDescending(m => m.Id).Take(count))
			{
				ret.AppendFormat("-- Migration {0}", migration).AppendLine()
					.AppendLine(migration.Down)
					.AppendLine(_logTable.BuildDeleteScript(migration));
			}
			return ret.ToString();
		}
	}
}