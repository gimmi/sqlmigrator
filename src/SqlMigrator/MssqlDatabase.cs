using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SqlMigrator
{
	public class MssqlDatabase : IDatabase
	{
		private readonly string _connstr;
		private readonly string _databaseName;
		private readonly int _commandTimeout;
		private readonly string _migrationsTableName;

		public MssqlDatabase(string connstr, string databaseName, int commandTimeout, string migrationsTableName)
		{
			_connstr = connstr;
			_databaseName = databaseName;
			_commandTimeout = commandTimeout;
			_migrationsTableName = migrationsTableName;
		}

		public bool MigrationsTableExists()
		{
			using (var conn = OpenConnectionAndChangeDb())
			{
				return new SqlCommand(string.Concat("SELECT OBJECT_ID('", _migrationsTableName, "', 'U')"), conn).ExecuteScalar() != DBNull.Value;
			}
		}

		public bool IsMigrationPending(Migration migration)
		{
			using (var conn = OpenConnectionAndChangeDb())
			{
				IDbCommand cmd = conn.CreateCommand();
				cmd.CommandText = string.Concat("SELECT COUNT(*) FROM [", _migrationsTableName, "] WHERE Id = ", migration.Id);
				return (int)cmd.ExecuteScalar() < 1;
			}
		}

		public IEnumerable<long> GetApplyedMigrations()
		{
			using (var conn = OpenConnectionAndChangeDb())
			{
				IDbCommand cmd = conn.CreateCommand();
				cmd.CommandText = string.Concat("SELECT Id FROM [", _migrationsTableName, "]");
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
		}

		public string BuildDeleteScript(Migration migration)
		{
			return BuildSqlScript("DELETE [", _migrationsTableName, "] WHERE Id = ", migration.Id);
		}

		public string BuildInsertScript(Migration migration)
		{
			return BuildSqlScript("INSERT INTO [", _migrationsTableName, "](Id) VALUES(", migration.Id, ")");
		}

		public string BuildCreateScript()
		{
			return BuildSqlScript(@"CREATE TABLE [", _migrationsTableName, "]([Id] BIGINT PRIMARY KEY NOT NULL, [Date] DATETIME NOT NULL DEFAULT GETDATE(), [User] NVARCHAR(128) NOT NULL DEFAULT SUSER_NAME(), [Host] NVARCHAR(128) NOT NULL DEFAULT HOST_NAME())");
		}

		public void Execute(string batch)
		{
			using(var conn = OpenConnectionAndChangeDb())
			{
				SqlTransaction tran = conn.BeginTransaction();
				try
				{
					foreach(string script in Regex.Split(batch, @"^\s*GO\s*$", RegexOptions.IgnoreCase | RegexOptions.Multiline).Where(s => !string.IsNullOrWhiteSpace(s)))
					{
						new SqlCommand(script, conn, tran) { CommandTimeout = _commandTimeout }.ExecuteNonQuery();
					}
					tran.Commit();
				}
				catch
				{
					tran.Rollback();
					throw;
				}
			}
		}

		private SqlConnection OpenConnectionAndChangeDb()
		{
			var conn = new SqlConnection(_connstr);
			conn.Open();
			if (!string.IsNullOrWhiteSpace(_databaseName))
			{
				conn.ChangeDatabase(_databaseName);
			}
			return conn;
		}

		private string BuildSqlScript(params object[] parts)
		{
			var ret = new StringBuilder();
			if (!string.IsNullOrWhiteSpace(_databaseName))
			{
				ret.AppendFormat("USE [{0}]", _databaseName).AppendLine()
					.AppendLine("GO");
			}
			return ret.Append(string.Concat(parts)).ToString();
		}

		public string GetStatementDelimiter()
		{
			return Environment.NewLine + "GO";
		}
	}
}