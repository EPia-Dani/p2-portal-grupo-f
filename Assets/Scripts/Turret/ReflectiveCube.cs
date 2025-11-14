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
        private LayerMask reflectiveLayerMask;
        private LayerMask laserButtonLayerMask;

        private LaserBeam reflectedBeam;
        private bool _sawHitThisFrame;

        private void Awake()
        {
            reflectiveLayerMask = LayerMask.GetMask("LaserReflective");
            laserButtonLayerMask = LayerMask.GetMask("LaserButton");

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

            // Laser buttons
            reflectedBeam.AddHit(laserButtonLayerMask).OnHit(hit =>
            {
                // TODO: Implement LaserButton class or whatever name you want to give it
                // if (hit.collider.TryGetComponent<LaserButton>(out var laserButton))
                // {
                //     TODO: Implement OnLaserHit method to activate the door or whatever
                //     laserButton.OnLaserHit();
                // }
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
            var reflectDir = transform.forward;

            reflectedBeam.SetStart(transform.position, reflectDir);
            reflectedBeam.SetActive(true);
            _sawHitThisFrame = true;
        }
    }
}
