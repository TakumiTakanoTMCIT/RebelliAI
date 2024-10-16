using UnityEngine;

public class StrongEnemyBody : MonoBehaviour, IDamageableFromShot, IPrefabEnemyBody
{
    [SerializeField] int hp = 5, initialHp = 5;
    public bool IsAlivingNow { get; set; }
    StrongExamplePoolHandler spawnHandler;

    private void Awake()
    {
        IsAlivingNow = false;
        spawnHandler = GameObject.Find("StrongEnemyFactory").MyGetComponent_NullChker<StrongExamplePoolHandler>();
    }

    public void MyAwake(Vector3 pos, Transform parent)
    {
        hp = initialHp;
        IsAlivingNow = true;
        transform.position = pos;
        transform.parent = parent;
    }

    //インターフェース実装--------------------
    public void TakeDamage(int damage)
    {
        hp -= damage;
        if (hp <= 0)
        {
            hp = 0;
            if (!IsAlivingNow) return;
            IsAlivingNow = false;
            spawnHandler.ReturnEnemy(gameObject);
        }
    }

    public void OnBecameInvisible()
    {
        if (!IsAlivingNow) return;
        IsAlivingNow = false;
        spawnHandler.ReturnEnemy(gameObject);
    }
}
