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
    [SerializeField] HPBarHandler hPBarHandler;
    [SerializeField] DamageTimeHandler damageTimeHandler;

    public void OnConflictEnemy(int damage)
    {
        if (damageTimeHandler.IsDamaging) return;

        Debug.Log("ConflictEnemy");
        hPBarHandler.Damage(damage);
    }
}
