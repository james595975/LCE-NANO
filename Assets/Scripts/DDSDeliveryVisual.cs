using UnityEngine;

namespace LCENano
{
    public class DDSDeliveryVisual : MonoBehaviour
    {
        public SimulationParameters parameters;
        public Transform needle;
        public Transform plunger;
        public Transform target;
        public float deployment;
        public float deliveredFraction;

        void Update()
        {
            deployment = Mathf.MoveTowards(deployment,
                deliveredFraction >= 0f && (int)GetComponentInParent<MicroSwimmer>().Phase >= (int)DeliveryPhase.Inject ? 1f : 0f,
                Time.deltaTime * 1.5f);
            if (needle)
            {
                needle.localScale = new Vector3(.055f, .055f, Mathf.Lerp(.06f, 1.15f, deployment));
                needle.localPosition = new Vector3(.20f, Mathf.Lerp(.37f, .92f, deployment), 0f);
            }
            if (plunger) plunger.localScale = new Vector3(Mathf.Max(.04f, 1f - deliveredFraction), 1f, 1f);
        }
    }
}
