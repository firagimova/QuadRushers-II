using System;
using Resources;
using UnityEngine;
using static EventList;

public class AnalyticsManager : MonoSingleton<AnalyticsManager>
{
    private int warningCount = 0;      
    private int crashCount = 0;        
    private float questStartTime;      
    private bool isQuestActive = false; 
    private void OnEnable()
    {
        EventBus<QuestStarted>.AddListener(HandleQuestStarted);
        EventBus<QuestCompleted>.AddListener(HandleQuestFinished);

        EventBus<DetectionWarning>.AddListener(HandleProximityWarning);
        EventBus<DroneCrashed>.AddListener(HandleDroneCrash);
    }

    private void OnDisable()
    {
        EventBus<QuestStarted>.RemoveListener(HandleQuestStarted);
        EventBus<QuestCompleted>.RemoveListener(HandleQuestFinished);

        EventBus<DetectionWarning>.RemoveListener(HandleProximityWarning);
        EventBus<DroneCrashed>.RemoveListener(HandleDroneCrash);
    }

    private void HandleQuestStarted(object sender, QuestStarted @event)
    {
        isQuestActive = true;
        warningCount = 0;
        crashCount = 0;
        questStartTime = Time.time; 
        Debug.Log("Analytics started");
    }

    private void HandleProximityWarning(object sender, DetectionWarning @event)
    {
        if (isQuestActive)
        {
            warningCount++;
        }
    }

    private void HandleDroneCrash(object sender, DroneCrashed @event)
    {
        if (isQuestActive)
        {
            crashCount++;
        }
    }

    private void HandleQuestFinished(object sender, QuestCompleted @event)
    {
        if (!isQuestActive) return;

        isQuestActive = false;
        float totalDuration = Time.time - questStartTime; 

        // report
        Debug.Log("============ Mission Report ============");
        Debug.Log($"Total Time: {totalDuration:F2} seconds");
        Debug.Log($"Total Warning: {warningCount}");
        Debug.Log($"Total Crash: {crashCount}");

        // calculate score
        float baseScore = 1000f;
        float finalScore = baseScore - (crashCount * 200) - (warningCount * 50) - (totalDuration * 2);
        Debug.Log($"Performance Point: {Mathf.Max(0, finalScore):F0}");
        Debug.Log("===========================================");
    }
}