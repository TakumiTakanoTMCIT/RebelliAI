using System;
using System.Collections;
using System.Collections.Generic;
using ActionStatusChk;
using DG.Tweening;
using KeyHandler;
using PlayerAction;
using PlayerInfo;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

namespace PlayerState
{
    public class PlayerStateMgr : MonoBehaviour
    {
        internal ActionStatusChecker actionStatusChk;
        internal PlayerStatus playerStatus;
        internal ActionHandler actionHandler;
        internal InputHandler inputHandler;
        internal Rigidbody2D rb;
        internal PlayerDashKeepManager dashKeepManager;
        internal PlayerAnimStateHandler animHandler;

        private WallKickDelayManager wallKickManager;

        public IState idleState;
        public IState walkState;
        public IState jumpState;
        public IState fallState;
        public IState wallFallState;
        public IState wallKick;
        public IState dashState;

        internal IState currentState;
        internal bool isExecutable;

        [SerializeField] internal DashSparkFactory dashSparkFactory;
        [SerializeField] internal WallKickFactory wallKickFactory;

        [SerializeField] bool isDebugCurrentState = false;

        public void Init(Rigidbody2D rb, PlayerStatus playerStatus, ActionHandler actionHandler, ActionStatusChecker actionStatusChk, InputHandler inputHandler, PlayerDashKeepManager dashKeepManager, WallKickDelayManager wallKickManager, PlayerAnimStateHandler animStateHandler)
        {
            isExecutable = false;

            this.rb = rb;
            this.playerStatus = playerStatus;
            this.actionHandler = actionHandler;
            this.actionStatusChk = actionStatusChk;
            this.inputHandler = inputHandler;
            this.dashKeepManager = dashKeepManager;
            this.wallKickManager = wallKickManager;
            this.animHandler = animStateHandler;
        }

        private void Start()
        {
            //Time.timeScale = 0.1f;

            //Debug.Log("dash.dashsparkfactory: " + dash.dashSparkFactory);

            idleState = new Idle();
            walkState = new Walk();
            jumpState = new Jump();
            fallState = new Fall();
            wallFallState = new WallFall();
            wallKick = new WallKick();

            currentState = idleState;
            currentState.Enter(this);

            isExecutable = true;
        }

        private void Update()
        {
            if (!isExecutable) return;
            currentState.Execute(this);
        }

        public void ChangeState(IState nextState)
        {
            IState previousState;
            previousState = currentState;

            isExecutable = false;
            currentState.Exit(this);
            currentState = nextState;

            isExecutable = true;
            currentState.Enter(this);

            /// <summary>
            /// 前のステートがwallfallで次のステートがfallだったら、
            /// fallの状態になったとしてもすぐにジャンプキーを押せばwallkickを行えるようにする処理を実行します
            /// </summary>
            if (previousState == wallFallState && nextState == fallState)
            {
                wallKickManager.Start_JumpKey_AcceptingTime();
            }

            if (isDebugCurrentState)
                Debug.Log("C: " + currentState);
        }

        public bool IsCurrentState_DashState()
        {
            if (currentState == dashState)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public IState GetCurrentState()
        {
            return currentState;
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
            /// 移動を止める処理を最初に実行し初期化しておく
            /// </summary>
            stateMgr.actionHandler.Stop();

            stateMgr.animHandler.ChangeAnimState(stateMgr.animHandler.idleState);
        }

        public void Execute(PlayerStateMgr stateMgr)
        {
            /// <summary>
            /// 地面についておらず、落下中ならfallstateに遷移
            /// なぜ地面の判定を取るのかというと、落ちる床に乗っている時にプレイヤーも落下中と判定されてしまうので、それでは不自然なので判定しました
            /// </summary>
            if (!stateMgr.actionStatusChk.IsGround() && stateMgr.actionStatusChk.IsFallingNow())
            {
                stateMgr.ChangeState(stateMgr.fallState);
                return;
            }

            /// <summary>
            /// ジャンプキーが押されたらJumpStateに遷移
            /// ここで地面の判定を取るべきかと悩みますが、上で落ちているかどうかの判定が取れているので、上のif文を通過している場合は地面にいないです
            /// </sumary>
            if (stateMgr.inputHandler.IsJumpKeyDown())
            {
                stateMgr.ChangeState(stateMgr.jumpState);
                return;
            }

            /// <summary>
            /// 歩行に遷移するかどうかの判定です
            /// 壁の検知を行い、移動キーを押していたとしても、進行方向の壁にぶつかっていたら遷移せずに待機します
            /// </summary>
            if (stateMgr.inputHandler.IsMoveKey())
            {
                if (stateMgr.inputHandler.IsMoveLeftKey())
                {
                    if (stateMgr.actionStatusChk.IsWall(false))
                        return;

                    stateMgr.ChangeState(stateMgr.walkState);
                }

                if (stateMgr.inputHandler.IsMoveRightKey())
                {
                    if (stateMgr.actionStatusChk.IsWall(true))
                        return;

                    stateMgr.ChangeState(stateMgr.walkState);
                }
            }

            if (stateMgr.inputHandler.IsDashKeyDown())
            {
                if (stateMgr.playerStatus.playerdirection)
                {
                    stateMgr.dashState = new Dash(true);
                    stateMgr.ChangeState(stateMgr.dashState);
                    return;
                }
                else
                {
                    stateMgr.dashState = new Dash(false);
                    stateMgr.ChangeState(stateMgr.dashState);
                    return;
                }
            }
        }

        public void Exit(PlayerStateMgr stateMgr) { }
    }

