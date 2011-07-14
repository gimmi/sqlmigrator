using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;

namespace SqlMigrator.Tests
{
	[TestFixture]
	public class MigrationRepositoryTest
	{
		private MigrationRepository _target;
		private string _dir;
		private IDatabase _database;

		[SetUp]
		public void SetUp()
		{
			_dir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

			_database = MockRepository.GenerateStub<IDatabase>();
			_target = new MigrationRepository(_dir, Encoding.UTF8, _database);

			Directory.CreateDirectory(_dir);
		}

		private string AddFile(string name, string content = "")
		{
			string file = Path.Combine(_dir, name);
			File.WriteAllText(file, content);
			return file;
		}

		[TearDown]
		public void TearDown()
		{
			Directory.Delete(_dir, true);
		}

		[Test]
		public void Shuld_load_all_valid_migration_files()
		{
			AddFile("001_First_script.sql", "Up\n -- @Down \nDown");
			AddFile("002_Second_script.sql");
			AddFile("003_Third_script.sql");

			IDictionary<long, Migration> actual = _target.GetAll();

			actual.Keys.Should().Have.SameValuesAs(new long[] { 1, 2, 3 });
			actual[1].Id.Should().Be.EqualTo(1);
			actual[1].Up.Should().Be.EqualTo("Up\n");
			actual[1].Down.Should().Be.EqualTo("\nDown");
			actual[2].Id.Should().Be.EqualTo(2);
			actual[3].Id.Should().Be.EqualTo(3);
		}

		[Test]
		public void Shuld_skip_non_migration_files()
		{
			AddFile("004_Text_file.txt");
			AddFile("Text_file.txt");

			IDictionary<long, Migration> actual = _target.GetAll();

			actual.Should().Be.Empty();
		}

		[Test]
		public void Shuld_fail_when_found_2_migration_files_with_the_same_id()
		{
			AddFile("001_First_script.sql");
			AddFile("001_Another_first_script.sql");

			Executing.This(() => _target.GetAll()).Should().Throw<ApplicationException>().And.ValueOf.Message.Should().Be.EqualTo("Found more than one migration with id #1");
		}
	}
}