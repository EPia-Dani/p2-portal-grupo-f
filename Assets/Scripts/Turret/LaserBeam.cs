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

            public bool HasMask => layerMask.HasValue;
            public bool HasTag => !string.IsNullOrEmpty(tag);
            public LayerMask? Mask => layerMask;

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
        private Transform origin;

        private readonly float range;
        private readonly float width;
        private readonly int segments;
        private readonly Color color;
        private readonly float intensity;

        private readonly List<LaserHit> hits = new List<LaserHit>();

        // Reusable mesh data to avoid per-frame allocations
        private Mesh _mesh;
        private Vector3[] _vertices;
        private Vector2[] _uvs;
        private int[] _triangles;
        private int _cachedSegments = -1;

        private bool isActive = true;
        private bool useManualStart;
        private Vector3 manualStartPosition;
        private Vector3 manualDirection = Vector3.forward;

        private float currentLength;
        public Vector3 CurrentStart { get; private set; }
        public Vector3 CurrentDirection { get; private set; }
        public bool IsActive => isActive;

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
            EnsureMeshInitialized();
        }

        private void InitializeVisuals()
        {
            meshRenderer.material.SetColor("_Color", color);
            meshRenderer.material.SetFloat("_Intensity", intensity);
            meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            meshRenderer.receiveShadows = false;
        }

        public void SetActive(bool active)
        {
            isActive = active;
            if (meshRenderer != null)
            {
                meshRenderer.enabled = active;
            }
        }

        public void SetStart(Transform newOrigin)
        {
            origin = newOrigin;
            useManualStart = false;
        }

        public void SetStart(Vector3 position, Vector3 direction)
        {
            useManualStart = true;
            manualStartPosition = position;
            manualDirection = direction.sqrMagnitude > 0f ? direction.normalized : Vector3.forward;

            // Align mesh transform immediately
            var tf = meshFilter.transform;
            tf.SetPositionAndRotation(manualStartPosition, Quaternion.LookRotation(manualDirection));
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
            if (!isActive) return;
            if (!useManualStart && origin == null) return;

            float newLength;
            RaycastHit hit;
            bool hasHit;

            // Determine start and direction
            Vector3 startPos;
            Vector3 dir;
            if (useManualStart)
            {
                startPos = manualStartPosition;
                dir = manualDirection;
            }
            else
            {
                startPos = origin.position;
                dir = origin.forward;
            }

            CurrentStart = startPos;
            CurrentDirection = dir;

            // Keep mesh object aligned with ray each frame
            var tf = meshFilter.transform;
            tf.SetPositionAndRotation(startPos, Quaternion.LookRotation(dir));

            LayerMask? combinedMask = GetCombinedLayerMask();
            if (combinedMask.HasValue)
            {
                hasHit = Physics.Raycast(startPos, dir, out hit, range, combinedMask.Value);
            }
            else
            {
                hasHit = Physics.Raycast(startPos, dir, out hit, range);
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
            bool hasTag = false;
            for (int i = 0; i < hits.Count; i++)
            {
                var h = hits[i];
                if (h.HasTag) hasTag = true;
                if (h.HasMask && h.Mask.HasValue)
                {
                    hasMask = true;
                    mask |= h.Mask.Value.value;
                }
            }
            // If any tag-based condition exists, don't filter; otherwise use the combined mask
            if (!hasTag && hasMask)
            {
                return (LayerMask)mask;
            }
            return null;
        }

        private void UpdateLaserMesh(float length)
        {
            EnsureMeshInitialized();

            float angleStep = 360f / segments;

            var tf = meshFilter.transform;
            var ls = tf.lossyScale;
            float scaleXY = (Mathf.Abs(ls.x) + Mathf.Abs(ls.y)) * 0.5f;
            if (scaleXY <= 0.0001f) scaleXY = 1f;
            float radius = width / 2f / scaleXY;

            float scaleZ = Mathf.Abs(ls.z);
            if (scaleZ <= 0.0001f) scaleZ = 1f;
            float localLength = length / scaleZ;

            for (int i = 0; i <= segments; i++)
            {
                float angle = i * angleStep * Mathf.Deg2Rad;
                float x = Mathf.Cos(angle) * radius;
                float y = Mathf.Sin(angle) * radius;

                _vertices[i * 2] = new Vector3(x, y, 0);
                _vertices[i * 2 + 1] = new Vector3(x, y, localLength);
            }

            _mesh.vertices = _vertices;
            _mesh.RecalculateBounds();
            _mesh.RecalculateNormals();
        }

        private void EnsureMeshInitialized()
        {
            if (_mesh != null && _cachedSegments == segments) return;

            _cachedSegments = segments;

            int vertexCount = segments * 2 + 2;
            _vertices = new Vector3[vertexCount];
            _uvs = new Vector2[vertexCount];
            _triangles = new int[segments * 6];

            for (int i = 0; i <= segments; i++)
            {
                _uvs[i * 2] = new Vector2((float)i / segments, 0);
                _uvs[i * 2 + 1] = new Vector2((float)i / segments, 1);
            }

            int triIndex = 0;
            for (int i = 0; i < segments; i++)
            {
                int current = i * 2;
                int next = (i + 1) * 2;

                _triangles[triIndex++] = current;
                _triangles[triIndex++] = next;
                _triangles[triIndex++] = current + 1;

                _triangles[triIndex++] = current + 1;
                _triangles[triIndex++] = next;
                _triangles[triIndex++] = next + 1;
            }

            _mesh = new Mesh();
            _mesh.MarkDynamic();
            _mesh.vertices = _vertices;
            _mesh.uv = _uvs;
            _mesh.triangles = _triangles;

            meshFilter.mesh = _mesh;
        }
    }
}

