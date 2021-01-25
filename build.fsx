#r "paket:
nuget Fake.DotNet.Cli
nuget Fake.IO.FileSystem
nuget Fake.Core.Target
nuget Fake.Tools.GitVersion 
nuget FSharp.Core 4.7.0
nuget Fake.DotNet.AssemblyInfoFile
nuget Fake.Core.Xml

github albumprinter/Fake.Extra
 //"
#load ".fake/build.fsx/intellisense.fsx"
#load ".fake/build.fsx/paket-files/albumprinter/Fake.Extra/GitVersionTool.fs"

open System.IO
open Fake.Core
open Fake.DotNet
open Fake.IO
open Fake.IO.FileSystemOperators
open Fake.IO.Globbing.Operators
open Fake.Core.TargetOperators
open Fake.Tools

open Newtonsoft.Json

open Albelli

    module ScriptVars =
        let artifacts = "./artifacts" |> Path.GetFullPath

        let version() = GitVersionTool.generateProperties()

        let nugetKey() = "NUGET_API_KEY" |> Environment.environVarOrFail

        let nugetSource() = "https://www.nuget.org/api/v2/package"


Target.initEnvironment ()

Target.create "Trace" (fun _ ->
    Trace.log "Hello world"
)

Target.create "SetVersion" (fun _ ->
    let gitVersion = ScriptVars.version()
    let version = gitVersion.FullSemVer
    
    Trace.logfn "Gitversion is %s" version

    AssemblyInfoFile.createCSharp "SharedAssemblyInfo.cs"
        [AssemblyInfo.Version version
         AssemblyInfo.FileVersion version
         AssemblyInfo.InformationalVersion version
         AssemblyInfo.Company "Albelli"
         AssemblyInfo.Copyright "Albelli"
         AssemblyInfo.ComVisible false]

    // workaround for nuget publish, which doesn't
    // care whether the version is already set for the dll
    let searchPattern = "<Version>(.+?)<\/Version>"
    let newVersionTag = sprintf "<Version>%s</Version>" gitVersion.NuGetVersionV2

    let projVersionPath = "/*[local-name()='Project']/*[local-name()='PropertyGroup']/*[local-name()='Version']/text()"

    !! "./src/**/*.*proj"
    |> Seq.iter (fun proj -> 
        version
        |> Xml.poke proj projVersionPath
        )

    version
    |> Xml.poke "./src/Template.nuspec" "/*[local-name()='package']/*[local-name()='metadata']/*[local-name()='version']/text()"
)

Target.create "Clean" (fun _ ->
    !! ScriptVars.artifacts
    ++ "src/*/bin"
    ++ "tests/*/bin"
    ++ "test/*/bin"
    ++ "src/*/obj"
    ++ "tests/*/obj"
    ++ "test/*/obj"
    |> Shell.cleanDirs
)

Target.create "Build" (fun _ ->
    !! "src/**/*.*proj"
    |> Seq.iter (DotNet.build id)
)

Target.create "All" ignore

"Clean"
//   ==> "Trace"
  ==> "SetVersion"
  //==> "Build"
  ==> "All"

Target.runOrDefault "All"
