using UnityEngine;

public class EnemyBody : MonoBehaviour, IDamageableFromShot
{
    [SerializeField]
    private int hp = 3;
    bool isAlive;

    EnemySpawnerHandler spawnerHandler;

    private void Awake()
    {
        spawnerHandler = GameObject.Find("EnemyFactory").MyGetComponent_NullChker<EnemySpawnerHandler>();
    }

    public void MyAwake(EnemySpawnPoser spawnPoser, Vector3 position, int initialHp)
    {
        isAlive = true;
        transform.position = position;
        hp = initialHp;
    }

    private void OnBecameInvisible()
    {
        if (isAlive) ReturnEnemy();
    }

    //自作関数
    private void ReturnEnemy()
    {
        if (gameObject != null)//←この状況ってありえるのか？しらんけど追加しとけば安定するっしょ笑笑
            spawnerHandler.ReturnEnemy(this.gameObject);
    }

    //インターフェース実装
    public void TakeDamage(int damage)
    {
        hp -= damage;
        if (hp <= 0)
        {
            hp = 0;
            isAlive = false;
            ReturnEnemy();
        }
    }
}
