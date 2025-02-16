using PlayerState;
using UnityEngine;
using KeyHandler;
using Zenject;
using UniRx;

namespace ActionStatusChk
{
    public class ActionStatusChecker : MonoBehaviour
    {
        PlayerStateMgr playerStateMgr;
        InputHandler inputHandler;
        Rigidbody2D rb;

        GroundChk groundChecker;
        SideChecker leftSideChecker, rightSideChecker, wallLeftChecker, wallRightChecker;

        private bool _direction;
        public bool Direction => _direction;

        private bool _isWallFallState;

        //Inject
        private IWallFallSubject wallfall;
        private PlayerStats playerStatus;

        [Inject]
        public void Construct(IWallFallSubject wallfall, PlayerStats playerStatus)
        {
            this.wallfall = wallfall;
            this.playerStatus = playerStatus;
        }

        private void Awake()
        {
            inputHandler = this.gameObject.MyGetComponent_NullChker<InputHandler>();
            playerStateMgr = this.gameObject.MyGetComponent_NullChker<PlayerStateMgr>();
            rb = this.gameObject.MyGetComponent_NullChker<Rigidbody2D>();

            wallfall.OnEnteredWallFall.Subscribe(_ =>
            {
                _isWallFallState = true;
            })
            .AddTo(this);

            wallfall.OnExitWallFall.Subscribe(_ =>
            {
                _isWallFallState = false;
            })
            .AddTo(this);
        }

        public void ChildComponentGetter(GroundChk groundChk, SideChecker left, SideChecker right, SideChecker wallleft, SideChecker wallright)
        {
            this.groundChecker = groundChk;
            this.leftSideChecker = left;
            this.rightSideChecker = right;
            this.wallLeftChecker = wallleft;
            this.wallRightChecker = wallright;
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
            //ダッシュステート時には、向きを変更しない
            if (playerStateMgr.WhatCurrentState(playerStateMgr.dashState)) return;

            //動くボタンを押していない場合と、同時押しの場合は、は向きを変更しない
            if (!inputHandler.IsMoveKey()) return;

            //左移動ボタンを押している場合
            if (inputHandler.IsMoveLeftKey()) _direction = false;

            //右移動ボタンを押している場合
            if (inputHandler.IsMoveRightKey()) _direction = true;

            //壁キック中には下の処理をしない
            //if (animStateHandler.WhatCurrentAnimState(animStateHandler.wallKickState)) return;

            //WallFall中の場合は向きを逆にする
            if (_isWallFallState)//これに一旦変えてみた
            //if (playerStateMgr.WhatCurrentState(playerStateMgr.wallFallState))
            {
                _direction = !_direction;
            }
        }
    }
}
