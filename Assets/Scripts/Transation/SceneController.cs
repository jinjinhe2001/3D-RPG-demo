using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class SceneController : Singleton<SceneController>,IEndGameObserver
{
    public GameObject playerPrefab;
    public SceneFader sceneFaderPrefab;

    private bool isfadeFinished;

    private GameObject player;
    private NavMeshAgent playerAgent;

    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(this);
    }

    private void Start()
    {
        GameManager.Instance.AddObserver(this);
        isfadeFinished = true;
    }

    public void TransationToDestination(TransitionPoint transitionPoint)
    {
        switch (transitionPoint.transitionType)
        {
            case TransitionPoint.TransitionType.SameScene:
                StartCoroutine(Transation(SceneManager.GetActiveScene().name,transitionPoint.desitnationTag));
                break;
            case TransitionPoint.TransitionType.DifferentScene:
                StartCoroutine(Transation(transitionPoint.sceneName, transitionPoint.desitnationTag));
                break;
        }
    }



    IEnumerator Transation(string sceneName, TransitionDestination.DesitnationTag desitnationTag)
    {
        //TODO:保存数据
        SaveManager.Instance.SavePlayerData();

        if (SceneManager.GetActiveScene().name != sceneName)
        {
            SceneFader fade = Instantiate(sceneFaderPrefab);

            yield return StartCoroutine(fade.FadeOut(1f));
            yield return SceneManager.LoadSceneAsync(sceneName);

            //创建角色
            yield return Instantiate(playerPrefab, GetDestination(desitnationTag).transform.position, GetDestination(desitnationTag).transform.rotation);
            //读取数据
            SaveManager.Instance.LoadPlayerData();

            yield return StartCoroutine(fade.FadeIn(1.5f));
            yield break;
        }
        else
        {
            player = GameManager.Instance.playerStats.gameObject;
            playerAgent = player.GetComponent<NavMeshAgent>();

            playerAgent.enabled = false;
            player.transform.SetPositionAndRotation(GetDestination(desitnationTag).transform.position, GetDestination(desitnationTag).transform.rotation);
            playerAgent.enabled = true;

            yield return null;
        }
    }

    private TransitionDestination GetDestination(TransitionDestination.DesitnationTag desitnationTag)
    {
        var entrance = FindObjectsOfType<TransitionDestination>();
        for (int i = 0; i < entrance.Length; i++) 
        {
            if (entrance[i].desitnationTag == desitnationTag)
                return entrance[i];
        }
        return null;
    }

    /// <summary>
    /// new game
    /// </summary>
    public void TransitionToFirstLevel()
    {
        isfadeFinished = true;
        StartCoroutine(LoadScene("Forest"));
    }

    /// <summary>
    /// continue
    /// </summary>
    public void TransitionToContinueGame()
    {
        isfadeFinished = true;
        StartCoroutine(LoadScene(SaveManager.Instance.SceneName));
    }

    /// <summary>
    /// back to mainmenu
    /// </summary>
    public void TransitionToMain()
    {
        StartCoroutine(LoadMain());
    }

    IEnumerator LoadScene(string scene)
    {
        SceneFader fade = Instantiate(sceneFaderPrefab);

        if (scene != "")
        {
            yield return StartCoroutine(fade.FadeOut(1f));
            yield return SceneManager.LoadSceneAsync(scene);
            yield return player = Instantiate(playerPrefab, GameManager.Instance.GetEntrance().position, GameManager.Instance.GetEntrance().rotation);

            SaveManager.Instance.SavePlayerData();

            yield return StartCoroutine(fade.FadeIn(1.5f));
            yield break;
        }
    }

    IEnumerator LoadMain()
    {
        SceneFader fade = Instantiate(sceneFaderPrefab);

        yield return StartCoroutine(fade.FadeOut(1f));
        yield return SceneManager.LoadSceneAsync("Main");

        yield return StartCoroutine(fade.FadeIn(1.5f));
        yield break;
    }

    public void EndNotify()
    {
        if (isfadeFinished)
        {
            isfadeFinished = false;
            StartCoroutine(LoadMain());
        }
    }
}