using UnityEngine;
using UniRx;
public class ChargedShellDamageAbleFinder : MonoBehaviour
{
    [SerializeField] private int damageAmount = 5, extraDamageAmount = 7;
    Animator animator;
    ChargedShellAnimatorCtrl animatorCtrl;
    ChargedShellBodyCtrl bodyCtrl;

    bool isExtraDamage = false;

    private void Awake()
    {
        animator = this.gameObject.MyGetComponent_NullChker<Animator>();
        bodyCtrl = this.gameObject.MyGetComponent_NullChker<ChargedShellBodyCtrl>();

        animatorCtrl = new ChargedShellAnimatorCtrl(animator, bodyCtrl);
    }

    public void IsExtraDamage(bool isExtraDamage)
    {
        this.isExtraDamage = isExtraDamage;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        IDamageableFromShot damageable = other.GetComponent<IDamageableFromShot>();
        if (damageable != null)
        {
            if (isExtraDamage) damageable.TakeDamage(extraDamageAmount);
            else damageable.TakeDamage(damageAmount);
            animatorCtrl.TakeDamage();
            SoundEffectCtrl.OnPlayHitSE.OnNext(0);
            return;
        }

        RefrectableBody refrectableBody = other.GetComponent<RefrectableBody>();
        if (refrectableBody != null)
        {
            animatorCtrl.RefrectShell();
            SoundEffectCtrl.OnPlayDeathSE.OnNext(1);
            return;
        }
    }
}
