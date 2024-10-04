using System.Collections;
using KeyHandler;
using PlayerInfo;
using Unity.Mathematics;
using UnityEngine;

public class ChargeShot_TimeHandler : MonoBehaviour
{
    [SerializeField] PlayerStatus playerStatus;

    private bool isMinimumChargeTime = false;
    public bool IsMinimumChargeTime
    {
        get { return isMinimumChargeTime; }
    }

    float timer;

    [SerializeField] private GameObject player;

    InputHandler inputHandler;

    [SerializeField] private ShellManager shellManager;

    /// <summary>
    /// 長押しの時間はこの値で設定する
    /// </summary>
    [SerializeField] private float chargedTime = 1.0f;

    /// <summary>
    /// さすがに短すぎるので豆を撃たない時間を設定する
    /// </summary>
    [SerializeField] private float cantshoot_mame_TimeThreshold = 0.5f;

    private bool isCharging;
    public bool IsCharging
    {
        get { return isCharging; }
    }

    private bool isCharged;
    public bool IsCharged
    {
        get { return isCharged; }
    }

    public void Init(PlayerStatus playerStatus)
    {
        this.playerStatus = playerStatus;
        if (this.playerStatus == null) Debug.Log("playerStatusはnullです");
    }

    private void Awake()
    {
        StopCoroutine_and_ResetSettings();
    }

    private void Start()
    {
        inputHandler = player.GetComponent<InputHandler>();
    }

    private void Update()
    {
        if (!isCharging) return;

        /// <summary>
        /// タイマーをカウントする
        /// </summary>
        timer += Time.deltaTime;

        if (!isMinimumChargeTime && timer >= cantshoot_mame_TimeThreshold)
        {
            isMinimumChargeTime = true;
            Debug.LogWarning("最低限のチャージ時間を超えた");
        }
    }

    Coroutine charge_Accepting_Coroutine;
    private IEnumerator ChargeAcceptingCoroutine()
    {
        Debug.Log("チャージ開始");
        isCharging = true;

        //何秒で長押しと判定するか
        yield return new WaitForSeconds(chargedTime);

        isCharging = false;

        isCharged = true;
        Debug.Log("チャージ完了！！");
    }

    private void StartCoroutine()
    {
        if (charge_Accepting_Coroutine != null)
        {
            StopCoroutine(charge_Accepting_Coroutine);
        }
        charge_Accepting_Coroutine = StartCoroutine(ChargeAcceptingCoroutine());
        timer = 0;
    }

    private void StopCoroutine_and_ResetSettings()
    {
        if (charge_Accepting_Coroutine != null)
        {
            StopCoroutine(charge_Accepting_Coroutine);
        }

        isCharging = false;
        isCharged = false;
        isMinimumChargeTime = false;
        timer = 0;
    }

    //--public--

    public void InterruputChaging()
    {
        StopCoroutine_and_ResetSettings();
        Debug.Log("チャージ完了しませんでした。");

        if (timer <= cantshoot_mame_TimeThreshold) return;

        shellManager.ShootMame();
    }

    public void ShootChargedShell()
    {
        shellManager.ShootChargedShell(shellManager.chargedShell_LevelLower_Prefab, playerStatus.playerDirection);

        StopCoroutine_and_ResetSettings();
        return;
    }

    public void StartCharge()
    {
        StartCoroutine();
    }
}
