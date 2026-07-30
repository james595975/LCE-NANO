using UnityEngine;

namespace LCENano
{
    public class OrbitCamera : MonoBehaviour
    {
        public Transform target;
        public bool followSwimmer;
        float yaw = -25f, pitch = 20f, distance = 12f;
        Vector3 fixedFocus;
        public void SetFollow(bool follow)
        {
            followSwimmer = follow;
            fixedFocus = Vector3.zero;
        }
        void LateUpdate()
        {
            if (!target) return;
            if (Input.GetMouseButton(1)) { yaw += Input.GetAxis("Mouse X") * 4f; pitch -= Input.GetAxis("Mouse Y") * 3f; }
            distance = Mathf.Clamp(distance - Input.mouseScrollDelta.y, 5f, 24f);
            pitch = Mathf.Clamp(pitch, -10f, 75f);
            Quaternion q = Quaternion.Euler(pitch, yaw, 0);
            Vector3 focus = followSwimmer ? target.position : fixedFocus;
            transform.position = focus + q * new Vector3(0, 0, -distance);
            transform.LookAt(focus);
        }
    }
}
