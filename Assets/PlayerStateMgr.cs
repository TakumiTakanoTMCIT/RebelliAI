using System;
using ActionStatusChk;
using KeyHandler;
using PlayerAction;
using UnityEngine;
using HPBar;
using Cysharp.Threading.Tasks;
using UniRx;
using Zenject;

namespace PlayerState
{

    public class PlayerStateMgr : MonoBehaviour
    {
        [Inject]
        private DiContainer container;
        [Inject]
        private ActionHandler actionHandler;

        [SerializeField]
        private bool isDebugCurrentState = false;

        private PlayerAnimStateHandler animHandler;
        private IState currentState;
        private bool isExecutable;
        private InputHandler inputHandler;
        private ActionStatusChecker actionStatusChecker;
        private Rigidbody2D rb;
        private PlayerStateData playerStateData;
        private WallKickDelayManager wallKickDelayManager;

        public IState idleState, walkState, jumpState, fallState, wallFallState, wallKick,
        dashState, damageState, deathState;

        public void Init(Rigidbody2D rb, PlayerAnimStateHandler animStateHandler, ActionStatusChecker actionStatusChecker, InputHandler inputHandler, ActionHandler actionHandler,WallKickDelayManager wallKickDelayManager)
        {
            this.actionHandler = actionHandler;
            this.inputHandler = inputHandler;
            this.rb = rb;
            this.animHandler = animStateHandler;
            this.actionStatusChecker = actionStatusChecker;
            this.wallKickDelayManager = wallKickDelayManager;
        }

        private void Awake()
        {
            playerStateData = new PlayerStateData(inputHandler, actionHandler, actionStatusChecker, animHandler);

            idleState = new Idle(playerStateData);
            walkState = new Walk(playerStateData);
            dashState = new Dash(playerStateData);
            jumpState = new Jump(playerStateData);
            fallState = new Fall(playerStateData);
            wallFallState = new WallFall(playerStateData);
            wallKick = new WallKick(playerStateData);
            damageState = new DamageState(playerStateData);
            deathState = new DeathState();

            //Injectしています
            container.Inject(dashState);
            container.Inject(wallKick);
            container.Inject(damageState);
            container.Inject(wallFallState);

            GameFlowManager.StartBattleAction.Subscribe(_ =>
            {
                ChangeState(idleState);
            })
            .AddTo(this);

            isExecutable = false;
        }

        //イベントの登録
        private void OnEnable()
        {
            HPBarHandler.onPlayerDeath += OnDeath;
            HPBarHandler.onPlayerDamage += OnDamage;
            wallKickDelayManager.OnWallKickRequest += () => ChangeState(wallKick);
        }

        //イベントの登録解除
        private void OnDisable()
        {
            HPBarHandler.onPlayerDeath -= OnDeath;
            HPBarHandler.onPlayerDamage -= OnDamage;

            wallKickDelayManager.OnWallKickRequest += () => ChangeState(wallKick);
        }

        private void Update()
        {
            if (!isExecutable) return;
            currentState.Execute(this);
        }

        public void ChangeState(IState nextState)
        {
            if (currentState == null)
            {
                currentState = nextState;
                nextState.Enter(this);
                isExecutable = true;
                return;
            }

            isExecutable = false;
            currentState.Exit(this);
            currentState = nextState;

            isExecutable = true;
            currentState.Enter(this);

            if (isDebugCurrentState)
                Debug.Log("C: " + currentState);
        }

        void OnDamage()
        {
            ChangeState(damageState);
        }

