param($installPath, $toolsPath, $package, $project)

$propsName = $package.Id + '.props'
$targetsName = $package.Id + '.targets'

# Need to load MSBuild assembly if it's not loaded yet.
Add-Type -AssemblyName 'Microsoft.Build, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'

# Grab the loaded MSBuild project for the project
$msbuild = [Microsoft.Build.Evaluation.ProjectCollection]::GlobalProjectCollection.GetLoadedProjects($project.FullName) | Select-Object -First 1

# Add the import with a condition, to allow the project to load without the props present.
$propsImport = $msbuild.Xml.AddImport($propsName)
$propsImport.Condition = "Exists('$propsName')"

# There's no public constructor to create a ProjectImportElement directly.
# So we have to cheat by adding Import at the end, then remove it and insert it at the correct location
Write-Host "Adding $propsName to $($project.Name)"
$targetsImport = $msbuild.Xml.Imports | Where-Object { $_.Project.EndsWith($targetsName) } | Select-Object -First 1
$propsImport.Parent.RemoveChild($propsImport)
$msbuild.Xml.InsertBeforeChild($propsImport, $targetsImport);

# Check if the project is missing a NuSpec and if so, lets add one
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
