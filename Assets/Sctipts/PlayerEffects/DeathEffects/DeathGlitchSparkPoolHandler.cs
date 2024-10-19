using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathGlitchSparkPoolHandler : MonoBehaviour, IPoolHandler
{
    [SerializeField] GameObject deathSparkPrefab;
    [SerializeField] public int maxInstanceCount = 20;

    //インターフェース実装-----------------------------↓↓↓
    public PoolHadnler spawnerHandler { get; set; }
    public GameObject wannaInstanceEnemy { get; set; }

    private void Awake()
    {
        wannaInstanceEnemy = deathSparkPrefab;
        spawnerHandler = new PoolHadnler(maxInstanceCount, wannaInstanceEnemy);
    }

    //インターフェース実装↓↓↓
    public GameObject GetObject()
    {
        var obj = spawnerHandler.Get();
        obj.SetActive(true);
        obj.transform.SetParent(transform);
        return obj;
    }

    public void ReturnObjct(GameObject obj)
    {
        spawnerHandler.Return(obj);
    }
}
