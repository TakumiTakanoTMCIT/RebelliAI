using ActionStatusChk;
using KeyHandler;
using UnityEngine;

/// <summary>
/// このクラスは、プレイヤーのダッシュ速度継続の管理を行うクラスです。
/// 通常のダッシュ時間の管理クラスとは別なので注意してください。
/// ダッシュジャンプ、ダッシュフォール、ダッシュキックの、ダッシュ速度継続を行います。
/// </summary>
public class PlayerDashKeepManager : MonoBehaviour
{
    ActionStatusChecker actionStatusChk;
    InputHandler inputHandler;

    public void Init(ActionStatusChecker actionStatusChk, InputHandler inputHandler)
    {
        this.actionStatusChk = actionStatusChk;
        this.inputHandler = inputHandler;
    }

    [SerializeField] private bool isKeepDashSpeed = false;
    public bool IsKeepDashSpeed => isKeepDashSpeed;

    public void KeepDashSpeed()
    {
        isKeepDashSpeed = true;
    }

    public void StopDashSpeed()
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
        if (actionStatusChk.IsGround())
        {
            if (!actionStatusChk.isJumpingNow() && !actionStatusChk.IsFallingNow())
            {
                StopDashSpeed();
                return;
            }
        }

        if (!inputHandler.IsMoveKey())
            return;

        if (actionStatusChk.IsWall(false) && inputHandler.IsMoveLeftKey())
        {
            StopDashSpeed();
            return;
        }
        else if (actionStatusChk.IsWall(true) && inputHandler.IsMoveRightKey())
        {
            StopDashSpeed();
            return;
        }
    }
}
