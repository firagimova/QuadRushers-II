using System;
using UnityEngine;

[System.Serializable]
public enum QuestType
{
    CollectRings,           // Collect X rings in Y seconds
    ReachSpeed,             // Reach X km/h speed
    ProximityWarnings,      // Trigger less than X proximity warnings
    MaintainAltitude,       // Maintain altitude above X meters for Y seconds
    CompleteWithoutCrash,   // Complete the run without crashing
    CollectRingsTotal,      // Collect X rings total (no time limit)
    AvoidBatteryDrain       // Keep battery above X% for Y seconds
}

[System.Serializable]
public class Quest
{
    public string questName;
    public string questDescription;
    public QuestType questType;
    public float targetValue;       // e.g., 5 rings, 20 km/h, 5 warnings
    public float timeLimit;         // Time limit in seconds (0 = no limit)
    public bool isCompleted;
    public bool isFailed;
    
    // Progress tracking
    public float currentProgress;
    public float progressTimer;
    
    public Quest(QuestType type, float target, float time = 0f)
    {
        questType = type;
        targetValue = target;
        timeLimit = time;
        isCompleted = false;
        isFailed = false;
        currentProgress = 0f;
        progressTimer = 0f;
        
        GenerateQuestDetails();
    }
    
    private void GenerateQuestDetails()
    {
        switch (questType)
        {
            case QuestType.CollectRings:
                questName = "Ring Collector";
                if (timeLimit > 0)
                    questDescription = $"Collect {targetValue} rings in {timeLimit} seconds";
                else
                    questDescription = $"Collect {targetValue} rings";
                break;
                
            case QuestType.ReachSpeed:
                questName = "Speed Demon";
                questDescription = $"Reach {targetValue} km/h speed";
                break;
                
            case QuestType.ProximityWarnings:
                questName = "Careful Pilot";
                questDescription = $"Trigger less than {targetValue} proximity warnings";
                break;
                
            case QuestType.MaintainAltitude:
                questName = "High Flyer";
                questDescription = $"Maintain altitude above {targetValue}m for {timeLimit} seconds";
                break;
                
            case QuestType.CompleteWithoutCrash:
                questName = "Perfect Run";
                questDescription = "Complete the run without crashing";
                break;
                
            case QuestType.CollectRingsTotal:
                questName = "Ring Master";
                questDescription = $"Collect {targetValue} rings";
                break;
                
            case QuestType.AvoidBatteryDrain:
                questName = "Battery Saver";
                questDescription = $"Keep battery above {targetValue}% for {timeLimit} seconds";
                break;
        }
    }
    
    public void UpdateProgress(float value)
    {
        currentProgress = value;
        CheckCompletion();
    }
    
    public void IncrementProgress(float amount = 1f)
    {
        currentProgress += amount;
        CheckCompletion();
    }
    
    public void UpdateTimer(float deltaTime)
    {
        if (timeLimit > 0 && !isCompleted && !isFailed)
        {
            progressTimer += deltaTime;
            if (progressTimer >= timeLimit)
            {
                if (currentProgress < targetValue)
                {
                    isFailed = true;
                }
            }
        }
    }
    
    private void CheckCompletion()
    {
        if (!isCompleted && !isFailed)
        {
            switch (questType)
            {
                case QuestType.CollectRings:
                case QuestType.CollectRingsTotal:
                case QuestType.ReachSpeed:
                    if (currentProgress >= targetValue)
                    {
                        isCompleted = true;
                    }
                    break;
                    
                case QuestType.ProximityWarnings:
                    if (currentProgress >= targetValue)
                    {
                        isFailed = true;
                    }
                    break;
                    
                case QuestType.MaintainAltitude:
                case QuestType.AvoidBatteryDrain:
                    if (timeLimit > 0 && progressTimer >= timeLimit && currentProgress >= targetValue)
                    {
                        isCompleted = true;
                    }
                    break;
            }
        }
    }
    
    public float GetProgressPercentage()
    {
        if (questType == QuestType.ProximityWarnings)
        {
            return Mathf.Clamp01(1f - (currentProgress / targetValue));
        }
        return Mathf.Clamp01(currentProgress / targetValue);
    }
    
    public string GetProgressText()
    {
        switch (questType)
        {
            case QuestType.CollectRings:
            case QuestType.CollectRingsTotal:
                return $"{currentProgress}/{targetValue}";
                
            case QuestType.ReachSpeed:
                return $"{currentProgress:F0}/{targetValue} km/h";
                
            case QuestType.ProximityWarnings:
                return $"{currentProgress}/{targetValue} warnings";
                
            case QuestType.MaintainAltitude:
                return $"{progressTimer:F0}/{timeLimit}s";
                
            case QuestType.AvoidBatteryDrain:
                return $"{progressTimer:F0}/{timeLimit}s";
                
            default:
                return $"{currentProgress}/{targetValue}";
        }
    }
}
