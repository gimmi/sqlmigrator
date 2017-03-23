using System;
using System.Collections.Generic;
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
                Run(args, Console.Out);
                Console.WriteLine("Done");
                return 0;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return 1;
            }
        }

        public static void Run(string[] args, TextWriter log)
        {
            try
            {
                log.WriteLine("{0} {1}",
                typeof(Program).Assembly.GetCustomAttributes(typeof(AssemblyProductAttribute), false)
                .Cast<AssemblyProductAttribute>()
                .Single()
                .Product,
                typeof(Program).Assembly.GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute), false)
                .Cast<AssemblyInformationalVersionAttribute>()
                .Single()
                .InformationalVersion);
                log.WriteLine(
                typeof(Program).Assembly.GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false)
                .Cast<AssemblyDescriptionAttribute>()
                .Single()
                .Description);
                log.WriteLine(
                typeof(Program).Assembly.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false)
                .Cast<AssemblyCopyrightAttribute>()
                .Single()
                .Copyright);
            }
            catch (Exception)
            {
                log.WriteLine("No Assembly Info available");
            }

            var commandLineParser = new CommandLineParser<Options>();
            if (args.Length == 0)
            {
                log.WriteLine("Command line parameters:");
                commandLineParser.PrintHelp(log);
                return;
            }
            var opts = commandLineParser.Parse(args);
            var db = new MssqlDatabase(opts.ConnStr, opts.DbName, opts.Timeout, opts.MigrationsTableName, log);
            var scriptBuilder = new ScriptBuilder(db, log);
            var migrationFilter = new MigrationFilter(
            new MigrationRepository(opts.MigrationsDir, opts.TextEncoding, db), db);

            IEnumerable<Migration> migrations;
            Direction direction;

            if (opts.Count >= 0)
            {
                log.WriteLine("Building Up script");
                migrations = scriptBuilder.EnumerateUp(migrationFilter.GetPendingMigrations(), opts.Count);
                direction = Direction.Up;
            }
            else
            {
                log.WriteLine("Building Down script");
                migrations = scriptBuilder.EnumerateDown(migrationFilter.GetApplyedMigrations(), -opts.Count);
                direction = Direction.Down;
            }

            if (string.IsNullOrWhiteSpace(opts.OutputFile))
            {
                log.WriteLine("Executing script to {0}", opts.ConnStr);
                db.Execute(migrations, direction);
            }
            else
            {
                log.WriteLine("Saving script to {0}", opts.OutputFile);
                File.WriteAllText(opts.OutputFile, scriptBuilder.Build(migrations, direction), opts.TextEncoding);
            }
        }
    }
}