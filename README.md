# OvermanGroup.NuGet.Packager
This NuGet package adds configurable MSBuild tasks to your project so that you can easily create and publish NuGet packages automatically after your project is built.

## PURPOSE
I found myself re-creating the same batch and powershell Post Build scripts to generate NuGet packages for my C# projects very often. When I first searched NuGet for existing tools to automate package creation, I wasn't pleased with the results. There were a bunch of existing packages that added a lot of bloat to my projects and weren't easy to configure. In the end, I wanted something very familiar to OctoPack which is used by Octopus Deploy in our environment. So this package was born and my very first contribution to NuGet. Hope you enjoy and please let me know any comments or suggestions.

## FEATURES
- Adds a NuSpec file if one doesn't already exist
- Automatically finds NuGet.exe
- Creates packages from project files (csproj, vbproj, etc)
- Creates packages from NuSpec files
- Publishes packages to a local folder
- Publishes packages to an online repository
- Fully customizable configuration
- Pre and post build events
- TFS build integration

## INSTALLATION
To install [OvermanGroup.NuGet.Packager][8], simply run the following command in the Package Manager Console:

    PM> Install-Package OvermanGroup.NuGet.Packager

## CONFIGURATION
After installing, your Visual Studio project will have an additional `OvermanGroup.NuGet.Packager.props` file which is used to configure MSBuild properties that specify how your NuGet package should be created and published. These properties may also be specified as additional MSBuild arguments in a TFS build definition.

Below is the minimal configuration required for ``OvermanGroup.NuGet.Packager.props`` to create a NuGet package:
```xml
<?xml version="1.0" encoding="utf-8"?>
<Project ToolsVersion="4.0" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
	<PropertyGroup>
		<RunOverPack Condition="'$(RunOverPack)'==''">true</RunOverPack>
	</PropertyGroup>
</Project>
```

### RunOverPack
By default, all the tasks are disabled and you must explicitly enable them by specifing the property below. If this property is omitted, blank, or false then your NuGet package will not be created or published.
```xml
<RunOverPack Condition="'$(RunOverPack)'==''">true</RunOverPack>
```

### OverPackNuGetExePath
Specifies the location for the NuGet.exe command. The default behavior is to check the following in order:
* The latest version of [NuGet.CommandLine][1] (under the 'tools' subfolder) in the solution packages folder.
* Attempting to find 'NuGet.exe' in the system's PATH environment variable by using the [SearchPath][2] Win32 API.

Usually one of these methods will always succeed, but if any of the above fail, then the user must specify the location of the NuGet.exe command with this property.
```xml
<OverPackNuGetExePath Condition="'$(OverPackNuGetExePath)' == ''"></OverPackNuGetExePath>
```

### OverPackInputFile
Specifies the input to the `NuGet.exe pack` command which can be either a [project file][3] or a [NuSpec file][4]. The default behavior specifies the project file in order to support [replacement tokens][5].
```xml
<OverPackInputFile Condition="'$(OverPackInputFile)' == ''">$(ProjectPath)</OverPackInputFile>
<OverPackInputFile Condition="'$(OverPackInputFile)' == ''">$(ProjectDir)$(TargetName).nuspec</OverPackInputFile>
```

### OverPackOutputDirectory
Specifies the directory where to place the newly created NuGet package file. The default specifies the build output directory.
```xml
<OverPackOutputDirectory Condition="'$(OverPackOutputDirectory)' == ''">$(TargetDir)</OverPackOutputDirectory>
```

### OverPackBasePath
The base path of the files defined in the nuspec file. The default specifies the build output directory. For any files to be properly included in the NuGet package, make sure to set the `Copy to Output Directory` property to anything other than `Do not copy`.
```xml
<OverPackBasePath Condition="'$(OverPackBasePath)' == ''">$(TargetDir)</OverPackBasePath>
```

