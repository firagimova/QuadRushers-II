using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ringYedek : MonoBehaviour
{

    public float rotationSpeed = 50f; // Yüzüklerin dönüþ hýzý

    private BoxCollider myCollider;

    void Start()
    {
        myCollider = GetComponent<BoxCollider>();
        myCollider.isTrigger = true;
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
            Debug.Log("Abi?!?!");
        }
    }

    void RingStartPosition()
    {
        float x = Random.Range(-15f, 15f);
        float y = Random.Range(0f, 10f);
        float z = Random.Range(-15f, 15f);
        transform.position = new Vector3(x, y, z);
    }

    private void CreateRing()
    {
        GameObject newRing = Instantiate(gameObject);

        float x = Random.Range(-20f, 20f);
        float y = Random.Range(1f, 10f);
        float z = Random.Range(-20f, 20f);
        newRing.transform.position = new Vector3(x, 0.9f, z);

        RingFunctions newRingFunctions = newRing.GetComponent<RingFunctions>();
        newRingFunctions.rotationSpeed = Random.Range(25f, 75f);
    }

    public static int collectedRings = 0;

    public static void CollectRing()
    {
        collectedRings++;
        Debug.Log("Collected Rings: " + collectedRings);
        CollectingUI ui = FindObjectOfType<CollectingUI>();
        ui.UpdateRingText();
    }


}
