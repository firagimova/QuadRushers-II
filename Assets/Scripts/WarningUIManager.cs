using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class WarningUIManager : MonoBehaviour
{
    [Header("Warning UI Components")]
    [SerializeField] private Image warningOverlay; // Red overlay image with material
    
    [Header("Warning Settings")]
    [SerializeField] private float blinkDuration = 2f;
    [SerializeField] private float blinkSpeed = 0.5f;
    [SerializeField] private Color warningColor = new Color(1f, 0f, 0f, 0.7f);
    
    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip warningSound;
    [SerializeField] private float audioVolume = 1f;
    
    private Sequence blinkSequence;
    
    private void Awake()
    {
        if (warningOverlay != null)
        {
            // Start fully transparent
            Color transparent = warningColor;
            transparent.a = 0f;
            warningOverlay.color = transparent;
        }
        
        // Automatically create AudioSource if not assigned
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            
            // If still null, add one
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                // Configure for UI sounds
                audioSource.playOnAwake = false;
                audioSource.spatialBlend = 0f; // 2D sound
                audioSource.volume = audioVolume;
            }
        }
    }
    
    public void ShowWarning()
    {
        // Play warning sound
        PlayWarningSound();
        
        if (blinkSequence != null && blinkSequence.IsActive())
        {
            blinkSequence.Kill();
        }
        
        blinkSequence = DOTween.Sequence();
        int blinkCount = Mathf.CeilToInt(blinkDuration / (blinkSpeed * 2));
        
        if (warningOverlay != null)
        {
            for (int i = 0; i < blinkCount; i++)
            {
                blinkSequence.Append(warningOverlay.DOFade(warningColor.a, blinkSpeed).SetEase(Ease.InOutSine));
                blinkSequence.Append(warningOverlay.DOFade(0f, blinkSpeed).SetEase(Ease.InOutSine));
            }
        }
        
        blinkSequence.OnComplete(() =>
        {
            if (warningOverlay != null)
            {
                Color transparent = warningColor;
                transparent.a = 0f;
                warningOverlay.color = transparent;
            }
        });
        
        blinkSequence.Play();
    }
    
    private void PlayWarningSound()
    {
        if (warningSound == null)
        {
            Debug.LogWarning("Warning sound clip is not assigned!");
            return;
        }
        
        if (audioSource != null)
        {
            // If AudioSource is assigned, use it
            audioSource.PlayOneShot(warningSound, audioVolume);
        }
        else
        {
            // Otherwise, play at the position of this GameObject
            AudioSource.PlayClipAtPoint(warningSound, transform.position, audioVolume);
        }
    }
    
    private void OnDestroy()
    {
        if (blinkSequence != null && blinkSequence.IsActive())
        {
            blinkSequence.Kill();
        }
    }
}
