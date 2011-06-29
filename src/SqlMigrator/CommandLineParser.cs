using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace SqlMigrator
{
	public class CommandLineParser<T>
	{
		public T Parse(string[] args)
		{
			var instance = Activator.CreateInstance<T>();
			foreach(var arg in ParseArgs(args))
			{
				FieldInfo[] fieldInfos = GetFields().Where(f => f.Name.Equals(arg.Key, StringComparison.InvariantCultureIgnoreCase)).ToArray();
				if(fieldInfos.Length != 1)
				{
					throw new CommandlineException("Unknown or ambiguous option '{0}'", arg.Key);
				}
				FieldInfo fieldInfo = fieldInfos[0];
				Type fieldType = fieldInfo.FieldType;
				object value;
				try
				{
					value = fieldType.IsEnum ? Enum.Parse(fieldType, arg.Value, true) : Convert.ChangeType(arg.Value, fieldType);
				}
				catch(Exception e)
				{
					throw new CommandlineException(string.Format("Cannot convert value '{0}' for option '{1}' to '{2}'", arg.Value, arg.Key, fieldType), e);
				}
				fieldInfo.SetValue(instance, value);
			}
			return instance;
		}

		private IEnumerable<FieldInfo> GetFields()
		{
			return typeof(T).GetFields();
		}

		public IDictionary<string, string> ParseArgs(string[] args)
		{
			var ret = new Dictionary<string, string>();
			IEnumerator<string> enumerator = args.AsEnumerable().GetEnumerator();
			while(enumerator.MoveNext())
			{
				Match match = new Regex(@"^/(?<name>.+)$").Match(enumerator.Current);
				if(!match.Success)
				{
					throw new CommandlineException("Expected option name, found '{0}'", enumerator.Current);
				}
				string name = match.Groups["name"].Value;
				if(!enumerator.MoveNext())
				{
					throw new CommandlineException("No value for option '{0}'", name);
				}
				ret.Add(name, enumerator.Current);
			}
			return ret;
		}

		public void PrintHelp(TextWriter writer)
		{
			int pad = GetFields().Max(f => f.Name.Length) + 2;
			foreach(FieldInfo fieldInfo in GetFields())
			{
				writer.WriteLine("/{0}{1}", fieldInfo.Name.PadRight(pad), FindAttribute(fieldInfo, new DescriptionAttribute()).Description);
			}
		}

		public TAttr FindAttribute<TAttr>(MemberInfo member, TAttr def) where TAttr : Attribute
		{
			return (TAttr)member.GetCustomAttributes(typeof(TAttr), false).FirstOrDefault() ?? def;
		}
	}
}