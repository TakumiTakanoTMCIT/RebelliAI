using UnityEngine;

public interface IConflictEnemy
{
    void OnConflictEnemy(int damage);
}

public class ConflictEnemyHandler : MonoBehaviour, IConflictEnemy
{
    HPBar.HPBarHandler hPBarHandler;
    DamageTimeHandler damageTimeHandler;

    bool isInvincible = false;

    public void OtherComponentGetter(HPBar.HPBarHandler hPBarHandler)
    {
        this.hPBarHandler = hPBarHandler;
    }

    public void ChildComponentGetter(DamageTimeHandler damageTimeHandler)
    {
        this.damageTimeHandler = damageTimeHandler;
    }

    private void Awake()
    {
        isInvincible = false;
    }

    private void OnEnable()
    {
        DamagingBlinking.onPlayerInvincible += OnPlayerInvincible;
        DamagingBlinking.onPlayerInvinvibleFinish += OnPlayerInvincibleFinish;
        IntroBossHPBarHandler.onDead += OnPlayerInvincible;
    }

    private void OnDisable()
    {
        DamagingBlinking.onPlayerInvincible -= OnPlayerInvincible;
        DamagingBlinking.onPlayerInvinvibleFinish -= OnPlayerInvincibleFinish;
        IntroBossHPBarHandler.onDead -= OnPlayerInvincible;
    }

    void OnPlayerInvincible()
    {
        isInvincible = true;
    }

    void OnPlayerInvincibleFinish()
    {
        isInvincible = false;
    }

    //敵に接触した時に敵から呼び出されます
    public void OnConflictEnemy(int damage)
    {
        if (isInvincible) return;
        if (damageTimeHandler.IsDamaging) return;
        hPBarHandler.Damage(damage);
    }
}
