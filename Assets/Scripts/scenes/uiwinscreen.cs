using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class uiwinscreen : MonoBehaviour
{
    public Button mainMenu;
    public Button playAgain;

    private void Start()
    {
        mainMenu.onClick.AddListener(StartMainMenu);
        playAgain.onClick.AddListener(StartPlayAgain);
    }

    void StartMainMenu()
    {
        sceneManager.Instance.LoadMainMenu();
    }

    void StartPlayAgain()
    {
        sceneManager.Instance.LoadNewGame();
    }
}
