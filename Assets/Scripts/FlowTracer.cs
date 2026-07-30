using UnityEngine;

namespace LCENano
{
    public class FlowTracer : MonoBehaviour
    {
        public BloodFlowField flow;
        public float offset;
        void Update()
        {
            transform.position += flow.VelocityAt(transform.position) * Time.deltaTime;
            Vector3 p = flow.transform.InverseTransformPoint(transform.position);
            if (p.x > flow.vesselHalfLengthWorld) p.x = -flow.vesselHalfLengthWorld;
            transform.position = flow.transform.TransformPoint(p);
            Vector3 v = flow.VelocityAt(transform.position);
            if (v.sqrMagnitude > .001f) transform.rotation = Quaternion.LookRotation(v) * Quaternion.Euler(0, 90, 0);
        }
    }
}
