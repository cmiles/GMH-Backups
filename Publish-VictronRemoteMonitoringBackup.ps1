dotnet clean .\GmhBackups.sln -property:Configuration=Release -property:Platform=x64 -verbosity:minimal

dotnet restore .\GmhBackups.sln -r win-x64 -verbosity:minimal

$vsWhere = "{0}\Microsoft Visual Studio\Installer\vswhere.exe" -f ${env:ProgramFiles(x86)}

$msBuild = & $vsWhere -latest -requires Microsoft.Component.MSBuild -find MSBuild\**\Bin\MSBuild.exe

& $msBuild .\GmhBackups.sln -property:Configuration=Release -property:Platform=x64 -verbosity:minimal

if ($lastexitcode -ne 0) { throw ("Exec: " + $errorMessage) }

$publishPath = "M:\GmhBackupsPublications\VictronRemoteMonitoringBackup"
if(!(test-path -PathType container $publishPath)) { New-Item -ItemType Directory -Path $publishPath }

Remove-Item -Path $publishPath\* -Recurse

& $msBuild .\GmhBackups.VictronRemoteMonitoringBackup\GmhBackups.VictronRemoteMonitoringBackup.csproj -t:publish -p:PublishProfile=.\GmhBackups.VictronRemoteMonitoringBackup\Properties\PublishProfile\FolderProfile.pubxml -verbosity:minimal

if ($lastexitcode -ne 0) { throw ("Exec: " + $errorMessage) }
