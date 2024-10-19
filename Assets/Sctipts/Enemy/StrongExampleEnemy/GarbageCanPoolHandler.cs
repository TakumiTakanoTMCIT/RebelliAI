using UnityEngine;

public class GarbageCanPoolHandler : MonoBehaviour, IPoolHandler
{
    //インターフェース
    public PoolHadnler spawnerHandler { get; set; }
    public GameObject wannaInstanceEnemy { get; set; }

    //インスペクタから弄りたい変数
    [SerializeField] private GameObject enemyprefab;
    [SerializeField] private int maxEnemyCount;
    [SerializeField] private Transform parentTransform;

    private void Awake()
    {
        wannaInstanceEnemy = enemyprefab;
        spawnerHandler = new PoolHadnler(maxEnemyCount, wannaInstanceEnemy);
    }

    public GameObject GetEnemy()
    {
        return spawnerHandler.Get();
    }

    public void ReturnEnemy(GameObject obj)
    {
        spawnerHandler.Return(obj);
    }
}
