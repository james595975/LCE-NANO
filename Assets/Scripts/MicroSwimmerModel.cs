using UnityEngine;

namespace LCENano
{
    public enum TailGeometry { FishFin, Helix, Ribbon }
    public enum HeadGeometry { Sphere, Prolate, Disk }

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
        public TailGeometry tail = TailGeometry.FishFin;
        public HeadGeometry head = HeadGeometry.Sphere;
    }

    public static class MicroHydrodynamics
    {
        public static float ViscositySI(SimulationParameters p) => p.bloodViscosityMPas * 1e-3f;
        public static float LengthSI(SimulationParameters p) => p.swimmerLengthUm * 1e-6f;
        public static float SpeedSI(SimulationParameters p) => p.centerlineSpeedMmS * 1e-3f;
        public static float Reynolds(SimulationParameters p) => p.bloodDensity * SpeedSI(p) * LengthSI(p) / Mathf.Max(ViscositySI(p), 1e-9f);

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

        public static float PropulsionCoefficient(TailGeometry tail)
        {
            // Slender-body / resistive-force inspired dimensionless coefficients.
            return tail == TailGeometry.Helix ? 0.34f : tail == TailGeometry.FishFin ? 0.22f : 0.16f;
        }
    }
}
