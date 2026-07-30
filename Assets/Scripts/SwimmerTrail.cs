using UnityEngine;

namespace LCENano
{
    public class SwimmerTrail : MonoBehaviour
    {
        public void Setup(Material material)
        {
            var trail = gameObject.AddComponent<TrailRenderer>();
            trail.material = material;
            trail.time = 30f;
            trail.startWidth = .08f;
            trail.endWidth = .01f;
            trail.minVertexDistance = .015f;
            trail.startColor = new Color(.1f, 1f, .8f, .9f);
            trail.endColor = new Color(.1f, .5f, 1f, 0f);
        }
    }
}
