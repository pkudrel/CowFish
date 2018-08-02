#Borrowed from psake
Function SyrupGenerateInfoFile {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory=$true, Position=0)][string]$filePath,
        [Parameter(Mandatory=$true, Position=1)][string]$productName,
        [Parameter(Mandatory=$true, Position=2)][string]$semVer,
        [Parameter(Mandatory=$true, Position=3)][string]$branchName,
        [Parameter(Mandatory=$true, Position=4)][string]$date
     
    )

		[System.IO.FileInfo] $f = Get-Item -Path $FilePath 

		 $r = [PSCustomObject]  @{
			"App" = $productName;
			"Name" =[IO.Path]::GetFileNameWithoutExtension( $filePath); 
			"File" = $f.Name; 
			"Sha" = (Get-FileHash $f.FullName -Algorithm SHA1).Hash;
			"SemVer" = $semVer;
			"Channel" = $branchName;
			"RelaseDate" = $date
		 }
		
		$dst = $f.FullName + ".syrup"
		$json = (ConvertTo-json $r)		
		[System.IO.File]::WriteAllLines($dst, $json, [text.encoding]::UTF8)
		"Syrup file: $dst"
}





