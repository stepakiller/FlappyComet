using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace FlappyComet.EditorTools
{
    public static class BuildScript
    {
        const string OutputDir = "Builds/WebGL";

        [MenuItem("Flappy Comet/Build WebGL")]
        public static void BuildWebGL()
        {
            ProjectSetup.EnsureGenerated();

            string[] scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
            var options = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = OutputDir,
                target = BuildTarget.WebGL,
                options = BuildOptions.None
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            BuildSummary summary = report.summary;

            if (Application.isBatchMode && summary.result != BuildResult.Succeeded)
                EditorApplication.Exit(1);
            else if (!Application.isBatchMode && summary.result == BuildResult.Succeeded)
                EditorUtility.RevealInFinder(OutputDir);
        }
    }
}
