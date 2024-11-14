using System;
using UnityEngine;

public interface IAttackableAndNotConflictable
{
    TriggerEnterPlayerFinder playerFinder{get; set;}
}

[RequireComponent(typeof(TriggerEnterPlayerFinder))]
public class MissileBody : MonoBehaviour, IDamageableFromShot
{
    [SerializeField] MissileStatus missileStatus;

    public TriggerEnterPlayerFinder playerFinder{get; set;}
    public bool IsAlivingNow { get; set; }

    Rigidbody2D rb;
    private void Awake()
    {
        IsAlivingNow = false;
        rb = gameObject.MyGetComponent_NullChker<Rigidbody2D>();
        playerFinder = gameObject.MyGetComponent_NullChker<TriggerEnterPlayerFinder>();
        playerFinder.Init(missileStatus.damage, this);
    }

    private void OnBecameVisible()
    {
        IsAlivingNow = true;
        rb.AddForce(missileStatus.missileForce, ForceMode2D.Impulse);
    }

    private void OnBecameInvisible()
    {
        Dead();
    }

    public void TakeDamage(int damage)
    {
        Dead();
    }

    void Dead()
    {
        IsAlivingNow = false;
        Destroy(gameObject);
    }
}

[Serializable]
public class MissileStatus
{
    [SerializeField] internal int damage = 1;
    [SerializeField] public Vector2 missileForce;
}
