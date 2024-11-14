using System.Collections;
using System.Collections.Generic;
using KeyHandler;
using UnityEngine;

public class PauseHandler : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenu;

    bool isPauseShow = false;

    private void Awake()
    {
        isPauseShow = false;
        pauseMenu.SetActive(false);
    }

    private void OnEnable()
    {
        InputHandler.onPauseKeyDown += OpenPauseMenu;
    }

    private void OnDisable()
    {
        InputHandler.onPauseKeyDown -= OpenPauseMenu;
    }

    void OpenPauseMenu()
    {
        if (isPauseShow)
        {
            Debug.Log("Resume");
            pauseMenu.SetActive(false);
            isPauseShow = false;
            Time.timeScale = 1;
        }
        else
        {
            Debug.Log("Pause");
            isPauseShow = true;
            pauseMenu.SetActive(true);
            Time.timeScale = 0;
        }
    }


    //--ボタンのButtonコンポーネントから呼び出されます--//
    public void BackToGame()
    {
        if (!isPauseShow) return;

        Debug.Log("Button Resume");
        pauseMenu.SetActive(false);
        isPauseShow = false;
        Time.timeScale = 1;
    }
}
