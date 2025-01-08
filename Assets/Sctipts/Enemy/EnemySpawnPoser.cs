using UnityEngine;
using Zenject;
using Enemy;
using ObjectPoolFactory;

public interface IEnemyPosController
{
    void GetSpawnHandler();
    void OnBecameVisible();
    void MakeInstance();
    void ResetInstance();
}

public class EnemySpawnPoser : MonoBehaviour, IEnemyPosController
{
    //Inject
    DanboruPool danboruPool;

    GameObject instance;
    ExplosionSpawner explosionSpawner;

    [Inject]
    public void Construct(DanboruPool danboruPool)
    {
        this.danboruPool = danboruPool;
    }

    private void Awake() => GetSpawnHandler();

    //インターフェース実装
    public void GetSpawnHandler()
    {
        explosionSpawner = GameObject.Find("ExplosionFactory").MyGetComponent_NullChker<ExplosionSpawner>();
    }

    private bool IsAbleToMakeInstance()
    {
        //一度も生成したことがないなら生成する
        if(instance == null)
        {
            return true;
        }

        //画面上に残っているのならば生成しない
        if(instance.activeSelf)
        {
            return false;
        }

        //画面外に出たら生成する
        return true;
    }

    //インターフェース実装
    public void MakeInstance()
    {
        //生成できるかどうか
        if(!IsAbleToMakeInstance()) return;

        Debug.Log("MakeInstance");
        instance = danboruPool.GetObject();
        instance.GetComponent<EnemyBody>().MyAwake(transform.position, transform, explosionSpawner);
    }

    //Unityから呼び出されます(インターフェース実装)
    public void OnBecameVisible() => MakeInstance();

    //インターフェース実装
    public void ResetInstance() => instance = null;
}
