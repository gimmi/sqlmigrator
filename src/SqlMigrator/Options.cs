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

		[Description(@"Te action to execute. Available actions are 'Up', 'Down' and 'Init'. Default to 'Up'")]
		public Action Action = Action.Up;

		[Description(@"If specified, script will be written to this file instead of executed against DB")]
		public string OutputFile;

		[Description(@"Encoding for migration files, default to UTF8")]
		public Encoding TextEncoding = Encoding.UTF8;

		[Description(@"Number of migrations to consider, default to all available migrations")]
		public int Count = int.MaxValue;
	}
}