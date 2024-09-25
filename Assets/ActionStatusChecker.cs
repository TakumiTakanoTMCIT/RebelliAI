using PlayerInfo;
using UnityEditor;
using UnityEngine;

namespace ActionStatusChk
{
    public class ActionStatusChecker : MonoBehaviour
    {
        [SerializeField] private GroundChk groundChecker;

        [SerializeField] private SideChecker leftSideChecker, rightSideChecker;

        PlayerStatus playerStatus;

        Rigidbody2D rb;

        private void Awake()
        {
            playerStatus = this.GetComponent<PlayerStatus>();

            rb = this.GetComponent<Rigidbody2D>();
        }

        private void Start()
        {
            if (groundChecker == null)
            {
                Debug.LogError("GroundChecker が取得できていません。");

                /// <summary>
                /// nullの場合、エディタを一時停止
                /// </summary>
                EditorApplication.isPaused = true;
            }
        }

        public bool IsFallingNow()
        {
            if (rb.velocity.y < playerStatus.FallSpeedLevel)
            {
                return true;
            }
            else
                return false;
        }

        public bool isJumpingNow()
        {
            if (rb.velocity.y > playerStatus.JumpForceLevel)
            {
                return true;
            }
            else
                return false;
        }

        public bool IsGround()
        {
            return groundChecker.IsGround;
        }

        public bool IsWall(bool direction)
        {
            //右向きの場合
            if (direction)
            {
                return rightSideChecker.IsEnteredWall;
            }
            else //左向きの場合
            {
                return leftSideChecker.IsEnteredWall;
            }
        }
    }
}
