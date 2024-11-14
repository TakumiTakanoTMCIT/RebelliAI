using System;
using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEngine;

public class DamagingBlinking : MonoBehaviour
{
    public static event Action onPlayerInvincible, onPlayerInvinvibleFinish;
    bool isInvincible = false;
    [SerializeField] bool isDebugMode = false;

    SpriteRenderer spriteRenderer;
    [SerializeField] float blikingInterval = 0.05f, invincibleTime = 1.5f;

    private void Awake()
    {
        isInvincible = false;
        spriteRenderer = gameObject.MyGetComponent_NullChker<SpriteRenderer>();
    }

    private void OnEnable()
    {
        PlayerState.DamageState.onPlayerDamageRecover += InvincibleTimer;
    }

    private void OnDisable()
    {
        PlayerState.DamageState.onPlayerDamageRecover -= InvincibleTimer;
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
            await UniTask.Delay(TimeSpan.FromSeconds(invincibleTime));
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
        Color playerColor = spriteRenderer.color;

        while (true)
        {
            if (!isInvincible) break;

            playerColor = spriteRenderer.color;
            playerColor.a = 0f;
            spriteRenderer.color = playerColor;

            //アルファ値のみ変更するように試してみる
            await UniTask.Delay(TimeSpan.FromSeconds(blikingInterval));

            playerColor = spriteRenderer.color;
            playerColor.a = 1f;
            spriteRenderer.color = playerColor;

            await UniTask.Delay(TimeSpan.FromSeconds(blikingInterval));
        }

        spriteRenderer.color = Color.white;
    }
}
