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
    //Inject
    EnemyBodyFactory enemyBodyFactory;

    EnemyBody instance;
    ExplosionSpawner explosionSpawner;

    [Inject]
    public void Construct(EnemyBodyFactory enemyBodyFactory)
    {
        this.enemyBodyFactory = enemyBodyFactory;
    }

    private void Awake() => GetSpawnHandler();

    //インターフェース実装
    public void GetSpawnHandler()
    {
        explosionSpawner = GameObject.Find("ExplosionFactory").MyGetComponent_NullChker<ExplosionSpawner>();
    }

    //インターフェース実装
    public void MakeInstance()
    {
        if (instance == null)
        {
            instance = enemyBodyFactory.Create();
            instance.MyAwake(transform.position, transform, explosionSpawner);
            return;
        }
        else return;
    }

    //Unityから呼び出されます(インターフェース実装)
    public void OnBecameVisible() => MakeInstance();

    //インターフェース実装
    public void ResetInstance() => instance = null;
}
