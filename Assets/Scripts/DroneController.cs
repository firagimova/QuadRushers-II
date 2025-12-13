using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(Rigidbody))]

public class DroneController : MonoBehaviour
{
    public TextMeshProUGUI speedText;
    public TextMeshProUGUI altitudeText;
    public TextMeshProUGUI batteryLevelText;

    private int collectedRings = 0; // Toplanan y�z�k say�s�
    private int maxRings = 20; // Toplam y�z�k say�s�
    private float batteryLevel = 100f; // Pil seviyesi
    [SerializeField] float batteryDecreaseRate = 0.5f; // Pil azalma h�z� (%/s)
    [SerializeField] float batteryPowerRate = 1f; // Pil azalma h�z� (%/s)
    [SerializeField] float batteryDecreaseRateActive = 2f; // Pil azalma h�z� kuvvet uyguland���nda (%/s)
    private bool isApplyingForce = false; // Drone'a kuvvet uygulan�yor mu

    void UpdateBatteryLevel()
    {
        float percentage = (float)collectedRings / maxRings * 100f;
        batteryLevel = Mathf.Clamp(percentage, 0f, 100f);
    }

    void Battery()
    {
        float currentDecreaseRate = isApplyingForce ? batteryDecreaseRateActive : batteryDecreaseRate;
        float decreaseAmount = currentDecreaseRate * Time.deltaTime;
        batteryLevel -= decreaseAmount;
    }

    public void CollectRing(Collider collider)
    {
        if (collider.gameObject.CompareTag("Ring"))
        {
            collectedRings++;
            batteryLevel += batteryPowerRate;
            batteryLevel = Mathf.Clamp(batteryLevel, 0f, 100f);
            batteryLevelText.text = "%" + batteryLevel.ToString("F0");
            
            // Notify QuestManager
            if (QuestManager.Instance != null)
            {
                QuestManager.Instance.OnRingCollected();
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        CollectRing(other);
    }

    void CheckBatteryLevel()
    {
        if (batteryLevel <= 0f)
        {
            // Batarya seviyesi 0'a d��t���nde yap�lmas� gereken i�lemler
            DroneFall();
        }
    }

    void DroneFall()
    {
        GetComponent<DroneController>().enabled = false;
        droneThrust = 5f;
        dronePropellerSpeed = 50f;
        
        // Notify QuestManager
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnCrash();
        }
    }

    // UNITY ZIPZIPLARI BA�LANGI�
    void Start()
    {
        droneRb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
       // Debug.Log(transform.position.y);
        isApplyingForce = false; // Reset force state each frame
        CheckController();
        DronePropeller();
        if (isForward)
        {
            MovementControllers();
            LeftControllersYaw();
        }
        else
        {
            LeftControllersAltitude();
            LeftControllersYaw();
            RightControllers();
        }

        float speed = droneRb.linearVelocity.magnitude * 15; // H�z� elde et
        speedText.text = Mathf.Floor(speed).ToString() + "km/h";

        float height = Mathf.Max(0, transform.position.y * 1.5f);
        altitudeText.text = height.ToString("F1") + "m";

        Battery();
        CheckBatteryLevel();
        batteryLevelText.text = "%" + batteryLevel.ToString("F0");
        
        // Update QuestManager
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.UpdateSpeed(speed);
            QuestManager.Instance.UpdateAltitude(height);
            QuestManager.Instance.UpdateBattery(batteryLevel);
        }

    }


    // UNITY ZIPZIPLARI B�T��

    // DRONE D�NAM�K DE���KENLER� BA�LANGI�
    [SerializeField] public float droneSpeed = 2.5f;
    [SerializeField] public float droneDynamic = 100f;
    [SerializeField] public float droneStabilizer = 150f;
    [SerializeField] public float droneYawer = 50f;
    [SerializeField] public float droneGravity = 15f;
    [SerializeField] public float droneYawStabilizer = 0.5f;
    [SerializeField] public float droneThrust = 9.81f;
    [SerializeField] public float dronePropellerSpeed = 10000;
    [SerializeField] FixedJoystick joystickLeft;
    [SerializeField] FixedJoystick joystickRight;
    [SerializeField] Transform DronePropellerLB;
    [SerializeField] Transform DronePropellerLF;
    [SerializeField] Transform DronePropellerRB;
    [SerializeField] Transform DronePropellerRF;
    public Rigidbody droneRb;
    private bool isForward;
    // DRONE D�NAM�K DE���KENLER� B�T��

