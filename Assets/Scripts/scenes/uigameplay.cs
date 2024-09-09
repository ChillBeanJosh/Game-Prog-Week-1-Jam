using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class uigameplay : MonoBehaviour
{

    public Button restart;
    public Button returnToMenu;

    private void Start()
    {
        restart.onClick.AddListener(StartReload);
        returnToMenu.onClick.AddListener(StartMainMenu);
    }

    void StartReload()
    {
        sceneManager.Instance.RestartScene();
    }

    void StartMainMenu()
    {
        sceneManager.Instance.LoadMainMenu();
    }
}
