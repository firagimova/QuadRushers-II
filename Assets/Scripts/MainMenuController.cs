using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private Button startButton;
    [SerializeField] private LevelSelectionPanel levelSelectionPanel;
        
    private void Awake()
    {
        startButton.onClick.AddListener(OpenLevelSelection);
    }

    private void OpenLevelSelection()
    {
        if (levelSelectionPanel != null)
        {
            levelSelectionPanel.OpenPanel();
            startButton.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogError("Level Selection Panel is not assigned!");
        }
    }

    public void ReturnToMainMenu()
    {
        startButton.gameObject.SetActive(true);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
