using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveManager : Singleton<SaveManager>
{
    private string sceneName = "currScene";

    public string SceneName
    {
        get
        {
            return PlayerPrefs.GetString(sceneName);
        }
    }

    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(this);
    }

    private void Update()
    {
        //press ESC to back to main
        if (Input.GetKeyDown(KeyCode.Escape)&&SceneManager.GetActiveScene().name!="Main")
        {
            SceneController.Instance.TransitionToMain();
        }

        //press S to save
        if (Input.GetKeyDown(KeyCode.S) && SceneManager.GetActiveScene().name != "Main")
        {
            SavePlayerData();
        }

        //press L to load
        if (Input.GetKeyDown(KeyCode.L) && SceneManager.GetActiveScene().name != "Main")
        {
            LoadPlayerData();
        }
    }

    public void SavePlayerData()
    {
        if (GameManager.Instance.playerStats == null) return;
        Save(GameManager.Instance.playerStats.characterData, GameManager.Instance.playerStats.characterData.name);
    }

    public void LoadPlayerData()
    {
        if (GameManager.Instance.playerStats == null) return;
        Load(GameManager.Instance.playerStats.characterData, GameManager.Instance.playerStats.characterData.name);
    }

    public void Save(Object data,string key)
    {
        var jsonData = JsonUtility.ToJson(data,true);
        PlayerPrefs.SetString(key, jsonData);
        PlayerPrefs.SetString(sceneName, SceneManager.GetActiveScene().name);
        PlayerPrefs.Save();
    }

    public void Load(Object data, string key)
    {
        if(PlayerPrefs.HasKey(key))
        {
            JsonUtility.FromJsonOverwrite(PlayerPrefs.GetString(key), data);

        }
    }
}
