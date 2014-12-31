param($installPath, $toolsPath, $package, $project)

$propsName = $package.Id + '.props'

# load the MSBuild assembly
Add-Type -AssemblyName 'Microsoft.Build, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'

# load the current MSBuild project
$msbuild = [Microsoft.Build.Evaluation.ProjectCollection]::GlobalProjectCollection.GetLoadedProjects($project.FullName) | Select-Object -First 1

# find all the imports and targets added by this package
$itemsToRemove = @()

# allow multiple items just in case a past package didn't uninstall correctly
$itemsToRemove += $msbuild.Xml.Imports | Where-Object { $_.Project.EndsWith($propsName) }

# remove the items
if ($itemsToRemove -and $itemsToRemove.length)
{
	foreach ($itemToRemove in $itemsToRemove)
	{
		$msbuild.Xml.RemoveChild($itemToRemove) | Out-Null
	}

	Write-Host "Saving project"
	$project.Save()
}
