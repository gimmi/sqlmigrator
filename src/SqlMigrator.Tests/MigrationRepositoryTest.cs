using System;
using System.IO;
using System.Text;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using System.Linq;

namespace SqlMigrator.Tests
{
	[TestFixture]
	public class MigrationRepositoryTest
	{
		private MigrationRepository _target;
		private string _dir;
		private ILogTable _logTable;

		[SetUp]
		public void SetUp()
		{
			_dir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

			_logTable = MockRepository.GenerateStub<ILogTable>();
			_target = new MigrationRepository(_dir, Encoding.UTF8, _logTable);

			Directory.CreateDirectory(_dir);
		}

		private string AddFile(string name, string content = "")
		{
			var file = Path.Combine(_dir, name);
			File.WriteAllText(file, content);
			return file;
		}

		[TearDown]
		public void TearDown()
		{
			Directory.Delete(_dir, true);
		}

		[Test]
		public void Shul_load_all_valid_migration_files()
		{
			AddFile("001_First_script.sql", "Up\n -- @Down \nDown");
			AddFile("002_Second_script.sql");
			AddFile("003_Third_script.sql");

			var actual = _target.GetAll();
	
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

			var actual = _target.GetAll();
	
			actual.Should().Be.Empty();
		}

		[Test]
		public void Shuld_fail_when_found_2_migration_files_with_the_same_id()
		{
			AddFile("001_First_script.sql");
			AddFile("001_Another_first_script.sql");

			Executing.This(() => _target.GetAll()).Should().Throw<ApplicationException>().And.ValueOf.Message.Should().Be.EqualTo("Found more than one migration with id #1");
		}

		[Test]
		public void Shuld_fail_when_an_applyed_migration_is_not_available()
		{
			_logTable.Stub(x => x.GetApplyedMigrations()).Return(new long[]{1});

			Executing.This(() => _target.GetApplyedMigrations()).Should().Throw<ApplicationException>().And.ValueOf.Message.Should().Be.EqualTo("Migration #1 has been applyed to database, but not found in migrations directory");
		}
	}
}