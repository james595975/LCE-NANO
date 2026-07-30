using UnityEngine;

namespace LCENano
{
    public enum TailGeometry { FishFin, Helix, Ribbon }
    public enum HeadGeometry { Sphere, Prolate, Disk }
    public enum StrokeMode { TravelingWave, Reciprocal, Disabled }

    [System.Serializable]
    public class SimulationParameters
    {
        [Header("Microscale SI values")]
        public float swimmerLengthUm = 120f;
        public float headRadiusUm = 18f;
        public float bloodViscosityMPas = 3.5f;
        public float bloodDensity = 1060f;
        public float centerlineSpeedMmS = 2.0f;
        [Header("LCE actuation")]
        public float frequencyHz = 3f;
        public float amplitude = 0.65f;
        public float waveNumber = 1.5f;
        public float fieldYaw = 0f;
        [Tooltip("Display-only position magnification; does not change reported SI speed or force.")]
        public float motionVisualizationGain = 1f;
        public TailGeometry tail = TailGeometry.FishFin;
        public HeadGeometry head = HeadGeometry.Sphere;
        public StrokeMode stroke = StrokeMode.TravelingWave;
    }

    public static class MicroHydrodynamics
    {
        public static float ViscositySI(SimulationParameters p) => p.bloodViscosityMPas * 1e-3f;
        public static float LengthSI(SimulationParameters p) => p.swimmerLengthUm * 1e-6f;
        public static float SpeedSI(SimulationParameters p) => p.centerlineSpeedMmS * 1e-3f;
        public static float Reynolds(SimulationParameters p) => p.bloodDensity * SpeedSI(p) * LengthSI(p) / Mathf.Max(ViscositySI(p), 1e-9f);

        public static float ReynoldsForSpeed(SimulationParameters p, float characteristicSpeedMS)
            => p.bloodDensity * Mathf.Abs(characteristicSpeedMS) * LengthSI(p) / Mathf.Max(ViscositySI(p), 1e-9f);

        public static float ViscosityForReMPas(SimulationParameters p, float characteristicSpeedMS, float targetRe)
            => p.bloodDensity * Mathf.Abs(characteristicSpeedMS) * LengthSI(p) / Mathf.Max(targetRe, 1e-8f) * 1e3f;

        // Shape-aware translational resistance relative to 6*pi*mu*a.
        public static Vector3 HeadDragMultiplier(HeadGeometry shape)
        {
            switch (shape)
            {
                case HeadGeometry.Prolate: return new Vector3(0.72f, 1.28f, 1.28f);
                case HeadGeometry.Disk: return new Vector3(1.65f, 1.05f, 1.65f);
                default: return Vector3.one;
            }
        }

        public struct RFTResult
        {
            public float speedMS;
            public float thrustN;
            public float tailResistance;
        }

        // Local resistive-force theory integrated over a discretized LCE centerline.
        // Force-free swimming gives (zetaHead + zetaTail) U + F_shape = 0.
        public static RFTResult SolveAxialRFT(SimulationParameters p, float time)
        {
            const int n = 64;
            float mu = ViscositySI(p);
            float totalLength = LengthSI(p);
            float tailLength = totalLength * .72f;
            float filamentRadius = Mathf.Max(totalLength * .018f, 0.5e-6f);
            float logTerm = Mathf.Max(Mathf.Log(2f * tailLength / filamentRadius), 1.2f);
            float xiParallel = 2f * Mathf.PI * mu / (logTerm - .5f);
            float xiPerp = 4f * Mathf.PI * mu / (logTerm + .5f);
            float dt = 1e-4f / Mathf.Max(p.frequencyHz, .1f);
            float ds = tailLength / (n - 1f);
            float shapeForce = 0f, tailDrag = 0f;

            for (int i = 0; i < n; i++)
            {
                float u = i / (n - 1f);
                Vector3 prev = CenterlineSI(p, Mathf.Max(0f, u - 1f / (n - 1f)), time);
                Vector3 next = CenterlineSI(p, Mathf.Min(1f, u + 1f / (n - 1f)), time);
                Vector3 segment = next - prev;
                // Vector3.normalized collapses vectors below Unity's world-scale epsilon.
                // Our SI segments are ~1e-6 m, so normalize explicitly at microscale.
                Vector3 tangent = segment / Mathf.Max(segment.magnitude, 1e-15f);
                Vector3 vShape = (CenterlineSI(p, u, time + dt) - CenterlineSI(p, u, time - dt)) / (2f * dt);
                float tx = tangent.x;
                float dragAlongX = xiPerp + (xiParallel - xiPerp) * tx * tx;
                float shapeFx = -(xiPerp * vShape.x + (xiParallel - xiPerp) * Vector3.Dot(vShape, tangent) * tx);
                shapeForce += shapeFx * ds;
                tailDrag += dragAlongX * ds;
            }

            float radius = p.headRadiusUm * 1e-6f;
            float headMultiplier = HeadDragMultiplier(p.head).x;
            float headDrag = 6f * Mathf.PI * mu * radius * headMultiplier;
            // F_total = F_shape - (zetaHead + zetaTail) U = 0.
            float speed = shapeForce / Mathf.Max(headDrag + tailDrag, 1e-15f);
            return new RFTResult { speedMS = speed, thrustN = -shapeForce, tailResistance = tailDrag };
        }

        public static Vector3 CenterlineSI(SimulationParameters p, float u, float time)
        {
            float l = LengthSI(p) * .72f;
            float x = -u * l;
            if (p.stroke == StrokeMode.Disabled) return new Vector3(x, 0f, 0f);
            float phase = 2f * Mathf.PI * p.frequencyHz * time;
            float a = p.amplitude * l * .16f * u;
            // With x=-uL, this phase convention sends the wave toward the tail and
            // produces force-free swimming in the swimmer's local +X direction.
            float traveling = -phase + u * p.waveNumber * Mathf.PI * 2f;
            if (p.stroke == StrokeMode.Reciprocal)
            {
                // One degree of freedom: the exact same shape sequence is retraced backwards.
                float q = Mathf.Sin(phase);
                if (p.tail == TailGeometry.Helix)
                {
                    float baseAngle = u * p.waveNumber * Mathf.PI * 2f;
                    return new Vector3(x, a * Mathf.Sin(baseAngle + q), a * Mathf.Cos(baseAngle + q));
                }
                return new Vector3(x, p.tail == TailGeometry.Ribbon ? a * q * .25f : 0f,
                    a * q * Mathf.Sin(Mathf.PI * u));
            }
            if (p.tail == TailGeometry.Helix)
                return new Vector3(x, a * Mathf.Sin(traveling), a * Mathf.Cos(traveling));
            if (p.tail == TailGeometry.Ribbon)
                return new Vector3(x, a * .30f * Mathf.Sin(traveling * .5f), a * Mathf.Sin(traveling));
            return new Vector3(x, 0f, a * Mathf.Sin(traveling));
        }
    }
}