    public class Walk : IState, IWalker
    {
        /// <summary>
        /// 歩いているかどうかのプロパティです。
        /// 正味、使用していないですが、今後の拡張性を考えて作成しました
        /// </summary>
        public bool isWalkNow { get; set; }

        public void Enter(PlayerStateMgr stateMgr)
        {
            /// <summary>
            /// 初期化
            /// </summary>
            isWalkNow = false;

            stateMgr.animHandler.ChangeAnimState(stateMgr.animHandler.walkState);
        }

        public void Execute(PlayerStateMgr stateMgr)
        {
            ///<summary>
            /// 落下中ならFallStateに遷移するが、↓
            /// 地面についているなら実行しない。これもまた、落ちる床に乗っている時にfallstateにいかないようにするためです。(idlestateにも同じことを書きました)
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
            /// ダッシュに遷移する処理
            /// </summary>
            if (stateMgr.inputHandler.IsDashKeyDown())
            {
                if (stateMgr.inputHandler.IsMoveLeftKey())
                {
                    stateMgr.dashState = new Dash(false);
                    stateMgr.ChangeState(stateMgr.dashState);
                    return;
                }

                if (stateMgr.inputHandler.IsMoveRightKey())
                {
                    stateMgr.dashState = new Dash(true);
                    stateMgr.ChangeState(stateMgr.dashState);
                    return;
                }
            }

            /// <summary>
            /// 上の条件をすべて回避したら歩く処理を実行
            /// </summary>
            ExecuteWalk(stateMgr);
        }

        /// <summary>
        /// 歩行の処理はすべてここに書く
        /// </summary>
        public void ExecuteWalk(PlayerStateMgr stateMgr)
        {
            /// <summary>
            /// どちらのキーも押されていない場合、同時押しはIdleStateに遷移
            /// IsMoveKeyは左右のキーどちらかが押されているかどうか、左右の同時押しをしていないか、何も押されていないかを判定しています
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
            /// WalkStateに入っている時点で歩行をしています。そのため、壁にぶつかっていたらIdleに遷移します
            /// 一瞬違和感を覚えますが、WalkStateに入ってる時点で歩行をしていると思うと納得できるかと思います
            /// </summary>
            if (stateMgr.inputHandler.IsMoveLeftKey())
            {
                if (stateMgr.actionStatusChk.IsWall(false))
                {
                    stateMgr.ChangeState(stateMgr.idleState);
                    return;
                }

                stateMgr.actionHandler.Walk(false);
            }
            else if (stateMgr.inputHandler.IsMoveRightKey())
            {
                if (stateMgr.actionStatusChk.IsWall(true))
                {
                    stateMgr.ChangeState(stateMgr.idleState);
                    return;
                }

                stateMgr.actionHandler.Walk(true);
            }
        }

        public void Exit(PlayerStateMgr stateMgr) { }
    }

    public class Dash : IState
    {
        DashSparkFactory dashSparkFactory;
        PlayerDashTimeCtrl dashTimeCtrl;

        bool direction;

        //インスタンスした時にdirectionを渡すようにしたい
        public Dash(bool direction)
        {
            this.direction = direction;
        }

