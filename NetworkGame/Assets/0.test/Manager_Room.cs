using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;
using System.IO;

public class Manager_Room : MonoBehaviourPunCallbacks
{
    public static Manager_Room Instance;
    public bool isConnect=false;
    private void Awake()
    {
        if (Instance)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
        Instance = this;
    }
    public override void OnEnable()
    {
        base.OnEnable();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    public override void OnDisable()
    {
        base.OnDisable();
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    IEnumerator SceneLoaded()
    {
        yield return new WaitUntil(() => isConnect);
        PhotonNetwork.Instantiate(Path.Combine("Prefabs", "PlayerManager"), Vector3.zero, Quaternion.identity);
    }
    void OnSceneLoaded(Scene scene,LoadSceneMode loadSceneMode)
    {
        if (scene.buildIndex==1)
        {
            StartCoroutine(SceneLoaded());
        }   
    }


}