### OverPackVersion
Overrides the version number for the NuGet package. If not specified, uses the version from the project or nuspec file.
```xml
<OverPackVersion Condition="'$(OverPackVersion)' == ''"></OverPackVersion>
```

### OverPackExclude
Specifies one or more wildcard patterns to exclude when creating the NuGet package. Separate multiple entries with a semicolon `;`
```xml
<OverPackExclude Condition="'$(OverPackExclude)' == ''"></OverPackExclude>
```

### OverPackSymbols
Determines if a NuGet package containing sources and symbols should be created. When specified with a nuspec, creates a regular NuGet package file and the corresponding symbols package.
```xml
<OverPackSymbols Condition="'$(OverPackSymbols)' == ''">false</OverPackSymbols>
```

### OverPackTool
Determines if the output files of the project should be in the tool folder.
```xml
<OverPackTool Condition="'$(OverPackTool)' == ''">false</OverPackTool>
```

### OverPackNoDefaultExcludes
Prevent default exclusion of NuGet package files and files and folders starting with a dot e.g. .svn.
```xml
<OverPackNoDefaultExcludes Condition="'$(OverPackNoDefaultExcludes)' == ''">false</OverPackNoDefaultExcludes>
```

### OverPackNoPackageAnalysis
Specifies if the command should not run package analysis after building the NuGet package.
```xml
<OverPackNoPackageAnalysis Condition="'$(OverPackNoPackageAnalysis)' == ''">false</OverPackNoPackageAnalysis>
```

### OverPackIncludeReferencedProjects
Include referenced projects either as dependencies or as part of the package. If a referenced project has a corresponding nuspec file that has the same name as the project, then that referenced project is added as a dependency. Otherwise, the referenced project is added as part of the NuGet package.
```xml
<OverPackIncludeReferencedProjects Condition="'$(OverPackIncludeReferencedProjects)' == ''">false</OverPackIncludeReferencedProjects>
```

### OverPackExcludeEmptyDirectories
Prevents inclusion of empty directories when building the NuGet package.
```xml
<OverPackExcludeEmptyDirectories Condition="'$(OverPackExcludeEmptyDirectories)' == ''">false</OverPackExcludeEmptyDirectories>
```

### OverPackVerbosity
Specifies the amount of details show from the command output: normal, quiet, detailed. This property is used for both the 'pack' and 'push' NuGet commands.
```xml
<OverPackVerbosity Condition="'$(OverPackVerbosity)' == ''">normal</OverPackVerbosity>
```

### OverPackMinClientVersion
Specifies the minClientVersion attribute for the created package. This value will override the value of the existing minClientVersion attribute (if any) in the .nuspec file.
```xml
<OverPackMinClientVersion Condition="'$(OverPackMinClientVersion)' == ''"></OverPackMinClientVersion>
```

### OverPackExtraArguments
Specifies any additional arguments to pass to NuGet when invoking the 'pack' command.
```xml
<OverPackExtraArguments Condition="'$(OverPackExtraArguments)' == ''"></OverPackExtraArguments>
```

### OverPackPublishToFolder
Specifies the UNC/folder location where to publish the package to. If not specified, the default uses [$(TF_BUILD_BINARIESDIRECTORY)][6] for TFS builds.
```xml
<OverPackPublishToFolder Condition="'$(OverPackPublishToFolder)' == ''"></OverPackPublishToFolder>
```

### OverPackPublishToHttp
Specifies the server URL where to publish the package to. If not specified, nuget.org is used unless DefaultPushSource config value is set in the NuGet config file.
```xml
<OverPackPublishToHttp Condition="'$(OverPackPublishToHttp)' == ''"></OverPackPublishToHttp>
```

### OverPackPublishApiKey
Specifies the API key for the publishing server.
```xml
<OverPackPublishApiKey Condition="'$(OverPackPublishApiKey)' == ''"></OverPackPublishApiKey>
```

