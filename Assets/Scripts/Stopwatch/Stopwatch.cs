using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class Stopwatch : MonoBehaviour
{
    public TextMeshProUGUI timerText;
    private float elapsedTime;
    private bool timerActive;

    private void Start()
    {
        timerActive = true;
        elapsedTime = 0f;
        UpdateTimer();
    }

    private void Update()
    {
        if (timerActive)
        {
            elapsedTime += Time.deltaTime;
            UpdateTimer();
        }
    }

    void UpdateTimer()
    {
        TimeSpan time = TimeSpan.FromSeconds(elapsedTime);

        timerText.text = time.ToString(@"mm\:ss\:fff");
    }
    
}
