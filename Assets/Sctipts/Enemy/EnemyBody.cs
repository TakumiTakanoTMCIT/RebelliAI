using UnityEngine;

public interface IConflictableAndAttackableToPlayer
{
    ConflictPlayerFinder conflictPlayerFinder { get; set; }
}

public interface IDamageableFromShot
{
    bool IsAlivingNow { get; set; }
    void TakeDamage(int damage);
}

public interface IPrefabEnemyBody
{
    void OnBecameInvisible();
    void MyAwake(Vector3 position, Transform parent, ExplosionSpawner explosionSpawner);
}

[RequireComponent(typeof(ConflictPlayerFinder))]
public class EnemyBody : MonoBehaviour, IDamageableFromShot, IPrefabEnemyBody,IConflictableAndAttackableToPlayer
{
    //インターフェース実装--------------------
    public ConflictPlayerFinder conflictPlayerFinder { get; set; }

    [SerializeField] int initialHp = 3;
    [SerializeField] private int hp = 3;
    [SerializeField] private int conflictDamage = 1;
    public bool IsAlivingNow { get; set; }

    EnemySpawnPoser spawnPoser;
    ExplosionSpawner explosionSpawner;

    ExamplePoolHandler poolHandler;
    DanboruAnimStateMgr animStateMgr;
    private void Awake()
    {
        IsAlivingNow = false;
        poolHandler = GameObject.Find("EnemyFactory").MyGetComponent_NullChker<ExamplePoolHandler>();
        animStateMgr = gameObject.MyGetComponent_NullChker<DanboruAnimStateMgr>();

        conflictPlayerFinder = gameObject.MyGetComponent_NullChker<ConflictPlayerFinder>();
        conflictPlayerFinder.Init(conflictDamage);
    }

    //インターフェース実装------------------------------
    public void MyAwake(Vector3 position, Transform parent, ExplosionSpawner explosionSpawner)
    {
        IsAlivingNow = true;
        hp = initialHp;
        transform.position = position;
        transform.parent = parent;
        spawnPoser = parent.gameObject.MyGetComponent_NullChker<EnemySpawnPoser>();
        this.explosionSpawner = explosionSpawner;

        animStateMgr.MyAwake();
    }

    //Unityから呼び出されます
    public void OnBecameInvisible()
    {
        if (!IsAlivingNow) return;
        IsAlivingNow = false;
        poolHandler.ReturnObjct(this.gameObject);
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
            poolHandler.ReturnObjct(this.gameObject);
            spawnPoser.ResetInstance();
            explosionSpawner.MakeExplosion(transform.position);
        }
    }
}
