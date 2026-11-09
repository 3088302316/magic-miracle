param(
    [string]$OutputDirectory = (Join-Path $PSScriptRoot 'dist'),
    [string]$ReleaseDirectory = (Join-Path $PSScriptRoot '发布')
)
$ErrorActionPreference = 'Stop'

$compiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
if (!(Test-Path -LiteralPath $compiler)) { throw 'The .NET Framework x64 C# compiler is required.' }
$out = [IO.Path]::GetFullPath($OutputDirectory)
New-Item -ItemType Directory -Path $out -Force | Out-Null
$assets = Join-Path $PSScriptRoot 'assets'
$icon = Join-Path $assets 'magic-miracle-round-v1.ico'
$avatar = Join-Path $assets 'lazy-sheep-avatar.jpg'
$app = Join-Path $out 'magic&miracle.exe'
$release = [IO.Path]::GetFullPath($ReleaseDirectory)
New-Item -ItemType Directory -Path $release -Force | Out-Null
$installer = Join-Path $release 'magic&miracle-安装程序.exe'

$common = @('/nologo','/target:winexe','/platform:x64','/optimize+','/codepage:65001',
    '/r:System.Windows.Forms.dll','/r:System.Drawing.dll',("/win32icon:$icon"))
$sources = @('CuteMouseEffects.cs','AnimatedCursorRenderer.cs','CursorPickerControl.cs',
    'CursorShapes.cs','CustomEditors.cs','DynamicCursorRenderer.cs','SheepControlPanel.cs') |
    ForEach-Object { Join-Path $PSScriptRoot $_ }
$resources = @("/resource:$avatar,LazySheepAvatar.jpg")
$resources += Get-ChildItem -LiteralPath (Join-Path $assets 'cursor-shapes') -Filter '*.cur' -File |
    ForEach-Object { '/resource:' + $_.FullName + ',Cursor.' + $_.BaseName.Replace('-', '.') + '.cur' }

& $compiler @common "/out:$app" @resources @sources
if ($LASTEXITCODE -ne 0) { throw 'Application compilation failed.' }
$defaults = Join-Path $PSScriptRoot 'CuteMouseEffects.ini'
$installerSource = Join-Path $PSScriptRoot 'MagicMiracleInstaller.cs'
& $compiler @common "/out:$installer" "/resource:$app,AppPayload.exe" `
    "/resource:$avatar,SetupAvatar.jpg" "/resource:$defaults,DefaultSettings.ini" $installerSource
if ($LASTEXITCODE -ne 0) { throw 'Installer compilation failed.' }
Write-Output $app, $installer
