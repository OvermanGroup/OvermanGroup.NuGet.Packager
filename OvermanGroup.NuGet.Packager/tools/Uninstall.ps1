param($installPath, $toolsPath, $package, $project)

$propsName = $package.Id + '.props'
$targetsName = $package.Id + '.targets'

# load the MSBuild assembly
Add-Type -AssemblyName 'Microsoft.Build, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'

#
# Manually remove some items just in case a previous package didn't uninstall correctly
#
$msbuild = [Microsoft.Build.Evaluation.ProjectCollection]::GlobalProjectCollection.GetLoadedProjects($project.FullName) | Select-Object -First 1
$itemsToRemove = @()
$itemsToRemove += $msbuild.Xml.Targets |? { $_.Name -eq "OverPackBookmark" }
$itemsToRemove += $msbuild.Xml.Imports |? { $_.Project.EndsWith($propsName) }
$itemsToRemove += $msbuild.Xml.Imports |? { $_.Project.EndsWith($targetsName) }
if ($itemsToRemove -and $itemsToRemove.length)
{
	foreach ($itemToRemove in $itemsToRemove)
	{
		$msbuild.Xml.RemoveChild($itemToRemove) | Out-Null
	}

	Write-Host "Saving project"
	$project.Save()
}
