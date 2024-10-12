using System.Collections;
using KeyHandler;
using PlayerInfo;
using Unity.Mathematics;
using UnityEngine;

public class ChargeShot_Handler : MonoBehaviour
{
    PlayerStatus playerStatus;

    private bool isMinimumChargeTime = false;
    public bool IsMinimumChargeTime
    {
        get { return isMinimumChargeTime; }
    }

    float timer;

    MameShellManager shellManager;

    [SerializeField] private float mameCharge_TimeThreshold = 0.5f, lowCharge_TimeThreshold = 1.0f, fullCharge_TimeThreshold = 1.7f;

    private bool isCharging;
    public bool IsCharging
    {
        get { return isCharging; }
    }

    private bool isLowCharged;
    public bool IsLowCharged
    {
        get { return isLowCharged; }
    }

    private bool isFullCharged;
    public bool IsFullCharged
    {
        get { return isFullCharged; }
    }

    public void Init(PlayerStatus playerStatus, MameShellManager shellManager)
    {
        this.shellManager = shellManager;
        this.playerStatus = playerStatus;
        StopCharge_and_ResetSettings();
    }

    private void FixedUpdate()
    {
        if (!isCharging) return;

        /// <summary>
        /// タイマーをカウントする
        /// </summary>
        timer += Time.deltaTime;

        if (!isMinimumChargeTime && timer >= mameCharge_TimeThreshold)
        {
            isMinimumChargeTime = true;
            Debug.LogWarning("最低限のチャージ時間を超えた");
        }

        if (!isLowCharged && timer >= lowCharge_TimeThreshold)
        {
            isLowCharged = true;
            Debug.LogWarning("低チャージ完了！！");
        }

        if (!isFullCharged && (timer - lowCharge_TimeThreshold) >= fullCharge_TimeThreshold)
        {
            isFullCharged = true;
            Debug.LogWarning("フルチャージ完了！！");
        }
    }

    private void StopCharge_and_ResetSettings()
    {
        isCharging = false;

        isLowCharged = false;
        isFullCharged = false;
        isMinimumChargeTime = false;

        timer = 0;
    }

    //--public--

    public void InterruputChaging()
    {
        StopCharge_and_ResetSettings();

        if (timer <= mameCharge_TimeThreshold)
            return;

        shellManager.ShootMame();
    }

    public void Shoot_Charged_Shell(GameObject shell)
    {
        shellManager.ShootChargedShell(shell);

        StopCharge_and_ResetSettings();
    }

    public void StartCharge()
    {
        StopCharge_and_ResetSettings();

        isCharging = true;
    }
}
