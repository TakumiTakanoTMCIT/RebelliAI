using System.Collections;
using System.Collections.Generic;
using ActionStatusChk;
using KeyHandler;
using PlayerState;
using UnityEngine;

namespace PlayerInfo
{
    public class PlayerStatus : MonoBehaviour
    {
        public void Init(InputHandler inputHandler, ActionStatusChecker actionStatusChecker, PlayerStateMgr stateMgr, PlayerAnimStateHandler animStateHandler)
        {
            /// <summary>
            /// 重要！フレームレートの設定
            /// </summary>
            Application.targetFrameRate = 60;
        }

        [SerializeField]
        private float moveSpeed = 5f;
        public float MoveSpeed
        {
            get { return moveSpeed; }
        }

        [SerializeField]
        private float jumpForce = 15f;
        public float JumpForce
        {
            get { return jumpForce; }
        }

        public float FallSpeedLevel = -0.5f;

        public float JumpForceLevel = 0.5f;

        public float WallFallSpeed = 2.8f;

        public float DashSpeed = 11f;
        public float DashTime = 0.365f;

        public float delayKey_reception_time = 0.15f;

        public float damagingTime = 3f;

        public Vector2 damageForce = new Vector2(5, 5);
    }
}
