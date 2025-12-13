using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class QuestUIPanel : MonoBehaviour
{
    [Header("Quest Display")]
    [SerializeField] private GameObject questItemPrefab;
    [SerializeField] private Transform questContainer;
    
    [Header("Quest Summary")]
    [SerializeField] private TextMeshProUGUI questSummaryText;
    
    private List<QuestUIItem> questUIItems = new List<QuestUIItem>();
    private bool isInitialized = false;
    
    void Start()
    {
        if (QuestManager.Instance != null)
        {
            // Subscribe to quest generation event
            QuestManager.Instance.OnQuestsGenerated += InitializeQuestUI;
        }
    }
    
    void OnDestroy()
    {
        if (QuestManager.Instance != null)
        {
            // Unsubscribe from event
            QuestManager.Instance.OnQuestsGenerated -= InitializeQuestUI;
        }
    }
    
    void Update()
    {
        UpdateQuestDisplay();
    }
    
    private void InitializeQuestUI()
    {
        Debug.Log("Initializing Quest UI...");
        
        // Clear existing UI items
        foreach (var item in questUIItems)
        {
            if (item != null && item.gameObject != null)
            {
                Destroy(item.gameObject);
            }
        }
        questUIItems.Clear();
        
        // Create UI items for each quest
        List<Quest> quests = QuestManager.Instance.GetActiveQuests();
        Debug.Log($"Found {quests.Count} quests to display");
        
        foreach (var quest in quests)
        {
            if (questItemPrefab == null)
            {
                Debug.LogError("QuestItemPrefab is not assigned!");
                return;
            }
            
            if (questContainer == null)
            {
                Debug.LogError("QuestContainer is not assigned!");
                return;
            }
            
            GameObject questObj = Instantiate(questItemPrefab, questContainer);
            Debug.Log($"Created quest item: {quest.questName}");
            
            QuestUIItem uiItem = questObj.GetComponent<QuestUIItem>();
            if (uiItem != null)
            {
                uiItem.Initialize(quest);
                questUIItems.Add(uiItem);
            }
            else
            {
                Debug.LogError("QuestUIItem component not found on prefab!");
            }
        }
        
        Debug.Log($"Quest UI initialized with {questUIItems.Count} items");
    }
    
    private void UpdateQuestDisplay()
    {
        if (QuestManager.Instance != null)
        {
            // Update summary
            if (questSummaryText != null)
            {
                int completed = QuestManager.Instance.GetCompletedQuestsCount();
                int total = QuestManager.Instance.GetTotalQuestsCount();
                questSummaryText.text = $"Quests: {completed}/{total}";
            }
            
            // Update individual quest items
            foreach (var item in questUIItems)
            {
                if (item != null)
                {
                    item.UpdateDisplay();
                }
            }
        }
    }
}

