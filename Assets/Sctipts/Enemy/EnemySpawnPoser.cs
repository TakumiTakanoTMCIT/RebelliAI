using UnityEngine;

public interface IEnemyPosController
{
    void GetSpawnHandler();
    void OnBecameVisible();
    void MakeInstance();
    void ResetInstance();
}

public class EnemySpawnPoser : MonoBehaviour, IEnemyPosController
{
    IPoolHandler enemySpawnerHandler;
    GameObject instance;
    ExplosionSpawner explosionSpawner;

    private void Awake() => GetSpawnHandler();

    //インターフェース実装
    public void GetSpawnHandler()
    {
        explosionSpawner = GameObject.Find("ExplosionFactory").MyGetComponent_NullChker<ExplosionSpawner>();
        enemySpawnerHandler = GameObject.Find("EnemyFactory").GetComponent<IPoolHandler>();
        if (enemySpawnerHandler == null)
        {
            Debug.Log("enemySpaenerHandlerがnullです");
        }
    }

    //インターフェース実装
    public void MakeInstance()
    {
        if (instance == null)
        {
            instance = enemySpawnerHandler.GetObject();
            instance.gameObject.MyGetComponent_NullChker<EnemyBody>().MyAwake(transform.position, transform, explosionSpawner);
            return;
        }
        else return;
    }

    //Unityから呼び出されます(インターフェース実装)
    public void OnBecameVisible() => MakeInstance();

    //インターフェース実装
    public void ResetInstance() => instance = null;
}
