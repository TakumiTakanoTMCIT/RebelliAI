using UnityEngine;

public class GarbageCanEnemyBody : MonoBehaviour, IDamageableFromShot, IPrefabEnemyBody
{
    [SerializeField] int hp = 5, initialHp = 5;
    public bool IsAlivingNow { get; set; }
    GarbageCanPoolHandler poolHandler;
    GarbageCanEnemyPoser posController;

    private void Awake()
    {
        IsAlivingNow = false;
        poolHandler = GameObject.Find("GarbageCanFactory").MyGetComponent_NullChker<GarbageCanPoolHandler>();
    }

    public void MyAwake(Vector3 pos, Transform parent)
    {
        hp = initialHp;
        IsAlivingNow = true;
        transform.position = pos;
        transform.parent = parent;
        posController = parent.gameObject.MyGetComponent_NullChker<GarbageCanEnemyPoser>();
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
            poolHandler.ReturnEnemy(gameObject);
            posController.ResetInstance();
        }
    }

    public void OnBecameInvisible()
    {
        if (!IsAlivingNow) return;
        IsAlivingNow = false;
        poolHandler.ReturnEnemy(gameObject);
        posController.ResetInstance();
    }
}
