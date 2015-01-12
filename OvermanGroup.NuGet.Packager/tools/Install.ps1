param($installPath, $toolsPath, $package, $project)

$propsName = $package.Id + '.props'
$targetsName = $package.Id + '.targets'

# load the MSBuild assembly
Add-Type -AssemblyName 'Microsoft.Build, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'

# load the current MSBuild project
$msbuild = [Microsoft.Build.Evaluation.ProjectCollection]::GlobalProjectCollection.GetLoadedProjects($project.FullName) | Select-Object -First 1

# check if an import for the props already exists
$propsImport = $msbuild.Xml.Imports | Where-Object { $_.Project.EndsWith($propsName) } | Select-Object -First 1

# if not, create a new import for the props with a condition
if (!$propsImport) {
	Write-Host "Adding $propsName to $($project.Name)"
	$propsImport = $msbuild.Xml.AddImport($propsName)
	$propsImport.Condition = "Exists('$propsName')"
}

#
# Make sure our targets import was added
#
$targetsImport = $msbuild.Xml.Imports | Where-Object { $_.Project.EndsWith($targetsName) } | Select-Object -First 1
if (!$targetsImport) {
	throw "ERROR: Cannot find $targetsName. Please verify that $package.Id was installed."
}

#
# OverPackBookmark
# We use a dummy target to make sure our import remains at the same exact line
# in the MSBuild file. This is necessary when clients upgrade their package
# which causes an uninstall and reinstall of the package and normally would
# re-add the package to bottom vs the same location. We do this so that if the
# client added pre/post build events, then their targets would always be after
# our import.
#
$bookmark = $msbuild.Xml.Targets | Where-Object { $_.Name -eq "OverPackBookmark" } | Select-Object -First 1
if (!$bookmark) {
	Write-Host "Adding new bookmark for $targetsName"
	$bookmark = $msbuild.Xml.CreateTargetElement("OverPackBookmark")
	$msbuild.Xml.InsertBeforeChild($bookmark, $targetsImport)
} elseif ($bookmark.NextSibling -ne $targetsImport) {
	Write-Host "Moving $targetsName below bookmark"
	$targetsImport.Parent.RemoveChild($targetsImport)
	$msbuild.Xml.InsertAfterChild($targetsImport, $bookmark)
}

#
# Make sure the props are immediately above the targets import
#
Write-Host "Moving $propsName import above $targetsName"
$propsImport.Parent.RemoveChild($propsImport)
$msbuild.Xml.InsertBeforeChild($propsImport, $targetsImport)

#
# Check if the project is missing a NuSpec
#
$nuspec = $project.ProjectItems | Where-Object { $_.Name.EndsWith(".nuspec", "OrdinalIgnoreCase") } | Select-Object -First 1
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
