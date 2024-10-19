using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionPoolMgr : MonoBehaviour, IPoolHandler
{
    [SerializeField] GameObject explosionPrefab;
    [SerializeField] int maxInstanceCount = 10;

    //インターフェース実装-----------------------------↓↓↓
    public PoolHadnler spawnerHandler { get; set; }
    public GameObject wannaInstanceEnemy { get; set; }

    //初期化のために使用してください。インスタンスの取得以外はStartを使用してください。
    private void Awake()
    {
        wannaInstanceEnemy = explosionPrefab;
        spawnerHandler = new PoolHadnler(maxInstanceCount, wannaInstanceEnemy);
    }

    //インターフェース実装-----------------------------↓↓↓

    public GameObject GetEnemy()
    {
        return spawnerHandler.Get();
    }

    public void ReturnEnemy(GameObject obj)
    {
        spawnerHandler.Return(obj);
    }
}
