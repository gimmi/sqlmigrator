using System.Collections.Generic;

namespace SqlMigrator
{
	public interface ILogTable
	{
		bool IsMigrationPending(Migration migration);
		IEnumerable<long> GetApplyedMigrations(long fromId, long toId);
		string BuildDeleteScript(Migration migration);
		string BuildInsertScript(Migration migration);
		string BuildCreateScript();
	}
}