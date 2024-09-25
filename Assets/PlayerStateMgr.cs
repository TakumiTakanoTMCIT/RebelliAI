using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KeyHandler;
using PlayerAction;
using PlayerInfo;
using ActionStatusChk;
using System;
using UnityEditor;

namespace PlayerState
{
    public class PlayerStateMgr : MonoBehaviour
    {
        internal ActionStatusChecker actionStatusChk;

        internal PlayerStatus playerStatus;

        internal ActionHandler actionHandler;

        internal InputHandler inputHandler;

        internal Rigidbody2D rb;

        /// <summary>
        /// Stateをインスタンス化して保持するための変数
        /// </summary>
        public IState idleState;
        public IState walkState;
        public IState jumpState;
        public IState fallState;
        public IState wallFallState;

        IState currentState;

        private void Awake()
        {
            rb = this.GetComponent<Rigidbody2D>();

            playerStatus = this.GetComponent<PlayerStatus>();
            inputHandler = this.GetComponent<InputHandler>();
            actionHandler = this.GetComponent<ActionHandler>();
            actionStatusChk = this.GetComponent<ActionStatusChecker>();
        }

        private void Start()
        {
            idleState = new Idle();
            walkState = new Walk();
            jumpState = new Jump();
            fallState = new Fall();
            wallFallState = new WallFall();

            currentState = idleState;
            Debug.Log("currentState is " + currentState);
            currentState.Enter(this);
        }

        private void Update()
        {
            currentState.Execute(this);
        }

        public void ChangeState(IState nextState)
        {
            currentState.Exit(this);
            currentState = nextState;
            currentState.Enter(this);
            Debug.Log("currentState is " + currentState);
        }
    }

    public interface IState
    {
        void Enter(PlayerStateMgr stateMgr);
        void Execute(PlayerStateMgr stateMgr);
        void Exit(PlayerStateMgr stateMgr);
    }

    public interface IWalker
    {
        /// <summary>
        /// 歩くインターフェース
        /// </summary>

        bool isWalkNow { get; set; }
        void ExecuteWalk(PlayerStateMgr stateMgr);
    }

    public class Idle : IState
    {
        public void Enter(PlayerStateMgr stateMgr)
        {
            /// <summary>
            /// 移動を止める処理を最初に実行
            /// </summary>
            stateMgr.actionHandler.Stop();
        }

        public void Execute(PlayerStateMgr stateMgr)
        {
            /// <summary>
            /// 地面についておらず、落下中ならfallstateに遷移
            /// </summary>
            if (!stateMgr.actionStatusChk.IsGround() && stateMgr.actionStatusChk.IsFallingNow())
            {
                stateMgr.ChangeState(stateMgr.fallState);
                return;
            }

            /// <summary>
            /// ジャンプキーが押されたらJumpStateに遷移
            /// </sumary>
            if (stateMgr.inputHandler.IsJumpKeyDown())
            {
                stateMgr.ChangeState(stateMgr.jumpState);
                return;
            }

            /// <summary>
            /// 壁の検知を行い、壁にぶつかっていたらIdleに待機
            /// </summary>
            if (stateMgr.inputHandler.IsMoveLeftKey())
            {
                if (stateMgr.actionStatusChk.IsWall(false))
                {
                    return;
                }

                stateMgr.ChangeState(stateMgr.walkState);
            }

            if (stateMgr.inputHandler.IsMoveRightKey())
            {
                if (stateMgr.actionStatusChk.IsWall(true))
                {
                    return;
                }

                stateMgr.ChangeState(stateMgr.walkState);
            }
        }

        public void Exit(PlayerStateMgr stateMgr)
        {

        }
    }

    public class Walk : IState, IWalker
    {
        public bool isWalkNow { get; set; }

        public void Enter(PlayerStateMgr stateMgr)
        {
            isWalkNow = false;

            ///<summary
            /// Executeと同じ処理を実行
            ///</summary>
            Execute(stateMgr);
        }

        public void Execute(PlayerStateMgr stateMgr)
        {
            ///<summary>
            /// 落下中ならFallStateに遷移
            /// 地面についているなら実行しない
            /// </summary>
            if (!stateMgr.actionStatusChk.IsGround() && stateMgr.actionStatusChk.IsFallingNow())
            {
                stateMgr.ChangeState(stateMgr.fallState);
                return;
            }

            /// <summary>
            /// ジャンプキーが押されたらJumpStateに遷移
            /// </summary>
            if (stateMgr.inputHandler.IsJumpKeyDown())
            {
                stateMgr.ChangeState(stateMgr.jumpState);
                return;
            }

            /// <summary>
            /// 上の条件をすべて回避したら歩く処理を実行
            /// </summary>
            ExecuteWalk(stateMgr);
        }

