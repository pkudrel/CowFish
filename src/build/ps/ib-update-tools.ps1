<#
Tools to update Invoke-Build
#>



function IbUpdateIsNeeded {
    param(
            [Parameter(Mandatory=$true)][string]$ibVersionFilePath
    )

    if ([System.IO.File]::Exists($ibVersionFilePath))
    {
        $currentVer =  [System.IO.File]::ReadAllLines($ibVersionFilePath)[0]
        $repoVersion = (Invoke-RestMethod -Uri "https://api.github.com/repos/nightroman/Invoke-Build/tags" ) | Select-Object -ExpandProperty Name -First 1
        if($currentVer -eq $repoVersion ){
           Write-Host "Invoke-Build: Same version on disk and in repostory:  $currentVer"
           return $false
        } else {
            Write-Host  "Invoke-Build: Version in the repostyry: $repoVersion"
            Write-Host  "Invoke-Build: Version on disk: $currentVer"
            Write-Host  "Invoke-Build: Update needed"
            return $true
        }

    } else {
        "Invoke-Build: Cannot find IB file version. Path: $ibVersionFilePath"
        return $true
    }
}

function IbUpdateInvokeBuild  {
    
         param(
                [Parameter(Mandatory=$true)][string]$toolsPath
        )
        Write-Host "Invoke-Build: Update InvokeBuild"
        $ibDir = (Join-Path $toolsPath "\ib\")
        $ibDirTmp = (Join-Path $toolsPath "\Invoke-Build\")
        If (Test-Path $ibDir){
            Remove-Item $ibDir -Force -Recurse
        }
        If (Test-Path $ibDirTmp ){
            Remove-Item $ibDirTmp -Force -Recurse
        }
        Push-Location 
        try {
            Set-Location  $toolsPath 
            Invoke-Expression "& {$((New-Object Net.WebClient).DownloadString('https://github.com/nightroman/PowerShelf/raw/master/Save-NuGetTool.ps1'))} Invoke-Build"
            Rename-Item -path "Invoke-Build" -newname "ib"
            $repoVersion = (Invoke-RestMethod -Uri "https://api.github.com/repos/nightroman/Invoke-Build/tags" ) | Select-Object -ExpandProperty Name -First 1
            [System.IO.File]::WriteAllText($ibVersionFile,  $repoVersion)
            "Invoke-Build: Current version: $repoVersion"
        }
        finally {
            Pop-Location
        }
        
    }