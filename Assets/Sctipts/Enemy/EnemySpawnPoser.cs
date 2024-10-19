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
    IEnemySpawnerHandler enemySpawnerHandler;
    GameObject instance;

    private void Awake()
    {
        GetSpawnHandler();
    }

    //インターフェース実装
    public void GetSpawnHandler()
    {
        enemySpawnerHandler = GameObject.Find("EnemyFactory").GetComponent<IEnemySpawnerHandler>();
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
            instance = enemySpawnerHandler.GetEnemy();
            instance.gameObject.MyGetComponent_NullChker<EnemyBody>().MyAwake(transform.position, transform);
            return;
        }
        else
        {
            Debug.Log($"instanceはすでに生成されていて、画面内にいるから生成しません!{gameObject.name}");
            return;
        }
    }

    //Unityから呼び出されます(インターフェース実装)
    public void OnBecameVisible()
    {
        Debug.Log($"表示された！ : {gameObject.name}");
        MakeInstance();
    }

    //インターフェース実装
    public void ResetInstance()
    {
        instance = null;
    }
}
