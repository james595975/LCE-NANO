using System.Collections.Generic;
using UnityEngine;

namespace LCENano
{
    public class LCETailVisual : MonoBehaviour
    {
        public SimulationParameters parameters;
        public Material material;
        public int segments = 30;
        readonly List<Transform> nodes = new List<Transform>();
        TailGeometry builtType = (TailGeometry)(-1);

        void Update()
        {
            if (builtType != parameters.tail) Rebuild();
            Deform(Time.time);
        }

        void Rebuild()
        {
            foreach (Transform n in nodes) if (n) Destroy(n.gameObject);
            nodes.Clear();
            builtType = parameters.tail;
            for (int i = 0; i < segments; i++)
            {
                var g = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                Destroy(g.GetComponent<Collider>());
                g.name = "LCE segment " + i;
                g.transform.SetParent(transform, false);
                g.GetComponent<Renderer>().sharedMaterial = material;
                nodes.Add(g.transform);
            }
        }

        void Deform(float time)
        {
            float phase = time * parameters.frequencyHz * Mathf.PI * 2f;
            float amp = parameters.amplitude;
            for (int i = 0; i < nodes.Count; i++)
            {
                float u = i / (float)(nodes.Count - 1);
                float x = -0.35f - u * 2.8f;
                Vector3 p;
                if (parameters.tail == TailGeometry.Helix)
                {
                    float a = phase - u * parameters.waveNumber * Mathf.PI * 2f;
                    float radius = amp * (0.10f + 0.40f * u);
                    p = new Vector3(x, Mathf.Sin(a) * radius, Mathf.Cos(a) * radius);
                }
                else if (parameters.tail == TailGeometry.Ribbon)
                {
                    float a = phase - u * parameters.waveNumber * Mathf.PI * 2f;
                    p = new Vector3(x, Mathf.Sin(a) * amp * u * .52f, Mathf.Sin(a * .5f) * amp * u * .12f);
                }
                else
                {
                    float a = phase - u * parameters.waveNumber * Mathf.PI * 2f;
                    p = new Vector3(x, 0f, Mathf.Sin(a) * amp * u * .62f);
                }
                nodes[i].localPosition = p;
                float width = parameters.tail == TailGeometry.FishFin ? Mathf.Lerp(.12f, .38f, u) : .12f;
                nodes[i].localScale = new Vector3(.16f, width, parameters.tail == TailGeometry.Ribbon ? .28f : width);
            }
        }
    }
}
