using UnityEngine;

public interface IDamageableFromShot
{
    bool IsAlivingNow { get; set; }
    void TakeDamage(int damage);
}

public interface IPrefabEnemyBody
{
    void OnBecameInvisible();
    void MyAwake(Vector3 position, Transform parent);
}

public class EnemyBody : MonoBehaviour, IDamageableFromShot, IPrefabEnemyBody
{
    [SerializeField] int initialHp = 3;

    [SerializeField]
    private int hp = 3;
    public bool IsAlivingNow { get; set; }

    ExamplePoolHandler poolHandler;
    private void Awake()
    {
        IsAlivingNow = false;
        poolHandler = GameObject.Find("EnemyFactory").MyGetComponent_NullChker<ExamplePoolHandler>();
    }

    //インターフェース実装------------------------------
    public void MyAwake(Vector3 position, Transform parent)
    {
        IsAlivingNow = true;
        hp = initialHp;
        transform.position = position;
        transform.parent = parent;
    }

    public void OnBecameInvisible()
    {
        if (!IsAlivingNow) return;
        IsAlivingNow = false;
        poolHandler.ReturnEnemy(this.gameObject);
    }

    public void TakeDamage(int damage)
    {
        if (!IsAlivingNow)
            return;
        hp -= damage;

        if (hp <= 0)
        {
            IsAlivingNow = false;
            hp = 0;
            poolHandler.ReturnEnemy(this.gameObject);
        }
    }
}
