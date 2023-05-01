using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Playables;

public class MainMenu : MonoBehaviour
{
    Button newGameBtn;
    Button continueBtn;
    Button quitBtn;

    PlayableDirector director;

    private void Awake()
    {
        newGameBtn = transform.GetChild(1).GetComponent<Button>();
        continueBtn = transform.GetChild(2).GetComponent<Button>();
        quitBtn = transform.GetChild(3).GetComponent<Button>();

        newGameBtn.onClick.AddListener(NewGame);
        continueBtn.onClick.AddListener(ContinueGame);
        quitBtn.onClick.AddListener(QuitGame);

        //director = FindObjectOfType<PlayableDirector>();
        //director.stopped += NewGame;
    }

    //void PlayTimeline()
    //{
    //    director.Play();
    //}

    void NewGame()
    {
        PlayerPrefs.DeleteAll();
        //load forest scene
        GameManager.Instance.curLevel = 1;
        SceneController.Instance.TransitionToLevel(GameManager.Instance.curLevel);
    }

    void ContinueGame()
    {
        //load forest scene
        SceneController.Instance.TransitionToLoadGame();
    }

    void QuitGame()
    {
        Application.Quit();
    }
}
