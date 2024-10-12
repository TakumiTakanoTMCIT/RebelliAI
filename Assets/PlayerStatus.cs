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

        public void Init(InputHandler inputHandler, ActionStatusChecker actionStatusChecker)
        {
            this.inputHandler = inputHandler;
            this.actionStatusChecker = actionStatusChecker;

            /// <summary>
            /// 重要！フレームレートの設定
            /// </summary>
            Application.targetFrameRate = 60;

            PlayerDirection = true;
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
