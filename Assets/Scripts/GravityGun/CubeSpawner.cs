using System.Collections.Generic;
using UnityEngine;
using Core.EventBus;
using Player;

public class CubeSpawner : MonoBehaviour
{
    public GameObject cubePrefab;
    public float spawnClearance = 0.01f; 

    private Camera playerCamera;
    public float pickRange = 5f;

    [Header("Limit")]
    public int maxCubes;

    private readonly List<GameObject> spawnedCubes = new List<GameObject>();

    void Start()
    {
        playerCamera = Camera.main;
        EventBusVoid<PlayerEventsEnum>.Subscribe(PlayerEventsEnum.Interact, HandleInteract);
    }

    void OnDisable()
    {
        EventBusVoid<PlayerEventsEnum>.Unsubscribe(PlayerEventsEnum.Interact, HandleInteract);
    }

    void HandleInteract()
    {
        SpawnCube();
    }

    void SpawnCube()
    {
        if (cubePrefab == null)
        {
            Debug.LogWarning("CubeSpawner: cubePrefab is not assigned.");
            return;
        }

        spawnedCubes.RemoveAll(item => item == null);

        if (spawnedCubes.Count >= maxCubes)
        {
            Debug.Log("CubeSpawner: max cubes reached, not spawning.");
            return;
        }

        if (playerCamera == null)
        {
            Debug.LogWarning("CubeSpawner: playerCamera is null.");
            return;
        }

        Ray ray = playerCamera.ScreenPointToRay(new Vector2(Screen.width / 2f, Screen.height / 2f));
        if (Physics.Raycast(ray, out RaycastHit hit, pickRange))
        {
            if (hit.collider.GetComponent<CubeSpawner>() != null)
            {
                float topY = transform.position.y;
                Collider spawnerCol = GetComponent<Collider>();
                if (spawnerCol != null) topY = spawnerCol.bounds.max.y;

                GameObject cube = Instantiate(cubePrefab, transform.position, Quaternion.identity);

                Collider cubeCol = cube.GetComponent<Collider>();
                if (cubeCol == null)
                {
                    cubeCol = cube.AddComponent<BoxCollider>();
                }

                Rigidbody rb = cube.GetComponent<Rigidbody>();
                if (rb == null)
                {
                    rb = cube.AddComponent<Rigidbody>();
                }

                float halfHeight = cubeCol.bounds.extents.y;
                Vector3 spawnPos = new Vector3(transform.position.x, topY + halfHeight + spawnClearance, transform.position.z);
                cube.transform.position = spawnPos;

                if (cube.GetComponent<PickableCube>() == null)
                {
                    cube.AddComponent<PickableCube>();
                }

                spawnedCubes.Add(cube);
            }
        }
    }
}