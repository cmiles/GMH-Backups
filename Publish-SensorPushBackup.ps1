dotnet clean .\GmhWorkshop.sln -property:Configuration=Release -property:Platform=x64 -verbosity:minimal

dotnet restore .\GmhWorkshop.sln -r win-x64 -verbosity:minimal

$vsWhere = "{0}\Microsoft Visual Studio\Installer\vswhere.exe" -f ${env:ProgramFiles(x86)}

$msBuild = & $vsWhere -latest -requires Microsoft.Component.MSBuild -find MSBuild\**\Bin\MSBuild.exe

& $msBuild .\GmhWorkshop.sln -property:Configuration=Release -property:Platform=x64 -verbosity:minimal

if ($lastexitcode -ne 0) { throw ("Exec: " + $errorMessage) }

$publishPath = "M:\GmhWorkshopPublications\SensorPushBackup"
if(!(test-path -PathType container $publishPath)) { New-Item -ItemType Directory -Path $publishPath }

Remove-Item -Path $publishPath\* -Recurse

& $msBuild .\GmhWorkshop.SensorPushBackup\GmhWorkshop.SensorPushBackup.csproj -t:publish -p:PublishProfile=.\GmhWorkshop.SensorPushBackup\Properties\PublishProfile\FolderProfile.pubxml -verbosity:minimal

if ($lastexitcode -ne 0) { throw ("Exec: " + $errorMessage) }
