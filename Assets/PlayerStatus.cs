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
        InputHandler inputHandler;
        ActionStatusChecker actionStatusChecker;

        private void Awake()
        {
            /// <summary>
            /// 重要！フレームレートの設定
            /// </summary>
            Application.targetFrameRate = 60;

            inputHandler = this.GetComponent<InputHandler>();
            actionStatusChecker = this.GetComponent<ActionStatusChecker>();

            PlayerDirection = true;
        }

        [SerializeField]
        private float moveSpeed = 1.0f;
        public float MoveSpeed
        {
            get { return moveSpeed; }
        }

        [SerializeField]
        private float jumpForce = 20.0f;
        public float JumpForce
        {
            get { return jumpForce; }
        }

        public float FallSpeedLevel = -0.5f;

        public float JumpForceLevel = 0.5f;

        public float WallFallSpeed = 1.0f;

        public float DashSpeed = 10.0f;
        public float DashTime = 1.0f;

        public float delayKey_reception_time = 0.1f;

        private bool PlayerDirection = true;
        public bool playerdirection
        {
            get { return PlayerDirection; }
        }

        private void Update()
        {
            if (!inputHandler.IsMoveKey()) return;

            if (inputHandler.IsMoveLeftKey()) PlayerDirection = false;

            if (inputHandler.IsMoveRightKey()) PlayerDirection = true;

            if (!actionStatusChecker.isJumpingNow() && !actionStatusChecker.IsGround() && actionStatusChecker.IsToushWallNow())
                PlayerDirection = !PlayerDirection;
        }
    }
}
