using UnityEngine;

namespace LCENano
{
    public class SimulationUI : MonoBehaviour
    {
        public SimulationParameters p;
        public MicroSwimmer swimmer;
        public OrbitCamera orbit;
        GUIStyle title, label, box;
        bool visible = true;
        bool reynoldsControl;
        float targetLogRe = -2f;
        Vector2 scroll;

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
            GUILayout.BeginArea(new Rect(16, 16, 385, Screen.height - 32), box);
            scroll = GUILayout.BeginScrollView(scroll, false, true);
            GUILayout.Label("LCE MICROSWIMMER LAB", title);
            GUILayout.Label("Overdamped Stokes-flow simulation", label);
            GUILayout.Space(8);
            GUILayout.Label("CONTROLLED EXPERIMENT", label);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Flow only")) ApplyPreset(0);
            if (GUILayout.Button("Swim only")) ApplyPreset(1);
            if (GUILayout.Button("Combined")) ApplyPreset(2);
            GUILayout.EndHorizontal();
            if (GUILayout.Button("MAX THRUST (ignore turning)")) ApplyMaximumThrustPreset();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Vessel camera")) orbit.SetFollow(false);
            if (GUILayout.Button("Follow swimmer")) orbit.SetFollow(true);
            GUILayout.EndHorizontal();
            GUILayout.Label("Camera: " + (orbit.followSwimmer ? "swimmer-relative" : "fixed laboratory frame"), label);
            GUILayout.Space(8);
            GUILayout.Label("TAIL: LCE DOME + 18 PASSIVE FRINGES", label);
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
            Slider("Blood center speed", ref p.centerlineSpeedMmS, 0f, 8f, " mm/s");
            GUILayout.Space(6);
            GUILayout.Label("REYNOLDS NUMBER CONTROL", label);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Direct Re")) reynoldsControl = true;
            if (GUILayout.Button("Manual viscosity")) reynoldsControl = false;
            GUILayout.EndHorizontal();
            float characteristicSpeed = p.centerlineSpeedMmS > .0001f
                ? p.centerlineSpeedMmS * 1e-3f : Mathf.Abs(swimmer.RFT.speedMS);
            if (reynoldsControl)
            {
                float targetRe = Mathf.Pow(10f, targetLogRe);
                GUILayout.Label("Target Re  " + targetRe.ToString("0.00000"), label);
                targetLogRe = GUILayout.HorizontalSlider(targetLogRe, -5f, 0f);
                p.bloodViscosityMPas = Mathf.Clamp(
                    MicroHydrodynamics.ViscosityForReMPas(p, characteristicSpeed, targetRe), .01f, 10000f);
                GUILayout.Label("Solved viscosity  " + p.bloodViscosityMPas.ToString("0.000") + " mPa.s", label);
                if (characteristicSpeed < 1e-10f)
                    GUILayout.Label("Set flow or actuation speed to define Re", label);
            }
            else Slider("Blood viscosity", ref p.bloodViscosityMPas, .5f, 300f, " mPa.s");
            Slider("Field direction", ref p.fieldYaw, -180f, 180f, " deg");
            Slider("Motion view gain", ref p.motionVisualizationGain, 1f, 200f, " x");
            if (p.motionVisualizationGain > 1.01f)
                GUILayout.Label("Display magnification only; SI physics unchanged", label);
            GUILayout.Space(8);
            float re = MicroHydrodynamics.ReynoldsForSpeed(p, characteristicSpeed);
            GUILayout.Label("LIVE MICROHYDRODYNAMICS", label);
            GUILayout.Label("Reynolds number     " + re.ToString("0.0000"), label);
            GUILayout.Label("Flow regime          " + (re < .1f ? "Creeping / Stokes" : "Low-Re laminar"), label);
            GUILayout.Label("Displayed velocity   " + swimmer.DisplayVelocity.magnitude.ToString("0.00") + " world/s", label);
            GUILayout.Label("RFT swim speed       " + (swimmer.RFT.speedMS * 1e6f).ToString("0.0") + " um/s", label);
            GUILayout.Label("Last-cycle mean      " + (swimmer.CycleMeanSpeedMS * 1e6f).ToString("0.0") + " um/s", label);
            GUILayout.Label("Centerline blood     " + (p.centerlineSpeedMmS * 1000f).ToString("0") + " um/s", label);
            float ratio = Mathf.Abs(swimmer.CycleMeanSpeedMS) > 1e-9f
                ? p.centerlineSpeedMmS * 1e-3f / Mathf.Abs(swimmer.CycleMeanSpeedMS) : 0f;
            GUILayout.Label("Flow / swim ratio    " + (ratio > 0f ? ratio.ToString("0.0") + " x" : "--"), label);
            GUILayout.Label("Instant tail thrust  " + (swimmer.RFT.thrustN * 1e12f).ToString("0.000") + " pN", label);
            GUILayout.Label("Observed displacement " + swimmer.ExperimentDisplacementWorld.ToString("0.000") + " world", label);
            GUILayout.Label("Integrated axial move " + swimmer.AccumulatedAxialWorld.ToString("0.000") + " world", label);
            GUILayout.Label("Physical length       " + p.swimmerLengthUm.ToString("0") + " um", label);
            GUILayout.Space(7);
            GUILayout.Label("DDS DELIVERY / LITERATURE DESIGN", label);
            GUILayout.Label("Phase                 " + swimmer.Phase, label);
            GUILayout.Label("Dose delivered        " + (swimmer.DeliveredFraction * 100f).ToString("0") + " %", label);
            GUILayout.Label("Body L x D             " + p.swimmerLengthUm.ToString("0") + " x " + p.swimmerDiameterUm.ToString("0") + " um (thrust preset)", label);
            GUILayout.Label("Vessel / opening      " + p.vesselDiameterUm.ToString("0") + " / " + p.openingDiameterUm.ToString("0.0") + " um", label);
            GUILayout.Label("Needle D x reach      " + p.needleDiameterUm.ToString("0.0") + " x " + p.needleExtensionUm.ToString("0.0") + " um", label);
            float layer = MicroHydrodynamics.HoldLayerUm(p, swimmer.RFT.speedMS);
            GUILayout.Label("Upstream hold layer   " + layer.ToString("0.000") + " um from wall", label);
            float required = MicroHydrodynamics.RequiredWallHoldSpeedMS(p);
            GUILayout.Label("Required wall speed   " + (required * 1e6f).ToString("0.0") + " um/s", label);
            GUILayout.Label("Station keeping       " + (swimmer.CanStationKeep ? "FEASIBLE in reduced model" : "INFEASIBLE - injection interlocked"), label);
            GUILayout.FlexibleSpace();
            GUILayout.Label("RMB drag: orbit | Wheel: zoom | H: hide", label);
            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        void Slider(string name, ref float value, float min, float max, string unit)
        {
            GUILayout.Space(5); GUILayout.Label(name + "  " + value.ToString("0.00") + unit, label);
            value = GUILayout.HorizontalSlider(value, min, max);
        }

        void ApplyPreset(int preset)
        {
            if (preset == 0) { p.centerlineSpeedMmS = 2f; p.stroke = StrokeMode.Disabled; p.motionVisualizationGain = 1f; }
            else if (preset == 1) { p.centerlineSpeedMmS = 0f; p.stroke = StrokeMode.TravelingWave; p.motionVisualizationGain = 100f; }
            else { p.centerlineSpeedMmS = 2f; p.stroke = StrokeMode.TravelingWave; p.motionVisualizationGain = 1f; }
            swimmer.ResetPosition();
            orbit.SetFollow(false);
        }

        void ApplyMaximumThrustPreset()
        {
            // Exhaustive cycle-mean scan of the UI's bounded actuation domain places
            // the optimum at maximum frequency/amplitude and about 1.6 waves.
            p.frequencyHz = 12f;
            p.amplitude = 1.15f;
            p.waveNumber = 1.6f;
            p.stroke = StrokeMode.TravelingWave;
            p.motionVisualizationGain = 1f;
            swimmer.ResetPosition();
            orbit.SetFollow(false);
        }
    }
}
