using System.Collections;
using System.Collections.Generic;
using PlayerInfo;
using PlayerState;
using UnityEngine;

public interface IConflictEnemy
{
    void OnConflictEnemy(int damage);
}

public class ConflictEnemyHandler : MonoBehaviour, IConflictEnemy
{
    [SerializeField] HPBar.HPBarHandler hPBarHandler;
    [SerializeField] DamageTimeHandler damageTimeHandler;

    bool isInvincible = false;

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
