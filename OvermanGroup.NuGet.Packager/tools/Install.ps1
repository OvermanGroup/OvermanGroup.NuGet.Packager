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

# make sure our targets import was added
$targetsImport = $msbuild.Xml.Imports | Where-Object { $_.Project.EndsWith($targetsName) } | Select-Object -First 1
if (!$targetsImport) {
	throw "ERROR: Cannot find $targetsName. Please verify that $package.Id was installed."
}

# make sure the props are immediately above the targets import
Write-Host "Moving $propsName import above $targetsName"
$propsImport.Parent.RemoveChild($propsImport)
$msbuild.Xml.InsertBeforeChild($propsImport, $targetsImport);

# check if the project is missing a NuSpec
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
