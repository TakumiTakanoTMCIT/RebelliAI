using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using PlayerInfo;
using PlayerState;

/// <summary>
/// このクラスは、壁蹴りの受付時間を管理するクラスです。
/// 壁蹴りの受付時間中にジャンプキーが押されたら、壁蹴り状態に遷移します。
/// </summary>
public class WallKickDelayManager : MonoBehaviour
{
    PlayerStatus playerStatus;
    PlayerStateMgr stateMgr;
    private void Awake()
    {
        stateMgr = GetComponent<PlayerStateMgr>();
        playerStatus = GetComponent<PlayerStatus>();
    }

    private bool isJumpKey_Accepting = false;

    /// <summary>
    /// ジャンプキーを一瞬だけ受け付けるフラグの管理をするコルーチン
    /// </summary>
    Coroutine jumpKey_Accepting_Coroutine;
    private IEnumerator jumpKey_Accepting()
    {
        isJumpKey_Accepting = true;
        yield return new WaitForSeconds(playerStatus.delayKey_reception_time);
        isJumpKey_Accepting = false;

        //一応nullを代入しておく
        jumpKey_Accepting_Coroutine = null;
    }

    /// <summary>
    /// ジャンプキーを一瞬だけ受け付けるコルーチンを開始する
    /// </summary>
    public void Start_JumpKey_AcceptingTime()
    {
        if(jumpKey_Accepting_Coroutine != null)
        {
            StopCoroutine(jumpKey_Accepting_Coroutine);
        }
        jumpKey_Accepting_Coroutine = StartCoroutine(jumpKey_Accepting());
    }

    /// <summary>
    /// コルーチンを停止する
    /// </summary>
    public void Stop_JumpKey_AcceptingTime()
    {
        if(jumpKey_Accepting_Coroutine != null)
        {
            StopCoroutine(jumpKey_Accepting_Coroutine);
            jumpKey_Accepting_Coroutine = null;
        }
        isJumpKey_Accepting = false;
    }

    private void Update()
    {
        //受付時間以外なら受け付けない
        if(!isJumpKey_Accepting) return;

        //ジャンプキーが押されたら
        if(stateMgr.inputHandler.IsJumpKeyDown())
        {
            //コルーチンの受け付け時間を終了する
            Stop_JumpKey_AcceptingTime();

            //Debug.Log("WallKickをfallから割り込んでします");

            //wallkickに遷移する
            stateMgr.ChangeState(stateMgr.wallKick);
        }
    }
}
