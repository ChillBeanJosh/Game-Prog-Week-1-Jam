using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.SceneManagement;


public class Stopwatch : MonoBehaviour
{
    public TextMeshProUGUI timerText;
    private float elapsedTime;
    private bool timerActive;
    public LayerMask stopwatchEnder;
    public static string finalTime;
    private static float fastestTime;

    private void Start()
    {
        timerActive = true;
        elapsedTime = 0f;
        UpdateTimer();

        fastestTime = PlayerPrefs.GetFloat("FastestTime", float.MaxValue);
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

    private void OnCollisionEnter(Collision collision)
    {
        if (((1 << collision.gameObject.layer) & stopwatchEnder) != 0) 
        {
            timerActive = false;
            SaveTime();
            UpdateFastest();
            sceneManager.Instance.LoadScene(sceneManager.Scene.winScreen);
        }
    }

    void SaveTime()
    {
        TimeSpan time = TimeSpan.FromSeconds(elapsedTime);
        finalTime = time.ToString(@"mm\:ss\:fff");
    }

    void UpdateFastest()
    {
        if ( elapsedTime < fastestTime)
        {
            fastestTime = elapsedTime;
            PlayerPrefs.SetFloat("FastestTime", fastestTime);
        }
    }

}
