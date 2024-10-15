using UnityEngine;

public class EnemyBody : MonoBehaviour, IDamageableFromShot
{
    [SerializeField]
    private int hp = 3;
    bool isAlivingNow;

    EnemySpawnerHandler spawnerHandler;

    private void Awake()
    {
        isAlivingNow = false;
        spawnerHandler = GameObject.Find("EnemyFactory").MyGetComponent_NullChker<EnemySpawnerHandler>();
    }

    public void MyAwake(EnemySpawnPoser spawnPoser, Vector3 position, int initialHp)
    {
        Debug.LogAssertion("MyAwake");
        isAlivingNow = true;
        transform.position = position;
        hp = initialHp;
    }

    private void OnBecameInvisible()
    {
        if (!isAlivingNow)
            return;
        isAlivingNow = false;

        Debug.Log("EnemyInvisible");
        spawnerHandler.ReturnEnemy(this.gameObject);
    }

    //インターフェース実装
    public void TakeDamage(int damage)
    {
        if (!isAlivingNow)
            return;

        hp -= damage;
        Debug.Log("TakeDamage");
        if (hp <= 0)
        {
            Debug.LogWarning("ReturnEnemy");
            isAlivingNow = false;
            hp = 0;
            spawnerHandler.ReturnEnemy(this.gameObject);
        }
    }
}
