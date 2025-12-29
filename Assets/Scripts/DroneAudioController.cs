using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class DroneAudioController : MonoBehaviour
{
    [Header("Audio Sources")]
    [SerializeField] private AudioSource propellerLBAudioSource; // Left Back
    [SerializeField] private AudioSource propellerLFAudioSource; // Left Front
    [SerializeField] private AudioSource propellerRBAudioSource; // Right Back
    [SerializeField] private AudioSource propellerRFAudioSource; // Right Front
    
    [Header("Audio Clips")]
    [SerializeField] private AudioClip propellerSound;
    
    [Header("Volume Settings")]
    [SerializeField] private float minVolume = 0.1f;
    [SerializeField] private float maxVolume = 0.8f;
    [SerializeField] private float idleVolume = 0.2f;
    
    [Header("Pitch Settings")]
    [SerializeField] private float minPitch = 0.8f;
    [SerializeField] private float maxPitch = 1.5f;
    
    [Header("Speed Thresholds")]
    [SerializeField] private float maxSpeed = 50f; // Max speed for audio scaling
    
    [Header("Movement Sensitivity")]
    [SerializeField] private float pitchSensitivity = 0.3f;
    [SerializeField] private float rollSensitivity = 0.3f;
    [SerializeField] private float yawSensitivity = 0.2f;
    [SerializeField] private float altitudeSensitivity = 0.25f;
    
    private Rigidbody droneRb;
    private DroneController droneController;
    
    // Previous rotation for calculating angular velocity effects
    private Quaternion previousRotation;
    
    void Start()
    {
        droneRb = GetComponent<Rigidbody>();
        droneController = GetComponent<DroneController>();
        previousRotation = transform.rotation;
        
        // Setup audio sources if they're assigned
        SetupAudioSource(propellerLBAudioSource);
        SetupAudioSource(propellerLFAudioSource);
        SetupAudioSource(propellerRBAudioSource);
        SetupAudioSource(propellerRFAudioSource);
    }
    
    void SetupAudioSource(AudioSource source)
    {
        if (source == null) return;
        
        source.clip = propellerSound;
        source.loop = true;
        source.volume = idleVolume;
        source.pitch = minPitch;
        source.spatialBlend = 0.5f; // Mix between 2D and 3D sound
        
        if (!source.isPlaying)
        {
            source.Play();
        }
    }
    
    void Update()
    {
        if (droneRb == null) return;
        
        // Calculate base volume from speed
        float speed = droneRb.linearVelocity.magnitude;
        float speedRatio = Mathf.Clamp01(speed / maxSpeed);
        
        // Get rotation differences for propeller intensity
        Vector3 currentEuler = transform.rotation.eulerAngles;
        
        // Calculate pitch (forward/backward tilt)
        float pitch = currentEuler.x;
        if (pitch > 180f) pitch -= 360f;
        float pitchIntensity = Mathf.Abs(pitch) / 90f * pitchSensitivity;
        
        // Calculate roll (left/right tilt)
        float roll = currentEuler.z;
        if (roll > 180f) roll -= 360f;
        float rollIntensity = Mathf.Abs(roll) / 90f * rollSensitivity;
        
        // Calculate yaw (rotation speed)
        float yawIntensity = Mathf.Abs(droneRb.angularVelocity.y) * yawSensitivity;
        
        // Calculate altitude changes
        float verticalSpeed = Mathf.Abs(droneRb.linearVelocity.y);
        float altitudeIntensity = verticalSpeed * altitudeSensitivity;
        
        // Calculate individual propeller volumes based on movement
        float baseVolume = Mathf.Lerp(idleVolume, maxVolume, speedRatio);
        float basePitch = Mathf.Lerp(minPitch, maxPitch, speedRatio * 0.5f + altitudeIntensity);
        
        // Left Back Propeller - increases with right roll and forward pitch
        float volumeLB = baseVolume;
        volumeLB += (roll > 0 ? rollIntensity : 0) * 0.3f; // Right roll
        volumeLB += (pitch < 0 ? pitchIntensity : 0) * 0.3f; // Forward pitch
        volumeLB += yawIntensity * 0.15f;
        volumeLB += altitudeIntensity * 0.2f;
        volumeLB = Mathf.Clamp(volumeLB, minVolume, maxVolume);
        
        // Left Front Propeller - increases with right roll and backward pitch
        float volumeLF = baseVolume;
        volumeLF += (roll > 0 ? rollIntensity : 0) * 0.3f; // Right roll
        volumeLF += (pitch > 0 ? pitchIntensity : 0) * 0.3f; // Backward pitch
        volumeLF += yawIntensity * 0.15f;
        volumeLF += altitudeIntensity * 0.2f;
        volumeLF = Mathf.Clamp(volumeLF, minVolume, maxVolume);
        
        // Right Back Propeller - increases with left roll and forward pitch
        float volumeRB = baseVolume;
        volumeRB += (roll < 0 ? rollIntensity : 0) * 0.3f; // Left roll
        volumeRB += (pitch < 0 ? pitchIntensity : 0) * 0.3f; // Forward pitch
        volumeRB += yawIntensity * 0.15f;
        volumeRB += altitudeIntensity * 0.2f;
        volumeRB = Mathf.Clamp(volumeRB, minVolume, maxVolume);
        
        // Right Front Propeller - increases with left roll and backward pitch
        float volumeRF = baseVolume;
        volumeRF += (roll < 0 ? rollIntensity : 0) * 0.3f; // Left roll
        volumeRF += (pitch > 0 ? pitchIntensity : 0) * 0.3f; // Backward pitch
        volumeRF += yawIntensity * 0.15f;
        volumeRF += altitudeIntensity * 0.2f;
        volumeRF = Mathf.Clamp(volumeRF, minVolume, maxVolume);
        
        // Apply volumes and pitches with smooth transitions
        if (propellerLBAudioSource != null)
        {
            propellerLBAudioSource.volume = Mathf.Lerp(propellerLBAudioSource.volume, volumeLB, Time.deltaTime * 5f);
            propellerLBAudioSource.pitch = Mathf.Lerp(propellerLBAudioSource.pitch, basePitch + volumeLB * 0.1f, Time.deltaTime * 3f);
        }
        
        if (propellerLFAudioSource != null)
        {
            propellerLFAudioSource.volume = Mathf.Lerp(propellerLFAudioSource.volume, volumeLF, Time.deltaTime * 5f);
            propellerLFAudioSource.pitch = Mathf.Lerp(propellerLFAudioSource.pitch, basePitch + volumeLF * 0.1f, Time.deltaTime * 3f);
        }
        
        if (propellerRBAudioSource != null)
        {
            propellerRBAudioSource.volume = Mathf.Lerp(propellerRBAudioSource.volume, volumeRB, Time.deltaTime * 5f);
            propellerRBAudioSource.pitch = Mathf.Lerp(propellerRBAudioSource.pitch, basePitch + volumeRB * 0.1f, Time.deltaTime * 3f);
        }
        
        if (propellerRFAudioSource != null)
        {
            propellerRFAudioSource.volume = Mathf.Lerp(propellerRFAudioSource.volume, volumeRF, Time.deltaTime * 5f);
            propellerRFAudioSource.pitch = Mathf.Lerp(propellerRFAudioSource.pitch, basePitch + volumeRF * 0.1f, Time.deltaTime * 3f);
        }
        
        previousRotation = transform.rotation;
    }
}