        public void Enter(PlayerStateMgr stateMgr)
        {
            stateMgr.playerStatus.SetPlayerDiresctionFromDashStateBigin(direction);

            if (dashSparkFactory == null)
            {
                dashSparkFactory = stateMgr.dashSparkFactory.gameObject.MyGetComponent_NullChker<DashSparkFactory>();
            }

            dashSparkFactory.MakeEffect();

            dashTimeCtrl = stateMgr.gameObject.MyGetComponent_NullChker<PlayerDashTimeCtrl>();

            stateMgr.actionHandler.Dash(direction);
            dashTimeCtrl.StartDashTimeCtrl();

            stateMgr.animHandler.ChangeAnimState(stateMgr.animHandler.dashState);
        }

        public void Execute(PlayerStateMgr stateMgr)
        {
            /// <summary>
            /// ダッシュが終了したときの処理
            /// どこに遷移するか判断します
            /// </summary>
            if (dashTimeCtrl.IsDashNow == false)
            {
                if (!stateMgr.inputHandler.IsMoveKey())
                    stateMgr.ChangeState(stateMgr.idleState);
                else if (stateMgr.inputHandler.IsMoveLeftKey())
                    stateMgr.ChangeState(stateMgr.walkState);
                else if (stateMgr.inputHandler.IsMoveRightKey())
                    stateMgr.ChangeState(stateMgr.walkState);
                else
                {
                    //ここに到達しないが、万が一のために書いておく
                    Debug.LogError("DashStateにて例外が発生!すぐに確認してください");
                    stateMgr.ChangeState(stateMgr.idleState);
                }
                return;
            }

            if (stateMgr.actionStatusChk.IsFallingNow())
            {
                /// <summary>
                /// ダッシュしている最中に落下したら、ダッシュを維持したままFallに遷移
                /// </summary>
                stateMgr.dashKeepManager.KeepDashSpeed();

                stateMgr.ChangeState(stateMgr.fallState);
                return;
            }

            if (stateMgr.inputHandler.IsJumpKeyDown())
            {
                stateMgr.dashKeepManager.KeepDashSpeed();

                stateMgr.ChangeState(stateMgr.jumpState);
                return;
            }

            if (stateMgr.actionStatusChk.IsWall(direction))
            {
                stateMgr.ChangeState(stateMgr.idleState);
                return;
            }

            //ダッシュ中にダッシュキーを押されたら、ダッシュをし直す
            if (stateMgr.inputHandler.IsDashKeyDown())
            {
                dashTimeCtrl.StopDashTimeCtrl();

                /// <summary>
                /// 向きをコンストラクタで渡し、ダッシュのステートに遷移する
                /// </summary>
                if (stateMgr.inputHandler.IsMoveLeftKey())
                {
                    stateMgr.dashState = new Dash(false);
                    stateMgr.ChangeState(stateMgr.dashState);
                    return;
                }

                if (stateMgr.inputHandler.IsMoveRightKey())
                {
                    stateMgr.dashState = new Dash(true);
                    stateMgr.ChangeState(stateMgr.dashState);
                    return;
                }
            }
        }

        public void Exit(PlayerStateMgr stateMgr)
        {
            stateMgr.actionHandler.Stop();
            dashTimeCtrl.StopDashTimeCtrl();
        }
    }

    public class Jump : IState, IWalker
    {
        //歩いているかどうかのプロパティです
        //Walkクラスのコメントを参照してください
        //IWalkerインターフェースを実装しているため、このプロパティを持っています
        public bool isWalkNow { get; set; }

        public void Enter(PlayerStateMgr stateMgr)
        {
            isWalkNow = false;

            stateMgr.actionHandler.Jump(stateMgr.playerStatus.JumpForce);

            stateMgr.animHandler.ChangeAnimState(stateMgr.animHandler.jumpState);
        }

        public void Execute(PlayerStateMgr stateMgr)
        {
            if (stateMgr.actionStatusChk.IsGround())
            {
                /// <summary>
                /// 地面についていても上昇中ならジャンプ状態を維持
                /// </summary>
                if (stateMgr.actionStatusChk.isJumpingNow())
                    return;

                if (isWalkNow == false)
                    stateMgr.ChangeState(stateMgr.idleState);
                else
                    stateMgr.ChangeState(stateMgr.walkState);
            }

            /// <summary>
            /// 落下を始めたらFallに遷移
            /// </summary>
            if (stateMgr.actionStatusChk.IsFallingNow())
            {
                stateMgr.ChangeState(stateMgr.fallState);
                return;
            }

            if (stateMgr.inputHandler.IsJumpKeyDown())
            {
                if (
                    stateMgr.actionStatusChk.IsFarWall(false)
                    || stateMgr.actionStatusChk.IsFarWall(true)
                )
                    stateMgr.ChangeState(stateMgr.wallKick);
            }

            /// <summary>
            /// 上の条件をすべて回避したらジャンプ中は移動可能
            /// </summary>
            ExecuteWalk(stateMgr);
        }

