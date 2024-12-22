using System;
using KeyHandler;
using PlayerState;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerWeapon_KeyController : MonoBehaviour
{
    PlayerStateMgr playerStateMgr;
    ChargeShot_Handler chargeShotHandler;
    AllShellManager allShellManager;
    InputHandler inputHandler;

    public static event Action onTooShortCharge;

    public void Init(InputHandler inputHandler, ChargeShot_Handler chargeShotHandler, AllShellManager shellManager)
    {
        this.allShellManager = shellManager;
        this.chargeShotHandler = chargeShotHandler;
        this.inputHandler = inputHandler;
        playerStateMgr = GetComponent<PlayerStateMgr>();
    }

    private void OnEnable()
    {
        DoorAnimHandler.onDoorClosed += OnDoorClosed;
        InputHandler.onAcceptInputCtrl += OnAcceptInputCtrl;
        IntroBossHPBarHandler.onDead += CheckCharge;
        PlayerState.DamageState.onPlayerDamageRecover += HandleChargeOnDamageRecover;
    }

    private void OnDisable()
    {
        DoorAnimHandler.onDoorClosed -= OnDoorClosed;
        InputHandler.onAcceptInputCtrl -= OnAcceptInputCtrl;
        IntroBossHPBarHandler.onDead -= CheckCharge;
        PlayerState.DamageState.onPlayerDamageRecover -= HandleChargeOnDamageRecover;
    }

    //FIX : ここが、ドアから入ったときにチャージが保存できないバグ
    void OnAcceptInputCtrl()
    {
        if (!inputHandler.IsAttackingKey) CheckCharge();
    }

    //　　イベントハンドラー
    // プレイヤーがダメージ状態から回復した際に、
    // チャージボタンが押されていればチャージを開始する。
    void HandleChargeOnDamageRecover()
    {
        return;
        if (inputHandler.IsShootKey() || inputHandler.IsShootKeyDown())
        {
            chargeShotHandler.StartCharge();
        }
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
        //ショットボタンを押したら豆を撃って、チャージを開始します
        //ダメージを受けている時は撃てません
        if (inputHandler.IsShootKeyDown() && playerStateMgr.currentState != playerStateMgr.damageState)
        {
            if (!chargeShotHandler.IsCharging)
            {
                chargeShotHandler.StartCharge();
            }
            allShellManager.ShootMame(false);
        }

        //もしショットキーを押されてるならチャージを開始します。とくに、ドアから出るときや、ダメージから回復したときなどです
        if (!inputHandler.IsShootKeyDown() && inputHandler.IsShootKey() && playerStateMgr.currentState != playerStateMgr.damageState)
        {
            if (chargeShotHandler.IsCharging)
                return;

            chargeShotHandler.StartCharge();
        }

        if (inputHandler.IsShootKeyUp())
        {
            if (!chargeShotHandler.IsCharging) return;

            CheckCharge();
            return;
        }
    }

    void CheckCharge()
    {
        if (!chargeShotHandler.IsCharging) return;

        Debug.Log("CheckCharge");

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
