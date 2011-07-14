using System;
using System.Collections.Generic;
using System.Linq;

namespace SqlMigrator
{
	public class MigrationFilter
	{
		private readonly MigrationRepository _repository;
		private readonly IDatabase _database;

		public MigrationFilter(MigrationRepository repository, IDatabase database)
		{
			_repository = repository;
			_database = database;
		}

		public IEnumerable<Migration> GetPendingMigrations()
		{
			IEnumerable<Migration> ret = _repository.GetAll().Values;
			if (_database.MigrationsTableExists())
			{
				ret = ret.Where(_database.IsMigrationPending);
			}
			return ret;
		}

		public IEnumerable<Migration> GetApplyedMigrations()
		{
			if (!_database.MigrationsTableExists())
			{
				return new Migration[0];
			}
			var ret = new List<Migration>();
			IDictionary<long, Migration> migrations = _repository.GetAll();
			foreach(long id in _database.GetApplyedMigrations())
			{
				if(!migrations.ContainsKey(id))
				{
					throw new ApplicationException(string.Format("Migration #{0} has been applyed to database, but not found in migrations directory", id));
				}
				ret.Add(migrations[id]);
			}
			return ret;
		}
	}
}