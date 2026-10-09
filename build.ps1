# Builds PowerModeToggle.exe with the C# compiler that ships with Windows (.NET Framework 4.x).
# "v4.0.30319" is the fixed folder of the whole .NET Framework 4 family (4.0 - 4.8.1), not a minor version.
$ErrorActionPreference = 'Stop'
$csc = 'Framework64', 'Framework' |
    ForEach-Object { Join-Path $env:WINDIR "Microsoft.NET\$_\v4.0.30319\csc.exe" } |
    Where-Object { Test-Path $_ } | Select-Object -First 1
if (-not $csc) { throw "csc.exe not found. .NET Framework 4.x is required (included with Windows 10/11)." }
$references = '/reference:System.Windows.Forms.dll', '/reference:System.Drawing.dll'

# Writes app.ico from the app's own drawing code (the Balanced tray icon), so the exe icon
# always matches the tray icons. Each size is a PNG entry, which Windows Vista and later read.
function Write-AppIcon {
    $dll = Join-Path ([IO.Path]::GetTempPath()) ("PowerModeToggle-icon-" + [Guid]::NewGuid() + ".dll")
    try {
        & $csc /nologo /target:library $references "/out:$dll" PowerModeToggle.cs
        if ($LASTEXITCODE -ne 0) { throw "Build failed" }
        Add-Type -AssemblyName System.Drawing
        $asm = [Reflection.Assembly]::Load([IO.File]::ReadAllBytes($dll)) # from bytes: no file lock
    } finally { Remove-Item $dll -ErrorAction SilentlyContinue }

    $draw = $asm.GetType('PowerModeToggle.Icons').GetMethod('Draw')
    $balanced = [Enum]::Parse($asm.GetType('PowerModeToggle.PowerMode'), 'Balanced')
    $sizes = 16, 20, 24, 32, 40, 48, 64, 128, 256
    $pngs = foreach ($size in $sizes) {
        $bmp = $draw.Invoke($null, @($balanced, $size))
        $ms = New-Object IO.MemoryStream
        $bmp.Save($ms, [Drawing.Imaging.ImageFormat]::Png); $bmp.Dispose()
        , $ms.ToArray()
    }

    $w = New-Object IO.BinaryWriter([IO.File]::Create((Join-Path $PWD 'app.ico')))
    try {
        $w.Write([int16]0); $w.Write([int16]1); $w.Write([int16]$sizes.Count)      # header: reserved, type=icon, count
        $offset = 6 + 16 * $sizes.Count
        for ($i = 0; $i -lt $sizes.Count; $i++) {
            $d = [byte]($sizes[$i] % 256)                                             # 256 is stored as 0
            $w.Write($d); $w.Write($d); $w.Write([byte]0); $w.Write([byte]0)          # width, height, colors, reserved
            $w.Write([int16]1); $w.Write([int16]32)                                   # planes, bits per pixel
            $w.Write([int]$pngs[$i].Length); $w.Write([int]$offset)
            $offset += $pngs[$i].Length
        }
        foreach ($png in $pngs) { $w.Write($png) }
    } finally { $w.Close() }
}

Push-Location $PSScriptRoot
try {
    Write-AppIcon
    & $csc /nologo /target:winexe /optimize+ /platform:anycpu `
        /win32manifest:app.manifest /win32icon:app.ico `
        /out:PowerModeToggle.exe $references `
        PowerModeToggle.cs
    if ($LASTEXITCODE -ne 0) { throw "Build failed" }
    Write-Host "Built $(Join-Path $PSScriptRoot 'PowerModeToggle.exe')"
} finally { Pop-Location }
