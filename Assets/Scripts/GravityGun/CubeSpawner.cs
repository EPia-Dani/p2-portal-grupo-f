using System.Collections.Generic;
using UnityEngine;
using Core.EventBus;
using Player;

namespace GravityGun
{

    public class CubeSpawner : MonoBehaviour, Interactable
    {
        public GameObject cubePrefab;
        public float spawnClearance = 0.01f;

        public float pickRange = 5f;

        [Header("Limit")]
        public int maxCubes;

        private readonly List<GameObject> spawnedCubes = new List<GameObject>();
        

        private void SpawnCube()
        {
            spawnedCubes.RemoveAll(item => item == null);
            Collider spawnerCol = GetComponent<Collider>();
            var topY = spawnerCol.bounds.max.y;
            GameObject cube = Instantiate(cubePrefab, transform.position, Quaternion.identity);
            Collider cubeCol = cube.GetComponent<Collider>();
            float halfHeight = cubeCol.bounds.extents.y;
            Vector3 spawnPos = new Vector3(transform.position.x, topY + halfHeight + spawnClearance, transform.position.z);
            cube.transform.position = spawnPos;
            spawnedCubes.Add(cube);
        }

        public void Interact()
        {
            SpawnCube();
        }
    }
}