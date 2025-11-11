using Core.EventBus;
using Player;
using UnityEngine;

namespace Turret
{
    public class TurretLogic : MonoBehaviour
    {
        [Header("Laser Settings")]
        [SerializeField] private float laserRange = 50f;
        [SerializeField] private float laserWidth = 0.1f;
        [SerializeField] private Color laserColor = Color.red;
        [SerializeField] private float laserIntensity = 2f;
        [SerializeField] private LayerMask hitLayers = -1;

        [Header("Visual")]
        [SerializeField] private int laserSegments = 20;
        private MeshFilter laserMeshFilter;
        private MeshRenderer laserMeshRenderer;
        private Transform laserOrigin;

        private void Start()
        {
            var laserObject = gameObject.GetChildRecursive("LaserBeam");
            var laserOriginObject = gameObject.GetChildRecursive("LaserOrigin");
            laserOrigin = laserOriginObject.transform;
            laserMeshFilter = laserObject.GetOrAddComponent<MeshFilter>();
            laserMeshRenderer = laserObject.GetOrAddComponent<MeshRenderer>();

            laserMeshRenderer.material.SetColor("_Color", laserColor);
            laserMeshRenderer.material.SetFloat("_Intensity", laserIntensity);

            laserMeshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            laserMeshRenderer.receiveShadows = false;
        }

        private void Update()
        {
            if (laserOrigin == null) return;


            float currentLaserLength;

            if (Physics.Raycast(laserOrigin.position, laserOrigin.forward, out RaycastHit hit, laserRange, hitLayers))
            {
                currentLaserLength = hit.distance;

                if (hit.collider.CompareTag("Player"))
                {
                    KillPlayer();
                }
            }
            else
            {
                currentLaserLength = laserRange;
            }

            UpdateLaserMesh(currentLaserLength);
        }

        private void UpdateLaserMesh(float length)
        {
            Mesh mesh = new Mesh();

            int vertexCount = laserSegments * 2 + 2;
            Vector3[] vertices = new Vector3[vertexCount];
            Vector2[] uvs = new Vector2[vertexCount];
            int[] triangles = new int[laserSegments * 6];

            float angleStep = 360f / laserSegments;
            float radius = laserWidth / 2f;

            for (int i = 0; i <= laserSegments; i++)
            {
                float angle = i * angleStep * Mathf.Deg2Rad;
                float x = Mathf.Cos(angle) * radius;
                float y = Mathf.Sin(angle) * radius;

                vertices[i * 2] = new Vector3(x, y, 0);
                uvs[i * 2] = new Vector2((float)i / laserSegments, 0);

                vertices[i * 2 + 1] = new Vector3(x, y, length);
                uvs[i * 2 + 1] = new Vector2((float)i / laserSegments, 1);
            }

            int triIndex = 0;
            for (int i = 0; i < laserSegments; i++)
            {
                int current = i * 2;
                int next = (i + 1) * 2;

                triangles[triIndex++] = current;
                triangles[triIndex++] = next;
                triangles[triIndex++] = current + 1;

                triangles[triIndex++] = current + 1;
                triangles[triIndex++] = next;
                triangles[triIndex++] = next + 1;
            }

            mesh.vertices = vertices;
            mesh.uv = uvs;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            laserMeshFilter.mesh = mesh;
        }

        private void KillPlayer()
        {
            EventBusVoid<PlayerEventsEnum>.Invoke(PlayerEventsEnum.Death);
        }

        private void OnDrawGizmosSelected()
        {
            if (laserOrigin == null)
            {
                laserOrigin = transform.Find("LaserOrigin");
                if (laserOrigin == null)
                    laserOrigin = transform;
            }

            Gizmos.color = laserColor;
            Gizmos.DrawRay(laserOrigin.position, laserOrigin.forward * laserRange);
        }
    }
}