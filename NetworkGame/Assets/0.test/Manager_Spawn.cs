using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;
using Photon.Realtime;
using UnityEngine.AI;

public class Manager_Spawn : MonoBehaviour
{
    public static Manager_Spawn Instance;
    void Awake()
    {
        if (Instance)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
        Instance = this;
    }
    #region Resource
    T Load<T>(string path) where T : UnityEngine.Object
    {
        if (typeof(T) == typeof(GameObject))
        {
            string name = path;
            int index = name.LastIndexOf('/');
            if (index >= 0) name = name.Substring(index + 1);
            GameObject go = GetOriginal(name);
            if (go != null) return go as T;
        }
        return Resources.Load<T>(path);
    }
    GameObject Instantiate(string path, Transform parent = null)
    {
        GameObject original = Load<GameObject>($"Prefabs/{path}");
        if (original == null)
        {
            Debug.Log($"Failed to load prefab : {path}");
            return null;
        }

        if (original.GetComponent<Poolable>() != null)
            return Pop(original, parent).gameObject;

        GameObject go = PhotonNetwork.Instantiate($"Prefabs/{path}", new Vector3(0, 0, 0), Quaternion.identity, 0);
        go.name = original.name;
        return go;
    }
    void Destroy(GameObject go)
    {
        if (go == null) return;
        Poolable poolable = go.GetComponent<Poolable>();
        if (poolable != null)
        {
            Push(poolable);
            return;
        }
        PhotonNetwork.Destroy(go);
    }
    #endregion

    #region Pool
    class Pool
    {
        public GameObject Original { get; private set; }
        public Transform Root { get; set; }

        Stack<Poolable> _poolStack = new Stack<Poolable>();

        public void Init(GameObject original, int count = 5)
        {
            Original = original;
            Root = new GameObject().transform;
            Root.name = $"{original.name}_Root";

            for (int i = 0; i < count; i++)
                Push(Create());
        }
        Poolable Create()
        {
            GameObject go = UnityEngine.Object.Instantiate<GameObject>(Original);
            go.name = Original.name;
            return go.GetOrAddComponent<Poolable>();
        }
        public void Push(Poolable poolable)
        {
            if (poolable == null) return;

            poolable.transform.parent = Root;
            poolable.gameObject.SetActive(false);
            poolable.IsUsing = false;

            _poolStack.Push(poolable);
        }
        public Poolable Pop(Transform parent)
        {
            Poolable poolable;

            if (_poolStack.Count > 0)
                poolable = _poolStack.Pop();
            else
                poolable = Create();

            poolable.gameObject.SetActive(true);

            // DontDestroyOnLoad 해제 용 코드
            if (parent == null)
                poolable.transform.parent = GameObject.Find("Manager").transform;

            poolable.transform.parent = parent;
            poolable.IsUsing = true;

            return poolable;
        }
    }
    Dictionary<string, Pool> _pool = new Dictionary<string, Pool>();
    Transform _root;
    public void Init()
    {
        if (_root == null)
        {
            _root = new GameObject { name = "@Pool_Root" }.transform;
            UnityEngine.Object.DontDestroyOnLoad(_root);
        }
    }
    public void Clear()
    {
        foreach (Transform child in _root)
            GameObject.Destroy(child.gameObject);

        _pool.Clear();
    }
    GameObject GetOriginal(string name)
    {
        if (_pool.ContainsKey(name) == false)
            return null;
        return _pool[name].Original;
    }
    void CreatePool(GameObject original, int count = 5)
    {
        Pool pool = new Pool();
        pool.Init(original, count);
        pool.Root.parent = _root;
        _pool.Add(original.name, pool);
    }
    void Push(Poolable poolable)
    {
        string name = poolable.gameObject.name;
        if (_pool.ContainsKey(name) == false)
        {
            GameObject.Destroy(poolable.gameObject);
            return;
        }

        _pool[name].Push(poolable);
    }
    Poolable Pop(GameObject original, Transform parent = null)
    {
        if (_pool.ContainsKey(original.name) == false)
            CreatePool(original);

        return _pool[original.name].Pop(parent);
    }
    #endregion

    #region Spawn
    public GameObject Spawn(string path,Vector3 spawnPos, Transform parent = null)
    {
        GameObject go = Instantiate(path, parent);
        go.transform.position = spawnPos;
        return go;
    }
    public GameObject SpawnInTerrain(GameObject go, float minDistance = 2.0f, Transform parent = null)
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
                Debug.Log("SpawnPositionFound");
            }
        }

        go.transform.position = spawnPosition;
        return go;
    }
    public GameObject SpawnInTerrainAroundObject(string path, GameObject referenceObject, float minDistance = 2.0f, float maxDistance = 4.0f, Transform parent = null)
    {
        Terrain terrain = Terrain.activeTerrain;
        if (!terrain || !referenceObject)
        {
            Debug.Log("Terrain or Reference Object not found");
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
            float offsetX = referenceObject.transform.position.x+UnityEngine.Random.Range(-minDistance, maxDistance);
            float offsetZ = referenceObject.transform.position.z+UnityEngine.Random.Range(-minDistance, maxDistance);
            float randomX = Mathf.Clamp(offsetX,0, terrainWidth);
            float randomZ = Mathf.Clamp(offsetZ,0, terrainLength);
            // 터레인 높이
            float terrainHeight = terrain.SampleHeight(new Vector3(randomX, 0, randomZ));
            spawnPosition = new Vector3(randomX, terrainHeight, randomZ);
            // 다른 오브젝트와의 거리를 확인
            bool isClose = false;
            Collider[] colliders = Physics.OverlapSphere(spawnPosition, 7.0f);
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
                Debug.Log("SpawnPositionFound");
            }
        }
        GameObject go = Spawn(path, spawnPosition, parent);
        return go;
    }
    public void Despawn(GameObject go)
    {
        Destroy(go);
    }
    Vector3 GetRandomNavMeshPosition()
    {
        NavMeshHit hit;
        Vector3 randomPosition = Vector3.zero;
        int maxAttempts = 10;
        for (int i = 0; i < maxAttempts; i++)
        {
            float radius = 10f; // 적절한 반지름 설정
            Vector3 randomPoint = transform.position + Random.insideUnitSphere * radius;
            if (NavMesh.SamplePosition(randomPoint, out hit, radius, NavMesh.AllAreas))
            {
                randomPosition = hit.position;
                break;
            }
        }
        return randomPosition;
    }
    #endregion

}
