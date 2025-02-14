using UnityEngine;
using UniRx;
using System;
using HPBar;

public class LifeManager : MonoBehaviour
{
    [SerializeField] private PlayerOnVoidChecker playerOnVoidChecker;
    [SerializeField] private HPBarHandler hpBarHandler;

    private Subject<Unit> onPlayerDead = new Subject<Unit>();
    public IObservable<Unit> OnPlayerDead => onPlayerDead;

    private Subject<Unit> onPlayerDeadInstantly = new Subject<Unit>();
    public IObservable<Unit> OnPlayerDeadInstantly => onPlayerDeadInstantly;

    private void OnEnable()
    {
        playerOnVoidChecker.OnPlayerInVoid.Subscribe(_ =>
            SuddenDeath()
            ).AddTo(this);

        hpBarHandler.OnHPZero.Subscribe(_ =>
            Death()
            ).AddTo(this);
    }

    private void Death()
    {
        Debug.Log("Player is dead.");
        onPlayerDead?.OnNext(Unit.Default);
    }

    private void SuddenDeath()
    {
        Death();
        Debug.Log("Player is dead instantly.");
        onPlayerDeadInstantly?.OnNext(Unit.Default);
    }
}
