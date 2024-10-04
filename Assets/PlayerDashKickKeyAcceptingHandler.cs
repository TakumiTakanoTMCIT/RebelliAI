using UnityEngine;
using System.Collections;
using PlayerInfo;
using PlayerState;
using KeyHandler;

/// <summary>
/// プレイヤーが壁蹴りをした時に、ダッシュ壁蹴りを受け付ける時間を管理するクラスです。
/// ダッシュ壁蹴りの受付時間中にダッシュキーが押されたら、移動速度を上げて壁蹴りをします。
/// </summary>
public class PlayerDashKickKeyAcceptingHandler : MonoBehaviour
{
    PlayerDashKeepManager dashKeepManager;
    PlayerStateMgr stateMgr;
    PlayerStatus playerStatus;
    InputHandler inputHandler;
    private void Awake()
    {
        inputHandler = GetComponent<InputHandler>();
        stateMgr = GetComponent<PlayerStateMgr>();
        playerStatus = GetComponent<PlayerStatus>();
        dashKeepManager = GetComponent<PlayerDashKeepManager>();
    }

    private bool isDashKickKey_Accepting = false;
    public bool IsDashKickKey_Accepting
    {
        get { return isDashKickKey_Accepting; }
    }

    private Coroutine dashKickKey_AcceptingCoroutine;
    private IEnumerator dashKickKey_Accepting()
    {
        isDashKickKey_Accepting = true;
        yield return new WaitForSeconds(playerStatus.delayKey_reception_time);
        isDashKickKey_Accepting = false;
    }

    /// <summary>
    /// ダッシュキーを一瞬だけ受け付けるコルーチンを開始する
    /// </summary>
    public void Start_DashKickKey_AcceptingTime()
    {
        if (dashKickKey_AcceptingCoroutine != null)
        {
            StopCoroutine(dashKickKey_AcceptingCoroutine);
        }
        dashKickKey_AcceptingCoroutine = StartCoroutine(dashKickKey_Accepting());
    }

    private void Stop_DashKickKey_AcceptingTime()
    {
        if (dashKickKey_AcceptingCoroutine != null)
        {
            StopCoroutine(dashKickKey_AcceptingCoroutine);
            dashKickKey_AcceptingCoroutine = null;
        }
        isDashKickKey_Accepting = false;
    }

    private void Update()
    {
        //受付時間でないなら、return
        if (!isDashKickKey_Accepting) return;

        //ダッシュキーが押されたら、壁蹴りをする
        if (stateMgr.inputHandler.IsDashKeyDown() || stateMgr.inputHandler.IsDashKey())
        {
            //ダッシュ速度を維持する
            dashKeepManager.KeepDashSpeed();

            //一応止める
            Stop_DashKickKey_AcceptingTime();
        }
    }
}
