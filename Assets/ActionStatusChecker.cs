using System;
using PlayerInfo;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

namespace ActionStatusChk
{
    public class ActionStatusChecker : MonoBehaviour
    {
        GroundChk groundChecker;

        SideChecker leftSideChecker, rightSideChecker, wallLeftChecker, wallRightChecker;

        PlayerStatus playerStatus;

        Rigidbody2D rb;

        public void Init(GroundChk groundChk,SideChecker left,SideChecker right,SideChecker wallleft,SideChecker wallright,PlayerStatus playerStatus,Rigidbody2D rb)
        {
            this.groundChecker = groundChk;
            this.leftSideChecker = left;
            this.rightSideChecker = right;
            this.wallLeftChecker = wallleft;
            this.wallRightChecker = wallright;
            this.playerStatus = playerStatus;
            this.rb = rb;
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
    }
}
