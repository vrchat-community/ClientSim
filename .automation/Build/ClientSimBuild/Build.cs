using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tools.DocFX;

class Build : NukeBuild
{
    AbsolutePath DocFxPath = RootDirectory.Parent / "Docs";
    AbsolutePath DocFxConfigPath => DocFxPath / "docfx.json";
    AbsolutePath DocFXServePath => DocFxPath / "_site";

    public static int Main () => Execute<Build>(x => x.DocFxBuild);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    Target DocFxServe => _ => _
        .DependsOn(DocFxBuild)
        .Executes(() =>
        {
            DocFXTasks.DocFXServe(s => s
                .SetFolder(DocFXServePath));
        });

    Target DocFxBuild => _ => _
        .DependsOn(DocFxMetadata)
        .Executes(() =>
        {
            DocFXTasks.DocFXBuild(s => s
                .SetConfigFile(DocFxConfigPath)
                .SetDisableGitFeatures(true)
            );
        });
    
    Target DocFxMetadata => _ => _
        .Executes(() =>
        {
            Assert.FileExists(DocFxConfigPath);
            DocFXTasks.DocFX($"metadata {DocFxConfigPath}", DocFxPath);
        });

}
