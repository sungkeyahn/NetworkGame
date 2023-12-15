using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.IO;

public class Manager_Player : MonoBehaviour
{
    public static Manager_Player Instance;

    PhotonView pv;
    #region GameContents
    public GameObject player;
    public  bool isPlayerDead;
    [SerializeField]
    int _monsterCount = 0;
    int _reserveCount = 0;
    [SerializeField]
    int _keepMonsterCount;
    [SerializeField]
    Vector3 _spawnPos;
    [SerializeField]
    float _spawnTime = 15.0f;
    #endregion

    void Awake()
    {
        if (Instance)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
        Instance = this;
        pv = GetComponent<PhotonView>();
    }
    void Start()
    {
        PhotonNetwork.IsMessageQueueRunning = true;
        if (pv.IsMine)
        {
            CreatePalyer(); 
            SetKeepMonsterCount(15);
        }
    }
    void Update()
    {
        if (pv.IsMine && !isPlayerDead)
        {
            while (_reserveCount + _monsterCount < _keepMonsterCount)
            {
                StartCoroutine("ReserveSpawn");
            }
        }
    }
    public void CreatePalyer()
    {
        GameObject player= PhotonNetwork.Instantiate("Prefabs/Player", Vector3.zero, Quaternion.identity);
        Manager_Spawn.Instance.SpawnInTerrain(player);
    }
    public void AddMonsterCount(int value) { _monsterCount += value; }
    public void SetKeepMonsterCount(int count) { _keepMonsterCount = count; }
    IEnumerator ReserveSpawn()
    {
        _reserveCount++;
        yield return new WaitForSeconds(Random.Range(0, _spawnTime));
        GameObject obj = Manager_Spawn.Instance.SpawnInTerrainAroundObject("Zombie",player,10,25);
        AddMonsterCount(1);
        _reserveCount--;
    }
    public void QuitGame()
    {
        Application.Quit();
    }
}
