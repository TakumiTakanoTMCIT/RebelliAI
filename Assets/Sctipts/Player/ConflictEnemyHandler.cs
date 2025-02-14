using UnityEngine;
using Zenject;

public interface IConflictEnemy
{
    void OnConflictEnemy(int damage);
}

public class ConflictEnemyHandler : MonoBehaviour, IConflictEnemy
{
    HPBar.IHealth health;
    DamageTimeHandler damageTimeHandler;

    bool isInvincible = false;

    [Inject]
    public void Construct(HPBar.IHealth health)
    {
        this.health = health;
    }

    public void ChildComponentGetter(DamageTimeHandler damageTimeHandler)
    {
        this.damageTimeHandler = damageTimeHandler;
    }

    private void Awake()
    {
        isInvincible = false;
        if (health == null)
        {
            Debug.Log("HPBarHandlerがアタッチされていないので、HPシステムは機能しません");
        }
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
        if (health == null) return;
        if (isInvincible) return;
        if (damageTimeHandler.IsDamaging) return;

        health.Damage(damage);
    }
}
