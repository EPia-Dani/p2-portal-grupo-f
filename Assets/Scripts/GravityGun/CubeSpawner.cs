using System.Collections.Generic;
using UnityEngine;
using Player;
using PrimeTween;

namespace GravityGun
{
    public class CubeSpawner : MonoBehaviour, Interactable
    {
        public int maxCubes;
        public GameObject cubePrefab;
        public Vector3 spawnRelativePosition;
        private readonly Queue<GameObject> spawnedCubes = new();


        private void SpawnCube()
        {
            if (spawnedCubes.Count >= maxCubes)
            {
                var firstCube = spawnedCubes.Dequeue();
                Tween.Scale(firstCube.transform, Vector3.zero, 0.2f, Ease.InBack).OnComplete(() =>
                {
                    Destroy(firstCube);
                });
            }
            var cube = Instantiate(cubePrefab, transform.position + spawnRelativePosition, Quaternion.identity);
            var originalScale = cube.transform.localScale;
            cube.transform.localScale = Vector3.zero;
            Tween.Scale(cube.transform, originalScale, 0.5f, Ease.OutBack);
            spawnedCubes.Enqueue(cube);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(transform.position + spawnRelativePosition, 0.1f);
        }

        public void Interact()
        {
            //SpawnCube();
        }
        
        public void ReceiveSignal(bool signal)
        {
            if (signal)
            {
                SpawnCube();
            }
        }
    }
}