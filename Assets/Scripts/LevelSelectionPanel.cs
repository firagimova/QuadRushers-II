using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class LevelSelectionPanel : MonoBehaviour
{
    [Header("Level Data")]
    [SerializeField] private LevelsDataSO levelsData;
    
    [Header("UI References")]
    [SerializeField] private GameObject panelObject;
    [SerializeField] private Image levelImageDisplay;
    [SerializeField] private TextMeshProUGUI levelNameText;
    [SerializeField] private Button previousButton;
    [SerializeField] private Button nextButton;
    [SerializeField] private Button playButton;
    [SerializeField] private Button backButton;

    private int currentLevelIndex = 0;

    private void Start()
    {
        // Hide panel initially
        panelObject.SetActive(false);

        // Setup button listeners
        if (previousButton != null)
            previousButton.onClick.AddListener(PreviousLevel);
        
        if (nextButton != null)
            nextButton.onClick.AddListener(NextLevel);
        
        if (playButton != null)
            playButton.onClick.AddListener(PlaySelectedLevel);
        
        if (backButton != null)
            backButton.onClick.AddListener(ClosePanel);
    }

    public void OpenPanel()
    {
        panelObject.SetActive(true);
        currentLevelIndex = 0;
        UpdateLevelDisplay();
    }

    public void ClosePanel()
    {
        panelObject.SetActive(false);
        MainMenuController mainMenu = FindFirstObjectByType<MainMenuController>();
        if (mainMenu != null)
        {
            mainMenu.ReturnToMainMenu();
        }
    }

    private void PreviousLevel()
    {
        if (levelsData == null || levelsData.LevelCount == 0) return;
        
        currentLevelIndex--;
        if (currentLevelIndex < 0)
        {
            currentLevelIndex = levelsData.LevelCount - 1;
        }
        UpdateLevelDisplay();
    }

    private void NextLevel()
    {
        if (levelsData == null || levelsData.LevelCount == 0) return;
        
        currentLevelIndex++;
        if (currentLevelIndex >= levelsData.LevelCount)
        {
            currentLevelIndex = 0;
        }
        UpdateLevelDisplay();
    }

    private void UpdateLevelDisplay()
    {
        if (levelsData == null)
        {
            Debug.LogError("Levels Data ScriptableObject is not assigned!");
            return;
        }

        if (levelsData.LevelCount == 0)
        {
            Debug.LogWarning("No levels configured in Levels Data!");
            return;
        }

        LevelData currentLevel = levelsData.GetLevel(currentLevelIndex);
        
        if (currentLevel == null)
        {
            Debug.LogError($"Could not get level at index {currentLevelIndex}");
            return;
        }

        // Update image
        if (levelImageDisplay != null && currentLevel.levelImage != null)
        {
            levelImageDisplay.sprite = currentLevel.levelImage;
        }

        // Update level name
        if (levelNameText != null)
        {
            levelNameText.text = currentLevel.levelName;
        }

        // Update arrow button states (optional: disable if only one level)
        if (levelsData.LevelCount <= 1)
        {
            if (previousButton != null) previousButton.interactable = false;
            if (nextButton != null) nextButton.interactable = false;
        }
        else
        {
            if (previousButton != null) previousButton.interactable = true;
            if (nextButton != null) nextButton.interactable = true;
        }
    }

    private void PlaySelectedLevel()
    {
        if (levelsData == null)
        {
            Debug.LogError("Levels Data ScriptableObject is not assigned!");
            return;
        }

        if (levelsData.LevelCount == 0 || currentLevelIndex >= levelsData.LevelCount)
        {
            Debug.LogError("No valid level selected!");
            return;
        }

        LevelData selectedLevel = levelsData.GetLevel(currentLevelIndex);
        
        if (selectedLevel == null)
        {
            Debug.LogError($"Could not get level at index {currentLevelIndex}");
            return;
        }

        string sceneToLoad = selectedLevel.sceneName;
        
        if (string.IsNullOrEmpty(sceneToLoad))
        {
            Debug.LogError("Scene name is empty for selected level!");
            return;
        }

        Debug.Log($"Loading scene: {sceneToLoad}");
        SceneManager.LoadScene(sceneToLoad);
    }
}