    // DRONE D�NAM�KLER� BA�LANGI�
    void LeftControllersAltitude()
    {
        float droneAltitude = joystickLeft.Vertical;

        if (Input.GetKey(KeyCode.W) || droneAltitude > 0)
        {
            droneRb.AddForce(transform.up * droneSpeed * droneGravity, ForceMode.Acceleration);
            isApplyingForce = true;
        }

        else if (Input.GetKey(KeyCode.S) || droneAltitude < 0)
        {
            transform.Translate(-Vector3.up * droneSpeed * Time.deltaTime);
            isApplyingForce = true;
        }

        else
        {
            droneRb.AddForce(Vector3.up * droneThrust, ForceMode.Acceleration);
        }
    }
    void LeftControllersYaw()
    {
        float droneYaw = joystickLeft.Horizontal;

        if (Input.GetKey(KeyCode.A) || droneYaw < 0)
        {
            droneRb.AddTorque(-Vector3.up * droneYawer, ForceMode.Acceleration);
            isApplyingForce = true;
        }
        else if (Input.GetKey(KeyCode.D) || droneYaw > 0)
        {
            droneRb.AddTorque(Vector3.up * droneYawer, ForceMode.Acceleration);
            isApplyingForce = true;
        }
        else
        {
            // D�nmeyi durdurma
            droneRb.angularVelocity *= droneYawStabilizer;
        }
    }

    void RightControllers()
    {
        float dronePitch = joystickRight.Vertical;
        float droneRoll = joystickRight.Horizontal;

        if (Input.GetKey(KeyCode.UpArrow) || dronePitch > 0)
        {
            transform.Rotate(Vector3.right * droneDynamic * Time.deltaTime);
            isApplyingForce = true;
        }

        else if (Input.GetKey(KeyCode.DownArrow) || dronePitch < 0)
        {
            transform.Rotate(-Vector3.right * droneDynamic * Time.deltaTime);
            isApplyingForce = true;
        }

        else if (Input.GetKey(KeyCode.LeftArrow) || droneRoll < 0)
        {
            transform.Rotate(Vector3.forward * droneDynamic * Time.deltaTime);
            isApplyingForce = true;
        }

        else if (Input.GetKey(KeyCode.RightArrow) || droneRoll > 0)
        {
            transform.Rotate(-Vector3.forward * droneDynamic * Time.deltaTime);
            isApplyingForce = true;
        }

        else
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(0f, transform.rotation.eulerAngles.y, 0f), droneStabilizer * Time.deltaTime);
        }
    }
    void MovementControllers()
    {
        float droneAltitude = joystickLeft.Vertical;
        float pitch = joystickRight.Vertical;
        float roll = joystickRight.Horizontal;

        if (Input.GetKey(KeyCode.W) && Input.GetKey(KeyCode.UpArrow) || droneAltitude > 0 && pitch > 0)
        {
            droneRb.AddForce(transform.forward * droneSpeed * droneGravity, ForceMode.Acceleration);
            droneRb.AddForce(Vector3.up * droneThrust, ForceMode.Acceleration);
            isForward = true;
            isApplyingForce = true;
        }

        else if (Input.GetKey(KeyCode.W) && Input.GetKey(KeyCode.DownArrow) || droneAltitude > 0 && pitch < 0)
        {
            droneRb.AddForce(transform.forward * -droneSpeed * droneGravity, ForceMode.Acceleration);
            droneRb.AddForce(Vector3.up * droneThrust, ForceMode.Acceleration);
            isForward = true;
            isApplyingForce = true;
        }

        else if (Input.GetKey(KeyCode.W) && Input.GetKey(KeyCode.RightArrow) || droneAltitude > 0 && roll > 0)
        {
            droneRb.AddForce(transform.right * droneSpeed * droneGravity, ForceMode.Acceleration);
            droneRb.AddForce(Vector3.up * droneThrust, ForceMode.Acceleration);
            isForward = true;
            isApplyingForce = true;
        }

        else if (Input.GetKey(KeyCode.W) && Input.GetKey(KeyCode.LeftArrow) || droneAltitude > 0 && roll < 0)
        {
            droneRb.AddForce(transform.right * -droneSpeed * droneGravity, ForceMode.Acceleration);
            droneRb.AddForce(Vector3.up * droneThrust, ForceMode.Acceleration);
            isForward = true;
            isApplyingForce = true;
        }
    }
    void CheckController()
    {
        float droneAltitude = joystickLeft.Vertical;
        float pitch = joystickRight.Vertical;
        float roll = joystickRight.Horizontal;

        if (Input.GetKey(KeyCode.W) && Input.GetKey(KeyCode.UpArrow) || droneAltitude > 0 && pitch > 0)
        {
            isForward = true;
        }

        else if (Input.GetKey(KeyCode.W) && Input.GetKey(KeyCode.DownArrow) || droneAltitude > 0 && pitch < 0)
        {
            isForward = true;
        }

        else if (Input.GetKey(KeyCode.W) && Input.GetKey(KeyCode.LeftArrow) || droneAltitude > 0 && roll > 0)
        {
            isForward = true;
        }

        else if (Input.GetKey(KeyCode.W) && Input.GetKey(KeyCode.RightArrow) || droneAltitude > 0 && roll < 0)
        {
            isForward = true;
        }

        else
        {
            isForward = false;
        }
    }
    void DronePropeller()
    {
        DronePropellerLB.Rotate(Vector3.up * dronePropellerSpeed);
        DronePropellerLF.Rotate(Vector3.up * dronePropellerSpeed);
        DronePropellerRB.Rotate(Vector3.up * dronePropellerSpeed);
        DronePropellerRF.Rotate(Vector3.up * dronePropellerSpeed);
    }
    // DRONE D�NAM�KLER� B�T��
}
