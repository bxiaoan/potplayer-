#Requires -Version 5.1
<#
    从本机已安装的 PotPlayer 提取官方皮肤素材，生成构建用的：
        源码/src      官方 Default.dsf 解包内容
        源码/src_wf   同上，但换成 WindowFrame.dsf 的 8 个布局文件（UseWindowFrame=3，圆角窗）

    这样仓库里就不需要放官方原始素材，只需要 .dsf 成品和构建代码。
#>
$ErrorActionPreference = 'Stop'

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Definition
$srcRoot   = Split-Path -Parent $scriptDir

function Get-PotPlayerFolder {
    foreach ($rp in @('HKLM:\SOFTWARE\DAUM\PotPlayer64','HKLM:\SOFTWARE\DAUM\PotPlayer','HKLM:\SOFTWARE\WOW6432Node\DAUM\PotPlayer')) {
        try {
            $k = Get-Item $rp -ErrorAction Stop
            $folder = $k.GetValue('ProgramFolder')
            if (-not $folder) { $exe = $k.GetValue('ProgramPath'); if ($exe) { $folder = Split-Path $exe -Parent } }
            if ($folder -and (Test-Path $folder)) { return $folder }
        } catch { }
    }
    if ($env:ProgramFiles) {
        $p = Join-Path $env:ProgramFiles 'DAUM\PotPlayer'
        if (Test-Path $p) { return $p }
    }
    return $null
}

$pot = Get-PotPlayerFolder
if (-not $pot) { throw '没有找到 PotPlayer 安装目录。' }
$skins = Join-Path $pot 'Skins'
$defaultDsf    = Join-Path $skins 'Default.dsf'
$windowFrameDsf = Join-Path $skins 'WindowFrame.dsf'
if (-not (Test-Path $defaultDsf)) { throw ('找不到 ' + $defaultDsf) }

Add-Type -AssemblyName System.IO.Compression.FileSystem

$srcDir = Join-Path $srcRoot 'src'
$wfDir  = Join-Path $srcRoot 'src_wf'
foreach ($d in @($srcDir, $wfDir)) {
    if (Test-Path $d) { Remove-Item $d -Recurse -Force }
    New-Item -ItemType Directory -Path $d -Force | Out-Null
}

[IO.Compression.ZipFile]::ExtractToDirectory($defaultDsf, $srcDir)
Write-Host ('  src/     <- ' + $defaultDsf)

if (Test-Path $windowFrameDsf) {
    $tmp = Join-Path $env:TEMP ('wf_' + [guid]::NewGuid().ToString('N'))
    [IO.Compression.ZipFile]::ExtractToDirectory($windowFrameDsf, $tmp)
    Copy-Item (Join-Path $srcDir '*') $wfDir -Recurse -Force
    $diff = @('AudioMini.xml','AudioSkin.xml','Chat.txt','RightAttach.txt','SubCommon.txt','VideoSkin.xml','TouchRight.txt')
    foreach ($n in $diff) {
        $f = Join-Path $tmp $n
        if (Test-Path $f) { Copy-Item $f (Join-Path $wfDir $n) -Force }
    }
    Remove-Item $tmp -Recurse -Force
    Write-Host ('  src_wf/  <- ' + $windowFrameDsf + ' 的布局文件')
} else {
    Copy-Item (Join-Path $srcDir '*') $wfDir -Recurse -Force
    Write-Host '  未找到 WindowFrame.dsf，src_wf 暂时等同 src（圆角窗版本会退化为直角）' -ForegroundColor Yellow
}

Write-Host ''
Write-Host '素材就绪。现在可以运行 build\rebuild-all.ps1 重新构建全部皮肤。' -ForegroundColor Green
