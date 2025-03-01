using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using HPBar;
using Zenject;
using UniRx;

public class DamageTimeHandler : MonoBehaviour
{
    [Inject]
    PlayerStats playerStatus;

    //Inject
    HPBar.EventMediator hpbarEventMediator;

    private bool isDamaging;
    public bool IsDamaging { get { return isDamaging; } }

    [Inject]
    public void Construct(HPBar.EventMediator eventMediator)
    {
        this.hpbarEventMediator = eventMediator;
    }

    private void Awake()
    {
        hpbarEventMediator.OnPlayerDamage.Subscribe(_ =>
        {
            StartDamageTime();
        })
        .AddTo(this);
    }

    //イベントハンドラー
    async void StartDamageTime()
    {
        isDamaging = true;
        await UniTask.Delay(TimeSpan.FromSeconds(playerStatus.damagingTime));
        isDamaging = false;
    }
}
