using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;
using UniRx;

public class DamagingBlinking : MonoBehaviour
{
    public static event Action onPlayerInvincible, onPlayerInvinvibleFinish;
    bool isInvincible = false;
    [SerializeField] bool isDebugMode = false;

    SpriteRenderer spriteRenderer;

    PlayerStats playerStats;

    //Inject
    private PlayerState.EventMediator stateEventMediator;

    [Inject]
    public void Construct(PlayerState.EventMediator eventMediator, PlayerStats playerStats)
    {
        stateEventMediator = eventMediator;
        this.playerStats = playerStats;
    }

    public void SctiptableObjectGetter(PlayerStats playerStats)
    {
        this.playerStats = playerStats;
    }

    private void Awake()
    {
        isInvincible = false;
        spriteRenderer = gameObject.MyGetComponent_NullChker<SpriteRenderer>();

        stateEventMediator.OnPlayerDamageRecover.Subscribe(_ =>
            InvincibleTimer())
        .AddTo(this);
    }

    //イベントハンドラー
    async void InvincibleTimer()
    {
        onPlayerInvincible?.Invoke();
        isInvincible = true;
        InvinsibleBlinking();

        if (isDebugMode) Debug.Log("StartInvincible");

        try
        {
            await UniTask.Delay(TimeSpan.FromSeconds(playerStats.invincibleTime));
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            //EditorApplication.isPaused = true;
            return;
        }

        if (isDebugMode) Debug.Log("FinishInvincible");

        onPlayerInvinvibleFinish?.Invoke();
        isInvincible = false;
    }

    //点滅処理
    async void InvinsibleBlinking()
    {
        Color playerColor;

        while (true)
        {
            if (!isInvincible) break;

            playerColor = spriteRenderer.color;
            playerColor.a = 0f;
            spriteRenderer.color = playerColor;

            //アルファ値のみ変更するように試してみる
            await UniTask.Delay(TimeSpan.FromSeconds(playerStats.blikingInterval));

            playerColor = spriteRenderer.color;
            playerColor.a = 1f;
            spriteRenderer.color = playerColor;

            await UniTask.Delay(TimeSpan.FromSeconds(playerStats.blikingInterval));
        }

        spriteRenderer.color = Color.white;
    }
}
