using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(BoxCollider))]

public class dronePhysicsController : MonoBehaviour
{

    [SerializeField] private Rigidbody droneRb;
    [SerializeField] private FixedJoystick droneAltYaw;
    [SerializeField] private FixedJoystick dronePitRoll;
    [SerializeField] private float droneMovementSpeed;
    [SerializeField] private float droneAngleSpeed;


    [SerializeField] Transform DronePropellerLB;
    [SerializeField] Transform DronePropellerLF;
    [SerializeField] Transform DronePropellerRB;
    [SerializeField] Transform DronePropellerRF;


    void Start()
    {
        droneRb = GetComponent<Rigidbody>();
        droneRb.constraints = RigidbodyConstraints.FreezeRotation;
    }
    private void droneSomersoults()
    {

        float pitch = dronePitRoll.Vertical;
        float roll = dronePitRoll.Horizontal;

        float angleOfPitch = Input.GetAxis("Vertical");
        float angleOfRoll = Input.GetAxis("Horizontal");

        if (pitch != 0 || roll != 0)
        {
            transform.Rotate(pitch * droneAngleSpeed * Time.deltaTime, 0, -roll * droneAngleSpeed * Time.deltaTime, Space.Self);
        }

        else
        {
            Vector3 droneZero = new Vector3(angleOfPitch, 0.0f, angleOfRoll);
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(droneZero), 0.15F);
            transform.Translate(droneZero * droneAngleSpeed * Time.deltaTime, Space.World);
        }

    }
    private void dronePropeller()
    {
        DronePropellerLB.Rotate(Vector3.up * droneMovementSpeed * 500);
        DronePropellerLF.Rotate(Vector3.up * droneMovementSpeed * 500);
        DronePropellerRB.Rotate(Vector3.up * droneMovementSpeed * 500);
        DronePropellerRF.Rotate(Vector3.up * droneMovementSpeed * 500);
    }
    private void droneVectoralsGravity()
    {
        float droneCurrentAltitude = transform.position.y;
        float droneAltitude = droneAltYaw.Vertical;
        float droneYaw = droneAltYaw.Horizontal;



        if (droneAltitude > 0f || Input.GetKey(KeyCode.T))
        {
            droneRb.AddForce(transform.up * droneMovementSpeed, ForceMode.Force);
        }

        if (droneAltitude < 0f || Input.GetKey(KeyCode.R))
        {
       
        }


        Debug.Log(droneCurrentAltitude);
    }

    void FixedUpdate()
    {

        if (transform.position.y < 10f)
        {
            droneSomersoults();
            droneVectoralsGravity();
            dronePropeller();
            GetComponent<Rigidbody>().isKinematic = false;
        }

        else
        {
            droneSomersoults();
            dronePropeller();
            GetComponent<Rigidbody>().isKinematic = true;
        }

    }
}


