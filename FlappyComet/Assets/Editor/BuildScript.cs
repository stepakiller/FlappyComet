using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace FlappyComet.EditorTools
{
    public static class BuildScript
    {
        const string BuildsRoot = "Builds";
        const string WindowsPath = BuildsRoot + "/Windows/FlappyComet.exe";
        const string AndroidPath = BuildsRoot + "/Android/FlappyComet.apk";
        const string WebGLPath = BuildsRoot + "/WebGL";
        const string WebGLTemplate = "PROJECT:Better2020";

        [MenuItem("Build/Windows", priority = 1)]
        public static void BuildWindows() => RunSingle(BuildTarget.StandaloneWindows64, WindowsPath);

        [MenuItem("Build/Android", priority = 2)]
        public static void BuildAndroid() => RunSingle(BuildTarget.Android, AndroidPath);

        [MenuItem("Build/WebGL", priority = 3)]
        public static void BuildWebGL() => RunSingle(BuildTarget.WebGL, WebGLPath);

        [MenuItem("Build/Build All", priority = 20)]
        public static void BuildAll()
        {
            bool win = Build(BuildTarget.StandaloneWindows64, WindowsPath);
            bool android = Build(BuildTarget.Android, AndroidPath);
            bool webgl = Build(BuildTarget.WebGL, WebGLPath);

            Finish(win && android && webgl, BuildsRoot);
        }

        [MenuItem("Flappy Comet/Build WebGL")]
        public static void BuildWebGLLegacy() => BuildWebGL();

        static void RunSingle(BuildTarget target, string path)
        {
            bool ok = Build(target, path);
            string reveal = target == BuildTarget.WebGL ? path : Path.GetDirectoryName(path);
            Finish(ok, reveal);
        }

        static bool Build(BuildTarget target, string path)
        {
            if (!BuildPipeline.IsBuildTargetSupported(BuildPipeline.GetBuildTargetGroup(target), target))
                return false;

            ProjectSetup.EnsureGenerated();

            string[] scenes = EditorBuildSettings.scenes
                .Where(s => s.enabled)
                .Select(s => s.path)
                .ToArray();

            if (scenes.Length == 0)
                return false;

            PrepareTarget(target);

            var options = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = path,
                target = target,
                options = BuildOptions.None
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            return report.summary.result == BuildResult.Succeeded;
        }

        static void PrepareTarget(BuildTarget target)
        {
            switch (target)
            {
                case BuildTarget.Android:
                    EditorUserBuildSettings.buildAppBundle = false;
                    break;

                case BuildTarget.WebGL:
                    PlayerSettings.WebGL.template = WebGLTemplate;
                    break;
            }
        }

        static void Finish(bool ok, string revealPath)
        {
            if (Application.isBatchMode)
            {
                EditorApplication.Exit(ok ? 0 : 1);
                return;
            }

            if (ok)
                EditorUtility.RevealInFinder(revealPath);
            else
                EditorUtility.DisplayDialog("Build", "Build failed. See Console for details.", "OK");
        }
    }
}
