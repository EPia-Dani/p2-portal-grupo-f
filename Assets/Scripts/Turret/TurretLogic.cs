using Core.EventBus;
using Player;
using UnityEngine;
using System;

namespace Turret
{
    public class TurretLogic : MonoBehaviour
    {
        [Header("Laser Settings")]
        [SerializeField] private float laserRange = 50f;
        [SerializeField] private float laserWidth = 0.1f;
        [SerializeField] private Color laserColor = Color.red;
        [SerializeField] private float laserIntensity = 2f;

        [Header("Hit Detection")]
        [SerializeField] private LayerMask reflectiveLayerMask; // e.g., "LaserReflective"

        [Header("Visual")]
        [SerializeField] private int laserSegments = 20;

        private LaserBeam laserBeam;
        private bool m_alive = true;

        private void Start()
        {
            var laserObject = gameObject.GetChildRecursive("LaserBeam");
            var laserOriginObject = gameObject.GetChildRecursive("LaserOrigin");
            var laserOrigin = laserOriginObject.transform;
            var laserMeshFilter = laserObject.GetOrAddComponent<MeshFilter>();
            var laserMeshRenderer = laserObject.GetOrAddComponent<MeshRenderer>();

            laserBeam = new LaserBeam(
                meshFilter: laserMeshFilter,
                meshRenderer: laserMeshRenderer,
                origin: laserOrigin,
                range: laserRange,
                width: laserWidth,
                segments: laserSegments,
                color: laserColor,
                intensity: laserIntensity
            );

            laserBeam.AddHit("Player").OnHit(_ => EventBusVoid<PlayerEventsEnum>.Invoke(PlayerEventsEnum.Death));

            // Reflective cubes
            laserBeam.AddHit(reflectiveLayerMask).OnHit(hit =>
            {
                if (hit.collider.TryGetComponent<ReflectiveCube>(out var reflectiveCube))
                {
                    Debug.Log($"Reflective cube hit: {hit.collider.name}");
                    reflectiveCube.OnLaserHit(hit.point, laserBeam.CurrentDirection, hit.normal);
                }
            });
        }

        private void Update()
        {
            if (m_alive)
            {
                m_alive = Vector3.Dot(transform.up, Vector3.up) > 0.85f;
                laserBeam?.Tick();
            }
            else
            {
                Debug.Log("Turret destroyed, disabling laser.");
                laserBeam.SetActive(false);
            }
            
        }

        private void OnDrawGizmosSelected()
        {
            Transform laserOrigin = transform.Find("LaserOrigin") ?? transform;
            Gizmos.color = laserColor;
            Gizmos.DrawRay(laserOrigin.position, laserOrigin.forward * laserRange);
        }
    }
}