using System;
using KeyHandler;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerWeapon_KeyController : MonoBehaviour
{
    ChargeShot_Handler chargeShotHandler;
    AllShellManager allShellManager;
    InputHandler inputHandler;

    public static event Action onTooShortCharge;

    public void Init(InputHandler inputHandler, ChargeShot_Handler chargeShotHandler, AllShellManager shellManager)
    {
        this.allShellManager = shellManager;
        this.chargeShotHandler = chargeShotHandler;
        this.inputHandler = inputHandler;
    }

    private void OnEnable()
    {
        DoorAnimHandler.onDoorClosed += OnDoorClosed;
        InputHandler.onAcceptInputCtrl += OnAcceptInputCtrl;
        IntroBossHPBarHandler.onDead += CheckCharge;
    }

    private void OnDisable()
    {
        DoorAnimHandler.onDoorClosed -= OnDoorClosed;
        InputHandler.onAcceptInputCtrl -= OnAcceptInputCtrl;
        IntroBossHPBarHandler.onDead -= CheckCharge;
    }

    void OnAcceptInputCtrl()
    {
        if (!inputHandler.isShootKey) CheckCharge();
    }

    //イベントハンドラー
    //TODO:名前変える。わかりにくい
    void OnDoorClosed()
    {
        if (inputHandler.IsShootKey())
        {
            return;
        }
        else
        {
            CheckCharge();
        }
    }

    private void Update()
    {
        if (inputHandler.IsShootKeyDown())
        {
            if (!chargeShotHandler.IsCharging)
            {
                chargeShotHandler.StartCharge();
            }
            allShellManager.ShootMame(false);
        }

        if (inputHandler.IsShootKeyUp())
        {
            CheckCharge();
            return;
        }
    }

    void CheckCharge()
    {
        if (!chargeShotHandler.IsCharging) return;

        if (chargeShotHandler.IsFullCharged)
        {
            allShellManager.ShootChargedShell(chargeShotHandler.fullLevel_EnergyBall);
            return;
        }

        if (chargeShotHandler.IsLowCharged)
        {
            allShellManager.ShootChargedShell(chargeShotHandler.levelLower_EnergyBall);
            return;
        }

        if (chargeShotHandler.IsMinimumChargeTime)
        {
            allShellManager.ShootMame(true);
            return;
        }

        //チャージをほぼしないでリリースした場合は撃つことは無いですが、チャージをリセットします
        //この設定重要です
        onTooShortCharge?.Invoke();
    }
}