        public void ExecuteWalk(PlayerStateMgr stateMgr)
        {
            if (!stateMgr.inputHandler.IsMoveKey())
            {
                /// <summary>
                /// 一度しか停止は実行できないようにする
                /// </summary>
                if (isWalkNow)
                {
                    stateMgr.actionHandler.Stop();
                    isWalkNow = false;
                }
                return;
            }

            /// <summary>
            /// 上の条件を回避したら、移動ができるようにする
            /// </summary>
            isWalkNow = true;

            if (stateMgr.inputHandler.IsMoveLeftKey())
            {
                stateMgr.actionHandler.Walk(false);
                return;
            }

            if (stateMgr.inputHandler.IsMoveRightKey())
            {
                stateMgr.actionHandler.Walk(true);
                return;
            }
        }

        public void Exit(PlayerStateMgr stateMgr) { }
    }

    public class Fall : IState, IWalker
    {
        //IWalkerインターフェースを実装しているため、このプロパティを持っています
        public bool isWalkNow { get; set; }

        public void Enter(PlayerStateMgr stateMgr)
        {
            stateMgr.animHandler.ChangeAnimState(stateMgr.animHandler.fallState);

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

            if (stateMgr.inputHandler.IsMoveLeftKey())
            {
                isWalkNow = stateMgr.actionHandler.Walk(false);
                return;
            }

            if (stateMgr.inputHandler.IsMoveRightKey())
            {
                isWalkNow = stateMgr.actionHandler.Walk(true);
                return;
            }
        }

        public void Execute(PlayerStateMgr stateMgr)
        {
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

            ExecuteWalk(stateMgr);

            if (stateMgr.inputHandler.IsMoveKey())
            {
                if (stateMgr.inputHandler.IsMoveLeftKey())
                {
                    if (stateMgr.actionStatusChk.IsFarWall(false) || stateMgr.actionStatusChk.IsWall(false))
                    {
                        stateMgr.ChangeState(stateMgr.wallFallState);
                        return;
                    }
                }
                else if (stateMgr.inputHandler.IsMoveRightKey())
                {
                    if (stateMgr.actionStatusChk.IsFarWall(true) || stateMgr.actionStatusChk.IsWall(true))
                    {
                        stateMgr.ChangeState(stateMgr.wallFallState);
                        return;
                    }
                }
            }
        }

        public void Exit(PlayerStateMgr stateMgr) { }
    }

    public class WallFall : IState
    {
        //壁にぶつかっている方向を判定するための変数
        //enterしたら初期化される
        bool wall_facing_which;

        /// <summary>
        /// WallFallからWallKickに遷移するときにExitしたのにもかかわらず数フレームだけWallFallが実行されてしまうのでExitしたらfalseにしてTrueでないと
        /// WallFallの処理をできないようにしました（力技☆）
        /// </summary>
        bool IsWallFallExecutable;

        public void Enter(PlayerStateMgr stateMgr)
        {
            /// <summary>
            /// 初期化
            /// </summary>
            IsWallFallExecutable = true;

            stateMgr.animHandler.ChangeAnimState(stateMgr.animHandler.wallFallState);

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
            if (stateMgr.inputHandler.IsJumpKeyDown())
            {
                stateMgr.ChangeState(stateMgr.wallKick);
            }

            //↑をすべて回避したら壁ずりをする
            WallFalling(stateMgr);
        }

        private void WallFalling(PlayerStateMgr stateMgr)
        {
            if (!IsWallFallExecutable)
                return;

            stateMgr.rb.velocity = new Vector2(0, -stateMgr.playerStatus.WallFallSpeed);
        }

        public void Exit(PlayerStateMgr stateMgr)
        {
            //ExitしたらfalseにしてWallFallの処理をできないようにする
            IsWallFallExecutable = false;
        }
    }

    public class WallKick : IState, IWalker
    {
        WallKickFactory wallKickFactory;

        public bool isWalkNow { get; set; }

        private bool isOneTimeAbleTo_TurnOn_KeepDashSpeed = false;

