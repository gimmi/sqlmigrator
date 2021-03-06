using System;
using System.ComponentModel;
using System.IO;
using System.Text;

namespace SqlMigrator
{
	public class Options
	{
		[Description(@"Required. Database connection string e.g. 'Data Source=.\SQLEXPRESS;Initial Catalog=Tests;Integrated Security=True'")]
		public string ConnStr;

		[Description(@"Path of the directory containing migration files. Default to '.\Migrations'")]
		public string MigrationsDir = Path.Combine(Environment.CurrentDirectory, "Migrations");

		[Description(@"If specified, script will be written to this file instead of executed against DB")]
		public string OutputFile;

		[Description(@"Encoding for migration files, default to UTF8")]
		public Encoding TextEncoding = Encoding.UTF8;

		[Description(@"Migrations to apply e.g. 2 to apply the next 2 migrations, -2 to rollback the last 2 migrations")]
		public int Count = int.MaxValue;

		[Description(@"Timeout for command statements in seconds, default to 30")]
		public int Timeout = 30;

		[Description(@"The database to switch to after connection, default to none meaning that no database switch is performed.")]
		public string DbName;

		[Description(@"The name of the migrations table, default to 'Migrations'")]
		public string MigrationsTableName = "Migrations";
	}
}