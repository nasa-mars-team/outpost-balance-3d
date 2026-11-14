using UnityEngine;

namespace OutpostBalance.World3D
{
    /// <summary>
    /// Moves the 3D science rover on a smooth patrol loop around the outpost when Science Lab is powered.
    /// </summary>
    public class RoverPatrol : MonoBehaviour
    {
        public Vector3 center = new Vector3(4f, 0.35f, -3f);
        public float radiusX = 2.6f;
        public float radiusZ = 1.8f;
        public float speed = 0.65f;

        private float _angle;
        private bool _active = true;

        public void SetPowered(int labLevel)
        {
            _active = labLevel > 0;
            speed = 0.35f + labLevel * 0.25f;
        }

        private void Update()
        {
            if (!_active) return;

            _angle += Time.deltaTime * speed;
            Vector3 nextPos = new Vector3(
                center.x + Mathf.Cos(_angle) * radiusX,
                center.y,
                center.z + Mathf.Sin(_angle) * radiusZ
            );

            Vector3 dir = nextPos - transform.position;
            if (dir.sqrMagnitude > 0.0001f)
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * 6f);

            transform.position = nextPos;
        }
    }
}
