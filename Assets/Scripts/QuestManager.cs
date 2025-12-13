using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;
using Resources;
using static EventList;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }
    
    // Event to notify when quests are generated
    public System.Action OnQuestsGenerated;
    
    [Header("Quest Settings")]
    [SerializeField] private int questsPerRun = 3;
    
    [Header("UI References")]
    [SerializeField] private GameObject questUIPanel;
    
    private List<Quest> activeQuests = new List<Quest>();
    private List<Quest> availableQuestTemplates = new List<Quest>();
    
    // Quest tracking variables
    private int proximityWarningCount = 0;
    private float maxSpeedReached = 0f;
    private int ringsCollectedThisRun = 0;
    private bool hasCrashed = false;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        InitializeQuestTemplates();
        GenerateRandomQuests();
    }
    
    void Update()
    {
        // Update quest timers
        foreach (var quest in activeQuests)
        {
            if (!quest.isCompleted && !quest.isFailed)
            {
                quest.UpdateTimer(Time.deltaTime);
            }
        }
        
        // Check if all quests are completed
        CheckAllQuestsCompleted();
    }
    
    private bool allQuestsCompletedEventSent = false;
    
    private void CheckAllQuestsCompleted()
    {
        if (allQuestsCompletedEventSent || activeQuests.Count == 0) return;
        
        bool allCompleted = true;
        foreach (var quest in activeQuests)
        {
            if (!quest.isCompleted)
            {
                allCompleted = false;
                break;
            }
        }
        
        if (allCompleted)
        {
            allQuestsCompletedEventSent = true;
            Debug.Log("All quests completed! Sending event to Analytics...");
            EventBus<QuestCompleted>.Emit(this, new QuestCompleted());
        }
    }
    
    private void InitializeQuestTemplates()
    {
        availableQuestTemplates.Clear();
        
        // Collect Rings with time limit
        availableQuestTemplates.Add(new Quest(QuestType.CollectRings, 5, 50));
        availableQuestTemplates.Add(new Quest(QuestType.CollectRings, 3, 30));
        availableQuestTemplates.Add(new Quest(QuestType.CollectRings, 8, 60));
        
        // Reach Speed
        availableQuestTemplates.Add(new Quest(QuestType.ReachSpeed, 20, 0));
        availableQuestTemplates.Add(new Quest(QuestType.ReachSpeed, 30, 0));
        availableQuestTemplates.Add(new Quest(QuestType.ReachSpeed, 40, 0));
        
        // Proximity Warnings
        availableQuestTemplates.Add(new Quest(QuestType.ProximityWarnings, 5, 0));
        availableQuestTemplates.Add(new Quest(QuestType.ProximityWarnings, 3, 0));
        availableQuestTemplates.Add(new Quest(QuestType.ProximityWarnings, 8, 0));
        
        // Maintain Altitude
        availableQuestTemplates.Add(new Quest(QuestType.MaintainAltitude, 10, 20));
        availableQuestTemplates.Add(new Quest(QuestType.MaintainAltitude, 15, 30));
        availableQuestTemplates.Add(new Quest(QuestType.MaintainAltitude, 20, 25));
        
        // Complete Without Crash
        availableQuestTemplates.Add(new Quest(QuestType.CompleteWithoutCrash, 1, 0));
        
        // Collect Rings Total (no time limit)
        availableQuestTemplates.Add(new Quest(QuestType.CollectRingsTotal, 10, 0));
        availableQuestTemplates.Add(new Quest(QuestType.CollectRingsTotal, 15, 0));
        
        // Battery Saver
        availableQuestTemplates.Add(new Quest(QuestType.AvoidBatteryDrain, 50, 30));
        availableQuestTemplates.Add(new Quest(QuestType.AvoidBatteryDrain, 70, 45));
    }
    
    public void GenerateRandomQuests()
    {
        activeQuests.Clear();
        allQuestsCompletedEventSent = false; // Reset flag for new quest set
        
        // Shuffle and pick random quests
        List<Quest> shuffled = availableQuestTemplates.OrderBy(x => Random.value).ToList();
        
        for (int i = 0; i < Mathf.Min(questsPerRun, shuffled.Count); i++)
        {
            // Create a new instance of the quest
            Quest newQuest = new Quest(
                shuffled[i].questType,
                shuffled[i].targetValue,
                shuffled[i].timeLimit
            );
            activeQuests.Add(newQuest);
        }
        
        // Reset tracking variables
        proximityWarningCount = 0;
        maxSpeedReached = 0f;
        ringsCollectedThisRun = 0;
        hasCrashed = false;
        
        Debug.Log($"Generated {activeQuests.Count} quests for this run");
        
        // Emit QuestStarted event for analytics
        EventBus<QuestStarted>.Emit(this, new QuestStarted());
        
        // Notify listeners that quests are ready
        OnQuestsGenerated?.Invoke();
    }
    
    // Called by DroneController when rings are collected
    public void OnRingCollected()
    {
        ringsCollectedThisRun++;
        
        foreach (var quest in activeQuests)
        {
            if (quest.questType == QuestType.CollectRings || quest.questType == QuestType.CollectRingsTotal)
            {
                quest.IncrementProgress();
            }
        }
    }
    
    // Called by DroneController or proximity system
    public void OnProximityWarning()
    {
        proximityWarningCount++;
        
        foreach (var quest in activeQuests)
        {
            if (quest.questType == QuestType.ProximityWarnings)
            {
                quest.UpdateProgress(proximityWarningCount);
            }
        }
    }
    
    // Called by DroneController with current speed
    public void UpdateSpeed(float speed)
    {
        if (speed > maxSpeedReached)
        {
            maxSpeedReached = speed;
        }
        
        foreach (var quest in activeQuests)
        {
            if (quest.questType == QuestType.ReachSpeed)
            {
                quest.UpdateProgress(maxSpeedReached);
            }
        }
    }
    
    // Called by DroneController with current altitude
    public void UpdateAltitude(float altitude)
    {
        foreach (var quest in activeQuests)
        {
            if (quest.questType == QuestType.MaintainAltitude)
            {
                if (altitude >= quest.targetValue)
                {
                    quest.IncrementProgress(Time.deltaTime);
                }
                else
                {
                    // Reset progress if altitude drops below target
                    quest.currentProgress = 0;
                    quest.progressTimer = 0;
                }
            }
        }
    }
    
    // Called by DroneController with current battery level
    public void UpdateBattery(float batteryLevel)
    {
        foreach (var quest in activeQuests)
        {
            if (quest.questType == QuestType.AvoidBatteryDrain)
            {
                if (batteryLevel >= quest.targetValue)
                {
                    quest.IncrementProgress(Time.deltaTime);
                }
                else
                {
                    // Reset progress if battery drops below target
                    quest.currentProgress = 0;
                    quest.progressTimer = 0;
                }
            }
        }
    }
    
    // Called when drone crashes
    public void OnCrash()
    {
        hasCrashed = true;
        
        foreach (var quest in activeQuests)
        {
            if (quest.questType == QuestType.CompleteWithoutCrash)
            {
                quest.isFailed = true;
            }
        }
    }

    public int GetCompletedQuestsCount()
    {
        return activeQuests.Count(q => q.isCompleted);
    }
    
    public int GetTotalQuestsCount()
    {
        return activeQuests.Count;
    }
    
    public List<Quest> GetActiveQuests()
    {
        return new List<Quest>(activeQuests);
    }
    
    // Call this to restart quests for a new run
    public void ResetQuests()
    {
        GenerateRandomQuests();
    }
}