### OverPackPublishConfigFile
Specifies the NuGet configuation file. If not specified, %AppData%\NuGet\NuGet.config is used as the configuration file.
```xml
<OverPackPublishConfigFile Condition="'$(OverPackPublishConfigFile)' == ''"></OverPackPublishConfigFile>
```

### OverPackPublishArguments
Specifies any additional arguments to pass to NuGet when invoking the 'push' command.
```xml
<OverPackPublishArguments Condition="'$(OverPackPublishArguments)' == ''"></OverPackPublishArguments>
```

## BUILD EVENTS
Custom targets for pre/post build events may be added by using either the predefined `BeforeOverPack` and `AfterOverPack` targets or by modifying the `OverPackDependsOn` property with custom targets. These custom pre/post build targets **MUST** be defined after the `import` statement for the package. See the example below:
```xml
<Import Project="..\example\path\OvermanGroup.NuGet.Packager.targets" />

<!-- The built-in C# build events -->
<Target Name="BeforeBuild">
  <Message Text="Running BeforeBuild" Importance="high" />
</Target>
<Target Name="AfterBuild">
  <Message Text="Running AfterBuild" Importance="high" />
</Target>

<!-- The predefined build events for OverPack -->
<Target Name="BeforeOverPack">
  <Message Text="Running BeforeOverPack" Importance="high" />
</Target>
<Target Name="AfterOverPack">
  <Message Text="Running AfterOverPack: @(OverPackOutputFile)" Importance="high" Condition="Exists('@(OverPackOutputFile)')" />
  <Message Text="Running AfterOverPack: @(OverPackSymbolsFile)" Importance="high" Condition="Exists('@(OverPackSymbolsFile)')" />
</Target>

<!-- Custom build events by modifying OverPackDependsOn -->
<Target Name="BeforeOverPackDependsOn">
  <Message Text="Running BeforeOverPackDependsOn" Importance="high" />
</Target>
<Target Name="AfterOverPackDependsOn">
  <Message Text="Running AfterOverPackDependsOn: @(OverPackOutputFile)" Importance="high" Condition="Exists('@(OverPackOutputFile)')" />
  <Message Text="Running AfterOverPackDependsOn: @(OverPackSymbolsFile)" Importance="high" Condition="Exists('@(OverPackSymbolsFile)')" />
</Target>
<PropertyGroup>
  <OverPackDependsOn>
    BeforeOverPackDependsOn;
    $(OverPackDependsOn);
    AfterOverPackDependsOn
  </OverPackDependsOn>
</PropertyGroup>
```

## TFS BUILD INTEGRATION
Since all the MSBuild properties check if they have already been specified, you can also make any customizations using TFS build definitions such as:
- `/p:RunOverPack=true`
- `/p:OverPackPublishToFolder=\\server\share\drop_folder\`

Here is an example TFS Build Definition
![OverPack Build Definition](images/OverPackBuildDefinition.png?raw=true)

## FEEDBACK
Please provide any feedback, comments, or issues to this GitHub project [here][7].

## What's with the name?
So "Overman Group" is an LLC that I created in college with the intent to someday start my own software company. Well 12 years later, I did nothing on that horizon and have been busy with my day-to-day engineering job. I prefixed this package with OvermanGroup solely for organizational purposes just in case there are naming conflicts with anyone else.

[1]: https://www.nuget.org/packages/NuGet.CommandLine/
[2]: http://msdn.microsoft.com/en-us/library/windows/desktop/aa365527%28v=vs.85%29.aspx
[3]: http://docs.nuget.org/docs/Creating-Packages/Creating-and-Publishing-a-Package#From_a_project
[4]: http://docs.nuget.org/docs/reference/nuspec-reference
[5]: http://docs.nuget.org/docs/reference/nuspec-reference#Replacement_Tokens
[6]: http://msdn.microsoft.com/en-us/library/hh850448.aspx
[7]: https://github.com/OvermanGroup/OvermanGroup.NuGet.Packager/issues
[8]: https://www.nuget.org/packages/OvermanGroup.NuGet.Packager/
