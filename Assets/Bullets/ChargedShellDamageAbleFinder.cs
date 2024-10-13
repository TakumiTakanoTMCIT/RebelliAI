using UnityEngine;

public class ChargedShellDamageAbleFinder : MonoBehaviour
{
    [SerializeField] private int damageAmount = 5;
    Animator animator;
    ChargedShellAnimatorCtrl animatorCtrl;
    ChargedShellBodyCtrl bodyCtrl;

    private void Awake()
    {
        animator = this.gameObject.MyGetComponent_NullChker<Animator>();
        bodyCtrl = this.gameObject.MyGetComponent_NullChker<ChargedShellBodyCtrl>();

        animatorCtrl = new ChargedShellAnimatorCtrl(animator, bodyCtrl);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        IDamageableFromShot damageable = other.GetComponent<IDamageableFromShot>();

        if (damageable == null)
            return;

        damageable.TakeDamage(damageAmount);
        animatorCtrl.TakeDamage();
    }
}
