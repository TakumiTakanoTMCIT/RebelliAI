using UnityEngine;

public class StrongExamplePoolHandler : MonoBehaviour, IEnemySpawnerHandler
{
    //インターフェース
    public SpawnerHandler spawnerHandler { get; set; }
    public GameObject wannaInstanceEnemy { get; set; }

    //インスペクタから弄りたい変数
    [SerializeField] private GameObject enemyprefab;
    [SerializeField] private int maxEnemyCount;
    [SerializeField] private Transform parentTransform;

    private void Awake()
    {
        wannaInstanceEnemy = enemyprefab;
        spawnerHandler = new SpawnerHandler(maxEnemyCount, wannaInstanceEnemy, parentTransform);
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
