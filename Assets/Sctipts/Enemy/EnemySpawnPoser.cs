using UnityEngine;

public class EnemySpawnPoser : MonoBehaviour
{
    [SerializeField] int initial_EnemyHp;
    EnemySpawnerHandler enemySpawnerHandler;
    GameObject instance;

    private void Awake()
    {
        enemySpawnerHandler = GameObject.Find("EnemyFactory").MyGetComponent_NullChker<EnemySpawnerHandler>();
    }

    private void OnBecameVisible()
    {
        if(instance != null && instance.activeSelf)
            return;
        instance = enemySpawnerHandler.GetEnemy();
        instance.gameObject.MyGetComponent_NullChker<EnemyBody>().MyAwake(this, transform.position, initial_EnemyHp);
    }
}
