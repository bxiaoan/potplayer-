#Requires -Version 5.1
<#  一键重建全部皮肤（维护用）。命名规则见 $defs。  #>
$ErrorActionPreference = 'Stop'

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Definition
$srcRoot   = Split-Path -Parent $scriptDir
$root      = Split-Path -Parent $srcRoot
$skinDir   = Join-Path $root '皮肤'
if (-not (Test-Path $skinDir)) { New-Item -ItemType Directory -Path $skinDir -Force | Out-Null }

Add-Type -Path (Join-Path $srcRoot 'build\PanelSkin.cs') -ReferencedAssemblies System.Drawing, System.IO.Compression, System.IO.Compression.FileSystem

$defs = @(
  @{ N='面板风-1-macOS浅色-圆角窗'; T='UseMacLight'; Src='src_wf'; Zh='面板风 macOS 浅色 圆角窗'; Tw='面板風 macOS 淺色 圓角窗'; En='Panel macOS Light Rounded' },
  @{ N='面板风-2-macOS深色-圆角窗'; T='UseMacDark';  Src='src_wf'; Zh='面板风 macOS 深色 圆角窗'; Tw='面板風 macOS 深色 圓角窗'; En='Panel macOS Dark Rounded' },
  @{ N='面板风-3-蓝灰-圆角窗';     T='UseDark';     Src='src_wf'; Zh='面板风 蓝灰 圆角窗';       Tw='面板風 藍灰 圓角窗';       En='Panel Slate Rounded' },
  @{ N='面板风-4-macOS浅色';       T='UseMacLight'; Src='src';    Zh='面板风 macOS 浅色';          Tw='面板風 macOS 淺色';         En='Panel macOS Light' },
  @{ N='面板风-5-macOS深色';       T='UseMacDark';  Src='src';    Zh='面板风 macOS 深色';          Tw='面板風 macOS 深色';         En='Panel macOS Dark' },
  @{ N='面板风-6-蓝灰';            T='UseDark';     Src='src';    Zh='面板风 蓝灰';                Tw='面板風 藍灰';               En='Panel Slate' },
  @{ N='面板风-7-浅灰';            T='UseLight';    Src='src';    Zh='面板风 浅灰';                Tw='面板風 淺灰';               En='Panel Grey' }
)

$tmp = Join-Path $env:TEMP ('pf_' + [guid]::NewGuid().ToString('N'))
foreach ($d in $defs) {
    $title = '{$CP936=' + $d.Zh + '$}{$CP950=' + $d.Tw + '$}{$CP0=' + $d.En + '$}'
    [PanelSkin]::$($d.T)()
    [void][PanelSkin]::Build((Join-Path $srcRoot $d.Src), $tmp, $title)
    [void][PanelSkin]::Zip($tmp, (Join-Path $skinDir ($d.N + '.dsf')))
    Remove-Item $tmp -Recurse -Force
    Write-Host ('  built ' + $d.N + '.dsf')
}

# 初版（第一代引擎产物）：补 BMP 头 + 设置标题后重新打包
$old = Join-Path $skinDir '面板风-8-初版.dsf'
if (-not (Test-Path $old)) { $old = Join-Path $skinDir 'Panel.dsf' }
if (Test-Path $old) {
    Add-Type -AssemblyName System.IO.Compression.FileSystem
    [IO.Compression.ZipFile]::ExtractToDirectory($old, $tmp)
    $t8  = New-Object Text.UTF8Encoding($true)
    $oldTitle = '{$CP936=面板风 初版$}{$CP950=面板風 初版$}{$CP0=Panel Classic$}'
    [IO.File]::WriteAllText((Join-Path $tmp 'Title.txt'), $oldTitle, $t8)
    foreach ($bf in Get-ChildItem $tmp -Filter '*.bmp') {
        $bb = [IO.File]::ReadAllBytes($bf.FullName)
        if ($bb.Length -lt 54) { continue }
        if ([BitConverter]::ToUInt32($bb, 34) -ne 0) { continue }
        $o2 = [BitConverter]::ToUInt32($bb, 10)
        $sz2 = [uint32]($bb.Length - [int]$o2)
        [Array]::Copy([BitConverter]::GetBytes($sz2), 0, $bb, 34, 4)
        [IO.File]::WriteAllBytes($bf.FullName, $bb)
    }
    [void][PanelSkin]::Zip($tmp, (Join-Path $skinDir '面板风-8-初版.dsf'))
    Remove-Item $tmp -Recurse -Force
    Write-Host '  built 面板风-8-初版.dsf'
}

# 删除旧文件名
Get-ChildItem $skinDir -Filter 'Panel*.dsf' -File | ForEach-Object {
    Remove-Item $_.FullName -Force
    Write-Host ('  删除旧名 ' + $_.Name)
}
Write-Host ''
Get-ChildItem $skinDir -Filter '*.dsf' -File | Sort-Object Name | ForEach-Object { Write-Host ('  ' + $_.Name) }