using PlayerState;
using UnityEngine;
using KeyHandler;
using Zenject;

namespace ActionStatusChk
{
    public class ActionStatusChecker : MonoBehaviour
    {
        PlayerStateMgr playerStateMgr;
        InputHandler inputHandler;
        PlayerAnimStateHandler animStateHandler;

        GroundChk groundChecker;
        SideChecker leftSideChecker, rightSideChecker, wallLeftChecker, wallRightChecker;

        [Inject]
        PlayerStats playerStatus;
        Rigidbody2D rb;

        private bool _direction;
        public bool Direction => _direction;

        public void Init(GroundChk groundChk, SideChecker left, SideChecker right, SideChecker wallleft, SideChecker wallright, Rigidbody2D rb, PlayerStateMgr playerStateMgr, InputHandler inputHandler, PlayerAnimStateHandler animStateHandler)
        {
            this.groundChecker = groundChk;
            this.leftSideChecker = left;
            this.rightSideChecker = right;
            this.wallLeftChecker = wallleft;
            this.wallRightChecker = wallright;
            this.rb = rb;
            this.playerStateMgr = playerStateMgr;
            this.inputHandler = inputHandler;
            this.animStateHandler = animStateHandler;
        }

        public bool IsMovingNow()
        {
            if (rb.velocity.x > 0.5f || rb.velocity.x < -0.5f)
                return true;
            else
                return false;
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

        public bool IsToushWallNow()
        {
            if (rightSideChecker.IsEnteredWall || leftSideChecker.IsEnteredWall) return true;
            else return false;
        }

        public bool IsFarWall(bool direction)
        {
            //右向きの場合
            if (direction)
            {
                return wallRightChecker.IsEnteredWall;
            }
            else //左向きの場合
            {
                return wallLeftChecker.IsEnteredWall;
            }
        }

        public bool WhichDirectionNow()
        {
            if (wallRightChecker.IsEnteredWall) return true;
            else return false;
        }

        //DashStateの開始時にプレイヤーの向きを設定する
        public void SetPlayerDirectionFromDashStart(bool direction)
        {
            _direction = direction;
        }

        private void Update()
        {
            if (playerStateMgr.WhatCurrentState(playerStateMgr.dashState)) return;

            if (!inputHandler.IsMoveKey()) return;

            if (inputHandler.IsMoveLeftKey()) _direction = false;

            if (inputHandler.IsMoveRightKey()) _direction = true;

            if (animStateHandler.WhatCurrentAnimState(animStateHandler.wallKickState)) return;

            //WallFall中の場合
            if(playerStateMgr.WhatCurrentState(playerStateMgr.wallFallState))
            {
                _direction = !_direction;
            }
        }
    }
}
