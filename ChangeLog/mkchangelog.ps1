$inputDir = $PSScriptRoot
$theRootestRoot = Split-Path -Parent $inputDir;
$outputDir = Join-Path -Path $theRootestRoot -ChildPath \_Build\ProductInfo
$changeLogFile = Join-Path -Path $outputDir -ChildPath "ChangeLog.txt"
$releaseNotesFile = Join-Path -Path $outputDir -ChildPath "ReleaseNotes.txt"

if (-not (Test-Path $outputDir)) { New-Item $outputDir -Type directory -Force }
if (Test-Path $changeLogFile) { Remove-Item $changeLogFile -Force }
if (Test-Path $releaseNotesFile) { Remove-Item $releaseNotesFile -Force }

#perform right files order
$items = Get-ChildItem -Path $inputDir -Filter *.txt |
           Select-Object -Property @{Name = "FileName"; Expression = {$_.BaseName}},
                                   @{Name = "File"; Expression = {$_}},
                                   @{Name = "ReleaseVersion"; Expression= {($_.BaseName -split "_", 2)[0] -as [Version]}},
                                   @{Name ="ReleaseName"; Expression= {($_.BaseName -split "_", 2)[1]}} |
           Sort-Object -Property @{Expression="ReleaseVersion";Descending=$true}, @{Expression="ReleaseName";Descending=$true}

#compose ChangeLog and ReleaseInfo files
Foreach ($complexObject in $items) {
  #on first itteration create ReleaseNotes
  if (-not(Test-Path $releaseNotesFile)) { Add-Content $releaseNotesFile (Get-Content $complexObject.File) }
  Add-Content $changeLogFile ("Changes in {0}" -f $complexObject.FileName)
  Add-Content $changeLogFile "`r`n" -NoNewline
  Add-Content $changeLogFile ( Get-Content $complexObject.File)
  Add-Content $changeLogFile "`r`n" -NoNewline
}
