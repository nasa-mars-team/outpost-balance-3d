using UnityEngine;
using UnityEngine.EventSystems;

namespace OutpostBalance.World3D
{
    /// <summary>
    /// Smooth 3D orbital camera around the outpost diorama (drag right/middle or left mouse on empty sky, scroll to zoom).
    /// Compatible with both Unity Legacy Input and Input System packages.
    /// </summary>
    public class CameraOrbit : MonoBehaviour
    {
        public Transform target;
        public float distance = 18f;
        public float minDistance = 9f;
        public float maxDistance = 30f;
        public float yaw = 35f;
        public float pitch = 28f;
        public float autoRotateSpeed = 4f;

        private Vector3 _lastMousePos;
        private bool _dragging;

        private void LateUpdate()
        {
            HandleInput();

            Vector3 pivot = target != null ? target.position : Vector3.zero;
            Quaternion rot = Quaternion.Euler(pitch, yaw, 0f);
            Vector3 desiredPos = pivot - (rot * Vector3.forward * distance);

            transform.position = Vector3.Lerp(transform.position, desiredPos, Time.deltaTime * 10f);
            transform.LookAt(pivot + Vector3.up * 1.2f);
        }

        private void HandleInput()
        {
#if ENABLE_LEGACY_INPUT_MANAGER
            bool overUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
            if (!overUI)
            {
                if (Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2) || Input.GetMouseButtonDown(0))
                {
                    _dragging = true;
                    _lastMousePos = Input.mousePosition;
                }
                float scroll = Input.mouseScrollDelta.y;
                if (Mathf.Abs(scroll) > 0.01f)
                {
                    distance = Mathf.Clamp(distance - scroll * 1.4f, minDistance, maxDistance);
                }
            }

            if (Input.GetMouseButtonUp(1) || Input.GetMouseButtonUp(2) || Input.GetMouseButtonUp(0))
                _dragging = false;

            if (_dragging)
            {
                Vector3 delta = Input.mousePosition - _lastMousePos;
                _lastMousePos = Input.mousePosition;
                yaw += delta.x * 0.25f;
                pitch = Mathf.Clamp(pitch - delta.y * 0.2f, 12f, 65f);
            }
            else
            {
                yaw += autoRotateSpeed * Time.deltaTime;
            }
#else
            yaw += autoRotateSpeed * Time.deltaTime;
#endif
        }
    }
}
