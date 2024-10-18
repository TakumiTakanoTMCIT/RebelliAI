using UnityEngine;

public class MissileBody : MonoBehaviour, IDamageableFromShot
{
    public bool IsAlivingNow { get; set; }
    [SerializeField] Vector2 missileForce;

    Rigidbody2D rb;
    private void Awake()
    {
        IsAlivingNow = false;
        rb = gameObject.MyGetComponent_NullChker<Rigidbody2D>();
    }

    private void OnBecameVisible()
    {
        IsAlivingNow = true;
        rb.AddForce(missileForce, ForceMode2D.Impulse);
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
