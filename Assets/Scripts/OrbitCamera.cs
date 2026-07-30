using UnityEngine;

namespace LCENano
{
    public class OrbitCamera : MonoBehaviour
    {
        public Transform target;
        float yaw = -25f, pitch = 20f, distance = 12f;
        void LateUpdate()
        {
            if (!target) return;
            if (Input.GetMouseButton(1)) { yaw += Input.GetAxis("Mouse X") * 4f; pitch -= Input.GetAxis("Mouse Y") * 3f; }
            distance = Mathf.Clamp(distance - Input.mouseScrollDelta.y, 5f, 24f);
            pitch = Mathf.Clamp(pitch, -10f, 75f);
            Quaternion q = Quaternion.Euler(pitch, yaw, 0);
            transform.position = target.position + q * new Vector3(0, 0, -distance);
            transform.LookAt(target);
        }
    }
}
