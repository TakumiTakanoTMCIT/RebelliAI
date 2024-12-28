using PlayerInfo;
using PlayerState;
using UnityEngine;
using KeyHandler;

namespace ActionStatusChk
{
    public class ActionStatusChecker : MonoBehaviour
    {
        [SerializeField] private PlayerStateMgr playerStateMgr;
        [SerializeField] private InputHandler inputHandler;
        [SerializeField] private PlayerAnimStateHandler animStateHandler;
        [SerializeField] private ActionStatusChecker actionStatusChecker;

        GroundChk groundChecker;
        SideChecker leftSideChecker, rightSideChecker, wallLeftChecker, wallRightChecker;
        PlayerStatus playerStatus;
        Rigidbody2D rb;

        private bool _direction;
        public bool Direction => _direction;

        public void Init(GroundChk groundChk, SideChecker left, SideChecker right, SideChecker wallleft, SideChecker wallright, PlayerStatus playerStatus, Rigidbody2D rb)
        {
            this.groundChecker = groundChk;
            this.leftSideChecker = left;
            this.rightSideChecker = right;
            this.wallLeftChecker = wallleft;
            this.wallRightChecker = wallright;
            this.playerStatus = playerStatus;
            this.rb = rb;
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
        public void SetPlayerDiresctionFromDashStateBigin(bool direction)
        {
            _direction = direction;
        }

        private void Update()
        {
            if (playerStateMgr.WhatCurrentState(playerStateMgr.dashState)) return;

            if (!inputHandler.IsMoveKey()) return;

            if (inputHandler.IsMoveLeftKey()) _direction = false;

            if (inputHandler.IsMoveRightKey()) _direction = true;

            if (animStateHandler.currentState == animStateHandler.wallKickState) return;

            //WallFall中の場合
            if(playerStateMgr.WhatCurrentState(playerStateMgr.wallFallState))
            {
                _direction = !_direction;
            }
        }
    }
}
