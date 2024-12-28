using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using HPBar;
using Zenject;

public class DamageTimeHandler : MonoBehaviour
{
    [Inject]
    PlayerStats playerStatus;

    private bool isDamaging;
    public bool IsDamaging { get { return isDamaging; } }

    //イベントの登録
    private void OnEnable()
    {
        HPBarHandler.onPlayerDamage += StartDamageTime;
    }

    //イベントの登録解除
    private void OnDisable()
    {
        HPBarHandler.onPlayerDamage -= StartDamageTime;
    }

    //イベントハンドラー
    async void StartDamageTime()
    {
        isDamaging = true;
        await UniTask.Delay(TimeSpan.FromSeconds(playerStatus.damagingTime));
        isDamaging = false;
    }
}
