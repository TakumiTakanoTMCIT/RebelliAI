using UnityEngine;

public class GarbageCanEnemyPoser : MonoBehaviour, IEnemyPosController
{
    ExplosionSpawner explosionSpawner;
    GarbageCanPoolHandler poolHandler;
    GameObject instance;
    private void Awake()
    {
        GetSpawnHandler();
    }

    //インターフェース実装
    public void GetSpawnHandler()
    {
        explosionSpawner = GameObject.Find("ExplosionFactory").MyGetComponent_NullChker<ExplosionSpawner>();
        poolHandler = GameObject.Find("GarbageCanFactory").MyGetComponent_NullChker<GarbageCanPoolHandler>();
        if (poolHandler == null)
        {
            Debug.Log("SpawnerHandlerがnullです" + gameObject);
        }
    }

    //Unityから呼び出されます
    public void OnBecameVisible()
    {
        MakeInstance();
    }

    //インターフェース実装
    public void MakeInstance()
    {
        if (instance == null)
        {
            instance = poolHandler.GetObject();
            instance.MyGetComponent_NullChker<GarbageCanEnemyBody>().MyAwake(this.transform.position, transform, explosionSpawner);
            return;
        }
        else
        {
            Debug.Log($"すでに生成されているので生成しません{gameObject.name}");
            return;
        }
    }

    /// <summary>
    /// ギズモを表示します。デバッグ用
    /// </summary>
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position, 0.5f);
    }

    //インターフェース実装
    public void ResetInstance()
    {
        instance = null;
    }
}
