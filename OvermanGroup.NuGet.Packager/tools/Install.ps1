param($installPath, $toolsPath, $package, $project)

$propsName = $package.Id + '.props'

# Need to load MSBuild assembly if it's not loaded yet.
Add-Type -AssemblyName 'Microsoft.Build, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'

# Grab the loaded MSBuild project for the project
$msbuild = [Microsoft.Build.Evaluation.ProjectCollection]::GlobalProjectCollection.GetLoadedProjects($project.FullName) | Select-Object -First 1

# Add the import with a condition, to allow the project to load without the props present.
$import = $msbuild.Xml.AddImport($propsName)
$import.Condition = "Exists('$propsName')"

# There's no public constructor to create a ProjectImportElement directly.
# So we have to cheat by adding Import at the end, then remove it and insert at the beginning
$import.Parent.RemoveChild($import)
$msbuild.Xml.InsertBeforeChild($import, $msbuild.Xml.FirstChild);

$project.Save()
