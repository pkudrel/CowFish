<#
.Synopsis
	Build script (https://github.com/nightroman/Invoke-Build)
#>

[cmdletBinding()]
param(
	# App defintion
	$appName = "CowFish",
	$sln  = (Join-Path $BL.RepoRoot  "/src/CowFish.sln" ),
	$target  = "Release",
	$platform  = "x64",

	# projects
	$projectCowFish = @{
		name = "CowFish";
		file = (Join-Path $BL.RepoRoot  "/src/CowFish/CowFish.csproj" );
		exe = "CowFish.exe";
		dir = "CowFish";
		dstExe = "cowfish.exe";
	},

	$projects = @($projectCowFish),

	# main path, dirs
	$toolsDir = (Join-Path  $BL.RepoRoot "tools"),
	$BHDir = (Join-Path  $toolsDir "dev-helpers"),
	$srcDir = (Join-Path $BL.RepoRoot "src"),
	$scriptsPath = $BL.ScriptsPath,
	$buildTmpDir  = (Join-Path $BL.BuildOutPath "tmp" ),
	$buildReadyDir  = (Join-Path $BL.BuildOutPath "ready" ),
	$serverDir = "C:\work\users\AntyPiracy\",
	$Dirs = (@{"marge" = "marge"; "build" = "build"; "nuget" = "nuget"; "main" = "main"}),
	$buildWorkDir  = (Join-Path $buildTmpDir "build" ),

	# tools
	$nuget = (Join-Path $toolsDir  "nuget.exe"),
	$libz = (Join-Path $toolsDir  "LibZ.Tool/tools/libz.exe"),
	$7zip = (Join-Path $toolsDir  "7-Zip.CommandLine/tools/7za.exe")

)

# Msbuild 
Set-Alias MSBuild (Resolve-MSBuild)

# insert tools
. (Join-Path $BHDir  "ps\misc.ps1")
. (Join-Path $BHDir  "ps\io.ps1")
. (Join-Path $BHDir  "ps\syrup.ps1")
. (Join-Path $BHDir  "ps\assembly-tools.ps1")



## Functions

function BuildFn ($currentProjects){


	$outMain = (Join-Path $buildTmpDir $Dirs.build  )
	EnsureDirExists $buildWorkDir

	foreach ($p in $currentProjects ) {

				Write-Build Green "*** Build $($p.Name) *** "
				$out = (Join-Path $outMain  $p.dir )
			
				try {

					EnsureDirExistsAndIsEmpty $out 
					$projectFile = $p.file

					"Build; Project file: $projectFile"
					"Build; out dir: $out"
					"Build; Target: $target"
				
					$bv = $BL.BuildVersion

					"AssemblyVersion: $($bv.AssemblyVersion)"
					"AssemblyVersion: $($bv.AssemblyFileVersion)"
					"AssemblyVersion: $($bv.AssemblyInformationalVersion)"

					$srcWorkDir = Join-Path $srcDir $p.dir
					BackupTemporaryFiles $srcWorkDir  "Properties\AssemblyInfo.cs"
					UpdateAssemblyInfo $srcWorkDir $bv.AssemblyVersion $bv.AssemblyFileVersion $bv.AssemblyInformationalVersion $p.name "DenebLab" "DenebLab"
					exec { MSBuild $projectFile /v:quiet  /p:Configuration=$target /p:OutDir=$out   } 
				}

				catch {
					RestoreTemporaryFiles $srcWorkDir
					throw $_.Exception
					exit 1
				}
				finally {
					RestoreTemporaryFiles $srcWorkDir
				}
		}
}

function MargFn ($currentProjects){

	foreach ($p in $currentProjects ) {

		$buildDir = [System.IO.Path]::Combine( $buildTmpDir , $Dirs.build,  $p.dir )
		$margedDir = [System.IO.Path]::Combine( $buildTmpDir , $Dirs.marge,  $p.dir )
	
		Set-Location  $buildDir
		EnsureDirExistsAndIsEmpty $margedDir 

		$dlls = [System.IO.Directory]::GetFiles($buildDir, "*.dll")
		$exclude = $donotMarge | Foreach-Object { "--exclude=$_" }

		foreach ($f in  $dlls ){
			Copy-Item $f -Destination $margedDir
		}

		$mainFile = [System.IO.Path]::Combine( $buildDir, $p.exe )
		CopyIfExistsFn  ($mainFile) $margedDir 
		$configFile = [System.IO.Path]::Combine( $buildDir,  "$($p.exe).config" )
		CopyIfExistsFn  ($configFile) $margedDir 


	
		
		
		$src = "$scriptsPath/nlog/$($p.name).NLog.config"
		$dst = "$margedDir/NLog.config"
		
		if([System.IO.File]::Exists("NLog.config"))
		{
			"Copy fixed version NLog.config; Src: $src ; Dst: $dst "
			Copy-Item  $src -Destination $dst -Force		
		} else {
			"Can not find fixed version NLog.config; Src: $src ; Dst: $dst "
		}


		
		if([System.IO.File]::Exists("NLog.config")){
			Copy-Item "NLog.config"  -Destination $margedDir
		}
		Set-Location  $margedDir
		"Marge in dir: $margedDir"
		$appConfigFile = $p.exe + ".config"
		If (-not (Test-Path $appConfigFile)){
			
			& $libz inject-dll --assembly $p.exe --include *.dll  $exclude --move 
		} else {
			"App config path exists: $appConfigFile"
			& $libz inject-dll --assembly $p.exe -b $appConfigFile --include *.dll  $exclude --move 
		}

		
		
	
	}

}

function DownloadIfNotExists($src , $dst){

	If (-not (Test-Path $dst)){
		$dir = [System.IO.Path]::GetDirectoryName($dst)
		If (-not (Test-Path $dir)){
			New-Item -ItemType directory -Path $dir
		}
	 	Invoke-WebRequest $src -OutFile $dst
	} 
}

function CopyIfExistsFn($src, $dst){

	if([System.IO.File]::Exists($src)){
		"Copy '$src' to '$dst'"
		Copy-Item $src  -Destination $dst
	} else {
		"Can't copy: $src - not found"
	}
	
}

## End functions

# Synopsis: Package-Restore
task RestorePackages {

	 Set-Location   $BL.RepoRoot
	"Restore packages: Sln: {$sln}"
	exec {  &$nuget restore $sln  }
}

# Synopsis: Remove temp files.
task Clean {

	Write-Host $buildTmpDir
	EnsureDirExistsAndIsEmpty $buildTmpDir
	Push-Location $buildTmpDir
	Remove-Item out -Recurse -Force -ErrorAction 0
	Pop-Location
}

task Get-Tools {
	Write-Build Green "Check: Nuget"
	DownloadIfNotExists "https://dist.nuget.org/win-x86-commandline/latest/nuget.exe" $nuget 


}

task Build {
	$currentProjects  = @( $projectCowFish  )
	BuildFn $currentProjects
}

task  Marge  {
	$currentProjects  = @( $projectCowFish  )
	MargFn $currentProjects
}

task Init  Clean, Get-Tools, RestorePackages
task Main Build, Marge
task . Init, Main