using UnityEngine;
using TMPro;

public class QuestUIItem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI questNameText;
    [SerializeField] private TextMeshProUGUI questDescriptionText;
    [SerializeField] private TextMeshProUGUI questProgressText;
    
    private Quest quest;
    
    public void Initialize(Quest questData)
    {
        quest = questData;
        if (questNameText != null)
            questNameText.text = quest.questName;
        if (questDescriptionText != null)
            questDescriptionText.text = quest.questDescription;
    }
    
    public void UpdateDisplay()
    {
        if (quest == null) return;
        
        // Update progress text
        if (questProgressText != null)
        {
            questProgressText.text = quest.GetProgressText();
            
            if (quest.timeLimit > 0 && !quest.isCompleted && !quest.isFailed)
            {
                float timeRemaining = quest.timeLimit - quest.progressTimer;
                questProgressText.text += $" | {timeRemaining:F0}s";
            }
        }
        
        // Update text colors and strikethrough
        if (questNameText != null)
        {
            if (quest.isCompleted)
            {
                questNameText.text = "✓ " + quest.questName;
                questNameText.color = Color.green;
                questNameText.fontStyle = FontStyles.Strikethrough;
            }
            else if (quest.isFailed)
            {
                questNameText.text = "✗ " + quest.questName;
                questNameText.color = Color.red;
                questNameText.fontStyle = FontStyles.Normal;
            }
            else
            {
                questNameText.color = Color.white;
                questNameText.fontStyle = FontStyles.Normal;
            }
        }
        
        if (questDescriptionText != null)
        {
            if (quest.isCompleted)
            {
                questDescriptionText.color = Color.green;
                questDescriptionText.fontStyle = FontStyles.Strikethrough;
            }
            else if (quest.isFailed)
            {
                questDescriptionText.color = Color.red;
                questDescriptionText.fontStyle = FontStyles.Normal;
            }
            else
            {
                questDescriptionText.color = Color.white;
                questDescriptionText.fontStyle = FontStyles.Normal;
            }
        }
        
        if (questProgressText != null)
        {
            if (quest.isCompleted)
            {
                questProgressText.color = Color.green;
                questProgressText.fontStyle = FontStyles.Strikethrough;
            }
            else if (quest.isFailed)
            {
                questProgressText.color = Color.red;
                questProgressText.fontStyle = FontStyles.Normal;
            }
            else
            {
                questProgressText.color = Color.white;
                questProgressText.fontStyle = FontStyles.Normal;
            }
        }
    }
}
