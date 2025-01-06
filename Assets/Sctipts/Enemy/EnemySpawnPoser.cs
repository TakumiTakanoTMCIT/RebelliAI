using System;
using ObjectPoolFactory;
using UnityEngine;
using Zenject;

public interface IEnemyPosController
{
    void GetSpawnHandler();
    void OnBecameVisible();
    void MakeInstance();
    void ResetInstance();
}

public class EnemySpawnPoser : MonoBehaviour, IEnemyPosController
{
    [Inject]
    Lazy<EnemyBody.Factory> enemyBodyFactory;

    GameObject instance;
    ExplosionSpawner explosionSpawner;

    private void Awake() => GetSpawnHandler();

    //インターフェース実装
    public void GetSpawnHandler()
    {
        Debug.Log($"EnemyBodyFactory is {enemyBodyFactory.Value}");
        Debug.Log($"EnemyBodyFactory is {enemyBodyFactory}");

        explosionSpawner = GameObject.Find("ExplosionFactory").MyGetComponent_NullChker<ExplosionSpawner>();
    }

    //インターフェース実装
    public void MakeInstance()
    {
        if (instance == null)
        {
            instance = enemyBodyFactory.Value.Create().gameObject;
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
