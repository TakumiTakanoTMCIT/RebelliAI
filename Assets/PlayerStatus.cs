using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlayerInfo
{
    public class PlayerStatus : MonoBehaviour
    {
        [SerializeField]
        private float moveSpeed = 1.0f;
        public float MoveSpeed
        {
            get { return moveSpeed; }
            private set { moveSpeed = value; }
        }

        [SerializeField]
        private float jumpForce = 20.0f;
        public float JumpForce
        {
            get { return jumpForce; }
            set { jumpForce = value; }
        }

        public float FallSpeedLevel = -0.5f;

        public float JumpForceLevel = 0.5f;

        public float WallFallSpeed = 1.0f;

        public float DashSpeed = 5.0f;
        public float DashTime = 1.0f;
    }
}
