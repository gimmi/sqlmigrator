using System.Data;

namespace SqlMigrator
{
	public class LogTable
	{
		private readonly IDbConnection _conn;

		public LogTable(IDbConnection conn)
		{
			_conn = conn;
		}

		public bool IsMigrationPending(Migration migration)
		{
			IDbCommand cmd = _conn.CreateCommand();
			cmd.CommandText = string.Format("SELECT COUNT(*) FROM Migrations WHERE Id = {0}", migration.Id);
			return (int)cmd.ExecuteScalar() < 1;
		}

		public string BuildDeleteScript(Migration migration)
		{
			return string.Format("DELETE Migrations WHERE Id = {0}", migration.Id);
		}

		public string BuildInsertScript(Migration migration)
		{
			return string.Format("INSERT INTO Migrations(Id) VALUES({0})", migration.Id);
		}

		public string BuildCreateScript()
		{
			return @"CREATE TABLE Migrations(Id BIGINT PRIMARY KEY NOT NULL)";
		}
	}
}