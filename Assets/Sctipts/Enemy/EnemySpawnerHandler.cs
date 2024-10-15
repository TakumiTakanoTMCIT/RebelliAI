using UnityEngine;
using UnityEngine.Pool;

public class EnemySpawnerHandler : MonoBehaviour
{
    [SerializeField] private int maxEnemyCount;
    [SerializeField] private GameObject enemyPrefab;
    ObjectPool<GameObject> objectPool;

    private void Awake()
    {
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
            var obj = objectPool.Get();
            obj.SetActive(false);
        }
    }

    private GameObject OnCreateObj()
    {
        GameObject obj = Instantiate(enemyPrefab);
        obj.SetActive(false);
        return obj;
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
        Destroy(obj);
    }

    public GameObject GetEnemy()
    {
        var obj = objectPool.Get();
        return obj;
    }

    public void ReturnEnemy(GameObject obj)
    {
        objectPool.Release(obj);
    }
}
