using UnityEngine;

namespace LCENano
{
    public class SimulationUI : MonoBehaviour
    {
        public SimulationParameters p;
        public MicroSwimmer swimmer;
        GUIStyle title, label, box;
        bool visible = true;

        void InitStyles()
        {
            title = new GUIStyle(GUI.skin.label) { fontSize = 20, fontStyle = FontStyle.Bold, normal = { textColor = Color.white } };
            label = new GUIStyle(GUI.skin.label) { fontSize = 13, normal = { textColor = new Color(.86f, .93f, 1f) } };
            box = new GUIStyle(GUI.skin.box); box.normal.background = MakeTex(new Color(.025f, .05f, .09f, .92f));
        }
        Texture2D MakeTex(Color c) { var t = new Texture2D(1, 1); t.SetPixel(0, 0, c); t.Apply(); return t; }
        void Update() { if (Input.GetKeyDown(KeyCode.H)) visible = !visible; }

        void OnGUI()
        {
            if (!visible) { GUI.Label(new Rect(15, 15, 220, 30), "H: show controls"); return; }
            if (title == null) InitStyles();
            GUILayout.BeginArea(new Rect(16, 16, 325, Screen.height - 32), box);
            GUILayout.Label("LCE MICROSWIMMER LAB", title);
            GUILayout.Label("Overdamped Stokes-flow simulation", label);
            GUILayout.Space(8);
            GUILayout.Label("TAIL GEOMETRY", label);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Fish fin")) p.tail = TailGeometry.FishFin;
            if (GUILayout.Button("Helix")) p.tail = TailGeometry.Helix;
            if (GUILayout.Button("Ribbon")) p.tail = TailGeometry.Ribbon;
            GUILayout.EndHorizontal();
            GUILayout.Label("Selected: " + p.tail, label);
            GUILayout.Space(5);
            GUILayout.Label("ACTUATION / SCALLOP TEST", label);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Traveling")) p.stroke = StrokeMode.TravelingWave;
            if (GUILayout.Button("Reciprocal")) p.stroke = StrokeMode.Reciprocal;
            if (GUILayout.Button("Off")) p.stroke = StrokeMode.Disabled;
            GUILayout.EndHorizontal();
            GUILayout.Label(p.stroke == StrokeMode.TravelingWave ? "Non-reciprocal: net propulsion possible"
                : p.stroke == StrokeMode.Reciprocal ? "Reciprocal: cycle-average propulsion = 0"
                : "No deformation: passive advection only", label);
            GUILayout.Space(7);
            GUILayout.Label("HEAD / DRAG", label);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Sphere")) p.head = HeadGeometry.Sphere;
            if (GUILayout.Button("Prolate")) p.head = HeadGeometry.Prolate;
            if (GUILayout.Button("Disk")) p.head = HeadGeometry.Disk;
            GUILayout.EndHorizontal();
            GUILayout.Label("Selected: " + p.head, label);
            Slider("LCE frequency", ref p.frequencyHz, .2f, 12f, " Hz");
            Slider("Deformation amplitude", ref p.amplitude, .05f, 1.15f, "");
            Slider("Traveling-wave count", ref p.waveNumber, .5f, 3.5f, "");
            Slider("Blood center speed", ref p.centerlineSpeedMmS, .1f, 8f, " mm/s");
            Slider("Blood viscosity", ref p.bloodViscosityMPas, 1f, 8f, " mPa.s");
            Slider("Field direction", ref p.fieldYaw, -180f, 180f, " deg");
            GUILayout.Space(8);
            float re = MicroHydrodynamics.Reynolds(p);
            GUILayout.Label("LIVE MICROHYDRODYNAMICS", label);
            GUILayout.Label("Reynolds number     " + re.ToString("0.0000"), label);
            GUILayout.Label("Flow regime          " + (re < .1f ? "Creeping / Stokes" : "Low-Re laminar"), label);
            GUILayout.Label("Displayed velocity   " + swimmer.DisplayVelocity.magnitude.ToString("0.00") + " world/s", label);
            GUILayout.Label("RFT swim speed       " + (swimmer.RFT.speedMS * 1e6f).ToString("0.0") + " um/s", label);
            GUILayout.Label("Last-cycle mean      " + (swimmer.CycleMeanSpeedMS * 1e6f).ToString("0.0") + " um/s", label);
            GUILayout.Label("Instant tail thrust  " + (swimmer.RFT.thrustN * 1e12f).ToString("0.000") + " pN", label);
            GUILayout.Label("Physical length       " + p.swimmerLengthUm.ToString("0") + " um", label);
            GUILayout.FlexibleSpace();
            GUILayout.Label("RMB drag: orbit | Wheel: zoom | H: hide", label);
            GUILayout.EndArea();
        }

        void Slider(string name, ref float value, float min, float max, string unit)
        {
            GUILayout.Space(5); GUILayout.Label(name + "  " + value.ToString("0.00") + unit, label);
            value = GUILayout.HorizontalSlider(value, min, max);
        }
    }
}
