using Core.EventBus;
using Player;
using UnityEngine;

namespace Turret
{
    public class ReflectiveCube : MonoBehaviour
    {
        [Header("Reflected Laser Settings")]
        [SerializeField] private float laserRange = 50f;
        [SerializeField] private float laserWidth = 0.1f;
        [SerializeField] private Color laserColor = Color.cyan;
        [SerializeField] private float laserIntensity = 2f;
        [SerializeField] private int laserSegments = 20;
        [SerializeField] private float surfaceEpsilon = 0.01f;
        [SerializeField] private LayerMask reflectiveLayerMask;

        private LaserBeam reflectedBeam;
        private bool _sawHitThisFrame;

        private void Awake()
        {
            var beamObject = gameObject.GetChildRecursive("LaserBeam");
            var meshFilter = beamObject.GetOrAddComponent<MeshFilter>();
            var meshRenderer = beamObject.GetOrAddComponent<MeshRenderer>();

            reflectedBeam = new LaserBeam(
                meshFilter: meshFilter,
                meshRenderer: meshRenderer,
                origin: transform,
                range: laserRange,
                width: laserWidth,
                segments: laserSegments,
                color: laserColor,
                intensity: laserIntensity
            );

            reflectedBeam.AddHit("Player").OnHit(_ => EventBusVoid<PlayerEventsEnum>.Invoke(PlayerEventsEnum.Death));

            // Reflective cubes
            reflectedBeam.AddHit(reflectiveLayerMask).OnHit(hit =>
            {
                if (hit.collider.TryGetComponent<ReflectiveCube>(out var reflectiveCube))
                {
                    Debug.Log($"Reflective cube hit: {hit.collider.name}");
                    reflectiveCube.OnLaserHit(hit.point, reflectedBeam.CurrentDirection, hit.normal);
                }
            });

            reflectedBeam.SetActive(false);
        }

        private void Update()
        {
            reflectedBeam?.Tick();

            if (reflectedBeam != null && reflectedBeam.IsActive && !_sawHitThisFrame)
            {
                reflectedBeam.SetActive(false);
            }

            _sawHitThisFrame = false;
        }

        public void OnLaserHit(object payload)
        {
            if (payload is Vector3[] arr && arr.Length >= 3)
            {
                OnLaserHit(arr[0], arr[1], arr[2]);
            }
        }

        public void OnLaserHit(Vector3 hitPoint, Vector3 incomingDirection, Vector3 surfaceNormal)
        {
            var localNormal = transform.InverseTransformDirection(surfaceNormal).normalized;
            var abs = new Vector3(Mathf.Abs(localNormal.x), Mathf.Abs(localNormal.y), Mathf.Abs(localNormal.z));

            Vector3 localRightDir;
            if (abs.z >= abs.x && abs.z >= abs.y)
            {
                localRightDir = Mathf.Sign(localNormal.z) > 0 ? Vector3.right : Vector3.left;
            }
            else if (abs.x >= abs.y && abs.x >= abs.z)
            {
                localRightDir = Mathf.Sign(localNormal.x) > 0 ? Vector3.back : Vector3.forward;
            }
            else
            {
                localRightDir = Vector3.right;
            }

            var reflectDir = transform.TransformDirection(localRightDir).normalized;
            var transformPos = transform.position;
            var startPos = new Vector3(transformPos.x, hitPoint.y, transformPos.z) + surfaceNormal.normalized * surfaceEpsilon;

            reflectedBeam.SetStart(startPos, reflectDir);
            reflectedBeam.SetActive(true);
            _sawHitThisFrame = true;
        }
    }
}

