using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveManager : Singleton<SaveManager>
{
    string sceneName = "sceneName";

    public string SceneName
    {
        get { return PlayerPrefs.GetString(sceneName); }
    }


    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(this);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SceneController.Instance.TransitionToMain();
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            SavePlayerData();
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            LoadPlayerData();
        }
    }

    public void SavePlayerData()
    {
        Save(GameManager.Instance.playerStats.characterData, GameManager.Instance.playerStats.characterData.name);
    }

    public void LoadPlayerData()
    {
        Load(GameManager.Instance.playerStats.characterData, GameManager.Instance.playerStats.characterData.name);
    }

    public void SaveLeadData(Dictionary<string, bool> data, string key)
    {
        var jsonData = JsonConvert.SerializeObject(data, Formatting.Indented);
        PlayerPrefs.SetString(key, jsonData);
        PlayerPrefs.Save();
    }

    public void LoadLeadData(Dictionary<string, bool> data, string key)
    {
        if (PlayerPrefs.HasKey(key))
        {
            JsonUtility.FromJsonOverwrite(PlayerPrefs.GetString(key), data);
        }
    }

    public void Save(Object data,string key)
    {
        //convert object to json
        var jsonData = JsonUtility.ToJson(data,true);
        //convert json to string and save to disk
        PlayerPrefs.SetString(key, jsonData);
        PlayerPrefs.SetString(sceneName, SceneManager.GetActiveScene().name);
        PlayerPrefs.Save();
    }

    public void Load(Object data,string key)
    {
        if (PlayerPrefs.HasKey(key))
        {
            //load string from disk
            //convert string to object
            JsonUtility.FromJsonOverwrite(PlayerPrefs.GetString(key), data);
        }
    }
}