        public void ExecuteWalk(PlayerStateMgr stateMgr)
        {
            /// <summary>
            /// どちらのキーも押されていない場合、同時押しはIdleStateに遷移
            /// </summary>
            if (!stateMgr.inputHandler.IsMoveKey())
            {
                stateMgr.ChangeState(stateMgr.idleState);
                isWalkNow = false;
                return;
            }

            isWalkNow = true;

            /// <summary>
            /// 壁の検知を行い、壁にぶつかっていたらIdleに遷移します
            /// </summary>
            if (stateMgr.inputHandler.IsMoveLeftKey())
            {
                if (stateMgr.actionStatusChk.IsWall(false))
                {
                    stateMgr.ChangeState(stateMgr.idleState);
                    return;
                }

                /// <summary>
                /// 壁検知を行い、回避したら移動
                /// </summary>
                stateMgr.actionHandler.Walk(false, stateMgr.playerStatus.MoveSpeed);
            }
            else if (stateMgr.inputHandler.IsMoveRightKey())
            {
                if (stateMgr.actionStatusChk.IsWall(true))
                {
                    stateMgr.ChangeState(stateMgr.idleState);
                    return;
                }

                /// <summary>
                /// 壁検知を行い、回避したら移動
                /// </summary>
                stateMgr.actionHandler.Walk(true, stateMgr.playerStatus.MoveSpeed);
            }
        }

        public void Exit(PlayerStateMgr stateMgr)
        {

        }
    }

    public class Jump : IState, IWalker
    {
        public bool isWalkNow { get; set; }

        public void Enter(PlayerStateMgr stateMgr)
        {
            isWalkNow = false;

            /// <summary>
            /// ジャンプする
            /// </summary>
            stateMgr.actionHandler.Jump(stateMgr.playerStatus.JumpForce);
        }

        public void Execute(PlayerStateMgr stateMgr)
        {
            ///<summary>
            /// 地面についたらついたら↓
            /// 移動中ならWalkに遷移
            /// 止まっていたらIdleに遷移
            /// </summary>
            if (stateMgr.actionStatusChk.IsGround())
            {
                if (stateMgr.actionStatusChk.isJumpingNow())
                {
                    /// <summary>
                    /// 地面についていても上昇中ならジャンプ状態を維持
                    /// </summary>
                    return;
                }

                /// <summary>
                /// 上の条件を回避したら遷移
                /// </summary>
                if (!isWalkNow)
                {
                    stateMgr.ChangeState(stateMgr.idleState);
                    return;
                }
                else
                {
                    stateMgr.ChangeState(stateMgr.walkState);
                    return;
                }
            }

            /// <summary>
            /// 落下を始めたらFallに遷移
            /// </summary>
            if (stateMgr.actionStatusChk.IsFallingNow())
            {
                stateMgr.ChangeState(stateMgr.fallState);
                return;
            }

            /// <summary>
            /// 上の条件をすべて回避したらジャンプ中は移動可能
            /// </summary>
            ExecuteWalk(stateMgr);
        }

        public void ExecuteWalk(PlayerStateMgr stateMgr)
        {
            /// <summary>
            /// どちらのキーも押されていない場合、同時押しは移動を止める
            /// </summary>
            if (!stateMgr.inputHandler.IsMoveKey())
            {
                stateMgr.actionHandler.Stop();
                isWalkNow = false;
                return;
            }


            /// <summary>
            /// 上の条件を回避したら、移動ができるようにする
            /// </summary>
            isWalkNow = true;

            if (stateMgr.inputHandler.IsMoveLeftKey())
            {
                stateMgr.actionHandler.Walk(false, stateMgr.playerStatus.MoveSpeed);
            }
            else if (stateMgr.inputHandler.IsMoveRightKey())
            {
                stateMgr.actionHandler.Walk(true, stateMgr.playerStatus.MoveSpeed);
            }
        }

        public void Exit(PlayerStateMgr stateMgr)
        {

        }
    }

    public class Fall : IState, IWalker
    {
        public bool isWalkNow { get; set; }

        public void Enter(PlayerStateMgr stateMgr)
        {
            /// <summary>
            /// 初期化
            /// </summary>
            isWalkNow = false;
        }

        public void ExecuteWalk(PlayerStateMgr stateMgr)
        {
            /// <summary>
            /// どちらのキーも押されていない場合か、同時押しの場合は、移動を止める
            /// </summary>
            if (!stateMgr.inputHandler.IsMoveKey())
            {
                stateMgr.actionHandler.Stop();
                isWalkNow = false;
                return;
            }

            /// <summary>
            /// 上の条件を回避したら、移動を再開
            /// </summary>

            /// <summary>
            /// isWalkNowがtrueの場合は、移動可能であり、falseの場合は移動不可
            /// </summary>

            if (stateMgr.inputHandler.IsMoveLeftKey())
            {
                /// Debug.Logしていないのでわかりにくいが、移動可能な場合はtrueを返しています
                isWalkNow = stateMgr.actionHandler.Walk(false, stateMgr.playerStatus.MoveSpeed);
            }
            else if (stateMgr.inputHandler.IsMoveRightKey())
            {
                isWalkNow = stateMgr.actionHandler.Walk(true, stateMgr.playerStatus.MoveSpeed);
            }
        }

