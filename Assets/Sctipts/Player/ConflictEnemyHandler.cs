using System.Collections;
using System.Collections.Generic;
using PlayerInfo;
using UnityEngine;

public interface IConflictEnemy
{
    void OnConflictEnemy(int damage);
}

public class ConflictEnemyHandler : MonoBehaviour, IConflictEnemy
{
    [SerializeField]HPBarHandler hPBarHandler;

    public void OnConflictEnemy(int damage)
    {
        Debug.Log("ConflictEnemy");
        hPBarHandler.Damage(damage);
    }
}
