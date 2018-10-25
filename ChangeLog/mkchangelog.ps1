$useLegacy = ($PSVersionTable.PSVersion -as [version]) -le ("2.0" -as [version])
if ($useLegacy){
  $inputDir = Split-Path -Parent $MyInvocation.MyCommand.Path
}
else {
  $inputDir = $PSScriptRoot
}
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
                                   @{Name ="ReleaseName"; Expression= {($_.BaseName -split "_", 2)[1] }},
                                   @{Name ="FixedReleaseName"; Expression = {($_.BaseName -split "_", 2)[1] -replace "Z_Final", "Final"}} |
           Sort-Object -Property @{Expression="ReleaseVersion";Descending=$true}, @{Expression="ReleaseName";Descending=$true}

#compose ChangeLog and ReleaseInfo files
Foreach ($complexObject in $items) {
  $isInitial = $complexObject.FileName -eq "0.0.0"
  if($isInitial){
    Add-Content $changeLogFile ( Get-Content $complexObject.File.FullName)
	Add-Content $changeLogFile ""
  }
  else{
    #on first itteration create ReleaseNotes
    if (-not(Test-Path $releaseNotesFile)) { Add-Content $releaseNotesFile (Get-Content $complexObject.File.FullName) }
    Add-Content $changeLogFile ("Changes in {0} {1}" -f $complexObject.ReleaseVersion, $complexObject.FixedReleaseName)
    Add-Content $changeLogFile ""
    Add-Content $changeLogFile ( Get-Content $complexObject.File.FullName)
    Add-Content $changeLogFile ""
  }
}
