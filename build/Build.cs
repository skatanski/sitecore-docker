using System;
using System.Linq;
using System.Threading;
using Nuke.Common;
using Nuke.Common.ProjectModel;
using static Nuke.Common.EnvironmentInfo;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.PathConstruction;
using static Nuke.Docker.DockerTasks;
using static Nuke.Common.Tools.Git.GitTasks;
using static DockerComposeTasks;
using Nuke.Docker;
using Nuke.Common.Tooling;

partial class Build : NukeBuild
{
    /// <summary>
    /// A build version that will be appended to the image name.
    /// Ignored if its empty
    /// </summary>
    [Parameter("Docker image build version")]
    public readonly string BuildVersion = GetBuildVersionFromTag();

    [Parameter("Force push without git tag on the current commit")]
    public readonly bool ForcePush = false;

    public static int Main () => Execute<Build>(x => x.All);

    private static readonly AbsolutePath Files = RootDirectory / "Files";

    // Docker options
    [Parameter("Docker image repository prefix, e.g. my.docker-image.repo/")]
    readonly string RepoImagePrefix = "";

    // Silence docker-compose output, otherwise non-error output is written to stderr
    private static readonly string DockerComposeSilenceOptions = "--log-level CRITICAL --no-ansi";

    // Get the container created by docker-compose
    private string GetContainerName(string serviceName) {
        var dirName = new System.IO.DirectoryInfo(System.IO.Directory.GetCurrentDirectory()).Name;
        return $"{dirName}_{serviceName}_1";
    }

    // Get the first tag on the current (ie. HEAD) commit
    // returns null if not tagged
    private static string GetCurrentGitTag() {
        try {
            var outputs = Git("describe --exact-match --tags", logOutput: false);

            if (outputs.Any()) {
                return outputs.Single().Text;
            }
        } catch {
            // no tag found
        }
        
        Console.WriteLine("No Git tag found for current commit");
        return null;
    }

    // Has the current commit a git tag?
    private static bool HasGitTag() {
        return GetCurrentGitTag() != null;
    }

    // Git tags should have the following format: <sitecore version>-<build version>
    // This set the build version from the tag (if it exists)
    private static string GetBuildVersionFromTag() {
        var gitTag = GetCurrentGitTag();
        if (gitTag != null) {
            return gitTag.Substring(gitTag.IndexOf("-") + 1);
        }
        return "";
    } 

    Target All => _ => _
        .DependsOn(Xp, XpSxa, XpJss, Xc, XcSxa, XcJss);

    Target Push => _ => _
        .OnlyWhenDynamic(() => HasGitTag() || ForcePush)
        .DependsOn(PushXp, PushXpSxa, PushXpJss, PushXc, PushXcSxa, PushXcJss);
}
