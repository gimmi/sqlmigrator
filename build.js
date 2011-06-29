load('jsmake.dotnet.DotNetUtils.js');

var fs = jsmake.Fs;
var utils = jsmake.Utils;
var sys = jsmake.Sys;
var dotnet = new jsmake.dotnet.DotNetUtils();

var version;

task('default', 'test');

task('version', function () {
	version = JSON.parse(fs.readFile('version.json'));
});

task('dependencies', function () {
	var pkgs = fs.createScanner('src').include('**/packages.config').scan();
	dotnet.downloadNuGetPackages(pkgs, 'lib');
});

task('assemblyinfo', 'version', function () {
	dotnet.writeAssemblyInfo('src/SharedAssemblyInfo.cs', {
		AssemblyTitle: 'SqlMigrator',
		AssemblyProduct: 'SqlMigrator',
		AssemblyDescription: 'SQL Database migration script handler',
		AssemblyCopyright: 'Copyright © Gian Marco Gherardi ' + new Date().getFullYear(),
		AssemblyTrademark: '',
		AssemblyCompany: 'Gian Marco Gherardi',
		AssemblyConfiguration: '', // Probably a good place to put Git SHA1 and build date
		AssemblyVersion: [ version.major, version.minor, version.patch, 0 ].join('.'),
		AssemblyFileVersion: [ version.major, version.minor, version.patch, 0 ].join('.'),
		AssemblyInformationalVersion: [ version.major, version.minor, version.patch, 0 ].join('.')
	});
});

task('build', [ 'dependencies', 'assemblyinfo' ], function () {
	dotnet.runMSBuild('src/SqlMigrator.sln', [ 'Clean', 'Rebuild' ]);
});

task('test', 'build', function () {
	var testDlls = fs.createScanner('build/bin').include('**/*Tests.dll').scan();
	dotnet.runNUnit(testDlls);
});

task('release', 'test', function () {
	fs.deletePath('build');
	dotnet.runMSBuild('src/SqlMigrator.sln', [ 'Clean', 'SqlMigrator:Rebuild' ]);
	fs.zipPath('build/bin', 'build/sqlmigrator-' + [ version.major, version.minor, version.patch ].join('.') + '.zip');
	version.patch += 1;
	fs.writeFile('version.json', JSON.stringify(version));
});

