# Background
If your writing Pester tests on scripts that have a lot of file system access, these can become overly complicated. You basically
have 2 options:
- Writing lots of mocks for Get-ChildItem, Test-Path and Get-Content, which isn't fun. 
- Increase the complexity of the code by wrapping file system operations into functions that can be mocked separately

PowershellTestProvider introduces a test drive in Poweshell that mimics the file system and means file IO cmdlets don't need to be
mocked.

# Usage
```powershell
# Installation:
Import-Module PowershellTestProvider.dll

# Create "files"
New-Item -Path "test:TestFolder\TestFile.txt" -Value "this is a test"
New-Item -Path "test:TestFolder\TestFile2.txt" -Value "this is another test"

# Get "files" in "directory"
Get-ChildItem "test:TestFolder"

# Read "file"
Get-Content "test:TestFolder\TestFile.txt" -Raw

# Test path
Test-Path "test:TestFolder\TestFile.txt"
```

# Progress
Fairly minimal support at the moment:
- New-Item can create a "file"
- Basic usage of Get-ChildItem work
- Test-Path works
- Get-Content only with -Raw
