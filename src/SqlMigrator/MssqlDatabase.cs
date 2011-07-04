using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace SqlMigrator
{
	public class MssqlDatabase : IDatabase
	{
		private readonly string _connstr;

		public MssqlDatabase(string connstr)
		{
			_connstr = connstr;
		}

		public bool IsMigrationPending(Migration migration)
		{
			var conn = new SqlConnection(_connstr);
			conn.Open();
			try
			{
				IDbCommand cmd = conn.CreateCommand();
				cmd.CommandText = string.Format("SELECT COUNT(*) FROM Migrations WHERE Id = {0}", migration.Id);
				return (int)cmd.ExecuteScalar() < 1;
			}
			finally
			{
				conn.Close();
			}
		}

		public IEnumerable<long> GetApplyedMigrations()
		{
			var conn = new SqlConnection(_connstr);
			conn.Open();
			try
			{
				IDbCommand cmd = conn.CreateCommand();
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
				conn.Close();
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
			return @"CREATE TABLE Migrations([Id] BIGINT PRIMARY KEY NOT NULL, [Date] DATETIME NOT NULL DEFAULT GETDATE(), [User] NVARCHAR(128) NOT NULL DEFAULT SUSER_NAME(), [Host] NVARCHAR(128) NOT NULL DEFAULT HOST_NAME())";
		}

		public void Execute(string script)
		{
			var conn = new SqlConnection(_connstr);
			conn.Open();
			try
			{
				IDbTransaction tran = conn.BeginTransaction();
				try
				{
					IDbCommand cmd = conn.CreateCommand();
					cmd.Transaction = tran;
					cmd.CommandText = script;
					cmd.ExecuteNonQuery();
					tran.Commit();
				}
				catch
				{
					tran.Rollback();
					throw;
				}
			}
			finally
			{
				conn.Close();
			}
		}
	}
}