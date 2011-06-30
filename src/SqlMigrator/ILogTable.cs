using System.Collections.Generic;

namespace SqlMigrator
{
	public interface ILogTable
	{
		bool IsMigrationPending(Migration migration);
		IEnumerable<long> GetApplyedMigrations();
		string BuildDeleteScript(Migration migration);
		string BuildInsertScript(Migration migration);
		string BuildCreateScript();
	}
}