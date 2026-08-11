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
        MicroHydrodynamics.RFTResult rft;
        float cycleIntegral, cycleElapsed, cycleMean;
        int cycleIndex = -1;
        Vector3 experimentStart;
        float accumulatedPropulsionWorld;
        DDSDeliveryVisual delivery;
        float phaseClock;
        public DeliveryPhase Phase { get; private set; }
        public float DeliveredFraction { get; private set; }
        public bool CanStationKeep => Mathf.Abs(rft.speedMS) >= MicroHydrodynamics.RequiredWallHoldSpeedMS(parameters);

        void Start() { experimentStart = transform.position; delivery = GetComponentInChildren<DDSDeliveryVisual>(); }
        HeadGeometry builtHead = (HeadGeometry)(-1);

        void Update()
        {
            if (builtHead != parameters.head) ApplyHeadShape();
            StepStokes(Time.deltaTime);
            UpdateDelivery(Time.deltaTime);
        }

        void UpdateDelivery(float dt)
        {
            if (!parameters.autonomousDelivery) return;
            phaseClock += dt;
            // The timed sequence is deliberately deterministic: it exposes each physical requirement
            // without claiming a clinically validated sensing/control implementation.
            float boundary = Phase == DeliveryPhase.Navigate ? 5f : Phase == DeliveryPhase.TurnUpstream ? 2f
                : Phase == DeliveryPhase.StationKeep ? 2f : parameters.injectionSeconds;
            if ((int)Phase < (int)DeliveryPhase.StationKeep && phaseClock >= boundary) { Phase++; phaseClock = 0f; }
            else if (Phase == DeliveryPhase.StationKeep && phaseClock >= boundary && CanStationKeep)
            { Phase = DeliveryPhase.Inject; phaseClock = 0f; }
            else if (Phase == DeliveryPhase.Inject)
            {
                DeliveredFraction = Mathf.Clamp01(phaseClock / Mathf.Max(parameters.injectionSeconds, .1f));
                if (DeliveredFraction >= 1f) Phase = DeliveryPhase.Complete;
            }
            if (delivery) delivery.deliveredFraction = DeliveredFraction;
        }

        void ApplyHeadShape()
        {
            builtHead = parameters.head;
            head.localScale = builtHead == HeadGeometry.Sphere ? new Vector3(1.28f, 1.28f, 1.28f)
                : builtHead == HeadGeometry.Prolate ? new Vector3(1.55f, 1.12f, 1.12f)
                : new Vector3(.72f, 1.42f, 1.42f);
        }

        void StepStokes(float dt)
        {
            // At Re << 1, inertia relaxes far faster than a frame: solve F_drag + F_propulsion = 0.
            Vector3 fluid = flow.VelocityAt(transform.position);
            rft = MicroHydrodynamics.SolveAxialRFT(parameters, Time.time);
            int nowCycle = Mathf.FloorToInt(Time.time * Mathf.Max(parameters.frequencyHz, .01f));
            if (cycleIndex >= 0 && nowCycle != cycleIndex)
            {
                cycleMean = cycleIntegral / Mathf.Max(cycleElapsed, 1e-6f);
                cycleIntegral = 0f; cycleElapsed = 0f;
            }
            cycleIndex = nowCycle;
            cycleIntegral += rft.speedMS * dt; cycleElapsed += dt;
            float yaw = parameters.fieldYaw;
            if (parameters.autonomousDelivery && (int)Phase >= (int)DeliveryPhase.TurnUpstream) yaw = 180f;
            Vector3 desiredAxis = Quaternion.Euler(0f, yaw, 0f) * Vector3.right;
            const float physicalToWorld = 420f;
            velocity = fluid + desiredAxis * rft.speedMS * physicalToWorld;

            Vector3 frameDelta = velocity * dt * parameters.motionVisualizationGain;
            transform.position += frameDelta;
            accumulatedPropulsionWorld += Vector3.Dot(frameDelta, desiredAxis);
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
        public MicroHydrodynamics.RFTResult RFT => rft;
        public float CycleMeanSpeedMS => cycleMean;
        public float ExperimentDisplacementWorld => Vector3.Distance(transform.position, experimentStart);
        public float AccumulatedAxialWorld => accumulatedPropulsionWorld;
        public void ResetPosition()
        {
            transform.position = new Vector3(-3f, .65f, 0f);
            experimentStart = transform.position;
            accumulatedPropulsionWorld = 0f;
            cycleIntegral = cycleElapsed = cycleMean = 0f;
            cycleIndex = -1;
            Phase = DeliveryPhase.Navigate; phaseClock = 0f; DeliveredFraction = 0f;
        }
    }
}
