using PlayerState;
using UnityEngine;

/// <summary>
/// このクラスは、プレイヤーのダッシュ速度継続の管理を行うクラスです。
/// 通常のダッシュ時間の管理クラスとは別なので注意してください。
/// ダッシュジャンプ、ダッシュフォール、ダッシュキックの、ダッシュ速度継続を行います。
/// </summary>
public class PlayerDashKeepManager : MonoBehaviour
{
    PlayerDashKickKeyAcceptingHandler dashKickKeyAcceptingHandler;
    PlayerStateMgr stateMgr;
    private void Start()
    {
        stateMgr = this.GetComponent<PlayerStateMgr>();
        dashKickKeyAcceptingHandler = this.GetComponent<PlayerDashKickKeyAcceptingHandler>();
    }

    [SerializeField] private bool isKeepDashSpeed = false;
    public bool IsKeepDashSpeed
    {
        get { return isKeepDashSpeed; }
    }

    public void KeepDashSpeed()
    {
        isKeepDashSpeed = true;
    }

    private void StopDashSpeed()
    {
        isKeepDashSpeed = false;
    }

    private void Update()
    {
        if (!isKeepDashSpeed)
            return;

        /// <summary>
        /// 地面についたらダッシュ速度継続を止める
        /// </summary>
        if (stateMgr.actionStatusChk.IsGround())
        {
            if (!stateMgr.actionStatusChk.isJumpingNow() && !stateMgr.actionStatusChk.IsFallingNow())
            {
                StopDashSpeed();
                return;
            }
        }

        /// <summary>
        /// ダッシュキックの受け付け中は以下の処理を行わない
        /// ダッシュキックの受付中に実行すると、ダッシュキックができなくなる
        /// なぜならダッシュキックの受付中にも壁に触れることもあるので、ダッシュキーを押したとしてもダッシュキックができなくなるからです。
        /// </summary>
        //if (dashKickKeyAcceptingHandler.IsDashKickKey_Accepting) return;


        /// <summary>
        /// 壁に触れたらダッシュ速度継続を止める
        /// </summary>
        if (!stateMgr.inputHandler.IsMoveKey())
        {
            //同時押し、どちらも押されていないならreturn
            return;
        }

        if (stateMgr.actionStatusChk.IsWall(false) && stateMgr.inputHandler.IsMoveLeftKey())
        {
            StopDashSpeed();
            return;
        }
        else if (stateMgr.actionStatusChk.IsWall(true) && stateMgr.inputHandler.IsMoveRightKey())
        {
            StopDashSpeed();
            return;
        }
    }
}
