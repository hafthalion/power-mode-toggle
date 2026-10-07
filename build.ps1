# Builds PowerModeToggle.exe with the C# compiler that ships with Windows (.NET Framework 4.x).
# "v4.0.30319" is the fixed folder of the whole .NET Framework 4 family (4.0 - 4.8.1), not a minor version.
$ErrorActionPreference = 'Stop'
$csc = 'Framework64', 'Framework' |
    ForEach-Object { Join-Path $env:WINDIR "Microsoft.NET\$_\v4.0.30319\csc.exe" } |
    Where-Object { Test-Path $_ } | Select-Object -First 1
if (-not $csc) { throw "csc.exe not found. .NET Framework 4.x is required (included with Windows 10/11)." }
Push-Location $PSScriptRoot
try {
    & $csc /nologo /target:winexe /optimize+ /platform:anycpu `
        /win32manifest:app.manifest /win32icon:app.ico `
        /out:PowerModeToggle.exe `
        /reference:System.Windows.Forms.dll /reference:System.Drawing.dll `
        PowerModeToggle.cs
    if ($LASTEXITCODE -ne 0) { throw "Build failed" }
    Write-Host "Built $(Join-Path $PSScriptRoot 'PowerModeToggle.exe')"
} finally { Pop-Location }
