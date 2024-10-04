using System.Collections;
using System.Collections.Generic;
using ActionStatusChk;
using PlayerState;
using UnityEngine;

public class GroundChk : MonoBehaviour
{
    //[SerializeField] private Dash_KickorJump_Manager dashCtrl;

    [SerializeField] private PlayerStateMgr stateMgr;
    [SerializeField] private ActionStatusChecker actionStatusChecker;

    private bool isGround;
    public bool IsGround
    {
        get
        {
            /// <summary>
            /// ダッシュジャンプに関する処理を追加します。
            /// ダッシュジャンプとは、ダッシュ中の歩行スピードを保持したままジャンプすることです。
            /// ダッシュジャンプが可能かどうかはisKeepDashSpeedで判定します。（詳細はJumpStateやFallStateのExecuteWalkを参照してください）
            /// ダッシュジャンプを地面についたら止める処理を実装します。
            /// しかし、地面に触れているが遷移してほしくない条件があります。
            /// ダッシュステートからジャンプステートに遷移したときに、ジャンプステート中に地面に触れているが、上昇している瞬間があります。
            /// ↑（ほんの一瞬です）
            /// その時に、地面に触れているからといって地面に触れている判定にするのはおかしいので、ActionHandlerに
            /// 上昇中や、落下ステートなら下降中などを判定する関数を用意しています。
            /// それらを利用してダッシュジャンプも同じように、上昇中や下降中などの判定を行い、正しく接地判定を行うようにします。
            /// </summary>

            /*if (dashCtrl.IsKeepingDashSpeed_Now() && !stateMgr.IsCurrentState_DashState())
            {
                //isGroundがtrueのときにしか通さないようにします
                //なぜならisGroundがfalseなら空中にいるので、止めてはいけないからです。
                if(!isGround) return false;

                //このif文が重要です。落下中でもジャンプ中でもない場合に地面についたらStopDashを呼び出します。
                if (!actionStatusChecker.IsFallingNow() && !actionStatusChecker.isJumpingNow())
                {
                    dashCtrl.StopKeepDash();
                    Debug.Log("StopDash");
                }
            }*/

            return isGround;
        }
        private set { isGround = value; }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Ground"))
        {
            isGround = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Ground"))
        {
            isGround = false;
        }
    }
}
