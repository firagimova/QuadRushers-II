using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneRespawn : MonoBehaviour
{
    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private bool isRespawning = false;
    private Camera mainCamera;
    public Camera respawnCamera;
    public Camera droneBackCamera;

    private void Start()
    {
        initialPosition = FindRandomPosition();
        initialRotation = transform.rotation;
        transform.position = initialPosition;

        mainCamera = Camera.main;

        //RaycastHit hit;
        //if (Physics.Raycast(transform.position, -transform.up, out hit, Mathf.Infinity))
        //{
        //    Debug.Log("Raycast çalýþtý, isabet eden obje: " + hit.collider.gameObject.name);
        //}
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Environment") && !isRespawning)
        {
            StartCoroutine(DisableControlsAndRespawn(3f));
            SwitchCamera(respawnCamera);
            Debug.Log("Çok sert bir þekilde çarptýn!");
        }
    }

    private IEnumerator DisableControlsAndRespawn(float respawnDelay)
    {
        isRespawning = true;

        DroneController droneController = GetComponent<DroneController>();
        if (droneController != null)
        {
            droneController.enabled = false;
        }

       
        yield return new WaitForSeconds(respawnDelay);

    
        if (droneController != null)
        {
            droneController.enabled = true;
        }

  
        transform.position = initialPosition;
        transform.rotation = initialRotation;

        isRespawning = false;

    
        SwitchCamera(droneBackCamera);
    }

    private void SwitchCamera(Camera targetCamera)
    {
  
        Camera[] cameras = Camera.allCameras;
        foreach (Camera camera in cameras)
        {
            camera.enabled = false;
        }

  
        targetCamera.enabled = true;
    }

    private Vector3 FindRandomPosition()
    {
        float x = Random.Range(-40f, 40f);
        float y = Random.Range(1f, 1.25f);
        float z = Random.Range(-40f, 40f);
        return new Vector3(x, y, z);
    }
}