using System;
using System.Data.SqlClient;
using System.IO;
using NUnit.Framework;
using SharpTestsEx;

namespace SqlMigrator.Tests
{
	[TestFixture]
	public class ProgramTest
	{
		/*
		 * Uses SQL server LocalDB
		 * Database file is located at: C:\Users\<user>\AppData\Local\Microsoft\Microsoft SQL Server Local DB\Instances\v11.0
		 * see http://msdn.microsoft.com/en-us/library/hh510202.aspx
		 * 
		 * For VS2010 you need to install:
		 * SQL Server 2012 express LocalDB
		 * http://support.microsoft.com/kb/2544514
		 */
		private const string ConnStr = @"Server=(localdb)\v11.0;Integrated Security=true";

		[Test]
		public void Functional_test()
		{
			DropDatabaseIfExists("SqlMigratorTests");
			CreateDatabase("SqlMigratorTests");

			Program.Run(new[] { "/connstr", ConnStr, "/dbname", "SqlMigratorTests", "/migrationsdir", @".\TestMigrations", "/outputfile", @".\TestScript.sql" }, TextWriter.Null);
			TableExists("SqlMigratorTests", "Migrations").Should().Be.False();
			File.ReadAllText(@".\TestScript.sql").Should().Contain("CREATE TABLE Migrations");
			TableExists("SqlMigratorTests", "Masters").Should().Be.False();
			File.ReadAllText(@".\TestScript.sql").Should().Contain("CREATE TABLE Masters");
			TableExists("SqlMigratorTests", "Details").Should().Be.False();
			File.ReadAllText(@".\TestScript.sql").Should().Contain("CREATE TABLE Details");

			Program.Run(new[] { "/connstr", ConnStr, "/dbname", "SqlMigratorTests", "/migrationsdir", @".\TestMigrations" }, TextWriter.Null);
			TableExists("SqlMigratorTests", "Migrations").Should().Be.True();
			TableExists("SqlMigratorTests", "Masters").Should().Be.True();
			TableExists("SqlMigratorTests", "Details").Should().Be.True();

			Program.Run(new[] { "/count", "-100", "/connstr", ConnStr, "/dbname", "SqlMigratorTests", "/migrationsdir", @".\TestMigrations" }, TextWriter.Null);
			TableExists("SqlMigratorTests", "Migrations").Should().Be.True();
			TableExists("SqlMigratorTests", "Masters").Should().Be.False();
			TableExists("SqlMigratorTests", "Details").Should().Be.False();

			Program.Run(new[] { "/count", "1", "/connstr", ConnStr, "/dbname", "SqlMigratorTests", "/migrationsdir", @".\TestMigrations" }, TextWriter.Null);
			TableExists("SqlMigratorTests", "Masters").Should().Be.True();
			TableExists("SqlMigratorTests", "Details").Should().Be.False();

			Program.Run(new[] { "/connstr", ConnStr, "/dbname", "SqlMigratorTests", "/migrationsdir", @".\TestMigrations" }, TextWriter.Null);
			TableExists("SqlMigratorTests", "Details").Should().Be.True();

			Program.Run(new[] { "/count", "-1", "/connstr", ConnStr, "/dbname", "SqlMigratorTests", "/migrationsdir", @".\TestMigrations" }, TextWriter.Null);
			TableExists("SqlMigratorTests", "Masters").Should().Be.True();
			TableExists("SqlMigratorTests", "Details").Should().Be.False();
		}

		[Test]
		public void Should_use_database_from_connstr_when_database_not_specified()
		{
			DropDatabaseIfExists("SqlMigratorTests");
			CreateDatabase("SqlMigratorTests");
			const string connStrWithDatabase = ConnStr + ";Initial Catalog=SqlMigratorTests";

			Program.Run(new[] { "/connstr", connStrWithDatabase, "/migrationsdir", @".\TestMigrations" }, TextWriter.Null);
			
			TableExists("SqlMigratorTests", "Migrations").Should().Be.True();
			TableExists("SqlMigratorTests", "Masters").Should().Be.True();
			TableExists("SqlMigratorTests", "Details").Should().Be.True();
		}

		[Test]
		public void Should_not_throw_exception_when_no_scripts_to_apply()
		{
			DropDatabaseIfExists("SqlMigratorTests");
			CreateDatabase("SqlMigratorTests");

			Program.Run(new[] { "/connstr", ConnStr, "/dbname", "SqlMigratorTests", "/migrationsdir", @".\TestMigrations" }, TextWriter.Null);

			// the second run will not apply any migration
			Program.Run(new[] { "/connstr", ConnStr, "/dbname", "SqlMigratorTests", "/migrationsdir", @".\TestMigrations" }, TextWriter.Null);
		}

		[Test]
		public void Should_set_timeout()
		{
			DropDatabaseIfExists("SqlMigratorTests");
			CreateDatabase("SqlMigratorTests");

			Executing.This(() => Program.Run(new[] { "/connstr", ConnStr, "/dbname", "SqlMigratorTests", "/migrationsdir", @".\TimeoutMigration", "/timeout", "2" }, TextWriter.Null))
				.Should().Throw<SqlException>().And.Exception.Message.Should().Contain("Timeout expired.");
		}

		private void CreateDatabase(string database)
		{
			ExecuteNonQuery(string.Concat("CREATE DATABASE ", database));
		}

		private void DropDatabaseIfExists(string database)
		{
			SqlConnection.ClearAllPools();
			ExecuteNonQuery(string.Format("IF DB_ID('{0}') IS NOT NULL DROP DATABASE {0}", database));
		}

		private void ExecuteNonQuery(string sql, string database = null)
		{
			var conn = new SqlConnection(ConnStr);
			conn.Open();
			try
			{
				if (!string.IsNullOrWhiteSpace(database))
				{
					conn.ChangeDatabase(database);
				}
				new SqlCommand(sql, conn).ExecuteNonQuery();
			}
			finally
			{
				conn.Close();
			}
		}

		private bool TableExists(string database, string table)
		{
			return ExecuteScalar("SELECT OBJECT_ID('" + table + "', 'U')", database) != DBNull.Value;
		}

		private object ExecuteScalar(string sql, string database = null)
		{
			var conn = new SqlConnection(ConnStr);
			conn.Open();
			try
			{
				if (!string.IsNullOrWhiteSpace(database))
				{
					conn.ChangeDatabase(database);
				}
				return new SqlCommand(sql, conn).ExecuteScalar();
			}
			finally
			{
				conn.Close();
			}
		}
	}
}