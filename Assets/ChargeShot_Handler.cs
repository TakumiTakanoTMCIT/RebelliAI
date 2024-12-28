using System;
using UnityEngine;
using HPBar;

public class ChargeShot_Handler : MonoBehaviour
{
    [SerializeField] public GameObject levelLower_EnergyBall, fullLevel_EnergyBall;

    private bool isMinimumChargeTime = false;
    public bool IsMinimumChargeTime => isMinimumChargeTime;

    float timer;

    [SerializeField] private float mameCharge_TimeThreshold = 0.5f, lowCharge_TimeThreshold = 1.0f, fullCharge_TimeThreshold = 1.7f;

    private bool isCharging;
    public bool IsCharging => isCharging;

    private bool isLowCharged;
    public bool IsLowCharged => isLowCharged;

    private bool isFullCharged;
    public bool IsFullCharged => isFullCharged;

    public static event Action onLowCharge, onFullCharge;

    private void Awake()
    {
        ResetSettings();

        if (levelLower_EnergyBall == null)
            Debug.Log("LevelLowerShellがResourcesディレクトリにありません。確認してください!!");

        if (fullLevel_EnergyBall == null)
            Debug.Log("FullChargeBallがResourcesディレクトリにありません。確認してください!!");
    }

    private void OnEnable()
    {
        HPBarHandler.onPlayerDamage += ResetSettings;
        HPBarHandler.onPlayerDeath += ResetSettings;
        AllShellManager.onShootChargedShell += ResetSettings;
        PlayerWeapon_KeyController.onTooShortCharge += ResetSettings;
    }

    private void OnDisable()
    {
        HPBarHandler.onPlayerDamage -= ResetSettings;
        HPBarHandler.onPlayerDeath -= ResetSettings;
        AllShellManager.onShootChargedShell -= ResetSettings;
        PlayerWeapon_KeyController.onTooShortCharge -= ResetSettings;
    }

    private void FixedUpdate()
    {
        if (!isCharging) return;

        /// タイマーをカウントする
        timer += Time.deltaTime;

        if ((timer - lowCharge_TimeThreshold) >= fullCharge_TimeThreshold)
        {
            if (!isFullCharged)
            {
                isFullCharged = true;
                onFullCharge?.Invoke();
                return;
            }
        }

        if (timer >= lowCharge_TimeThreshold)
        {
            if (!isLowCharged)
            {
                isLowCharged = true;
                onLowCharge?.Invoke();
                return;
            }
        }

        if (timer >= mameCharge_TimeThreshold)
        {
            if (!isMinimumChargeTime) isMinimumChargeTime = true;
            return;
        }
    }

    private void ResetSettings()
    {
        isCharging = false;

        isLowCharged = false;
        isFullCharged = false;
        isMinimumChargeTime = false;

        timer = 0;
    }

    //--public--//

    public void StartCharge()
    {
        ResetSettings();

        isCharging = true;
    }
}
