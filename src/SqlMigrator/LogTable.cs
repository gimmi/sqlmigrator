using System.Collections.Generic;
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

		public IEnumerable<long> GetApplyedMigrations(long fromId, long toId)
		{
			IDbCommand cmd = _conn.CreateCommand();
			cmd.CommandText = string.Format("SELECT Id FROM Migrations WHERE Id BETWEEN {0} AND {1}", fromId, toId);
			var ret = new List<long>();
			using(var rdr = cmd.ExecuteReader())
			{
				while(rdr.Read())
				{
					ret.Add((long)rdr[0]);
				}
			}
			return ret;
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