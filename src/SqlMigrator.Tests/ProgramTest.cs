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
		private const string TestConnStr = @"Data Source=.\SQLEXPRESS;Initial Catalog=SqlMigratorTests;Integrated Security=True";
		private const string SetupConnStr = @"Data Source=.\SQLEXPRESS;Initial Catalog=master;Integrated Security=True";

		[SetUp]
		public void TestFixtureSetUp()
		{
			SqlConnection.ClearAllPools();
			Execute("IF DB_ID('SqlMigratorTests') IS NOT NULL DROP DATABASE SqlMigratorTests");
			Execute("CREATE DATABASE SqlMigratorTests");
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
			Program.Run(new[] { "/action", "up", "/connstr", TestConnStr, "/migrationsdir", @".\TestMigrations", "/outputfile", @".\TestScript.sql" }, new StringWriter(new StringBuilder()));
			TableExists("Migrations").Should().Be.False();
			File.ReadAllText(@".\TestScript.sql").Should().Contain("CREATE TABLE Migrations");
			TableExists("Masters").Should().Be.False();
			File.ReadAllText(@".\TestScript.sql").Should().Contain("CREATE TABLE Masters");
			TableExists("Details").Should().Be.False();
			File.ReadAllText(@".\TestScript.sql").Should().Contain("CREATE TABLE Details");

			Program.Run(new[] { "/action", "up", "/connstr", TestConnStr, "/migrationsdir", @".\TestMigrations" }, new StringWriter(new StringBuilder()));
			TableExists("Migrations").Should().Be.True();
			TableExists("Masters").Should().Be.True();
			TableExists("Details").Should().Be.True();

			Program.Run(new[] { "/action", "down", "/connstr", TestConnStr, "/migrationsdir", @".\TestMigrations" }, new StringWriter(new StringBuilder()));
			TableExists("Migrations").Should().Be.True();
			TableExists("Masters").Should().Be.False();
			TableExists("Details").Should().Be.False();

			Program.Run(new[] { "/action", "up", "/count", "1", "/connstr", TestConnStr, "/migrationsdir", @".\TestMigrations" }, new StringWriter(new StringBuilder()));
			TableExists("Masters").Should().Be.True();
			TableExists("Details").Should().Be.False();

			Program.Run(new[] { "/action", "up", "/connstr", TestConnStr, "/migrationsdir", @".\TestMigrations" }, new StringWriter(new StringBuilder()));
			TableExists("Details").Should().Be.True();

			Program.Run(new[] { "/action", "down", "/count", "1", "/connstr", TestConnStr, "/migrationsdir", @".\TestMigrations" }, new StringWriter(new StringBuilder()));
			TableExists("Masters").Should().Be.True();
			TableExists("Details").Should().Be.False();
		}

		[Test]
		public void Should_not_throw_exception_when_no_scripts_to_apply()
		{
			Program.Run(new[] { "/action", "up", "/connstr", TestConnStr, "/migrationsdir", @".\TestMigrations" }, new StringWriter(new StringBuilder()));

			// the second run will not apply any migration
			Program.Run(new[] { "/action", "up", "/connstr", TestConnStr, "/migrationsdir", @".\TestMigrations" }, new StringWriter(new StringBuilder()));
		}
	}
}