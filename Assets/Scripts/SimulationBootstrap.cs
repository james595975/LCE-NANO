using UnityEngine;

namespace LCENano
{
    public class SimulationBootstrap : MonoBehaviour
    {
        static Material Mat(Color color, bool transparent = false)
        {
            Shader s = Shader.Find("LCENano/Unlit");
            if (!s) s = Shader.Find("Sprites/Default");
            var m = new Material(s); m.color = color;
            if (transparent) m.renderQueue = 3000;
            return m;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Launch()
        {
            if (FindFirstObjectByType<SimulationBootstrap>()) return;
            new GameObject("LCE Simulation").AddComponent<SimulationBootstrap>().Build();
        }

        void Build()
        {
            var p = new SimulationParameters();
            QualitySettings.antiAliasing = 4;
            RenderSettings.ambientLight = new Color(.20f, .24f, .30f);
            Camera cam = new GameObject("Main Camera").AddComponent<Camera>();
            cam.tag = "MainCamera"; cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = new Color(.008f, .015f, .03f); cam.fieldOfView = 50;
            var light = new GameObject("Key Light").AddComponent<Light>(); light.type = LightType.Directional; light.intensity = 1.35f; light.transform.rotation = Quaternion.Euler(35, -35, 0);

            var vessel = new GameObject("Blood vessel + Poiseuille field");
            var flow = vessel.AddComponent<BloodFlowField>(); flow.parameters = p;
            BuildVessel(flow);
            BuildTracers(flow);

            var root = new GameObject("LCE Microswimmer"); root.transform.position = new Vector3(-3f, .65f, 0);
            var head = GameObject.CreatePrimitive(PrimitiveType.Sphere); head.name = "Shape-dependent head"; head.transform.SetParent(root.transform, false); head.GetComponent<Renderer>().material = Mat(new Color(.16f, .70f, .94f)); Destroy(head.GetComponent<Collider>());
            var tailObj = new GameObject("LCE deforming tail"); tailObj.transform.SetParent(root.transform, false);
            var tail = tailObj.AddComponent<LCETailVisual>(); tail.parameters = p; tail.material = Mat(new Color(1f, .30f, .45f));
            var swimmer = root.AddComponent<MicroSwimmer>(); swimmer.parameters = p; swimmer.flow = flow; swimmer.head = head.transform; swimmer.tail = tail;
            root.AddComponent<SwimmerTrail>().Setup(Mat(new Color(.1f, 1f, .8f, .9f), true));
            if (System.Array.Exists(System.Environment.GetCommandLineArgs(), a => a == "-lceDiagnostic"))
                gameObject.AddComponent<PlayerDiagnostics>().swimmer = swimmer;

            var orbit = cam.gameObject.AddComponent<OrbitCamera>(); orbit.target = root.transform; orbit.SetFollow(false);
            var ui = gameObject.AddComponent<SimulationUI>(); ui.p = p; ui.swimmer = swimmer; ui.orbit = orbit;
        }

        void BuildVessel(BloodFlowField flow)
        {
            var wallMat = Mat(new Color(.35f, .04f, .08f, .16f), true);
            for (int i = 0; i < 32; i++)
            {
                float a = i / 32f * Mathf.PI * 2f;
                var strip = GameObject.CreatePrimitive(PrimitiveType.Cube); strip.name = "Vessel wall";
                strip.transform.SetParent(flow.transform, false);
                strip.transform.localPosition = new Vector3(0, Mathf.Sin(a) * flow.vesselRadiusWorld, Mathf.Cos(a) * flow.vesselRadiusWorld);
                strip.transform.localRotation = Quaternion.Euler(a * Mathf.Rad2Deg, 0, 0);
                strip.transform.localScale = new Vector3(flow.vesselHalfLengthWorld * 2f, .025f, .58f);
                strip.GetComponent<Renderer>().sharedMaterial = wallMat; Destroy(strip.GetComponent<Collider>());
            }
        }

        void BuildTracers(BloodFlowField flow)
        {
            var red = Mat(new Color(1f, .12f, .18f, .72f), true);
            var cyan = Mat(new Color(.10f, .75f, 1f, .75f), true);
            Random.InitState(731);
            for (int i = 0; i < 95; i++)
            {
                float a = Random.value * Mathf.PI * 2f;
                float r = Mathf.Sqrt(Random.value) * (flow.vesselRadiusWorld - .18f);
                var g = GameObject.CreatePrimitive(i % 7 == 0 ? PrimitiveType.Capsule : PrimitiveType.Sphere);
                g.name = i % 7 == 0 ? "Flow direction tracer" : "Blood cell tracer";
                g.transform.position = new Vector3(Random.Range(-flow.vesselHalfLengthWorld, flow.vesselHalfLengthWorld), Mathf.Sin(a) * r, Mathf.Cos(a) * r);
                g.transform.localScale = i % 7 == 0 ? new Vector3(.08f, .25f, .08f) : new Vector3(.10f, .06f, .10f);
                g.GetComponent<Renderer>().sharedMaterial = i % 7 == 0 ? cyan : red; Destroy(g.GetComponent<Collider>());
                g.AddComponent<FlowTracer>().flow = flow;
            }
        }
    }
}
