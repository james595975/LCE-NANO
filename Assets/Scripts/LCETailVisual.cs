using System.Collections.Generic;
using UnityEngine;

namespace LCENano
{
    // Visual model of an LCE dome carrying passive silicone-like flexible fringes.
    // Adjacent fringe rings use a phase lag so the shape cycle is non-reciprocal.
    public class LCETailVisual : MonoBehaviour
    {
        public SimulationParameters parameters;
        public Material material;
        readonly List<Transform> fringes = new List<Transform>();
        Transform dome;

        void Start() { Build(); }

        void Build()
        {
            dome = GameObject.CreatePrimitive(PrimitiveType.Sphere).transform;
            dome.name = "Reciprocating LCE dome"; dome.SetParent(transform, false);
            dome.localPosition = new Vector3(-3.05f, 0f, 0f); dome.localScale = new Vector3(1.05f, 1.28f, 1.28f);
            dome.GetComponent<Renderer>().sharedMaterial = material; Destroy(dome.GetComponent<Collider>());
            for (int i = 0; i < 18; i++)
            {
                float angle = i * Mathf.PI * 2f / 18f;
                var f = GameObject.CreatePrimitive(PrimitiveType.Capsule).transform;
                f.name = "Passive silicone fringe " + i; f.SetParent(transform, false);
                f.localPosition = new Vector3(-3.48f, Mathf.Sin(angle) * .61f, Mathf.Cos(angle) * .61f);
                f.localScale = new Vector3(.055f, .30f, .055f);
                f.GetComponent<Renderer>().sharedMaterial = material; Destroy(f.GetComponent<Collider>());
                fringes.Add(f);
            }
        }

        void Update()
        {
            if (!dome) return;
            float phase = Time.time * parameters.frequencyHz * Mathf.PI * 2f;
            float stroke = parameters.stroke == StrokeMode.Disabled ? 0f : Mathf.Sin(phase);
            dome.localScale = new Vector3(1.05f + .16f * stroke * parameters.amplitude, 1.28f, 1.28f);
            for (int i = 0; i < fringes.Count; i++)
            {
                float a = i * 360f / fringes.Count;
                // Elastic lag (0.65 rad) makes extension and recovery follow different configurations.
                float bend = parameters.stroke == StrokeMode.Disabled ? 0f
                    : 24f * parameters.amplitude * Mathf.Sin(phase - .65f);
                fringes[i].localRotation = Quaternion.Euler(0f, a, 90f + bend);
            }
        }
    }
}
