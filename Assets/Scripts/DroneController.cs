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
        // Debug.Log($"LS V: {Input.GetAxis("LeftStickVertical"):F2} | " +
        //       $"LS H: {Input.GetAxis("LeftStickHorizontal"):F2} | " +
        //       $"RS V: {Input.GetAxis("RightStickVertical"):F2} | " +
        //       $"RS H: {Input.GetAxis("RightStickHorizontal"):F2}");
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
    [SerializeField] private float inputDeadzone = 0.2f; // Deadzone for controller inputs
    // DRONE D�NAM�K DE���KENLER� B�T��
    
    // Helper function to apply deadzone to input
    private float ApplyDeadzone(float input, float deadzone = 0.2f)
    {
        if (Mathf.Abs(input) < deadzone)
            return 0f;
        
        // Remap the value to account for deadzone
        float sign = Mathf.Sign(input);
        float absInput = Mathf.Abs(input);
        float remapped = (absInput - deadzone) / (1f - deadzone);
        return sign * Mathf.Clamp01(remapped);
    }

    // DRONE DINAMIKLER BALANGI
    void LeftControllersAltitude()
    {
        // Left Stick controls Altitude (W/S equivalent)
        float joystickInput = joystickLeft != null ? joystickLeft.Vertical : 0f;
        float gamepadInput = ApplyDeadzone(Input.GetAxis("LeftStickVertical"), inputDeadzone);
        float droneAltitude = Mathf.Clamp(joystickInput + gamepadInput, -1f, 1f);

        if (Input.GetKey(KeyCode.W) || Mathf.Abs(droneAltitude) > 0.01f && droneAltitude > 0)
        {
            droneRb.AddForce(transform.up * droneSpeed * droneGravity, ForceMode.Acceleration);
            isApplyingForce = true;
        }

        else if (Input.GetKey(KeyCode.S) || Mathf.Abs(droneAltitude) > 0.01f && droneAltitude < 0)
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
        // Left Stick Horizontal controls Yaw (A/D equivalent)
        float joystickInput = joystickLeft != null ? joystickLeft.Horizontal : 0f;
        float gamepadInput = ApplyDeadzone(Input.GetAxis("LeftStickHorizontal"), inputDeadzone);
        float droneYaw = Mathf.Clamp(joystickInput + gamepadInput, -1f, 1f);

        if (Input.GetKey(KeyCode.A) || Mathf.Abs(droneYaw) > 0.01f && droneYaw < 0)
        {
            droneRb.AddTorque(-Vector3.up * droneYawer, ForceMode.Acceleration);
            isApplyingForce = true;
        }
        else if (Input.GetKey(KeyCode.D) || Mathf.Abs(droneYaw) > 0.01f && droneYaw > 0)
        {
            droneRb.AddTorque(Vector3.up * droneYawer, ForceMode.Acceleration);
            isApplyingForce = true;
        }
        else
        {
            // Stop rotation
            droneRb.angularVelocity *= droneYawStabilizer;
        }
    }

    void RightControllers()
    {
        // Right Stick controls Pitch and Roll (Arrow Keys equivalent)
        float joystickPitchInput = joystickRight != null ? joystickRight.Vertical : 0f;
        float gamepadPitchInput = ApplyDeadzone(Input.GetAxis("RightStickVertical"), inputDeadzone);
        float dronePitch = Mathf.Clamp(joystickPitchInput + gamepadPitchInput, -1f, 1f);
        
        float joystickRollInput = joystickRight != null ? joystickRight.Horizontal : 0f;
        float gamepadRollInput = ApplyDeadzone(Input.GetAxis("RightStickHorizontal"), inputDeadzone);
        float droneRoll = Mathf.Clamp(joystickRollInput + gamepadRollInput, -1f, 1f);

        if (Input.GetKey(KeyCode.UpArrow) || Mathf.Abs(dronePitch) > 0.01f && dronePitch > 0)
        {
            transform.Rotate(Vector3.right * droneDynamic * Time.deltaTime);
            isApplyingForce = true;
        }

        else if (Input.GetKey(KeyCode.DownArrow) || Mathf.Abs(dronePitch) > 0.01f && dronePitch < 0)
        {
            transform.Rotate(-Vector3.right * droneDynamic * Time.deltaTime);
            isApplyingForce = true;
        }

        else if (Input.GetKey(KeyCode.LeftArrow) || Mathf.Abs(droneRoll) > 0.01f && droneRoll < 0)
        {
            transform.Rotate(Vector3.forward * droneDynamic * Time.deltaTime);
            isApplyingForce = true;
        }

        else if (Input.GetKey(KeyCode.RightArrow) || Mathf.Abs(droneRoll) > 0.01f && droneRoll > 0)
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
        // Left Stick controls altitude (W/S) and yaw (A/D)
        float joystickAltInput = joystickLeft != null ? joystickLeft.Vertical : 0f;
        float gamepadAltInput = ApplyDeadzone(Input.GetAxis("LeftStickVertical"), inputDeadzone);
        float droneAltitude = Mathf.Clamp(joystickAltInput + gamepadAltInput, -1f, 1f);
        
        // Right Stick controls pitch (Up/Down arrows) and roll (Left/Right arrows)
        float joystickPitchInput = joystickRight != null ? joystickRight.Vertical : 0f;
        float gamepadPitchInput = ApplyDeadzone(Input.GetAxis("RightStickVertical"), inputDeadzone);
        float pitch = Mathf.Clamp(joystickPitchInput + gamepadPitchInput, -1f, 1f);
        
        float joystickRollInput = joystickRight != null ? joystickRight.Horizontal : 0f;
        float gamepadRollInput = ApplyDeadzone(Input.GetAxis("RightStickHorizontal"), inputDeadzone);
        float roll = Mathf.Clamp(joystickRollInput + gamepadRollInput, -1f, 1f);

        if (Input.GetKey(KeyCode.W) && Input.GetKey(KeyCode.UpArrow) || Mathf.Abs(droneAltitude) > 0.01f && droneAltitude > 0 && Mathf.Abs(pitch) > 0.01f && pitch > 0)
        {
            droneRb.AddForce(transform.forward * droneSpeed * droneGravity, ForceMode.Acceleration);
            droneRb.AddForce(Vector3.up * droneThrust, ForceMode.Acceleration);
            isForward = true;
            isApplyingForce = true;
        }

        else if (Input.GetKey(KeyCode.W) && Input.GetKey(KeyCode.DownArrow) || Mathf.Abs(droneAltitude) > 0.01f && droneAltitude > 0 && Mathf.Abs(pitch) > 0.01f && pitch < 0)
        {
            droneRb.AddForce(transform.forward * -droneSpeed * droneGravity, ForceMode.Acceleration);
            droneRb.AddForce(Vector3.up * droneThrust, ForceMode.Acceleration);
            isForward = true;
            isApplyingForce = true;
        }

        else if (Input.GetKey(KeyCode.W) && Input.GetKey(KeyCode.RightArrow) || Mathf.Abs(droneAltitude) > 0.01f && droneAltitude > 0 && Mathf.Abs(roll) > 0.01f && roll > 0)
        {
            droneRb.AddForce(transform.right * droneSpeed * droneGravity, ForceMode.Acceleration);
            droneRb.AddForce(Vector3.up * droneThrust, ForceMode.Acceleration);
            isForward = true;
            isApplyingForce = true;
        }

        else if (Input.GetKey(KeyCode.W) && Input.GetKey(KeyCode.LeftArrow) || Mathf.Abs(droneAltitude) > 0.01f && droneAltitude > 0 && Mathf.Abs(roll) > 0.01f && roll < 0)
        {
            droneRb.AddForce(transform.right * -droneSpeed * droneGravity, ForceMode.Acceleration);
            droneRb.AddForce(Vector3.up * droneThrust, ForceMode.Acceleration);
            isForward = true;
            isApplyingForce = true;
        }
    }
    void CheckController()
    {
        // Left Stick controls altitude (W/S)
        float joystickAltInput = joystickLeft != null ? joystickLeft.Vertical : 0f;
        float gamepadAltInput = ApplyDeadzone(Input.GetAxis("LeftStickVertical"), inputDeadzone);
        float droneAltitude = Mathf.Clamp(joystickAltInput + gamepadAltInput, -1f, 1f);
        
        // Right Stick controls pitch and roll (Arrow keys)
        float joystickPitchInput = joystickRight != null ? joystickRight.Vertical : 0f;
        float gamepadPitchInput = ApplyDeadzone(Input.GetAxis("RightStickVertical"), inputDeadzone);
        float pitch = Mathf.Clamp(joystickPitchInput + gamepadPitchInput, -1f, 1f);
        
        float joystickRollInput = joystickRight != null ? joystickRight.Horizontal : 0f;
        float gamepadRollInput = ApplyDeadzone(Input.GetAxis("RightStickHorizontal"), inputDeadzone);
        float roll = Mathf.Clamp(joystickRollInput + gamepadRollInput, -1f, 1f);

        if (Input.GetKey(KeyCode.W) && Input.GetKey(KeyCode.UpArrow) || Mathf.Abs(droneAltitude) > 0.01f && droneAltitude > 0 && Mathf.Abs(pitch) > 0.01f && pitch > 0)
        {
            isForward = true;
        }

        else if (Input.GetKey(KeyCode.W) && Input.GetKey(KeyCode.DownArrow) || Mathf.Abs(droneAltitude) > 0.01f && droneAltitude > 0 && Mathf.Abs(pitch) > 0.01f && pitch < 0)
        {
            isForward = true;
        }

        else if (Input.GetKey(KeyCode.W) && Input.GetKey(KeyCode.LeftArrow) || Mathf.Abs(droneAltitude) > 0.01f && droneAltitude > 0 && Mathf.Abs(roll) > 0.01f && roll > 0)
        {
            isForward = true;
        }

        else if (Input.GetKey(KeyCode.W) && Input.GetKey(KeyCode.RightArrow) || Mathf.Abs(droneAltitude) > 0.01f && droneAltitude > 0 && Mathf.Abs(roll) > 0.01f && roll < 0)
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
