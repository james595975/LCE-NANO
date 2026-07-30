using UnityEngine;

namespace LCENano
{
    public class BloodFlowField : MonoBehaviour
    {
        public SimulationParameters parameters;
        public float vesselRadiusWorld = 3.2f;
        public float vesselHalfLengthWorld = 9f;

        // Laminar Poiseuille profile. World +X is vessel axial direction.
        public Vector3 VelocityAt(Vector3 worldPosition)
        {
            Vector3 local = transform.InverseTransformPoint(worldPosition);
            float r2 = local.y * local.y + local.z * local.z;
            float profile = Mathf.Max(0f, 1f - r2 / (vesselRadiusWorld * vesselRadiusWorld));
            float displayScale = parameters.centerlineSpeedMmS * 0.42f;
            return transform.TransformDirection(Vector3.right) * displayScale * profile;
        }

        public float ShearRateAt(Vector3 worldPosition)
        {
            Vector3 p = transform.InverseTransformPoint(worldPosition);
            float r = new Vector2(p.y, p.z).magnitude;
            return 2f * MicroHydrodynamics.SpeedSI(parameters) * r / Mathf.Max(vesselRadiusWorld * vesselRadiusWorld, .001f);
        }
    }
}
