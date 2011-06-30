using System.Collections.Generic;
using System.Data;

namespace SqlMigrator
{
	public class LogTable : ILogTable
	{
		private readonly IDbConnection _conn;

		public LogTable(IDbConnection conn)
		{
			_conn = conn;
		}

		public bool IsMigrationPending(Migration migration)
		{
			_conn.Open();
			try
			{
				IDbCommand cmd = _conn.CreateCommand();
				cmd.CommandText = string.Format("SELECT COUNT(*) FROM Migrations WHERE Id = {0}", migration.Id);
				return (int)cmd.ExecuteScalar() < 1;
			}
			finally
			{
				_conn.Close();
			}
		}

		public IEnumerable<long> GetApplyedMigrations()
		{
			_conn.Open();
			try
			{
				IDbCommand cmd = _conn.CreateCommand();
				cmd.CommandText = "SELECT Id FROM Migrations";
				var ret = new List<long>();
				using(IDataReader rdr = cmd.ExecuteReader())
				{
					while(rdr.Read())
					{
						ret.Add((long)rdr[0]);
					}
				}
				return ret;
			}
			finally
			{
				_conn.Close();
			}
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