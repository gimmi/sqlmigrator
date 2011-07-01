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

		[TestFixtureSetUp]
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
			TableExists("Migrations").Should().Be.False();
			Program.Main(new[] { "/action", "init", "/connstr", TestConnStr, "/migrationsdir", @".\TestMigrations" }, new StringWriter(new StringBuilder()));
			TableExists("Migrations").Should().Be.True();

			Program.Main(new[] { "/action", "up", "/connstr", TestConnStr, "/migrationsdir", @".\TestMigrations", "/outputfile", @".\TestScript.sql" }, new StringWriter(new StringBuilder()));
			TableExists("Masters").Should().Be.False();
			File.ReadAllText(@".\TestScript.sql").Should().Contain("CREATE TABLE Masters");
			TableExists("Details").Should().Be.False();
			File.ReadAllText(@".\TestScript.sql").Should().Contain("CREATE TABLE Details");

			Program.Main(new[] { "/action", "up", "/connstr", TestConnStr, "/migrationsdir", @".\TestMigrations" }, new StringWriter(new StringBuilder()));
			TableExists("Masters").Should().Be.True();
			TableExists("Details").Should().Be.True();

			Program.Main(new[] { "/action", "down", "/connstr", TestConnStr, "/migrationsdir", @".\TestMigrations" }, new StringWriter(new StringBuilder()));
			TableExists("Masters").Should().Be.False();
			TableExists("Details").Should().Be.False();
		}
	}
}