        public void Enter(PlayerStateMgr stateMgr)
        {
            isWalkNow = false;
            isOneTimeAbleTo_TurnOn_KeepDashSpeed = false;

            stateMgr.animHandler.ChangeAnimState(stateMgr.animHandler.wallKickState);

            if (wallKickFactory == null)
            {
                wallKickFactory = stateMgr.wallKickFactory.gameObject.MyGetComponent_NullChker<WallKickFactory>();
            }
            wallKickFactory.MakeEffect(stateMgr.transform);

            /// <summary>
            /// スピードを0にして、壁キックを行う
            /// </summary>
            stateMgr.rb.velocity = Vector2.zero;
            stateMgr.actionHandler.Jump(stateMgr.playerStatus.JumpForce);

            /// <summary>
            /// ダッシュキックの受付を開始する
            /// </summary>
            //dashKickKeyAcceptingHandler.Start_DashKickKey_AcceptingTime();
        }

        public void Execute(PlayerStateMgr stateMgr)
        {
            if (stateMgr.actionStatusChk.IsFallingNow())
            {
                if (stateMgr.inputHandler.IsMoveLeftKey() && stateMgr.actionStatusChk.IsWall(false))
                {
                    stateMgr.animHandler.ChangeAnimState(stateMgr.animHandler.wallFallState);
                    stateMgr.ChangeState(stateMgr.wallFallState);
                    return;
                }

                if (stateMgr.inputHandler.IsMoveRightKey() && stateMgr.actionStatusChk.IsWall(true))
                {
                    stateMgr.animHandler.ChangeAnimState(stateMgr.animHandler.wallFallState);
                    stateMgr.ChangeState(stateMgr.wallFallState);
                    return;
                }


                stateMgr.ChangeState(stateMgr.fallState);
                return;
            }

            if (stateMgr.actionStatusChk.IsGround())
            {
                //上昇中ならジャンプ状態を維持
                if (stateMgr.actionStatusChk.isJumpingNow())
                {
                    return;
                }

                /// <summary>
                /// 歩きながら地面についときと歩かずに地面についたときで遷移先を変える処理
                /// </summary>
                if (!isWalkNow)
                    stateMgr.ChangeState(stateMgr.idleState);
                else
                    stateMgr.ChangeState(stateMgr.walkState);
                return;
            }

            //ボタンの押した方向に壁があった状態で、壁キックを行った場合は壁キックを再度行う
            if (stateMgr.inputHandler.IsJumpKeyDown())
            {
                if (stateMgr.inputHandler.IsMoveLeftKey() && stateMgr.actionStatusChk.IsWall(false))
                {
                    stateMgr.ChangeState(stateMgr.wallKick);
                    return;
                }

                if (stateMgr.inputHandler.IsMoveRightKey() && stateMgr.actionStatusChk.IsWall(true))
                {
                    stateMgr.ChangeState(stateMgr.wallKick);
                    return;
                }
            }

            ExecuteWalk(stateMgr);

            /// <summary>
            /// どの壁にも触れなくなったとき、ダッシュキーを押されていたらkeepdashにする
            /// 一度のみ実行可能(重複防止の為)
            /// </summary>
            if (!stateMgr.actionStatusChk.IsWall(true) && !stateMgr.actionStatusChk.IsWall(false))
            {
                if (stateMgr.inputHandler.IsDashKeyDown() || stateMgr.inputHandler.IsDashKey())
                {
                    if (isOneTimeAbleTo_TurnOn_KeepDashSpeed)
                        return;

                    stateMgr.dashKeepManager.KeepDashSpeed();
                    isOneTimeAbleTo_TurnOn_KeepDashSpeed = true;
                }
            }
        }

        public void ExecuteWalk(PlayerStateMgr stateMgr)
        {
            if (!stateMgr.inputHandler.IsMoveKey())
            {
                /// <summary>
                /// 同時押しの判定を行っています。
                /// </summary>
                isWalkNow = false;
                stateMgr.actionHandler.Stop();
                return;
            }

            if (stateMgr.inputHandler.IsMoveLeftKey())
            {
                stateMgr.actionHandler.Walk(false);
                isWalkNow = true;
                return;
            }

            if (stateMgr.inputHandler.IsMoveRightKey())
            {
                stateMgr.actionHandler.Walk(true);
                isWalkNow = true;
                return;
            }
        }

        public void Exit(PlayerStateMgr stateMgr) { }
    }
}
