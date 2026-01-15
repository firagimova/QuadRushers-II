using UnityEngine;
using System.Collections.Generic;
using Resources;
using static EventList;

public class RingSpawner : MonoBehaviour
{
    public static RingSpawner Instance { get; private set; }

    public enum SpawnMode
    {
        Random,
        Predetermined
    }

    [Header("Spawn Settings")]
    [SerializeField] private SpawnMode spawnMode = SpawnMode.Random;
    [SerializeField] private GameObject ringPrefab;

    [Header("Predetermined Mode")]
    [SerializeField] private GameObject[] spawnPoints;

    [Header("Random Mode")]
    [SerializeField] private int randomRingCount = 5;
    [SerializeField] private Vector2 xRange = new Vector2(-40f, 40f);
    [SerializeField] private Vector2 yRange = new Vector2(2f, 5f);
    [SerializeField] private Vector2 zRange = new Vector2(-40f, 40f);

    private List<GameObject> spawnedRings = new List<GameObject>();
    private int collectedPredeterminedRings = 0;
    private int totalPredeterminedRings = 0;

    public SpawnMode CurrentSpawnMode => spawnMode;
    public bool IsPredeterminedMode => spawnMode == SpawnMode.Predetermined && spawnPoints != null && spawnPoints.Length > 0;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Auto-detect mode based on spawn points
        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            spawnMode = SpawnMode.Predetermined;
        }
        
        SpawnRings();
    }

    public void SpawnRings()
    {
        ClearRings();
        collectedPredeterminedRings = 0;

        if (IsPredeterminedMode)
        {
            SpawnPredeterminedRings();
        }
        else
        {
            SpawnRandomRings();
        }
    }

    private void SpawnPredeterminedRings()
    {
        totalPredeterminedRings = spawnPoints.Length;
        
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            if (spawnPoints[i] == null) continue;
            
            Vector3 position = spawnPoints[i].transform.position;
            Quaternion rotation = spawnPoints[i].transform.rotation;

            GameObject ring = Instantiate(ringPrefab, position, rotation);
            spawnedRings.Add(ring);
        }

        Debug.Log($"Spawned {spawnPoints.Length} rings at predetermined positions");
    }

    private void SpawnRandomRings()
    {
        for (int i = 0; i < randomRingCount; i++)
        {
            Vector3 position = FindRandomPosition();
            
            int attempts = 0;
            while (!IsValidPosition(position) && attempts < 10)
            {
                position = FindRandomPosition();
                attempts++;
            }

            GameObject ring = Instantiate(ringPrefab, position, Quaternion.identity);
            spawnedRings.Add(ring);
        }

        Debug.Log($"Spawned {randomRingCount} rings at random positions");
    }

    // Called by RingFunctions when a predetermined ring is collected
    public void OnPredeterminedRingCollected()
    {
        collectedPredeterminedRings++;
        Debug.Log($"Collected {collectedPredeterminedRings}/{totalPredeterminedRings} predetermined rings");

        if (collectedPredeterminedRings >= totalPredeterminedRings)
        {
            Debug.Log("All predetermined rings collected! Sending QuestCompleted...");
            EventBus<QuestCompleted>.Emit(this, new QuestCompleted());
        }
    }

    private Vector3 FindRandomPosition()
    {
        float x = Random.Range(xRange.x, xRange.y);
        float y = Random.Range(yRange.x, yRange.y);
        float z = Random.Range(zRange.x, zRange.y);
        return new Vector3(x, y, z);
    }

    private bool IsValidPosition(Vector3 position)
    {
        RaycastHit hit;
        if (Physics.Raycast(position, Vector3.down, out hit))
        {
            if (hit.distance < 1f)
            {
                return false;
            }
        }
        return true;
    }

    public void ClearRings()
    {
        foreach (var ring in spawnedRings)
        {
            if (ring != null)
            {
                Destroy(ring);
            }
        }
        spawnedRings.Clear();
    }

}
