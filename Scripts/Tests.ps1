<# 
  Scripts\Tests.ps1  (PowerShell 5.1 compatible)

  Examples:
    powershell -NoProfile -ExecutionPolicy Bypass -File .\Scripts\Tests.ps1
    powershell -NoProfile -ExecutionPolicy Bypass -File .\Scripts\Tests.ps1 -OrdersUrl "http://localhost:5107" -CatalogUrl "http://localhost:5107"
    powershell -NoProfile -ExecutionPolicy Bypass -File .\Scripts\Tests.ps1 -AuthUrl "http://localhost:5107"
    powershell -NoProfile -ExecutionPolicy Bypass -File .\Scripts\Tests.ps1 -Smoke
#>

param(
  [string]$ConnectionString = "Server=localhost;Database=LSSDb;User Id=LSSUser;Password=LSS-P@ssw0rd;Encrypt=True;TrustServerCertificate=True;",
  [string]$OrdersUrl    = "http://localhost:5107",
  [string]$CatalogUrl   = "http://localhost:5107",
  [string]$InventoryUrl = "http://localhost:5107",
  [string]$ProductsUrl  = "http://localhost:5107",
  [string]$AuthUrl      = "http://localhost:5107",
  [string]$OrdersCli    = "",
  [string]$Filter       = "",
  [switch]$Smoke
)

# ---------- locate repo/tests ----------
$repoRoot  = (Split-Path -Parent $PSScriptRoot)
$testsRoot = Join-Path $repoRoot "tests"
if (-not (Test-Path $testsRoot)) { Write-Error ("tests folder not found at " + $testsRoot); exit 1 }
Set-Location $testsRoot

# choose test csproj (avoid bin/obj)
$proj = Get-ChildItem -Path $testsRoot -Filter *.csproj -File | Select-Object -First 1
if (-not $proj) {
  $proj = Get-ChildItem -Path $testsRoot -Filter *.csproj -File -Recurse |
          Where-Object { $_.FullName -notmatch '\\(bin|obj)\\' } |
          Select-Object -First 1
}
if (-not $proj) { Write-Error ("No .csproj found under " + $testsRoot); exit 1 }

# ---------- report paths ----------
$reportsRoot = Join-Path $repoRoot "Reports"
$stamp       = Get-Date -Format "yyyy-MM-dd_HH-mm-ss"
$runDir      = Join-Path $reportsRoot $stamp
New-Item -ItemType Directory -Force -Path $runDir | Out-Null

$consoleLog  = Join-Path $runDir "dotnet-test-output.txt"
$trxName     = "TestResults.trx"
$trxPath     = Join-Path $runDir $trxName
$mdReport    = Join-Path $runDir "TestReport.md"

# ---------- test env ----------
$env:ConnectionStrings__Default = $ConnectionString

if ([string]::IsNullOrWhiteSpace($OrdersUrl))    { Remove-Item Env:\ORDERS_URL    -EA SilentlyContinue } else { $env:ORDERS_URL    = $OrdersUrl }
if ([string]::IsNullOrWhiteSpace($OrdersCli))    { Remove-Item Env:\ORDERS_CLI    -EA SilentlyContinue } else { $env:ORDERS_CLI    = $OrdersCli }
if ([string]::IsNullOrWhiteSpace($CatalogUrl))   { Remove-Item Env:\CATALOG_URL   -EA SilentlyContinue } else { $env:CATALOG_URL   = $CatalogUrl }
if ([string]::IsNullOrWhiteSpace($InventoryUrl)) { Remove-Item Env:\INVENTORY_URL -EA SilentlyContinue } else { $env:INVENTORY_URL = $InventoryUrl }
if ([string]::IsNullOrWhiteSpace($ProductsUrl))  { Remove-Item Env:\PRODUCTS_URL  -EA SilentlyContinue } else { $env:PRODUCTS_URL  = $ProductsUrl }
if ([string]::IsNullOrWhiteSpace($AuthUrl))      { Remove-Item Env:\AUTH_URL      -EA SilentlyContinue } else { $env:AUTH_URL      = $AuthUrl }

