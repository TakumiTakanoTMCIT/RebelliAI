using UnityEngine;

[RequireComponent(typeof(ConflictPlayerFinder))]
public class GarbageCanEnemyBody : MonoBehaviour, IDamageableFromShot, IPrefabEnemyBody,IConflictableAndAttackableToPlayer
{
    //インターフェース実装--------------------
    [SerializeField] int hp = 5, initialHp = 5,conflictDamage = 1;

    public bool IsAlivingNow { get; set; }
    GarbageCanPoolHandler poolHandler;
    GarbageCanEnemyPoser posController;
    ExplosionSpawner explosionSpawner;

    //インターフェース実装--------------------
    public ConflictPlayerFinder conflictPlayerFinder { get; set; }

    private void Awake()
    {
        IsAlivingNow = false;
        poolHandler = GameObject.Find("GarbageCanFactory").MyGetComponent_NullChker<GarbageCanPoolHandler>();

        conflictPlayerFinder = gameObject.MyGetComponent_NullChker<ConflictPlayerFinder>();
        conflictPlayerFinder.Init(conflictDamage);
    }

    public void MyAwake(Vector3 pos, Transform parent, ExplosionSpawner explosionSpawner)
    {
        hp = initialHp;
        IsAlivingNow = true;
        transform.position = pos;
        transform.parent = parent;
        posController = parent.gameObject.MyGetComponent_NullChker<GarbageCanEnemyPoser>();
        this.explosionSpawner = explosionSpawner;
    }

    //インターフェース実装--------------------
    public void TakeDamage(int damage)
    {
        if (!gameObject.activeSelf)
            return;
        hp -= damage;

        if (hp <= 0)
        {
            hp = 0;
            IsAlivingNow = false;
            poolHandler.ReturnObjct(gameObject);
            posController.ResetInstance();
            explosionSpawner.MakeExplosion(new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z));
        }
    }

    public void OnBecameInvisible()
    {
        if (!IsAlivingNow) return;
        IsAlivingNow = false;
        poolHandler.ReturnObjct(gameObject);
        posController.ResetInstance();
    }
}
