using UnityEngine;

public class StrongEnemyPosController : MonoBehaviour, IEnemyPosController
{
    StrongExamplePoolHandler spawnHandler;
    GameObject instance;
    private void Awake()
    {
        GetSpawnHandler();
    }

    //インターフェース実装
    public void GetSpawnHandler()
    {
        spawnHandler = GameObject.Find("StrongEnemyFactory").MyGetComponent_NullChker<StrongExamplePoolHandler>();
        if (spawnHandler == null)
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
            instance = spawnHandler.GetEnemy();
            instance.MyGetComponent_NullChker<StrongEnemyBody>().MyAwake(this.transform.position, transform);
        }
        else
        {
            Debug.Log($"すでに画面内に生成されているので生成しません{gameObject.name}");
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
