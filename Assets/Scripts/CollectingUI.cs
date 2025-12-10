using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CollectingUI : MonoBehaviour
{
    private TextMeshProUGUI ringText;

    void Start()
    {
        ringText = GetComponent<TextMeshProUGUI>();
    }

    public void UpdateRingText()
    {
        ringText.text = RingFunctions.collectedRings.ToString();
    }

}
