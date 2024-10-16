using UnityEngine;

public interface IEnemyPosController
{
    void GetSpawnHandler();
    void OnBecameVisible();
    void MakeInstance();
}

public class EnemySpawnPoser : MonoBehaviour, IEnemyPosController
{
    IEnemySpawnerHandler enemySpawnerHandler;
    GameObject instance;

    private void Awake()
    {
        GetSpawnHandler();
    }

    public void GetSpawnHandler()
    {
        enemySpawnerHandler = GameObject.Find("EnemyFactory").GetComponent<IEnemySpawnerHandler>();
        if (enemySpawnerHandler == null)
        {
            Debug.Log("enemySpaenerHandlerがnullです");
        }
    }

    public void MakeInstance()
    {
        /*if(instance == null) return;
        if (instance.activeSelf) return;*/
        instance = enemySpawnerHandler.GetEnemy();
        instance.gameObject.MyGetComponent_NullChker<EnemyBody>().MyAwake(transform.position, transform);
    }

    public void ReturnGameObject(GameObject obj)
    {
        enemySpawnerHandler.ReturnEnemy(obj);
    }

    public void OnBecameVisible()
    {
        Debug.Log("表示された！");
        MakeInstance();
    }
}
