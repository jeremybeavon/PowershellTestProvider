function Add-TestFiles {
	param
	(
		[Parameter(Mandatory=$true)]
		[object[]]$files
	)

	[PowershellTestProvider.TestProvider]::AddTestFiles($files)
}

function Remove-AllTestFiles {
	[PowershellTestProvider.TestProvider]::RemoveAllTestFiles()
}

Export-ModuleMember Add-TestFiles
Export-ModuleMember Remove-AllTestFiles