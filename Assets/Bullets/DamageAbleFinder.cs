using UnityEngine;

public class DamageAbleFinder : MonoBehaviour
{
    Animator animator;
    MameAnimator mamaeAnimator;
    ShellMainBodyCrtl shellMainBodyCrtl;

    [SerializeField] private int damageAmount = 1, dashExtraDamageAmount = 2;
    bool isDashExtraDamage, isRefrected;

    private void Awake()
    {
        shellMainBodyCrtl = gameObject.MyGetComponent_NullChker<ShellMainBodyCrtl>();
        animator = this.gameObject.MyGetComponent_NullChker<Animator>();
        mamaeAnimator = new MameAnimator(animator);
    }

    private void OnEnable()
    {
        shellMainBodyCrtl.onDead += OnDead;
    }

    private void OnDisable()
    {
        shellMainBodyCrtl.onDead -= OnDead;
    }

    void OnDead()
    {
        mamaeAnimator.isExcuted = false;
    }

    public void SetDamageAmount(bool isDashExtraDamage)
    {
        isRefrected = false;

        if (isDashExtraDamage)
            this.isDashExtraDamage = true;
        else
            this.isDashExtraDamage = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isRefrected) return;

        IDamageableFromShot damageable = other.GetComponent<IDamageableFromShot>();
        if (damageable != null)
        {
            if (!isDashExtraDamage) damageable.TakeDamage(damageAmount);
            else damageable.TakeDamage(dashExtraDamageAmount);

            mamaeAnimator.TakeDamage();
            SoundEffectCtrl.OnPlayHitSE.OnNext(0);
            return;
        }

        RefrectableBody refrectableBody = other.GetComponent<RefrectableBody>();
        if (refrectableBody != null)
        {
            Debug.Log($"RefrectableBody Hit! {other.name}");
            isRefrected = true;
            mamaeAnimator.RefrectShell();
            SoundEffectCtrl.OnPlayHitSE.OnNext(1);
            return;
        }
    }
}
