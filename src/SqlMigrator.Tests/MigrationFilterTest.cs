using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;

namespace SqlMigrator.Tests
{
	[TestFixture]
	public class MigrationFilterTest
	{
		private MigrationFilter _target;
		private MigrationRepository _repository;
		private IDatabase _database;

		[SetUp]
		public void SetUp()
		{
			_repository = MockRepository.GenerateStub<MigrationRepository>("", Encoding.UTF8, null);
			_database = MockRepository.GenerateStub<IDatabase>();
			_target = new MigrationFilter(_repository, _database);
		}

		[Test]
		public void Should_get_only_pending_migrations_if_migration_table_exists()
		{
			_database.Stub(x => x.MigrationsTableExists()).Return(true);
			var migration1 = new Migration(1, "up", "dn");
			var migration2 = new Migration(2, "up", "dn");
			_repository.Stub(x => x.GetAll()).Return(new Dictionary<long, Migration> {
				{ 1, migration1 },
				{ 2, migration2 }
			});
			_database.Stub(x => x.IsMigrationPending(Arg<Migration>.Is.Same(migration1))).Return(true);
			_database.Stub(x => x.IsMigrationPending(Arg<Migration>.Is.Same(migration2))).Return(false);

			var actual = _target.GetPendingMigrations();

			actual.Should().Have.SameValuesAs(new[] { migration1 });
		}

		[Test]
		public void Should_get_all_migrations_if_migration_table_does_not_exists()
		{
			_database.Stub(x => x.MigrationsTableExists()).Return(false);
			var migration1 = new Migration(1, "up", "dn");
			var migration2 = new Migration(2, "up", "dn");
			_repository.Stub(x => x.GetAll()).Return(new Dictionary<long, Migration> {
				{ 1, migration1 },
				{ 2, migration2 }
			});

			var actual = _target.GetPendingMigrations();

			actual.Should().Have.SameValuesAs(new[] { migration1, migration2 });
		}

		[Test]
		public void Shuld_get_applyed_migrations_if_migration_table_exists()
		{
			_database.Stub(x => x.MigrationsTableExists()).Return(true);
			var migration1 = new Migration(1, "up", "dn");
			var migration2 = new Migration(2, "up", "dn");
			_repository.Stub(x => x.GetAll()).Return(new Dictionary<long, Migration> {
				{ 1, migration1 },
				{ 2, migration2 }
			});
			_database.Stub(x => x.GetApplyedMigrations()).Return(new long[] { 2 });

			var actual = _target.GetApplyedMigrations();

			actual.Should().Have.SameValuesAs(new[] { migration2 });
		}

		[Test]
		public void Shuld_get_no_migrations_if_migration_table_does_not_exists()
		{
			_database.Stub(x => x.MigrationsTableExists()).Return(false);
			var migration1 = new Migration(1, "up", "dn");
			var migration2 = new Migration(2, "up", "dn");
			_repository.Stub(x => x.GetAll()).Return(new Dictionary<long, Migration> {
				{ 1, migration1 },
				{ 2, migration2 }
			});

			var actual = _target.GetApplyedMigrations();

			actual.Should().Have.Count.EqualTo(0);
		}

		[Test]
		public void Shuld_fail_when_an_applyed_migration_is_not_available()
		{
			_database.Stub(x => x.MigrationsTableExists()).Return(true);
			var migration1 = new Migration(1, "up", "dn");
			var migration2 = new Migration(2, "up", "dn");
			_repository.Stub(x => x.GetAll()).Return(new Dictionary<long, Migration> {
				{ 1, migration1 },
				{ 2, migration2 }
			});
			_database.Stub(x => x.GetApplyedMigrations()).Return(new long[] { 3 });

			Executing.This(() => _target.GetApplyedMigrations()).Should().Throw<ApplicationException>().And.ValueOf.Message.Should().Be.EqualTo("Migration #3 has been applyed to database, but not found in migrations directory");
		}
	}
}