using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace SqlMigrator
{
	public class Program
	{
		public static int Main(string[] args)
		{
			try
			{
				Main(args, Console.Out);
				return 0;
			}
			catch(Exception e)
			{
				Console.WriteLine(e);
				return 1;
			}
		}

		public static void Main(string[] args, TextWriter log)
		{
			log.WriteLine("{0} {1}", typeof(Program).Assembly.GetCustomAttributes(typeof(AssemblyProductAttribute), false).Cast<AssemblyProductAttribute>().Single().Product, typeof(Program).Assembly.GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute), false).Cast<AssemblyInformationalVersionAttribute>().Single().InformationalVersion);
			log.WriteLine(typeof(Program).Assembly.GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false).Cast<AssemblyDescriptionAttribute>().Single().Description);
			log.WriteLine(typeof(Program).Assembly.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false).Cast<AssemblyCopyrightAttribute>().Single().Copyright);

			var commandLineParser = new CommandLineParser<Options>();
			if(args.Length == 0)
			{
				log.WriteLine("Command line parameters:");
				commandLineParser.PrintHelp(log);
				return;
			}
			Options opts = commandLineParser.Parse(args);
			var db = new MssqlDatabase(opts.ConnStr);
			var scriptBuilder = new ScriptBuilder(db);
			var migrationFilter = new MigrationFilter(new MigrationRepository(opts.MigrationsDir, opts.TextEncoding, db), db);

			string script = null;
			if(opts.Action == Action.Up)
			{
				script = scriptBuilder.BuildUp(migrationFilter.GetPendingMigrations(), opts.Count);
			}
			else if(opts.Action == Action.Down)
			{
				script = scriptBuilder.BuildDown(migrationFilter.GetApplyedMigrations(), opts.Count);
			}
			else if(opts.Action == Action.Init)
			{
				script = db.BuildCreateScript();
			}

			if(string.IsNullOrWhiteSpace(opts.OutputFile))
			{
				db.Execute(script);
			}
			else
			{
				File.WriteAllText(opts.OutputFile, script, opts.TextEncoding);
			}
		}
	}
}