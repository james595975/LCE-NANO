using UnityEngine;

namespace LCENano
{
    public class MicroSwimmer : MonoBehaviour
    {
        public SimulationParameters parameters;
        public BloodFlowField flow;
        public Transform head;
        public LCETailVisual tail;
        Vector3 velocity;
        HeadGeometry builtHead = (HeadGeometry)(-1);

        void Update()
        {
            if (builtHead != parameters.head) ApplyHeadShape();
            StepStokes(Time.deltaTime);
        }

        void ApplyHeadShape()
        {
            builtHead = parameters.head;
            head.localScale = builtHead == HeadGeometry.Sphere ? new Vector3(.72f, .72f, .72f)
                : builtHead == HeadGeometry.Prolate ? new Vector3(1.08f, .56f, .56f)
                : new Vector3(.38f, 1.08f, 1.08f);
        }

        void StepStokes(float dt)
        {
            // At Re << 1, inertia relaxes far faster than a frame: solve F_drag + F_propulsion = 0.
            Vector3 fluid = flow.VelocityAt(transform.position);
            float omega = 2f * Mathf.PI * parameters.frequencyHz;
            float tailSpeed = MicroHydrodynamics.PropulsionCoefficient(parameters.tail)
                * parameters.amplitude * parameters.amplitude * omega * .11f;
            Vector3 desiredAxis = Quaternion.Euler(0f, parameters.fieldYaw, 0f) * Vector3.right;
            Vector3 drag = MicroHydrodynamics.HeadDragMultiplier(parameters.head);
            float axialResistance = Vector3.Dot(drag, new Vector3(Mathf.Abs(desiredAxis.x), Mathf.Abs(desiredAxis.y), Mathf.Abs(desiredAxis.z)));
            velocity = fluid + desiredAxis * tailSpeed / Mathf.Max(axialResistance, .2f);

            transform.position += velocity * dt;
            Quaternion targetRotation = Quaternion.FromToRotation(Vector3.right, desiredAxis);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 6f * dt);

            Vector3 local = flow.transform.InverseTransformPoint(transform.position);
            float radial = new Vector2(local.y, local.z).magnitude;
            float wall = flow.vesselRadiusWorld - .42f;
            if (radial > wall)
            {
                float k = wall / radial;
                local.y *= k; local.z *= k;
                transform.position = flow.transform.TransformPoint(local);
            }
            if (local.x > flow.vesselHalfLengthWorld) local.x = -flow.vesselHalfLengthWorld;
            if (local.x < -flow.vesselHalfLengthWorld) local.x = flow.vesselHalfLengthWorld;
            transform.position = flow.transform.TransformPoint(local);
        }

        public Vector3 DisplayVelocity => velocity;
    }
}
