using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;


public class MainMenu : MonoBehaviour
{
    private Button newGameBtn;
    private Button continueBtn;
    private Button tipsBtn;
    private Button quitBtn;
    private Canvas tips;

    private PlayableDirector director;


    private void Awake()
    {
        newGameBtn = transform.GetChild(1).GetComponent<Button>();
        continueBtn = transform.GetChild(2).GetComponent<Button>();
        tipsBtn = transform.GetChild(3).GetComponent<Button>();
        quitBtn = transform.GetChild(4).GetComponent<Button>();
        tips = transform.GetChild(5).GetComponent<Canvas>();
        tips.gameObject.SetActive(false);

        director = FindObjectOfType<PlayableDirector>();
        director.stopped += NewGame;

        newGameBtn.onClick.AddListener(playTimeline);
        continueBtn.onClick.AddListener(ContinueGame);
        tipsBtn.onClick.AddListener(showTips);
        quitBtn.onClick.AddListener(QuitGame);
    }

    private void playTimeline()
    {
        if (tips.gameObject.activeInHierarchy) return;
        director.Play();
    }

    private void NewGame(PlayableDirector obj)
    {
        PlayerPrefs.DeleteAll();
        //转换场景
        SceneController.Instance.TransitionToFirstLevel();

    }

    private void ContinueGame()
    {
        if (tips.gameObject.activeInHierarchy) return;
        //转换场景，读取进度
        SceneController.Instance.TransitionToContinueGame();
    }

    private void showTips()
    {
        if (tips.gameObject.activeInHierarchy) return;
        tips.gameObject.SetActive(true);
        
    }
    private void QuitGame()
    {
        if (tips.gameObject.activeInHierarchy) return;
        Application.Quit();
        Debug.Log("Quit");
    }

}