Write-Host "== Test configuration ==" -ForegroundColor Cyan
Write-Host ("Project:                    " + $proj.FullName)
Write-Host ("Reports dir:                " + $runDir)
Write-Host ("ConnectionStrings__Default: " + $env:ConnectionStrings__Default)
Write-Host ("ORDERS_URL:                 " + ($env:ORDERS_URL))
Write-Host ("ORDERS_CLI:                 " + ($env:ORDERS_CLI))
Write-Host ("CATALOG_URL:                " + ($env:CATALOG_URL))
Write-Host ("INVENTORY_URL:              " + ($env:INVENTORY_URL))
Write-Host ("PRODUCTS_URL:               " + ($env:PRODUCTS_URL))
Write-Host ("AUTH_URL:                   " + ($env:AUTH_URL))
if ($Filter) { Write-Host ("dotnet test filter:         " + $Filter) }

# ---------- optional Smoke ping (quick health) ----------
function Invoke-Ping {
  param([string]$Method,[string]$Url,[int[]]$Expect)
  try {
    $r = Invoke-WebRequest -Method $Method -Uri $Url -UseBasicParsing -TimeoutSec 10 -ErrorAction Stop
    if ($Expect -notcontains [int]$r.StatusCode) {
      Write-Warning ("SMOKE: {0} {1} -> {2} (expected: {3})" -f $Method,$Url,[int]$r.StatusCode,($Expect -join ","))
      return $false
    }
    Write-Host ("SMOKE: {0} {1} -> {2}" -f $Method,$Url,[int]$r.StatusCode)
    return $true
  }
  catch {
    Write-Warning ("SMOKE: {0} {1} FAILED: {2}" -f $Method,$Url,$_.Exception.Message)
    return $false
  }
}

if ($Smoke) {
  Write-Host "`n== Smoke check ==" -ForegroundColor Yellow
  $ok = $true
  $ok = (Invoke-Ping GET    ($CatalogUrl.TrimEnd('/') + "/")                                    @(200)) -and $ok
  # auth endpoints should require JWT (401 is OK)
  $ok = (Invoke-Ping GET    ($AuthUrl.TrimEnd('/')    + "/auth/users")                          @(401)) -and $ok
  $ok = (Invoke-Ping GET    ($AuthUrl.TrimEnd('/')    + "/auth/roles")                          @(401)) -and $ok
  # simple 404 probe on a known not found id
  $ok = (Invoke-Ping GET    ($ProductsUrl.TrimEnd('/') + "/products/999999")                    @(404)) -and $ok
  $ok = (Invoke-Ping GET    ($CatalogUrl.TrimEnd('/')  + "/warehouses/999999")                  @(404)) -and $ok
  $ok = (Invoke-Ping GET    ($OrdersUrl.TrimEnd('/')   + "/orders/999999")                      @(404)) -and $ok
  if (-not $ok) { Write-Error "Smoke failed."; exit 2 }
}

# ---------- run tests ----------
dotnet restore $proj.FullName | Tee-Object -FilePath $consoleLog -Append | Out-Null

$argList = @("test", $proj.FullName, "--results-directory", $runDir, "--logger", ("trx;LogFileName=" + $trxName))
if ($Filter) { $argList += @("--filter", $Filter) }

$testOutput = & dotnet @$argList 2>&1
$testExit   = $LASTEXITCODE
$testOutput | Tee-Object -FilePath $consoleLog -Append | Out-Null

# ---------- report builder (unchanged structure) ----------
$lines = New-Object System.Collections.Generic.List[string]
function Add-Line([string]$s) { [void]$lines.Add($s) }
function Add-Empty { [void]$lines.Add("") }
function Add-CodeBlock([string[]]$content) { Add-Line('```'); foreach ($l in $content) { Add-Line($l) }; Add-Line('```') }

