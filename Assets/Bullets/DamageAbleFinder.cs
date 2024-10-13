using UnityEngine;

public class DamageAbleFinder : MonoBehaviour
{
    Animator animator;
    MameAnimator mamaeAnimator;

    [SerializeField] private int damageAmount = 1;

    private void Awake()
    {
        animator = this.gameObject.MyGetComponent_NullChker<Animator>();
        mamaeAnimator = new MameAnimator(animator);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        IDamageableFromShot damageable = other.GetComponent<IDamageableFromShot>();

        if (damageable == null)
            return;
        damageable.TakeDamage(damageAmount);
        mamaeAnimator.TakeDamage();
    }
}
