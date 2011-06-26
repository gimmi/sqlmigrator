using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace SqlMigrator
{
	public class CommandlineParser
	{
		private readonly string[] _args;

		public CommandlineParser(string[] args)
		{
			_args = args;
		}

		public bool GetFlag(string[] names)
		{
			return SeekParam(names) != null;
		}

		public T GetParam<T>(string[] names, T def)
		{
			var param = GetParam(names);
			if(param == null)
			{
				return def;
			}
			return (T)Convert.ChangeType(param, typeof(T));
		}

		public T GetParam<T>(string[] names)
		{
			var param = GetParam(names);
			if(param == null)
			{
				throw new CommandlineException("Missing required option '{0}'", String.Join(", ", names));
			}
			return (T)Convert.ChangeType(param, typeof(T));
		}

		private string GetParam(string[] names)
		{
			var en = SeekParam(names);
			if(en == null)
			{
				return null;
			}
			if(!en.MoveNext())
			{
				throw new CommandlineException("No value for option '{0}'", String.Join(", ", names));
			}
			return en.Current;
		}

		private IEnumerator<string> SeekParam(string[] names)
		{
			IEnumerator<string> enumerator = _args.AsEnumerable().GetEnumerator();
			while (enumerator.MoveNext())
			{
				Match match = new Regex(@"^(--|-|/)" + String.Join("|", names.Select(n => "(" + Regex.Escape(n) + ")")) + "$").Match(enumerator.Current);
				if (match.Success)
				{
					return enumerator;
				}
			}
			return null;
		}
	}
}