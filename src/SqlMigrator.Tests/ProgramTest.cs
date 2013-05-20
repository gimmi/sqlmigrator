using System;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using NUnit.Framework;
using SharpTestsEx;

namespace SqlMigrator.Tests
{
	[TestFixture]
	public class ProgramTest
	{
		private StringBuilder _sb;

		/*
		 * Uses SQL server LocalDB
		 * Database file is located at: C:\Users\<user>\AppData\Local\Microsoft\Microsoft SQL Server Local DB\Instances\v11.0
		 * see http://msdn.microsoft.com/en-us/library/hh510202.aspx
		 * 
		 * For VS2010 you need to install:
		 * SQL Server 2012 express LocalDB
		 * http://support.microsoft.com/kb/2544514
		 */
		private const string TestConnStr = @"Server=(localdb)\v11.0;Integrated Security=true;Initial Catalog=SqlMigratorTests";
		private const string SetupConnStr = @"Server=(localdb)\v11.0;Integrated Security=true;Initial Catalog=master";

		[SetUp]
		public void SetUp()
		{
			SqlConnection.ClearAllPools();
			Execute("IF DB_ID('SqlMigratorTests') IS NOT NULL DROP DATABASE SqlMigratorTests");
			Execute("CREATE DATABASE SqlMigratorTests");
			_sb = new StringBuilder();
		}

		private void Execute(string sql)
		{
			var conn = new SqlConnection(SetupConnStr);
			conn.Open();
			try
			{
				new SqlCommand(sql, conn).ExecuteNonQuery();
			}
			finally
			{
				conn.Close();
			}
		}

		private bool TableExists(string table)
		{
			var conn = new SqlConnection(TestConnStr);
			conn.Open();
			try
			{
				return new SqlCommand("SELECT OBJECT_ID('" + table + "', 'U')", conn).ExecuteScalar() != DBNull.Value;
			}
			finally
			{
				conn.Close();
			}
		}

		[Test]
		public void Functional_test()
		{
			Program.Run(new[] { "/connstr", TestConnStr, "/migrationsdir", @".\TestMigrations", "/outputfile", @".\TestScript.sql" }, new StringWriter(_sb));
			TableExists("Migrations").Should().Be.False();
			File.ReadAllText(@".\TestScript.sql").Should().Contain("CREATE TABLE Migrations");
			TableExists("Masters").Should().Be.False();
			File.ReadAllText(@".\TestScript.sql").Should().Contain("CREATE TABLE Masters");
			TableExists("Details").Should().Be.False();
			File.ReadAllText(@".\TestScript.sql").Should().Contain("CREATE TABLE Details");

			Program.Run(new[] { "/connstr", TestConnStr, "/migrationsdir", @".\TestMigrations" }, new StringWriter(_sb));
			TableExists("Migrations").Should().Be.True();
			TableExists("Masters").Should().Be.True();
			TableExists("Details").Should().Be.True();

			Program.Run(new[] { "/count", "-100", "/connstr", TestConnStr, "/migrationsdir", @".\TestMigrations" }, new StringWriter(_sb));
			TableExists("Migrations").Should().Be.True();
			TableExists("Masters").Should().Be.False();
			TableExists("Details").Should().Be.False();

			Program.Run(new[] { "/count", "1", "/connstr", TestConnStr, "/migrationsdir", @".\TestMigrations" }, new StringWriter(_sb));
			TableExists("Masters").Should().Be.True();
			TableExists("Details").Should().Be.False();

			Program.Run(new[] { "/connstr", TestConnStr, "/migrationsdir", @".\TestMigrations" }, new StringWriter(_sb));
			TableExists("Details").Should().Be.True();

			Program.Run(new[] { "/count", "-1", "/connstr", TestConnStr, "/migrationsdir", @".\TestMigrations" }, new StringWriter(_sb));
			TableExists("Masters").Should().Be.True();
			TableExists("Details").Should().Be.False();
		}

		[Test]
		public void Should_not_throw_exception_when_no_scripts_to_apply()
		{
			Program.Run(new[] {"/connstr", TestConnStr, "/migrationsdir", @".\TestMigrations" }, new StringWriter(new StringBuilder()));

			// the second run will not apply any migration
			Program.Run(new[] { "/connstr", TestConnStr, "/migrationsdir", @".\TestMigrations" }, new StringWriter(new StringBuilder()));
		}

		[Test]
		public void Should_set_timeout()
		{
			Executing.This(() => Program.Run(new[] { "/connstr", TestConnStr, "/migrationsdir", @".\TimeoutMigration", "/timeout", "2" }, new StringWriter(new StringBuilder())))
				.Should().Throw<SqlException>().And.Exception.Message.Should().Contain("Timeout expired.");
		}
	}
}