using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Playables;

public class MainMenu : MonoBehaviour
{
    public Button newGameBtn;
    public Button continueBtn;
    public Button quitBtn;

    PlayableDirector director;

    private void Awake()
    {
        newGameBtn.onClick.AddListener(NewGame);
        continueBtn.onClick.AddListener(ContinueGame);
        quitBtn.onClick.AddListener(QuitGame);
    }

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
