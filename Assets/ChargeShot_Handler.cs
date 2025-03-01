using System;
using UnityEngine;
using HPBar;
using UniRx;
using Zenject;

public class ChargeShot_Handler : MonoBehaviour
{
    //Inject
    LifeManager lifeManager;
    HPBar.EventMediator hpbarEventMediator;

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

    [Inject]
    public void Construct(LifeManager lifeManager, HPBar.EventMediator eventMediator)
    {
        this.lifeManager = lifeManager;
        this.hpbarEventMediator = eventMediator;
    }

    private void Awake()
    {
        ResetSettings();

        if (levelLower_EnergyBall == null)
            Debug.Log("LevelLowerShellがResourcesディレクトリにありません。確認してください!!");

        if (fullLevel_EnergyBall == null)
            Debug.Log("FullChargeBallがResourcesディレクトリにありません。確認してください!!");

        lifeManager.OnPlayerDead.Subscribe(_ =>
        {
            ResetSettings();
        })
        .AddTo(this);

        hpbarEventMediator.OnPlayerDamage.Subscribe(_ =>
        {
            ResetSettings();
        })
        .AddTo(this);
    }

    private void OnEnable()
    {
        AllShellManager.onShootChargedShell += ResetSettings;
        PlayerWeapon_KeyController.onTooShortCharge += ResetSettings;
    }

    private void OnDisable()
    {
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
