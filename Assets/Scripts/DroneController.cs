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
        // Combine all input sources: keyboard, gamepad, and touch joystick
        float joystickInput = joystickLeft != null ? joystickLeft.Vertical : 0f;
        float gamepadInput = Input.GetAxis("Vertical");
        float droneAltitude = Mathf.Clamp(joystickInput + gamepadInput, -1f, 1f);
        
        // Log gamepad input
        if (Mathf.Abs(gamepadInput) > 0.1f)
        {
            Debug.Log($"Gamepad Left Stick Vertical: {gamepadInput:F2}");
        }

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
        // Combine all input sources: keyboard, gamepad, and touch joystick
        float joystickInput = joystickLeft != null ? joystickLeft.Horizontal : 0f;
        float gamepadInput = Input.GetAxis("Horizontal");
        float droneYaw = Mathf.Clamp(joystickInput + gamepadInput, -1f, 1f);
        
        // Log gamepad input
        if (Mathf.Abs(gamepadInput) > 0.1f)
        {
            Debug.Log($"Gamepad Left Stick Horizontal: {gamepadInput:F2}");
        }

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
        // Combine all input sources: keyboard, gamepad, and touch joystick
        float joystickPitchInput = joystickRight != null ? joystickRight.Vertical : 0f;
        float gamepadPitchInput = Input.GetAxis("RightStickVertical");
        float dronePitch = Mathf.Clamp(joystickPitchInput + gamepadPitchInput, -1f, 1f);
        
        float joystickRollInput = joystickRight != null ? joystickRight.Horizontal : 0f;
        float gamepadRollInput = Input.GetAxis("RightStickHorizontal");
        float droneRoll = Mathf.Clamp(joystickRollInput + gamepadRollInput, -1f, 1f);
        
        // Log gamepad input
        if (Mathf.Abs(gamepadPitchInput) > 0.1f)
        {
            Debug.Log($"Gamepad Right Stick Vertical (Pitch): {gamepadPitchInput:F2}");
        }
        if (Mathf.Abs(gamepadRollInput) > 0.1f)
        {
            Debug.Log($"Gamepad Right Stick Horizontal (Roll): {gamepadRollInput:F2}");
        }

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
        // Combine all input sources
        float joystickAltInput = joystickLeft != null ? joystickLeft.Vertical : 0f;
        float gamepadAltInput = Input.GetAxis("Vertical");
        float droneAltitude = Mathf.Clamp(joystickAltInput + gamepadAltInput, -1f, 1f);
        
        float joystickPitchInput = joystickRight != null ? joystickRight.Vertical : 0f;
        float gamepadPitchInput = Input.GetAxis("RightStickVertical");
        float pitch = Mathf.Clamp(joystickPitchInput + gamepadPitchInput, -1f, 1f);
        
        float joystickRollInput = joystickRight != null ? joystickRight.Horizontal : 0f;
        float gamepadRollInput = Input.GetAxis("RightStickHorizontal");
        float roll = Mathf.Clamp(joystickRollInput + gamepadRollInput, -1f, 1f);

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
        // Combine all input sources
        float joystickAltInput = joystickLeft != null ? joystickLeft.Vertical : 0f;
        float gamepadAltInput = Input.GetAxis("Vertical");
        float droneAltitude = Mathf.Clamp(joystickAltInput + gamepadAltInput, -1f, 1f);
        
        float joystickPitchInput = joystickRight != null ? joystickRight.Vertical : 0f;
        float gamepadPitchInput = Input.GetAxis("RightStickVertical");
        float pitch = Mathf.Clamp(joystickPitchInput + gamepadPitchInput, -1f, 1f);
        
        float joystickRollInput = joystickRight != null ? joystickRight.Horizontal : 0f;
        float gamepadRollInput = Input.GetAxis("RightStickHorizontal");
        float roll = Mathf.Clamp(joystickRollInput + gamepadRollInput, -1f, 1f);

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
