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

    EnemySpawnPoser spawnPoser;

    ExamplePoolHandler poolHandler;
    DanboruAnimStateMgr animStateMgr;
    private void Awake()
    {
        IsAlivingNow = false;
        poolHandler = GameObject.Find("EnemyFactory").MyGetComponent_NullChker<ExamplePoolHandler>();
        animStateMgr = gameObject.MyGetComponent_NullChker<DanboruAnimStateMgr>();
    }

    //インターフェース実装------------------------------
    public void MyAwake(Vector3 position, Transform parent)
    {
        IsAlivingNow = true;
        hp = initialHp;
        transform.position = position;
        transform.parent = parent;
        spawnPoser = parent.gameObject.MyGetComponent_NullChker<EnemySpawnPoser>();

        animStateMgr.MyAwake();
    }

    //Unityから呼び出されます
    public void OnBecameInvisible()
    {
        if (!IsAlivingNow) return;
        IsAlivingNow = false;
        poolHandler.ReturnEnemy(this.gameObject);
        spawnPoser.ResetInstance();
    }

    public void TakeDamage(int damage)
    {
        if (!gameObject.activeSelf)
            return;
        hp -= damage;

        if (hp <= 0)
        {
            IsAlivingNow = false;
            hp = 0;
            poolHandler.ReturnEnemy(this.gameObject);
            spawnPoser.ResetInstance();
        }
    }
}