        public void Execute(PlayerStateMgr stateMgr)
        {
            /// <summary>
            /// ただ地面についたらIdleに遷移
            /// 移動中に地面についたらWalkに遷移
            /// </summary>
            if (stateMgr.actionStatusChk.IsGround())
            {
                if (!isWalkNow)
                {
                    stateMgr.ChangeState(stateMgr.idleState);
                    return;
                }
                else
                {
                    stateMgr.ChangeState(stateMgr.walkState);
                    return;
                }
            }

            /// <summary>
            /// wallfallに遷移するかどうかを判定する
            /// 壁にぶつかっていて、壁に向かって移動をしていたらWallFallに遷移
            /// </summary>
            if (stateMgr.inputHandler.IsMoveLeftKey())
            {
                if (stateMgr.actionStatusChk.IsWall(false))
                {
                    stateMgr.ChangeState(stateMgr.wallFallState);
                    return;
                }
            }
            else if (stateMgr.inputHandler.IsMoveRightKey())
            {
                if (stateMgr.actionStatusChk.IsWall(true))
                {
                    stateMgr.ChangeState(stateMgr.wallFallState);
                    return;
                }
            }

            /// <summary>
            /// 上の処理を回避したら、落下中は移動可能
            /// </summary>
            ExecuteWalk(stateMgr);

            /// <summary>
            /// 可能性としては薄いが、上昇したらJumpに遷移を考慮（ダブルジャンプの時に書いたりすると思うのでその時に書く）
            /// </summary>
        }

        public void Exit(PlayerStateMgr stateMgr)
        {

        }
    }

    public class WallFall : IState
    {
        bool wall_facing_which;

        public void Enter(PlayerStateMgr stateMgr)
        {
            /// <summary>
            /// どちらの壁にもぶつかっていない場合はFallに遷移
            /// </summary>
            DidnotAllWallHit(stateMgr);

            //もし地面についているならIdleに遷移
            if (stateMgr.actionStatusChk.IsGround())
            {
                stateMgr.ChangeState(stateMgr.idleState);
                return;
            }

            /// <summary>
            /// 壁にぶつかっている方向を判定
            /// </summary>
            if (stateMgr.actionStatusChk.IsWall(true))
            {
                wall_facing_which = true;
            }
            else if (stateMgr.actionStatusChk.IsWall(false))
            {
                wall_facing_which = false;
            }
        }

        public void Execute(PlayerStateMgr stateMgr)
        {
            /// <summary>
            /// どちらの壁にふれなくなったら問答無用でFallに遷移させる
            /// </summary>
            DidnotAllWallHit(stateMgr);

            //地面についたらIdleに遷移
            if (stateMgr.actionStatusChk.IsGround())
            {
                stateMgr.ChangeState(stateMgr.idleState);
                return;
            }


            //Fallに遷移させるかどうかの判定の処理を書く
            if (wall_facing_which)
            {
                //もし壁にぶつからなくなったらFallに遷移
                if (!stateMgr.actionStatusChk.IsWall(true))
                {
                    stateMgr.ChangeState(stateMgr.fallState);
                    return;
                }

                //壁に向かって歩くのをやめたらFallに遷移
                if (!stateMgr.inputHandler.IsMoveRightKey())
                {
                    stateMgr.ChangeState(stateMgr.fallState);
                    return;
                }
            }
            else
            {
                //もし壁にぶつからなくなったらFallに遷移
                if (!stateMgr.actionStatusChk.IsWall(false))
                {
                    stateMgr.ChangeState(stateMgr.fallState);
                    return;
                }

                //壁に向かって歩くのをやめたらFallに遷移
                if (!stateMgr.inputHandler.IsMoveLeftKey())
                {
                    stateMgr.ChangeState(stateMgr.fallState);
                    return;
                }
            }

            //壁キック判定
            if(stateMgr.inputHandler.IsJumpKeyDown())
            {
                stateMgr.ChangeState(stateMgr.)
            }

            //↑をすべて回避したら壁ずりをする
            WallFalling(stateMgr);
        }

        private void DidnotAllWallHit(PlayerStateMgr stateMgr)
        {
            if (!stateMgr.actionStatusChk.IsWall(true) && !stateMgr.actionStatusChk.IsWall(false))
            {
                stateMgr.ChangeState(stateMgr.fallState);
                return;
            }
        }
        //やれ！最初に壁にぶつかってきた方向を取得できたので、その方向に移動キーを押し続けるのをやめたらFallに遷移させるのを書け！

        private void WallFalling(PlayerStateMgr stateMgr)
        {
            stateMgr.rb.velocity = new Vector2(0, -stateMgr.playerStatus.WallFallSpeed);
        }

        public void Exit(PlayerStateMgr stateMgr)
        {

        }
    }

    public class WallKick : IState
    {
        public void Enter(PlayerStateMgr stateMgr)
        {

        }

        public void Execute(PlayerStateMgr stateMgr)
        {

        }

        public void Exit(PlayerStateMgr stateMgr)
        {

        }
    }
}
