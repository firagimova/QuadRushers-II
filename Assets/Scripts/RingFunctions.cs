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
        RingStartPosition();
    }

    void Update()
    {
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Drone"))
        {
            CreateRing();
            Destroy(gameObject);
            CollectRing();
        }
        else if (other.gameObject.CompareTag("Environment"))
        {
            Debug.Log("Yüzük açýk alanda oluþamadý.");
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
        // Ýþlem yapmadan önce yüzüðün rigidbody'sini etkinleþtirir
        myRigidbody.isKinematic = false;

        RaycastHit hit;
        // Yapay bir ýþýn oluþturuyoruz
        if (Physics.Raycast(point, Vector3.down, out hit))
        {
            // Iþýn, çevredeki bir objeye temas ettiyse, yüzük collider'ý ile obje arasýnda bir þey var demektir
            if (hit.distance < 1f)
            {
                myRigidbody.isKinematic = true; // Yüzükleri tekrar hareketsiz hale getirir
                return false;
            }
        }
        // Yüzüklerin rigidbody'sini tekrar etkisiz hale getirir
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
        Debug.Log("Toplanan Yüzük Sayýsý: " + collectedRings);
        CollectingUI ui = FindObjectOfType<CollectingUI>();
        ui.UpdateRingText();
    }
}

