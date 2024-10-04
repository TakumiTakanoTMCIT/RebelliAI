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
        [SerializeField]ChargeShot_TimeHandler chargeShotTimeHandler;

        InputHandler inputHandler;
        ActionStatusChecker actionStatusChecker;

        private void Awake()
        {
            chargeShotTimeHandler.Init(this);
            inputHandler = this.GetComponent<InputHandler>();
            actionStatusChecker = this.GetComponent<ActionStatusChecker>();
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

        public bool playerDirection = true;

        private void Update()
        {
            if (!inputHandler.IsMoveKey()) return;

            if (inputHandler.IsMoveLeftKey()) playerDirection = false;

            if (inputHandler.IsMoveRightKey()) playerDirection = true;

            if (actionStatusChecker.IsToushWallNow())
                playerDirection = !playerDirection;
        }
    }
}
