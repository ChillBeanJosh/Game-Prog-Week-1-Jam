using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class uihowtoplay : MonoBehaviour
{

    public Button returnToMainMenu;

    private void Start()
    {
        returnToMainMenu.onClick.AddListener(StartMainMenu);
    }

    void StartMainMenu()
    {
        sceneManager.Instance.LoadMainMenu();
    }

}
