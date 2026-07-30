#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using UnityEditor.Build.Reporting;

namespace LCENano.EditorTools
{
    public static class RuntimeDiagnostics
    {
        static double started;
        static Vector3 initial;
        static bool configured;

        public static void RunSwimOnly()
        {
            EditorSceneManager.OpenScene("Assets/Scenes/Main.unity", OpenSceneMode.Single);
            configured = false;
            EditorApplication.playModeStateChanged += StateChanged;
            EditorApplication.update += Tick;
            EditorApplication.isPlaying = true;
        }

        public static void BuildLinuxPlayer()
        {
            var options = new BuildPlayerOptions
            {
                scenes = new[] { "Assets/Scenes/Main.unity" },
                locationPathName = "/tmp/LCE-Diagnostic/LCE-NANO.x86_64",
                target = BuildTarget.StandaloneLinux64,
                options = BuildOptions.Development
            };
            BuildReport report = BuildPipeline.BuildPlayer(options);
            Debug.Log("LCE_BUILD_RESULT " + report.summary.result);
            EditorApplication.Exit(report.summary.result == BuildResult.Succeeded ? 0 : 1);
        }

        static void StateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode) started = EditorApplication.timeSinceStartup;
        }

        static void Tick()
        {
            if (!EditorApplication.isPlaying) return;
            var swimmer = Object.FindFirstObjectByType<MicroSwimmer>();
            if (!swimmer) return;
            if (!configured)
            {
                swimmer.parameters.centerlineSpeedMmS = 0f;
                swimmer.parameters.stroke = StrokeMode.TravelingWave;
                swimmer.parameters.motionVisualizationGain = 100f;
                swimmer.ResetPosition();
                initial = swimmer.transform.position;
                configured = true;
                started = EditorApplication.timeSinceStartup;
            }
            if (EditorApplication.timeSinceStartup - started < 5.0) return;
            Debug.Log($"LCE_DIAGNOSTIC initial={initial} final={swimmer.transform.position} " +
                      $"delta={swimmer.transform.position - initial} speed_um_s={swimmer.RFT.speedMS * 1e6f} " +
                      $"gain={swimmer.parameters.motionVisualizationGain}");
            EditorApplication.update -= Tick;
            EditorApplication.playModeStateChanged -= StateChanged;
            EditorApplication.ExitPlaymode();
            EditorApplication.Exit(0);
        }
    }
}
#endif
