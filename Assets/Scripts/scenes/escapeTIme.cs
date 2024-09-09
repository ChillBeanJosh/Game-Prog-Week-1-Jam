using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class escapeTIme : MonoBehaviour
{
    public TextMeshProUGUI finalTimeText;
    public TextMeshProUGUI fastestTimeText;


    private void Start()
    {
        finalTimeText.text = Stopwatch.finalTime;

        // Load the fastest time from PlayerPrefs
        float fastestTime = PlayerPrefs.GetFloat("FastestTime", float.MaxValue);
        if (fastestTime == float.MaxValue)
        {
            fastestTimeText.text = "No record yet"; // Handle the case where there's no record
        }
        else
        {
            TimeSpan fastestTimeSpan = TimeSpan.FromSeconds(fastestTime);
            fastestTimeText.text = fastestTimeSpan.ToString(@"mm\:ss\:fff");
        }
    }
}

