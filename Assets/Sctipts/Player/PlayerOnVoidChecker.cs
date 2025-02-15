using UnityEngine;
using UniRx;
using System;
using Zenject;

/// <summary>
/// 奈落に落ちたかどうかを判定するクラス
/// </summary>
public class PlayerOnVoidChecker : MonoBehaviour
{
    [SerializeField] private GamePlayerManager gamePlayerManager;

    //プレイヤーが奈落に落ちた時のSubject
    private Subject<Unit> onPlayerInVoid = new Subject<Unit>();
    public IObservable<Unit> OnPlayerInVoid => onPlayerInVoid;

    private void OnBecameInvisible()
    {
        if (!gamePlayerManager.isInGameArea) return;
        onPlayerInVoid.OnNext(Unit.Default);
    }
}
