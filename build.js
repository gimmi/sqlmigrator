load('jsmake.dotnet.DotNetUtils.js');

var fs = jsmake.Fs;
var utils = jsmake.Utils;
var sys = jsmake.Sys;
var dotnet = new jsmake.dotnet.DotNetUtils();

var versionObj, versionStr;

task('default', 'test');

task('version', function () {
	versionObj = JSON.parse(fs.readFile('version.json'));
	versionStr = [ versionObj.major, versionObj.minor, versionObj.patch ].join('.');
});

task('dependencies', function () {
	var pkgs = fs.createScanner('src').include('**/packages.config').scan();
	dotnet.downloadNuGetPackages(pkgs, 'lib');
});

task('assemblyinfo', 'version', function () {
	dotnet.writeAssemblyInfo('src/SharedAssemblyInfo.cs', {
		AssemblyTitle: 'SqlMigrator',
		AssemblyProduct: 'SqlMigrator',
		AssemblyDescription: 'http://gimmi.github.com/sqlmigrator/',
		AssemblyCopyright: 'Copyright © Gian Marco Gherardi ' + new Date().getFullYear(),
		AssemblyTrademark: '',
		AssemblyCompany: 'Gian Marco Gherardi',
		AssemblyConfiguration: '', // Probably a good place to put Git SHA1 and build date
		AssemblyVersion: [ versionStr, 0 ].join('.'),
		AssemblyFileVersion: [ versionStr, 0 ].join('.'),
		AssemblyInformationalVersion: versionStr
	});
});

task('build', [ 'dependencies', 'assemblyinfo' ], function () {
	dotnet.runMSBuild('src/SqlMigrator.sln', [ 'Clean', 'Rebuild' ], { Configuration: 'Release' });
});

task('test', 'build', function () {
	var testDlls = fs.createScanner('src').include('*.Tests/bin/Release/*.Tests.dll').scan();
	dotnet.runNUnit(testDlls);
});

task('release', 'test', function () {
	fs.deletePath('build');
	fs.createDirectory('build/tools')
	fs.copyPath('src/SqlMigrator/bin/Release', 'build/tools');

	fs.writeFile('build/Package.nuspec', [
		'<?xml version="1.0"?>',
		'<package >',
		'  <metadata>',
		'    <id>SqlMigrator</id>',
		'    <version>' + versionStr + '</version>',
		'    <authors>gimmi</authors>',
		'    <owners>gimmi</owners>',
		'    <licenseUrl>https://raw.github.com/gimmi/sqlmigrator/master/LICENSE</licenseUrl>',
		'    <projectUrl>http://gimmi.github.com/sqlmigrator</projectUrl>',
		'    <iconUrl>https://github.com/gimmi/sqlmigrator/raw/master/icon.png</iconUrl>',
		'    <requireLicenseAcceptance>false</requireLicenseAcceptance>',
		'    <description>Database change management tool for .NET</description>',
		'    <copyright>Gian Marco Gherardi ' + new Date().getFullYear() + '</copyright>',
		'    <tags>database sql migration migrations db agile change script tsql sqlserver refactoring tool commandline cli build</tags>',
		'  </metadata>',
		'</package>'
	].join('\n'));

	sys.run('tools/nuget/nuget.exe', 'pack', 'build/Package.nuspec');
	sys.run('tools/nuget/nuget.exe', 'push', 'SqlMigrator.' + versionStr + '.nupkg');

	versionObj.patch += 1;
	fs.writeFile('version.json', JSON.stringify(versionObj));
});

