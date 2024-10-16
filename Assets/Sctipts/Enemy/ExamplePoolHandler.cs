using UnityEngine;
using UnityEngine.Pool;

public interface IEnemySpawnerHandler
{
    SpawnerHandler spawnerHandler { get; set; }
    GameObject wannaInstanceEnemy { get; set; }
    GameObject GetEnemy();
    void ReturnEnemy(GameObject obj);
}

public class ExamplePoolHandler : MonoBehaviour, IEnemySpawnerHandler
{
    [SerializeField] private Transform parentTransform;
    [SerializeField] private int maxEnemyCount;
    [SerializeField] private GameObject enmeyPrefab;
    public GameObject wannaInstanceEnemy { get; set; }

    public SpawnerHandler spawnerHandler { get; set; }
    private void Awake()
    {
        wannaInstanceEnemy = enmeyPrefab;
        spawnerHandler = new SpawnerHandler(maxEnemyCount, wannaInstanceEnemy, parentTransform);
    }

    public GameObject GetEnemy()
    {
        return spawnerHandler.Get();
    }

    public void ReturnEnemy(GameObject obj)
    {
        spawnerHandler.Return(obj);
    }
}

public class SpawnerHandler
{
    ObjectPool<GameObject> objectPool;
    GameObject wannaInstanceEnemy;
    Transform parentTransform;
    public SpawnerHandler(int maxEnemyCount, GameObject wannaInstanceEnemy, Transform parentTransform)
    {
        this.wannaInstanceEnemy = wannaInstanceEnemy;
        this.parentTransform = parentTransform;

        objectPool = new ObjectPool<GameObject>(
            createFunc: OnCreateObj,
            actionOnGet: OnGetObj,
            actionOnRelease: OnReturnObj,
            actionOnDestroy: OnDestroyObj,
            collectionCheck: true,
            defaultCapacity: maxEnemyCount,
            maxSize: maxEnemyCount
        );

        for (int count = 0; count < maxEnemyCount; count++)
        {
            var instance = objectPool.Get();
            instance.SetActive(false);
        }
    }

    private GameObject OnCreateObj()
    {
        var instance = GameObject.Instantiate(wannaInstanceEnemy);
        instance.transform.SetParent(parentTransform);
        //instance.SetActive(false);
        return instance;
    }

    private void OnGetObj(GameObject obj)
    {
        obj.SetActive(true);
    }

    private void OnReturnObj(GameObject obj)
    {
        obj.SetActive(false);
    }

    private void OnDestroyObj(GameObject obj)
    {
        GameObject.Destroy(obj);
    }

    public GameObject Get()
    {
        return objectPool.Get();
    }

    public void Return(GameObject obj)
    {
        objectPool.Release(obj);
    }
}
