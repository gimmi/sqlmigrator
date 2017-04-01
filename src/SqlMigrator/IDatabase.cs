using System.Collections.Generic;

namespace SqlMigrator
{
	public interface IDatabase
	{
		bool IsMigrationPending(Migration migration);
		IEnumerable<long> GetApplyedMigrations();
		string BuildDeleteScript(Migration migration);
		string BuildInsertScript(Migration migration);
		string BuildCreateScript();
		void Execute(string batch);
        void Execute(IEnumerable<string> batch);
        void Execute(Migration migration, Direction upDown);
        void Execute(IEnumerable<Migration> migrations, Direction upDown);
        bool MigrationsTableExists();
		string GetStatementDelimiter();
	}
}