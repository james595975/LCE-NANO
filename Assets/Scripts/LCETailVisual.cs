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
            for (int i = 0; i < nodes.Count; i++)
            {
                float u = i / (float)(nodes.Count - 1);
                Vector3 si = MicroHydrodynamics.CenterlineSI(parameters, u, time);
                float scale = 2.8f / (MicroHydrodynamics.LengthSI(parameters) * .72f);
                Vector3 p = si * scale + new Vector3(-.35f, 0f, 0f);
                nodes[i].localPosition = p;
                float width = parameters.tail == TailGeometry.FishFin ? Mathf.Lerp(.12f, .38f, u) : .12f;
                nodes[i].localScale = new Vector3(.16f, width, parameters.tail == TailGeometry.Ribbon ? .28f : width);
            }
        }
    }
}
