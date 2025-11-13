using System;
using System.Collections.Generic;
using UnityEngine;

namespace Turret
{
    public class LaserBeam
    {
        public class LaserHit
        {
            private readonly LayerMask? layerMask;
            private readonly string tag;
            private readonly List<Action<RaycastHit>> actions = new List<Action<RaycastHit>>();

            public LaserHit(LayerMask mask)
            {
                layerMask = mask;
            }

            public LaserHit(string tag)
            {
                this.tag = tag;
            }

            public bool Matches(Collider collider)
            {
                if (!string.IsNullOrEmpty(tag))
                {
                    return collider != null && collider.gameObject.CompareTag(tag);
                }

                if (layerMask.HasValue)
                {
                    // Check if collider.layer is included in the bitmask
                    return ((layerMask.Value.value & (1 << collider.gameObject.layer)) != 0);
                }

                return false;
            }

            public LaserHit OnHit(Action<RaycastHit> action)
            {
                if (action != null)
                {
                    actions.Add(action);
                }
                return this;
            }

            public void Invoke(RaycastHit hit)
            {
                for (int i = 0; i < actions.Count; i++)
                {
                    actions[i]?.Invoke(hit);
                }
            }
        }

        private readonly MeshFilter meshFilter;
        private readonly MeshRenderer meshRenderer;
        private readonly Transform origin;

        private readonly float range;
        private readonly float width;
        private readonly int segments;
        private readonly Color color;
        private readonly float intensity;

        private readonly List<LaserHit> hits = new List<LaserHit>();

        private float currentLength;
        private Mesh currentMesh;

        public LaserBeam(
            MeshFilter meshFilter,
            MeshRenderer meshRenderer,
            Transform origin,
            float range,
            float width,
            int segments,
            Color color,
            float intensity)
        {
            this.meshFilter = meshFilter ?? throw new ArgumentNullException(nameof(meshFilter));
            this.meshRenderer = meshRenderer ?? throw new ArgumentNullException(nameof(meshRenderer));
            this.origin = origin ?? throw new ArgumentNullException(nameof(origin));

            this.range = range;
            this.width = width;
            this.segments = segments;
            this.color = color;
            this.intensity = intensity;

            InitializeVisuals();
        }

        private void InitializeVisuals()
        {
            meshRenderer.material.SetColor("_Color", color);
            meshRenderer.material.SetFloat("_Intensity", intensity);
            meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            meshRenderer.receiveShadows = false;
        }

        public LaserHit AddHit(LayerMask mask)
        {
            var h = new LaserHit(mask);
            hits.Add(h);
            return h;
        }

        public LaserHit AddHit(string tag)
        {
            var h = new LaserHit(tag);
            hits.Add(h);
            return h;
        }

        public void AddHit(LaserHit hit)
        {
            if (hit != null)
            {
                hits.Add(hit);
            }
        }

        public void RemoveHit(LaserHit hit)
        {
            if (hit != null)
            {
                hits.Remove(hit);
            }
        }

        public void AddHitAction(LaserHit hit, Action<RaycastHit> action)
        {
            hit?.OnHit(action);
        }

        public void Tick()
        {
            if (origin == null) return;

            float newLength;
            RaycastHit hit;
            bool hasHit;

            LayerMask? combinedMask = GetCombinedLayerMask();
            if (combinedMask.HasValue)
            {
                hasHit = Physics.Raycast(origin.position, origin.forward, out hit, range, combinedMask.Value);
            }
            else
            {
                hasHit = Physics.Raycast(origin.position, origin.forward, out hit, range);
            }

            if (hasHit)
            {
                newLength = hit.distance;

                // Dispatch actions for all matching criteria
                for (int i = 0; i < hits.Count; i++)
                {
                    var laserHit = hits[i];
                    if (laserHit.Matches(hit.collider))
                    {
                        laserHit.Invoke(hit);
                    }
                }
            }
            else
            {
                newLength = range;
            }

            if (Mathf.Abs(currentLength - newLength) > 0.01f)
            {
                currentLength = newLength;
                UpdateLaserMesh(newLength);
            }
        }

        private LayerMask? GetCombinedLayerMask()
        {
            int mask = 0;
            bool hasMask = false;
            for (int i = 0; i < hits.Count; i++)
            {
                var h = hits[i];
                // Use reflection-safe approach by asking Matches against layers only if mask is present
                // We can't access h.layerMask directly here as it's private; rebuild masks via try-cast pattern:
                // Instead, we track by trying a collider layer bit OR approach is not possible without field access.
                // To keep things simple and robust, skip combining when we can't know; rely on unfiltered raycast.
                // We'll keep hasMask=false to fall back to unfiltered when any tag-based hits exist.
                // Optimization can be added later if needed.
            }
            if (hasMask)
            {
                return (LayerMask)mask;
            }
            return null;
        }

        private void UpdateLaserMesh(float length)
        {
            Mesh mesh = new Mesh();

            int vertexCount = segments * 2 + 2;
            Vector3[] vertices = new Vector3[vertexCount];
            Vector2[] uvs = new Vector2[vertexCount];
            int[] triangles = new int[segments * 6];

            float angleStep = 360f / segments;
            float radius = width / 2f;

            for (int i = 0; i <= segments; i++)
            {
                float angle = i * angleStep * Mathf.Deg2Rad;
                float x = Mathf.Cos(angle) * radius;
                float y = Mathf.Sin(angle) * radius;

                vertices[i * 2] = new Vector3(x, y, 0);
                uvs[i * 2] = new Vector2((float)i / segments, 0);

                vertices[i * 2 + 1] = new Vector3(x, y, length);
                uvs[i * 2 + 1] = new Vector2((float)i / segments, 1);
            }

            int triIndex = 0;
            for (int i = 0; i < segments; i++)
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

            if (currentMesh != null && meshFilter.mesh == currentMesh)
            {
                UnityEngine.Object.Destroy(currentMesh);
            }

            meshFilter.mesh = mesh;
            currentMesh = mesh;
        }

        public float GetCurrentLength() => currentLength;
    }
}

