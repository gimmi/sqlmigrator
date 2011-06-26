using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SqlMigrator
{
	public class ScriptBuilder
	{
		private readonly LogTable _logTable;

		public ScriptBuilder(LogTable logTable)
		{
			_logTable = logTable;
		}

		public string BuildUp(IEnumerable<Migration> migrations)
		{
			var ret = new StringBuilder();
			foreach(Migration migration in migrations.OrderBy(m => m.Id))
			{
				ret.AppendFormat("-- Migration {0}", migration).AppendLine()
					.AppendLine(migration.Up).AppendLine()
					.Append(_logTable.BuildInsertScript(migration)).AppendLine();
			}
			return ret.ToString();
		}

		public string BuildDown(IEnumerable<Migration> migrations)
		{
			var ret = new StringBuilder();
			foreach(Migration migration in migrations.OrderByDescending(m => m.Id))
			{
				ret.AppendFormat("-- Migration {0}", migration).AppendLine()
					.AppendLine(migration.Down).AppendLine()
					.Append(_logTable.BuildDeleteScript(migration)).AppendLine();
			}
			return ret.ToString();
		}
	}
}