function Extract-ExpectedActual {
  param([string]$text)
  $result = @{ Expected = $null; Actual = $null }
  if ([string]::IsNullOrWhiteSpace($text)) { return $result }
  $m = [regex]::Match($text, "(?im)^\s*Expected:\s*(.+)\r?\n\s*Actual:\s*(.+)$")
  if ($m.Success) { $result.Expected = $m.Groups[1].Value.Trim(); $result.Actual = $m.Groups[2].Value.Trim(); return $result }
  $m2 = [regex]::Match($text, "(?is)to be\s+(.+?)\s+but\s+found\s+(.+?)(?:[.\r\n]|$)")
  if ($m2.Success) { $result.Expected = $m2.Groups[1].Value.Trim(); $result.Actual = $m2.Groups[2].Value.Trim(); return $result }
  $m3 = [regex]::Match($text, "(?i)Expected\s*<(.+?)>\s*,?\s*Actual\s*<(.+?)>")
  if ($m3.Success) { $result.Expected = $m3.Groups[1].Value.Trim(); $result.Actual = $m3.Groups[2].Value.Trim(); return $result }
  $m4 = [regex]::Match($text, "(?im)^\s*EXPECT:\s*(.+)\r?\n\s*ACTUAL:\s*(.+)$")
  if ($m4.Success) { $result.Expected = $m4.Groups[1].Value.Trim(); $result.Actual = $m4.Groups[2].Value.Trim(); return $result }
  return $result
}

function Build-TestMap { param([xml]$xml)
  $map = @{}
  if ($xml.TestRun -and $xml.TestRun.TestDefinitions -and $xml.TestRun.TestDefinitions.UnitTest) {
    $defs = @($xml.TestRun.TestDefinitions.UnitTest)
    foreach ($d in $defs) {
      $id = "" + $d.id; $name = $d.Name; $class = $null
      if ($d.TestMethod -and $d.TestMethod.className) { $class = $d.TestMethod.className }
      if ($class) { $name = $class + "::" + $name }
      $map[$id] = $name
    }
  }
  return $map
}

Add-Line("# Test Report")
Add-Empty
Add-Line("- **Timestamp:** " + $stamp)
Add-Line("- **Project:** " + $proj.FullName)
Add-Line("- **Results directory:** " + $runDir)
Add-Line("- **ORDERS_URL:** " + ($env:ORDERS_URL))
Add-Line("- **ORDERS_CLI:** " + ($env:ORDERS_CLI))
Add-Line("- **CATALOG_URL:** " + ($env:CATALOG_URL))
Add-Line("- **INVENTORY_URL:** " + ($env:INVENTORY_URL))
Add-Line("- **PRODUCTS_URL:** " + ($env:PRODUCTS_URL))
Add-Line("- **AUTH_URL:** " + ($env:AUTH_URL))
Add-Line("- **ConnectionStrings__Default:** " + '`' + $env:ConnectionStrings__Default + '`')
Add-Empty

# Find TRX
$trx = $null
if (Test-Path $trxPath) {
  $trx = Get-Item $trxPath
} else {
  $trx = Get-ChildItem -Path $runDir -Filter *.trx | Sort-Object LastWriteTime -Desc | Select-Object -First 1
}

