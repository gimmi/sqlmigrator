using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SqlMigrator
{
	public class MigrationRepository
	{
		private readonly string _migrationsPath;
		private readonly Encoding _encoding;
		private readonly IDatabase _database;

		public MigrationRepository(string migrationsPath, Encoding encoding, IDatabase database)
		{
			_migrationsPath = migrationsPath;
			_encoding = encoding;
			_database = database;
		}

		public virtual IDictionary<long, Migration> GetAll()
		{
			var ret = new Dictionary<long, Migration>();
			IEnumerable<FileInfo> files = Directory.GetFiles(_migrationsPath).Select(n => new FileInfo(n));
			foreach (Migration migration in files.Where(f => GetMigrationIdFromFileName(f).HasValue).Select(Create))
			{
				if (ret.ContainsKey(migration.Id))
				{
					throw new ApplicationException(string.Format("Found more than one migration with id #{0}", migration.Id));
				}
				ret.Add(migration.Id, migration);
			}
			return ret;
		}

		private long? GetMigrationIdFromFileName(FileInfo fileInfo)
		{
			Match match = Regex.Match(fileInfo.Name, @"^(\d+)");
			bool isValidMigrationFile = fileInfo.Exists
			                            && fileInfo.Extension.Equals(".sql", StringComparison.InvariantCultureIgnoreCase)
			                            && match.Success;
			if(isValidMigrationFile)
			{
				return Int64.Parse(match.Groups[0].Value);
			}
			return null;
		}

		private Migration Create(FileInfo file)
		{
			string[] upDown = SplitScript(File.ReadAllText(file.FullName, _encoding));
			long id = GetMigrationIdFromFileName(file).Value;
			return new Migration(id, upDown[0], upDown[1]);
		}

		private string[] SplitScript(string script)
		{
			script = script.Trim();

			Match match = Regex.Match(script, @"^\s*--\s*@DOWN\s*$", RegexOptions.IgnoreCase | RegexOptions.Multiline);
			if(match.Success)
			{
				return new[] { script.Substring(0, match.Index), script.Substring(match.Index + match.Length) };
			}
			return new[] { script, "" };
		}
	}
}