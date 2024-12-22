using UnityEngine;
using PlayerShot;

public class ChargedShellDamageAbleFinder : MonoBehaviour
{
    [SerializeField] private int damageAmount = 5, extraDamageAmount = 7;
    IAnimatable animatorCtrl;

    bool isExtraDamage = false;

    private void Awake()
    {
        animatorCtrl = GetComponent<IAnimatable>();
    }

    public void IsExtraDamage(bool isExtraDamage)
    {
        this.isExtraDamage = isExtraDamage;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        IDamageableFromShot damageable = other.GetComponent<IDamageableFromShot>();
        RefrectableBody refrectableBody = other.GetComponent<RefrectableBody>();

        //同時にダメージを受けるオブジェクトと、反射可能なオブジェクトがある場合
        if(damageable != null && refrectableBody != null)
        {
            damageable.TakeDamage(extraDamageAmount);
            animatorCtrl.TakeDamage();
            GetComponent<IChargeShot>().TakeDamage();
            SoundEffectCtrl.OnPlayHitSE.OnNext(0);
            return;
        }

        if (damageable != null)
        {
            if (isExtraDamage) damageable.TakeDamage(extraDamageAmount);
            else damageable.TakeDamage(damageAmount);
            animatorCtrl.TakeDamage();
            GetComponent<IChargeShot>().TakeDamage();
            SoundEffectCtrl.OnPlayHitSE.OnNext(0);
            return;
        }

        if (refrectableBody != null)
        {
            animatorCtrl.RefrectShell();
            SoundEffectCtrl.OnPlayDeathSE.OnNext(1);
            GetComponent<IChargeShot>().Refrect();
            return;
        }
    }
}
