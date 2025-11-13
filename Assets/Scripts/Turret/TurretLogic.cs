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

        [Header("Visual")]
        [SerializeField] private int laserSegments = 20;

        private LaserBeam laserBeam;

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
        }

        private void Update()
        {
            laserBeam?.Tick();
        }

        private void OnDrawGizmosSelected()
        {
            Transform laserOrigin = transform.Find("LaserOrigin") ?? transform;
            Gizmos.color = laserColor;
            Gizmos.DrawRay(laserOrigin.position, laserOrigin.forward * laserRange);
        }
    }
}