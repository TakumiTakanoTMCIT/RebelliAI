using UnityEngine;
using UnityEngine.Pool;

public interface IPoolHandler
{
    PoolHadnler spawnerHandler { get; set; }
    GameObject wannaInstanceEnemy { get; set; }
    GameObject GetEnemy();
    void ReturnEnemy(GameObject obj);
}

public class ExamplePoolHandler : MonoBehaviour, IPoolHandler
{
    [SerializeField] private int maxEnemyCount;
    [SerializeField] private GameObject enmeyPrefab;
    public GameObject wannaInstanceEnemy { get; set; }

    public PoolHadnler spawnerHandler { get; set; }
    private void Awake()
    {
        wannaInstanceEnemy = enmeyPrefab;
        spawnerHandler = new PoolHadnler(maxEnemyCount, wannaInstanceEnemy);
    }

    //インターフェース実装
    public GameObject GetEnemy()
    {
        return spawnerHandler.Get();
    }

    //インターフェース実装
    public void ReturnEnemy(GameObject obj)
    {
        spawnerHandler.Return(obj);
    }
}

public class PoolHadnler
{
    ObjectPool<GameObject> objectPool;
    GameObject wannaInstanceEnemy;
    public PoolHadnler(int maxEnemyCount, GameObject wannaInstanceEnemy)
    {
        this.wannaInstanceEnemy = wannaInstanceEnemy;

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
            var instance = OnCreateObj();
            instance.SetActive(false);
            objectPool.Release(instance);
        }
    }

    private GameObject OnCreateObj()
    {
        var instance = GameObject.Instantiate(wannaInstanceEnemy);
        //instance.transform.SetParent(parentTransform);
        instance.SetActive(false);
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
