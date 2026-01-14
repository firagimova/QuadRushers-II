using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(Rigidbody))]

public class RingFunctions : MonoBehaviour
{
    public float rotationSpeed = 50f;

    private Rigidbody myRigidbody;

    void Start()
    {
        myRigidbody = GetComponent<Rigidbody>();
        myRigidbody.isKinematic = true;
        
        // Only auto-position if not spawned by RingSpawner in predetermined mode
        if (RingSpawner.Instance == null || !RingSpawner.Instance.IsPredeterminedMode)
        {
            RingStartPosition();
        }
    }

    void Update()
    {
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Drone"))
        {
            CollectRing();
            
            // Check spawn mode
            if (RingSpawner.Instance != null && RingSpawner.Instance.IsPredeterminedMode)
            {
                // Predetermined mode: notify spawner and destroy (no respawn)
                RingSpawner.Instance.OnPredeterminedRingCollected();
                Destroy(gameObject);
            }
            else
            {
                // Random mode: spawn new ring and destroy this one
                CreateRing();
                Destroy(gameObject);
            }
        }
        else if (other.gameObject.CompareTag("Environment"))
        {
            Debug.Log("Ring spawned in invalid location, repositioning...");
            RingStartPosition();
        }
    }

    void RingStartPosition()
    {
        Vector3 position = FindRandomPosition();
        transform.position = position;
        while (!CheckSpawnPoint(position))
        {
            position = FindRandomPosition();
            transform.position = position;
        }
    }

    private Vector3 FindRandomPosition()
    {
        float x = Random.Range(-40f, 40f);
        float y = Random.Range(2f, 5f);
        float z = Random.Range(-40f, 40f);
        return new Vector3(x, y, z);
    }

    private bool CheckSpawnPoint(Vector3 point)
    {
        myRigidbody.isKinematic = false;

        RaycastHit hit;
        if (Physics.Raycast(point, Vector3.down, out hit))
        {
            if (hit.distance < 1f)
            {
                myRigidbody.isKinematic = true;
                return false;
            }
        }
        myRigidbody.isKinematic = true;
        return true;
    }

    private void CreateRing()
    {
        GameObject newRing = Instantiate(gameObject);

        Vector3 position = FindRandomPosition();
        newRing.transform.position = position;
        while (!CheckSpawnPoint(position))
        {
            position = FindRandomPosition();
            newRing.transform.position = position;
        }

        RingFunctions newRingFunctions = newRing.GetComponent<RingFunctions>();
        newRingFunctions.rotationSpeed = Random.Range(25f, 75f);
    }

    public static int collectedRings = 0;

    public static void CollectRing()
    {
        collectedRings++;
        Debug.Log("Collected Rings: " + collectedRings);
        CollectingUI ui = FindObjectOfType<CollectingUI>();
        if (ui != null)
        {
            ui.UpdateRingText();
        }
    }
}