        void OnDeath()
        {
            ChangeState(deathState);

            actionHandler.StopX();
            actionHandler.StopY();
            //TODO:ここで重力を操作するのは責務に反するので、後で修正
            rb.gravityScale = 0;
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

        public bool WhatCurrentState(IState checkeState)
        {
            return currentState == checkeState;
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

    public class PlayerStateData
    {
        public InputHandler InputHandler { get; }
        public ActionHandler ActionHandler { get; }
        public ActionStatusChecker ActionStatusChecker { get; }
        public PlayerAnimStateHandler AnimHandler { get; }

        public PlayerStateData(InputHandler inputHandler, ActionHandler actionHandler, ActionStatusChecker actionStatusChecker, PlayerAnimStateHandler animHandler)
        {
            InputHandler = inputHandler;
            ActionHandler = actionHandler;
            ActionStatusChecker = actionStatusChecker;
            AnimHandler = animHandler;
        }
    }

    public class Idle : IState
    {
        private readonly PlayerStateData stateData;

        public Idle(PlayerStateData playerStateData)
        {
            this.stateData = playerStateData;
        }

        public void Enter(PlayerStateMgr stateMgr)
        {
            /// <summary>
            /// 移動を止める処理を最初に実行し初期化しておく
            /// </summary>
            stateData.ActionHandler.StopX();

            stateData.AnimHandler.ChangeAnimState(stateData.AnimHandler.idleState);
        }

        public void Execute(PlayerStateMgr stateMgr)
        {
            /// <summary>
            /// 地面についておらず、落下中ならfallstateに遷移
            /// なぜ地面の判定を取るのかというと、落ちる床に乗っている時にプレイヤーも落下中と判定されてしまうので、それでは不自然なので判定しました
            /// </summary>
            if (!stateData.ActionStatusChecker.IsGround() && stateData.ActionStatusChecker.IsFallingNow())
            {
                stateMgr.ChangeState(stateMgr.fallState);
                return;
            }

            /// <summary>
            /// ジャンプキーが押されたらJumpStateに遷移
            /// ここで地面の判定を取るべきかと悩みますが、上で落ちているかどうかの判定が取れているので、上のif文を通過している場合は地面にいないです
            /// </sumary>
            if (stateData.InputHandler.IsJumpKeyDown())
            {
                stateMgr.ChangeState(stateMgr.jumpState);
                return;
            }

            /// <summary>
            /// 歩行に遷移するかどうかの判定です
            /// 壁の検知を行い、移動キーを押していたとしても、進行方向の壁にぶつかっていたら遷移せずに待機します
            /// </summary>
            if (stateData.InputHandler.IsMoveKey())
            {
                if (stateData.InputHandler.IsMoveLeftKey())
                {
                    if (stateData.ActionStatusChecker.IsWall(false))
                        return;

                    stateMgr.ChangeState(stateMgr.walkState);
                }

                if (stateData.InputHandler.IsMoveRightKey())
                {
                    if (stateData.ActionStatusChecker.IsWall(true))
                        return;

                    stateMgr.ChangeState(stateMgr.walkState);
                }
            }

            if (stateData.InputHandler.IsDashKeyDown())
            {
                if (stateData.ActionStatusChecker.Direction)
                {
                    (stateMgr.dashState as Dash)?.DirectionSetter(true);
                    stateMgr.ChangeState(stateMgr.dashState);
                    return;
                }
                else
                {
                    (stateMgr.dashState as Dash)?.DirectionSetter(false);
                    stateMgr.ChangeState(stateMgr.dashState);
                    return;
                }
            }
        }

        public void Exit(PlayerStateMgr stateMgr) { }
    }

    public class Walk : IState, IWalker
    {
        private readonly PlayerStateData stateData;
        /// <summary>
        /// 歩いているかどうかのプロパティです。
        /// 正味、使用していないですが、今後の拡張性を考えて作成しました
        /// </summary>
        public bool isWalkNow { get; set; }

        public Walk(PlayerStateData playerStateData)
        {
            this.stateData = playerStateData;
        }

        public void Enter(PlayerStateMgr stateMgr)
        {
            /// <summary>
            /// 初期化
            /// </summary>
            isWalkNow = false;

            stateData.AnimHandler.ChangeAnimState(stateData.AnimHandler.walkState);
        }

        public void Execute(PlayerStateMgr stateMgr)
        {
            ///<summary>
            /// 落下中ならFallStateに遷移するが、↓
            /// 地面についているなら実行しない。これもまた、落ちる床に乗っている時にfallstateにいかないようにするためです。(idlestateにも同じことを書きました)
            /// </summary>
            if (!stateData.ActionStatusChecker.IsGround() && stateData.ActionStatusChecker.IsFallingNow())
            {
                stateMgr.ChangeState(stateMgr.fallState);
                return;
            }

            /// <summary>
            /// ジャンプキーが押されたらJumpStateに遷移
            /// </summary>
            if (stateData.InputHandler.IsJumpKeyDown())
            {
                stateMgr.ChangeState(stateMgr.jumpState);
                return;
            }

            /// <summary>
            /// ダッシュに遷移する処理
            /// </summary>
            if (stateData.InputHandler.IsDashKeyDown())
            {
                if (stateData.InputHandler.IsMoveLeftKey())
                {
                    (stateMgr.dashState as Dash)?.DirectionSetter(false);
                    stateMgr.ChangeState(stateMgr.dashState);
                    return;
                }

                if (stateData.InputHandler.IsMoveRightKey())
                {
                    (stateMgr.dashState as Dash)?.DirectionSetter(true);
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
            if (!stateData.InputHandler.IsMoveKey())
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
            if (stateData.InputHandler.IsMoveLeftKey())
            {
                if (stateData.ActionStatusChecker.IsWall(false))
                {
                    stateMgr.ChangeState(stateMgr.idleState);
                    return;
                }

                stateData.ActionHandler.Walk(false);
            }
            else if (stateData.InputHandler.IsMoveRightKey())
            {
                if (stateData.ActionStatusChecker.IsWall(true))
                {
                    stateMgr.ChangeState(stateMgr.idleState);
                    return;
                }

                stateData.ActionHandler.Walk(true);
            }
        }

        public void Exit(PlayerStateMgr stateMgr) { }
    }

    //Injectしています
    public class Dash : IState
    {
        [Inject]
        private DashSparkFactory sparkFactory;
        [Inject]
        private PlayerDashKeepManager dashKeepManager;
        [Inject]
        private readonly PlayerDashTimeCtrl dashTimeCtrl;

        private readonly PlayerStateData stateData;
        private bool direction;

        //インスタンスした時にdirectionを渡すようにしたい
        public Dash(PlayerStateData stateData)
        {
            this.stateData = stateData;
        }

        public void DirectionSetter(bool direction)
        {
            this.direction = direction;
        }

        public void Enter(PlayerStateMgr stateMgr)
        {
            stateData.ActionStatusChecker.SetPlayerDirectionFromDashStart(direction);

            sparkFactory.MakeEffect();

            stateData.ActionHandler.Dash(direction);
            dashTimeCtrl.StartDashTimeCtrl();

            stateData.AnimHandler.ChangeAnimState(stateData.AnimHandler.dashState);

            PlayerAcitonSECtrl.OnPlaySE.OnNext(PlayerAcitonSECtrl.dashSound);
        }

        public void Execute(PlayerStateMgr stateMgr)
        {
            /// <summary>
            /// ダッシュが終了したときの処理
            /// どこに遷移するか判断します
            /// </summary>
            if (dashTimeCtrl.IsDashNow == false)
            {
                if (!stateData.InputHandler.IsMoveKey())
                    stateMgr.ChangeState(stateMgr.idleState);
                else if (stateData.InputHandler.IsMoveLeftKey())
                    stateMgr.ChangeState(stateMgr.walkState);
                else if (stateData.InputHandler.IsMoveRightKey())
                    stateMgr.ChangeState(stateMgr.walkState);
                else
                {
                    //ここに到達しないが、万が一のために書いておく
                    Debug.LogError("DashStateにて例外が発生!すぐに確認してください");
                    stateMgr.ChangeState(stateMgr.idleState);
                }
                return;
            }

            if (stateData.ActionStatusChecker.IsFallingNow())
            {
                /// <summary>
                /// ダッシュしている最中に落下したら、ダッシュを維持したままFallに遷移
                /// </summary>
                dashKeepManager.KeepDashSpeed();

                stateMgr.ChangeState(stateMgr.fallState);
                return;
            }

            if (stateData.ActionStatusChecker.IsWall(direction))
            {
                dashKeepManager.StopDashSpeed();
                stateMgr.ChangeState(stateMgr.idleState);
                return;
            }

            if (stateData.InputHandler.IsJumpKeyDown())
            {
                dashKeepManager.KeepDashSpeed();
                stateMgr.ChangeState(stateMgr.jumpState);
                return;
            }

            //ダッシュ中にダッシュキーを押されたら、ダッシュをし直す
            if (stateData.InputHandler.IsDashKeyDown())
            {
                dashTimeCtrl.StopDashTimeCtrl();

                /// <summary>
                /// 向きをコンストラクタで渡し、ダッシュのステートに遷移する
                /// </summary>
                if (stateData.InputHandler.IsMoveLeftKey())
                {
                    (stateMgr.dashState as Dash)?.DirectionSetter(false);
                    stateMgr.ChangeState(stateMgr.dashState);
                    return;
                }

                if (stateData.InputHandler.IsMoveRightKey())
                {
                    (stateMgr.dashState as Dash)?.DirectionSetter(true);
                    stateMgr.ChangeState(stateMgr.dashState);
                    return;
                }
            }
        }

        public void Exit(PlayerStateMgr stateMgr)
        {
            stateData.ActionHandler.StopX();
            dashTimeCtrl.StopDashTimeCtrl();
        }
    }

    public class Jump : IState, IWalker
    {
        //歩いているかどうかのプロパティです
        //Walkクラスのコメントを参照してください
        //IWalkerインターフェースを実装しているため、このプロパティを持っています
        public bool isWalkNow { get; set; }

        private readonly PlayerStateData stateData;

        public Jump(PlayerStateData stateData)
        {
            this.stateData = stateData;
        }

        public void Enter(PlayerStateMgr stateMgr)
        {
            isWalkNow = false;

            stateData.ActionHandler.Jump();

            stateData.AnimHandler.ChangeAnimState(stateData.AnimHandler.jumpState);

            PlayerAcitonSECtrl.OnPlaySE.OnNext(PlayerAcitonSECtrl.jumpSound);
        }

        public void Execute(PlayerStateMgr stateMgr)
        {
            if (stateData.ActionStatusChecker.IsGround())
            {
                /// <summary>
                /// 地面についていても上昇中ならジャンプ状態を維持
                /// </summary>
                if (stateData.ActionStatusChecker.isJumpingNow())
                    return;

                if (isWalkNow == false)
                    stateMgr.ChangeState(stateMgr.idleState);
                else
                    stateMgr.ChangeState(stateMgr.walkState);
            }

            /// <summary>
            /// 落下を始めたらFallに遷移
            /// </summary>
            if (stateData.ActionStatusChecker.IsFallingNow())
            {
                stateMgr.ChangeState(stateMgr.fallState);
                return;
            }

            if (stateData.InputHandler.IsJumpKeyDown())
            {
                if (
                    stateData.ActionStatusChecker.IsFarWall(false)
                    || stateData.ActionStatusChecker.IsFarWall(true)
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
            if (!stateData.InputHandler.IsMoveKey())
            {
                /// <summary>
                /// 一度しか停止は実行できないようにする
                /// </summary>
                if (isWalkNow)
                {
                    stateData.ActionHandler.StopX();
                    isWalkNow = false;
                }
                return;
            }

            /// <summary>
            /// 上の条件を回避したら、移動ができるようにする
            /// </summary>
            isWalkNow = true;

            if (stateData.InputHandler.IsMoveLeftKey())
            {
                stateData.ActionHandler.Walk(false);
                return;
            }

            if (stateData.InputHandler.IsMoveRightKey())
            {
                stateData.ActionHandler.Walk(true);
                return;
            }
        }

        public void Exit(PlayerStateMgr stateMgr) { }
    }

    public class Fall : IState, IWalker
    {
        private readonly PlayerStateData stateData;
        //IWalkerインターフェースを実装しているため、このプロパティを持っています
        public bool isWalkNow { get; set; }

        public Fall(PlayerStateData stateData)
        {
            this.stateData = stateData;
        }

        public void Enter(PlayerStateMgr stateMgr)
        {
            stateData.AnimHandler.ChangeAnimState(stateData.AnimHandler.fallState);

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
            if (!stateData.InputHandler.IsMoveKey())
            {
                stateData.ActionHandler.StopX();
                isWalkNow = false;
                return;
            }

            if (stateData.InputHandler.IsMoveLeftKey())
            {
                isWalkNow = stateData.ActionHandler.Walk(false);
                return;
            }

            if (stateData.InputHandler.IsMoveRightKey())
            {
                isWalkNow = stateData.ActionHandler.Walk(true);
                return;
            }
        }

        public void Execute(PlayerStateMgr stateMgr)
        {
            if (stateData.ActionStatusChecker.IsGround())
            {
                PlayerAcitonSECtrl.OnPlaySE.OnNext(PlayerAcitonSECtrl.landSound);
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

            if (stateData.InputHandler.IsMoveKey())
            {
                if (stateData.InputHandler.IsMoveLeftKey())
                {
                    if (stateData.ActionStatusChecker.IsFarWall(false) || stateData.ActionStatusChecker.IsWall(false))
                    {
                        stateMgr.ChangeState(stateMgr.wallFallState);
                        return;
                    }
                }
                else if (stateData.InputHandler.IsMoveRightKey())
                {
                    if (stateData.ActionStatusChecker.IsFarWall(true) || stateData.ActionStatusChecker.IsWall(true))
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
        private readonly PlayerStateData stateData;
        //壁にぶつかっている方向を判定するための変数
        //enterしたら初期化される
        bool wall_facing_which;

        /// <summary>
        /// WallFallからWallKickに遷移するときにExitしたのにもかかわらず数フレームだけWallFallが実行されてしまうのでExitしたらfalseにしてTrueでないと
        /// WallFallの処理をできないようにしました（力技☆）
        /// </summary>
        bool IsWallFallExecutable;

        [Inject]
        WallKickDelayManager wallKickManger;

        public WallFall(PlayerStateData stateData)
        {
            this.stateData = stateData;
        }

        public void Enter(PlayerStateMgr stateMgr)
        {
            /// <summary>
            /// 初期化
            /// </summary>
            IsWallFallExecutable = true;

            stateData.AnimHandler.ChangeAnimState(stateData.AnimHandler.wallFallState);

            /// <summary>
            /// 壁にぶつかっている方向を判定
            /// </summary>
            if (stateData.ActionStatusChecker.IsWall(true))
            {
                wall_facing_which = true;
            }
            else if (stateData.ActionStatusChecker.IsWall(false))
            {
                wall_facing_which = false;
            }
        }

        public void Execute(PlayerStateMgr stateMgr)
        {
            //地面についたらIdleに遷移
            if (stateData.ActionStatusChecker.IsGround())
            {
                stateMgr.ChangeState(stateMgr.idleState);
                return;
            }

            //Fallに遷移させるかどうかの判定の処理を書く
            if (wall_facing_which)
            {
                //もし壁にぶつからなくなったらFallに遷移
                if (!stateData.ActionStatusChecker.IsWall(true))
                {
                    ChangeFallState(stateMgr);
                    stateData.ActionHandler.StopY();
                    return;
                }

                //壁に向かって歩くのをやめたらFallに遷移
                if (!stateData.InputHandler.IsMoveRightKey())
                {
                    ChangeFallState(stateMgr);
                    stateData.ActionHandler.StopY();
                    return;
                }
            }
            else
            {
                //もし壁にぶつからなくなったらFallに遷移
                if (!stateData.ActionStatusChecker.IsWall(false))
                {
                    ChangeFallState(stateMgr);
                    stateData.ActionHandler.StopY();
                    return;
                }

                //壁に向かって歩くのをやめたらFallに遷移
                if (!stateData.InputHandler.IsMoveLeftKey())
                {
                    ChangeFallState(stateMgr);
                    stateData.ActionHandler.StopY();
                    return;
                }
            }

            //壁キック判定
            if (stateData.InputHandler.IsJumpKeyDown())
            {
                stateMgr.ChangeState(stateMgr.wallKick);
            }

            //どちらの移動キーも押されていない場合はFallに遷移
            if (!stateData.InputHandler.IsMoveKey())
            {
                ChangeFallState(stateMgr);
                return;
            }

            //↑をすべて回避したら壁ずりをする
            WallFalling(stateMgr);
        }

        private void WallFalling(PlayerStateMgr stateMgr)
        {
            if (!IsWallFallExecutable)
                return;

            stateData.ActionHandler.WallFall();
        }

        public void Exit(PlayerStateMgr stateMgr)
        {
            //ExitしたらfalseにしてWallFallの処理をできないようにする
            IsWallFallExecutable = false;
        }

        private void ChangeFallState(PlayerStateMgr stateMgr)
        {
            wallKickManger.Start_JumpKey_AcceptingTime();
            stateMgr.ChangeState(stateMgr.fallState);
        }
    }

    //Injectしています
    public class WallKick : IState, IWalker
    {
        private readonly PlayerStateData stateData;

        //Injectされるフィールド達
        private WallKickFactory wallKickFactory;
        private PlayerDashKeepManager dashKeepManager;

        public bool isWalkNow { get; set; }

        private bool isOneTimeAbleTo_TurnOn_KeepDashSpeed = false;

        public WallKick(PlayerStateData stateData)
        {
            this.stateData = stateData;
        }

        [Inject]
        public void MyInject(WallKickFactory wallKickFactory, PlayerDashKeepManager dashKeepManager)
        {
            this.wallKickFactory = wallKickFactory;
            this.dashKeepManager = dashKeepManager;
        }

        public void Enter(PlayerStateMgr stateMgr)
        {
            isWalkNow = false;
            isOneTimeAbleTo_TurnOn_KeepDashSpeed = false;

            stateData.AnimHandler.ChangeAnimState(stateData.AnimHandler.wallKickState);

            if (wallKickFactory != null)
            {
                wallKickFactory.MakeEffect(stateMgr.transform);
            }
            else
            {
                Debug.LogError("WallKickFactory is not assigned!");
            }

            /// <summary>
            /// スピードを0にして、壁キックを行う
            /// </summary>
            stateData.ActionHandler.StopX();
            stateData.ActionHandler.StopY();

            stateData.ActionHandler.Jump();

            PlayerAcitonSECtrl.OnPlaySE.OnNext(PlayerAcitonSECtrl.jumpSound);
        }

        public void Execute(PlayerStateMgr stateMgr)
        {
            if (stateData.ActionStatusChecker.IsFallingNow())
            {
                if (stateData.InputHandler.IsMoveLeftKey() && stateData.ActionStatusChecker.IsWall(false))
                {
                    stateData.AnimHandler.ChangeAnimState(stateData.AnimHandler.wallFallState);
                    stateMgr.ChangeState(stateMgr.wallFallState);
                    return;
                }

                if (stateData.InputHandler.IsMoveRightKey() && stateData.ActionStatusChecker.IsWall(true))
                {
                    stateData.AnimHandler.ChangeAnimState(stateData.AnimHandler.wallFallState);
                    stateMgr.ChangeState(stateMgr.wallFallState);
                    return;
                }


                stateMgr.ChangeState(stateMgr.fallState);
                return;
            }

            if (stateData.ActionStatusChecker.IsGround())
            {
                //上昇中ならジャンプ状態を維持
                if (stateData.ActionStatusChecker.isJumpingNow())
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
            if (stateData.InputHandler.IsJumpKeyDown())
            {
                if (stateData.InputHandler.IsMoveLeftKey() && stateData.ActionStatusChecker.IsWall(false))
                {
                    stateMgr.ChangeState(stateMgr.wallKick);
                    return;
                }

                if (stateData.InputHandler.IsMoveRightKey() && stateData.ActionStatusChecker.IsWall(true))
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
            if (!stateData.ActionStatusChecker.IsWall(true) && !stateData.ActionStatusChecker.IsWall(false))
            {
                if (stateData.InputHandler.IsDashKeyDown() || stateData.InputHandler.IsDashKey())
                {
                    //すでにKeepDashSpeedがオンになっていたら、何もしない
                    if (dashKeepManager.IsKeepDashSpeed)
                        return;
                    if (isOneTimeAbleTo_TurnOn_KeepDashSpeed)
                        return;

                    dashKeepManager.KeepDashSpeed();
                    isOneTimeAbleTo_TurnOn_KeepDashSpeed = true;
                }
            }
        }

        public void ExecuteWalk(PlayerStateMgr stateMgr)
        {
            if (!stateData.InputHandler.IsMoveKey())
            {
                /// <summary>
                /// 同時押しの判定を行っています。
                /// </summary>
                isWalkNow = false;
                stateData.ActionHandler.StopX();
                return;
            }

            if (stateData.InputHandler.IsMoveLeftKey())
            {
                stateData.ActionHandler.Walk(false);
                isWalkNow = true;
                return;
            }

            if (stateData.InputHandler.IsMoveRightKey())
            {
                stateData.ActionHandler.Walk(true);
                isWalkNow = true;
                return;
            }
        }

        public void Exit(PlayerStateMgr stateMgr) { }
    }

    // Injectしています
    public class DamageState : IState
    {
        private readonly PlayerStateData stateData;

        public static event Action onPlayerDamageRecover;

        [Inject]
        private DamageTimeHandler damageTimeHandler;

        public DamageState(PlayerStateData stateData)
        {
            this.stateData = stateData;
        }

        public void Enter(PlayerStateMgr stateMgr)
        {
            stateData.ActionHandler.StopX();
            stateData.ActionHandler.StopY();
            stateData.ActionHandler.Damage();
        }

        public void Execute(PlayerStateMgr stateMgr)
        {
            if (damageTimeHandler.IsDamaging) return;

            stateMgr.ChangeState(stateMgr.idleState);
        }

        public void Exit(PlayerStateMgr stateMgr)
        {
            onPlayerDamageRecover?.Invoke();
        }
    }

    public class DeathState : IState
    {
        public DeathState() { }

        public void Enter(PlayerStateMgr stateMgr)
        {
            return;
        }

        public void Execute(PlayerStateMgr stateMgr)
        {
            return;
        }

        public void Exit(PlayerStateMgr stateMgr)
        {
            return;
        }
    }

    //会話やワープの時に使うステート
    public class NeutralState : IState
    {
        public void Enter(PlayerStateMgr stateMgr)
        {
            return;
        }

        public void Execute(PlayerStateMgr stateMgr)
        {
            return;
        }

        public void Exit(PlayerStateMgr stateMgr)
        {
            return;
        }
    }
}
