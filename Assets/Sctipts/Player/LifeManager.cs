using UnityEngine;
using UniRx;
using System;
using HPBar;

public class LifeManager : MonoBehaviour
{
    [SerializeField] GamePlayerManager gamePlayerManager;
    [SerializeField] private PlayerOnVoidChecker playerOnVoidChecker;
    [SerializeField] private HPBarHandler hpBarHandler;

    private Subject<Unit> onPlayerDead = new Subject<Unit>();
    public IObservable<Unit> OnPlayerDead => onPlayerDead;

    private void OnEnable()
    {
        playerOnVoidChecker.OnPlayerInVoid.Subscribe(_ =>
            Death()
            ).AddTo(this);

        hpBarHandler.OnHPZero.Subscribe(_ =>
            Death()
            ).AddTo(this);
    }

    private void Death()
    {
        if(!gamePlayerManager.isInGameArea) return;
        Debug.LogAssertion("Player is dead.");
        onPlayerDead?.OnNext(Unit.Default);
    }
}
