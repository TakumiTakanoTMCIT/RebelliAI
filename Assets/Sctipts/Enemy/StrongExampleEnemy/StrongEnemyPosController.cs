using UnityEngine;

public class StrongEnemyPosController : MonoBehaviour, IEnemyPosController
{
    StrongExamplePoolHandler spawnHandler;
    GameObject instance;
    private void Awake()
    {
        GetSpawnHandler();
    }

    public void GetSpawnHandler()
    {
        spawnHandler = GameObject.Find("StrongEnemyFactory").MyGetComponent_NullChker<StrongExamplePoolHandler>();
        if (spawnHandler == null)
        {
            Debug.Log("SpawnerHandlerがnullです" + gameObject);
        }
    }

    public void OnBecameVisible()
    {
        Debug.Log("表示された！");
        MakeInstance();
    }

    public void MakeInstance()
    {
        /*if (instance != null) return;
        if (!instance.activeInHierarchy) return;*/
        instance = spawnHandler.GetEnemy();
        instance.MyGetComponent_NullChker<StrongEnemyBody>().MyAwake(this.transform.position, transform);
    }

    /// <summary>
    /// ギズモを表示します。デバッグ用
    /// </summary>
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position, 0.5f);
    }
}
