param($installPath, $toolsPath, $package, $project)

$propsName = $package.Id + '.props'
$targetsName = $package.Id + '.targets'

# load the MSBuild assembly
Add-Type -AssemblyName 'Microsoft.Build, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'

# load the current MSBuild project
$msbuild = [Microsoft.Build.Evaluation.ProjectCollection]::GlobalProjectCollection.GetLoadedProjects($project.FullName) | Select-Object -First 1

#
# Manually remove some items just in case a previous package didn't uninstall correctly
#
$itemsToRemove = @()
$itemsToRemove += $msbuild.Xml.Targets |? { $_.Name -eq "OverPackBookmark" }
$itemsToRemove += $msbuild.Xml.Imports |? { $_.Project.EndsWith($propsName) }
if ($itemsToRemove -and $itemsToRemove.length)
{
	foreach ($itemToRemove in $itemsToRemove)
	{
		$msbuild.Xml.RemoveChild($itemToRemove) | Out-Null
	}
}

#
# Make sure our targets import was added
#
$targetsImport = $msbuild.Xml.Imports |? { $_.Project.EndsWith($targetsName) } | Select-Object -First 1
$nugetTarget = $msbuild.Xml.Targets |? { $_.Name -eq "EnsureNuGetPackageBuildImports" } | Select-Object -First 1
if (!$targetsImport -or !$nugetTarget) {
	throw "ERROR: Cannot find $targetsName. Please verify that $package.Id was installed."
}

#
# Check if the project is missing a NuSpec
#
$nuspec = $project.ProjectItems |? { $_.Name.EndsWith(".nuspec", "OrdinalIgnoreCase") } | Select-Object -First 1
if (!$nuspec) {
	$projectPath = Get-Item ($project.FullName)
	$projectFolder = $projectPath.DirectoryName
	$nuspecName = "$($project.Name).nuspec"
	$nuspecSource = Join-Path $toolsPath "NuSpecTemplate.xml"
	$nuspecTarget = Join-Path $projectFolder $nuspecName

	Write-Host "Adding $nuspecName to $($project.Name)"
	Copy-Item $nuspecSource $nuspecTarget
	$project.ProjectItems.AddFromFile($nuspecTarget)
}

Write-Host "Saving project"
$project.Save()