$failedCount = $null
try {
  if ($trx) {
    [xml]$xml = Get-Content -LiteralPath $trx.FullName
    $c = $xml.TestRun.ResultSummary.Counters
    $total=0;$passed=0;$failed=0;$skipped=0
    if ($c) {
      if ($c.PSObject.Properties.Name -contains 'total')       { $total   = [int]$c.total }
      if ($c.PSObject.Properties.Name -contains 'passed')      { $passed  = [int]$c.passed }
      if ($c.PSObject.Properties.Name -contains 'failed')      { $failed  = [int]$c.failed }
      if ($c.PSObject.Properties.Name -contains 'notExecuted') { $skipped = [int]$c.notExecuted }
    }
    $failedCount = $failed

    Add-Line("## Summary"); Add-Empty
    Add-Line("| Total | Passed | Failed | Skipped |")
    Add-Line("|------:|------:|------:|--------:|")
    Add-Line("| $total | $passed | $failed | $skipped |")
    Add-Empty

    $testMap = Build-TestMap -xml $xml
    $allResults = @()
    if ($xml.TestRun.Results -and $xml.TestRun.Results.UnitTestResult) {
      $allResults = @($xml.TestRun.Results.UnitTestResult)
    }

    $passedNodes = @(); $failedNodes = @(); $skippedNodes = @()
    foreach ($r in $allResults) {
      switch ("" + $r.outcome) {
        "Passed"  { $passedNodes  += $r }
        "Failed"  { $failedNodes  += $r }
        "NotExecuted" { $skippedNodes += $r }
        default   { }
      }
    }

    if ($failedNodes.Count -gt 0) {
      Add-Line("## Failed tests"); Add-Empty
      foreach ($n in $failedNodes) {
        $name = $n.testName
        if ($n.testId -and $testMap.ContainsKey("" + $n.testId)) { $name = $testMap["" + $n.testId] }
        Add-Line("### " + $name)
        $msg = $null
        if ($n.Output -and $n.Output.ErrorInfo -and $n.Output.ErrorInfo.Message) {
          $msg = ($n.Output.ErrorInfo.Message -join "`n")
        }
        if ($msg) { Add-Empty; Add-Line("**Message:**"); Add-Empty; Add-CodeBlock(@($msg.Trim())) }
        $ea = Extract-ExpectedActual -text $msg
        if (-not $ea.Expected -and $n.Output -and $n.Output.StdOut) {
          $ea2 = Extract-ExpectedActual -text ($n.Output.StdOut -join "`n")
          if ($ea2.Expected) { $ea = $ea2 }
        }
        if ($ea.Expected -or $ea.Actual) {
          Add-Empty; Add-Line("**Expected vs Actual (parsed):**"); Add-Empty
          Add-Line("| Expected | Actual |")
          Add-Line("|----------|--------|")
          Add-Line("| " + ($ea.Expected -replace '\|','\|') + " | " + ($ea.Actual -replace '\|','\|') + " |")
          Add-Empty
        }
        $st = $null
        if ($n.Output -and $n.Output.ErrorInfo -and $n.Output.ErrorInfo.StackTrace) {
          $st = ($n.Output.ErrorInfo.StackTrace -join "`n")
        }
        if ($st) { Add-Line("**StackTrace:**"); Add-Empty; $stLines = [regex]::Split($st, "`r?`n") | Select-Object -First 80; Add-CodeBlock($stLines) }
        if ($n.duration) { Add-Line("**Duration:** " + $n.duration); Add-Empty }
        Add-Empty
      }
    }

    if ($passedNodes.Count -gt 0) {
      Add-Line("## Passed tests"); Add-Empty
      Add-Line("| Test | Duration | Notes |")
      Add-Line("|------|----------|-------|")
      foreach ($n in $passedNodes) {
        $name = $n.testName
        if ($n.testId -and $testMap.ContainsKey("" + $n.testId)) { $name = $testMap["" + $n.testId] }
        $dur  = ""; if ($n.duration) { $dur = "" + $n.duration }
        $notes = "Matched expected behaviour."
        if ($n.Output -and $n.Output.StdOut) {
          $ea = Extract-ExpectedActual -text ($n.Output.StdOut -join "`n")
          if ($ea.Expected -and $ea.Actual) { $notes = "Expected = " + $ea.Expected + "; Actual = " + $ea.Actual }
        }
        $safeName  = ($name  -replace '\|','\|'); $safeNotes = ($notes -replace '\|','\|')
        Add-Line("| $safeName | $dur | $safeNotes |")
      }
      Add-Empty
    }

    if ($skippedNodes.Count -gt 0) {
      Add-Line("## Skipped tests"); Add-Empty
      foreach ($n in $skippedNodes) {
        $name = $n.testName
        if ($n.testId -and $testMap.ContainsKey("" + $n.testId)) { $name = $testMap["" + $n.testId] }
        Add-Line("- " + $name)
      }
      Add-Empty
    }
  } else {
    Add-Line("No TRX file found in " + $runDir + "."); Add-Empty
  }
}
catch {
  $desc = "unknown"
  if ($trx) { $desc = $trx.FullName }
  Add-Line("Could not parse TRX: " + $desc); Add-Empty
}

Add-Line("## Console output (tail)"); Add-Empty
$tail = @("No console log found.")
if (Test-Path $consoleLog) { $tail = Get-Content -LiteralPath $consoleLog -Tail 200 }
Add-CodeBlock($tail)

$lines -join "`r`n" | Set-Content -LiteralPath $mdReport -Encoding UTF8

Write-Host ""
Write-Host ("Report written to: " + $mdReport) -ForegroundColor Green
if ($trx) { Write-Host ("TRX: " + $trx.FullName) -ForegroundColor DarkGray }
Write-Host ("Console log: " + $consoleLog) -ForegroundColor DarkGray

# ---------- exit code ----------
if ($Smoke -and $testExit -eq $null) { $testExit = 0 }
if ($null -ne $failedCount) {
  if ($failedCount -gt 0) { exit 1 } else { exit 0 }
} else {
  exit $testExit
}
