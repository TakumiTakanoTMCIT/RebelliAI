using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using HPBar;

public class ChargingParticleHandler : MonoBehaviour
{
    //ショット時にリリースされるのを受け取るイベント
    //今どの程度チャージされているかを受け取る変数

    GameObject particleBody;
    [SerializeField] SpriteRenderer playerSpRenderer;
    Color32 playerColor;

    [SerializeField] bool isLowCharging, isFullCharging, isColorChanging;

    readonly string oneLowColorString = "#f164ff", twoLowColorString = "#ffde00", oneFullColorString = "#a6ffed", twoFullColorString = "#226cff";
    Color oneLowColor, twoLowColor, oneFullColor, twoFullColor;

    Animator animator;
    private void Awake()
    {
        ColorUtility.TryParseHtmlString(oneLowColorString, out oneLowColor);
        ColorUtility.TryParseHtmlString(twoLowColorString, out twoLowColor);

        ColorUtility.TryParseHtmlString(oneFullColorString, out oneFullColor);
        ColorUtility.TryParseHtmlString(twoFullColorString, out twoFullColor);

        playerColor = playerSpRenderer.color;
        particleBody = GameObject.Find("ChargingParticle");

        GetAnimator();
    }

    void GetAnimator()
    {
        if (animator == null) animator = particleBody.MyGetComponent_NullChker<Animator>();
    }

    private void OnEnable()
    {
        ChargeShot_Handler.onLowCharge += OnLowCharge;
        ChargeShot_Handler.onFullCharge += OnFullCharge;

        HPBarHandler.onPlayerDamage += OnResetCharge;
        HPBarHandler.onPlayerDeath += OnResetCharge;

        AllShellManager.onShootChargedShell += OnResetCharge;
        PlayerWeapon_KeyController.onTooShortCharge += OnResetCharge;

        particleBody.SetActive(false);
    }

    private void OnDisable()
    {
        ChargeShot_Handler.onLowCharge -= OnLowCharge;
        ChargeShot_Handler.onFullCharge -= OnFullCharge;

        HPBarHandler.onPlayerDamage -= OnResetCharge;
        HPBarHandler.onPlayerDeath -= OnResetCharge;

        AllShellManager.onShootChargedShell -= OnResetCharge;
        PlayerWeapon_KeyController.onTooShortCharge -= OnResetCharge;
    }

    void OnResetCharge()
    {
        OffBool("isLowCharge");
        OffBool("isFullCharge");
        isLowCharging = false;
        isFullCharging = false;

        isColorChanging = false;

        playerColor = Color.white;
        playerSpRenderer.color = playerColor;

        particleBody.SetActive(false);
    }

    async void OnPlayerColorChange()
    {
        isColorChanging = true;
        while (isColorChanging)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(0.05));

            if (isFullCharging)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(0.05));
                playerSpRenderer.color = oneFullColor;

                await UniTask.Delay(TimeSpan.FromSeconds(0.05));
                playerSpRenderer.color = twoFullColor;
                continue;
            }

            if (isLowCharging)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(0.05));
                playerSpRenderer.color = oneLowColor;

                await UniTask.Delay(TimeSpan.FromSeconds(0.05));
                playerSpRenderer.color = twoLowColor;
                continue;
            }
            else
            {
                //例外処理です。
                //この状況おこらないはず
                break;
            }
        }

        playerSpRenderer.color = Color.white;
    }

    void SetBool(string name)
    {
        GetAnimator();
        animator.SetBool(name, true);
    }

    void OffBool(string name)
    {
        GetAnimator();
        animator.SetBool(name, false);
    }

    void OnFullCharge()
    {
        isFullCharging = true;
        SetBool("isFullCharge");
    }

    void OnLowCharge()
    {
        if (particleBody.activeSelf) return;

        particleBody.SetActive(true);
        isLowCharging = true;
        SetBool("isLowCharge");

        OnPlayerColorChange();
    }
}
