using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManagerEx
{
    GameObject _player;
    //Dictionary<int, GameObject> _players = new Dictionary<int, GameObject>();
    HashSet<GameObject> _monsters = new HashSet<GameObject>();

    public Action<int> OnSpawnEvent;

    public GameObject GetPlayer() { return _player; }
    public Define.WorldObject GetWorldObjectType(GameObject go)
    {
        BaseController bc = go.GetComponent<BaseController>();
        if (bc == null)
            return Define.WorldObject.Unknown;

        return bc.WorldObjectType;
    }

    public GameObject Spawn(Define.WorldObject type, string path,Vector3 pos, Transform parent = null)
    {
        GameObject go = Spawn(type, path, parent);
        go.transform.position = pos;
        return go;
    }
    public GameObject Spawn(Define.WorldObject type, string path, Transform parent = null)
    {
        GameObject go = Managers.Resource.Instantiate(path, parent);
        switch (type)
        {
            case Define.WorldObject.Monster:
                _monsters.Add(go);
                if (OnSpawnEvent != null)
                    OnSpawnEvent.Invoke(1);
                break;
            case Define.WorldObject.Player:
                _player = go;
                break;
        }
        return go;
    }
    public GameObject SpawnInTerrain(Define.WorldObject type, string path, Transform parent = null, float minDistance = 2.0f)
    {  
        Terrain terrain = Terrain.activeTerrain;
        if (!terrain)
        {
            Debug.Log("Not Terrain");
            return null;
        }
        TerrainData terrainData = terrain.terrainData;     
        float terrainWidth = terrainData.size.x;
        float terrainLength = terrainData.size.z;
        bool positionFound = false;
        Vector3 spawnPosition = Vector3.zero;
        // 무한 루프로 겹치지 않는 위치 찾기
        while (!positionFound)
        {
            //랜덤 좌표 지정
            float randomX = UnityEngine.Random.Range(0, terrainWidth);
            float randomZ = UnityEngine.Random.Range(0, terrainLength);
            // 터레인 높이
            float terrainHeight = terrain.SampleHeight(new Vector3(randomX, 0, randomZ));
            // 터레인의 높이를 고려한 스폰 위치 설정
            spawnPosition = new Vector3(randomX, terrainHeight, randomZ);
            // 다른 오브젝트와의 거리를 확인
            bool isClose = false;
            Collider[] colliders = Physics.OverlapSphere(spawnPosition, minDistance);
            foreach (Collider collider in colliders)
            {
                if (collider.gameObject.name != terrain.gameObject.name)
                {
                    isClose = true;
                    break;
                }
            }
            if (!isClose)
            {
                positionFound = true;
                //Debug.Log("SpawnPositionFound");
            }
        }

        GameObject go = Spawn(type, path, spawnPosition, parent);
        return go;
    }
    public void Despawn(GameObject go)
    {
        Define.WorldObject type = GetWorldObjectType(go);

        switch (type)
        {
            case Define.WorldObject.Monster:
                {
                    if (_monsters.Contains(go))
                    {
                        _monsters.Remove(go);
                        if (OnSpawnEvent != null)
							OnSpawnEvent.Invoke(-1);
					}   
                }
                break;
            case Define.WorldObject.Player:
                {
					if (_player == go)
						_player = null;
				}
                break;
        }

        Managers.Resource.Destroy(go);
    }
}
