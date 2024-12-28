using System;
using ActionStatusChk;
using KeyHandler;
using PlayerAction;
using PlayerInfo;
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

        //このクラスのみだけどコンストラクタで使用している変数
        private PlayerAnimStateHandler animHandler;

        //このクラスのみで使用している変数
        [SerializeField] private bool isDebugCurrentState = false;
        [SerializeField] private HPBarHandler hPBarHandler;
        private WallKickDelayManager wallKickManager;
        private IState currentState;
        private bool isExecutable;
        private InputHandler inputHandler;
        private ActionHandler actionHandler;
        private ActionStatusChecker actionStatusChecker;
        private Rigidbody2D rb;
        private PlayerStatus playerStatus;

        //他のクラスからアクセスされる変数
        public IState CurrentState => currentState;
        public PlayerStatus PlayerStatus => playerStatus;
        public InputHandler InputHandler => inputHandler;
        public ActionHandler ActionHandler => actionHandler;
        public ActionStatusChecker ActionStatusChecker => actionStatusChecker;

        //ステートクラスから頻繁にアクセスされるので、publicにしています
        public IState idleState, walkState, jumpState, fallState, wallFallState, wallKick,
        dashState, damageState, deathState;

        public void Init(Rigidbody2D rb, PlayerStatus playerStatus, ActionHandler ActionHandler, ActionStatusChecker ActionStatusChecker, InputHandler InputHandler, PlayerDashKeepManager dashKeepManager, WallKickDelayManager wallKickManager, PlayerAnimStateHandler animStateHandler)
        {
            isExecutable = false;

            this.rb = rb;
            this.playerStatus = playerStatus;
            this.actionHandler = ActionHandler;
            this.actionStatusChecker = ActionStatusChecker;
            this.inputHandler = InputHandler;
            this.wallKickManager = wallKickManager;
            this.animHandler = animStateHandler;
        }

        private void Awake()
        {
            idleState = new Idle(this, animHandler);
            walkState = new Walk(animHandler);
            dashState = new Dash(animHandler);
            jumpState = new Jump(animHandler);
            fallState = new Fall(animHandler);
            wallFallState = new WallFall(animHandler);
            wallKick = new WallKick(animHandler);
            damageState = new DamageState(animHandler, ActionHandler);
            deathState = new DeathState();

            //Injectしています
            container.Inject(dashState);
            container.Inject(wallKick);
            container.Inject(damageState);

            Debug.LogError("All states are initialized!");

            GameFlowManager.StartBattleAction.Subscribe(_ =>
            {
                ChangeState(idleState);
            })
            .AddTo(this);
        }

        private void Start()
        {
            if (hPBarHandler == null)
            {
                Debug.LogWarning("HPBarHandler is not assigned!");
            }
        }

        //プレイヤーが画面外に出たら
        private void OnBecameInvisible()
        {
            //TODO:画面外で死ぬ処理はここに書くべきなのか？責務に反するので、後で修正
            hPBarHandler.onPlayerInVoid.OnNext(Unit.Default);
        }

        //イベントの登録
        private void OnEnable()
        {
            HPBarHandler.onPlayerDeath += OnDeath;
            HPBarHandler.onPlayerDamage += OnDamage;
        }

        //イベントの登録解除
        private void OnDisable()
        {
            HPBarHandler.onPlayerDeath -= OnDeath;
            HPBarHandler.onPlayerDamage -= OnDamage;
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

            IState previousState;
            previousState = currentState;

            Debug.Log("PreviousState: " + previousState);
            Debug.Log("NextState: " + nextState);

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

        void OnDamage()
        {
            ChangeState(damageState);
        }

        void OnDeath()
        {
            ChangeState(deathState);

            ActionHandler.Stop();
            ActionHandler.StopY();
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

        public IState GetCurrentState()
        {
            return currentState;
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

    public class Idle : IState
    {
        PlayerStateMgr playerStateMgr;
        PlayerAnimStateHandler animHandler;

        public Idle(PlayerStateMgr playerStateMgr, PlayerAnimStateHandler animStateHandler)
        {
            this.playerStateMgr = playerStateMgr;
            this.animHandler = animStateHandler;
        }

        public void Enter(PlayerStateMgr stateMgr)
        {
            /// <summary>
            /// 移動を止める処理を最初に実行し初期化しておく
            /// </summary>
            stateMgr.ActionHandler.Stop();

            animHandler.ChangeAnimState(animHandler.idleState);
        }

        //イベントハンドラ
        //TODO:なんでこれを書いた？
        void CheckCorrectAnim()
        {
            if (playerStateMgr.ActionStatusChecker.IsGround())
            {
                animHandler.ChangeAnimState(animHandler.idleState);
            }
        }

        public void Execute(PlayerStateMgr stateMgr)
        {
            /// <summary>
            /// 地面についておらず、落下中ならfallstateに遷移
            /// なぜ地面の判定を取るのかというと、落ちる床に乗っている時にプレイヤーも落下中と判定されてしまうので、それでは不自然なので判定しました
            /// </summary>
            if (!stateMgr.ActionStatusChecker.IsGround() && stateMgr.ActionStatusChecker.IsFallingNow())
            {
                stateMgr.ChangeState(stateMgr.fallState);
                return;
            }

            /// <summary>
            /// ジャンプキーが押されたらJumpStateに遷移
            /// ここで地面の判定を取るべきかと悩みますが、上で落ちているかどうかの判定が取れているので、上のif文を通過している場合は地面にいないです
            /// </sumary>
            if (stateMgr.InputHandler.IsJumpKeyDown())
            {
                stateMgr.ChangeState(stateMgr.jumpState);
                return;
            }

            /// <summary>
            /// 歩行に遷移するかどうかの判定です
            /// 壁の検知を行い、移動キーを押していたとしても、進行方向の壁にぶつかっていたら遷移せずに待機します
            /// </summary>
            if (stateMgr.InputHandler.IsMoveKey())
            {
                if (stateMgr.InputHandler.IsMoveLeftKey())
                {
                    if (stateMgr.ActionStatusChecker.IsWall(false))
                        return;

                    stateMgr.ChangeState(stateMgr.walkState);
                }

                if (stateMgr.InputHandler.IsMoveRightKey())
                {
                    if (stateMgr.ActionStatusChecker.IsWall(true))
                        return;

                    stateMgr.ChangeState(stateMgr.walkState);
                }
            }

            if (stateMgr.InputHandler.IsDashKeyDown())
            {
                if (stateMgr.ActionStatusChecker.Direction)
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
        /// <summary>
        /// 歩いているかどうかのプロパティです。
        /// 正味、使用していないですが、今後の拡張性を考えて作成しました
        /// </summary>
        public bool isWalkNow { get; set; }

        PlayerAnimStateHandler animHandler;

        public Walk(PlayerAnimStateHandler animStateHandler)
        {
            this.animHandler = animStateHandler;
        }

        public void Enter(PlayerStateMgr stateMgr)
        {
            /// <summary>
            /// 初期化
            /// </summary>
            isWalkNow = false;

            animHandler.ChangeAnimState(animHandler.walkState);
        }

        public void Execute(PlayerStateMgr stateMgr)
        {
            ///<summary>
            /// 落下中ならFallStateに遷移するが、↓
            /// 地面についているなら実行しない。これもまた、落ちる床に乗っている時にfallstateにいかないようにするためです。(idlestateにも同じことを書きました)
            /// </summary>
            if (!stateMgr.ActionStatusChecker.IsGround() && stateMgr.ActionStatusChecker.IsFallingNow())
            {
                stateMgr.ChangeState(stateMgr.fallState);
                return;
            }

            /// <summary>
            /// ジャンプキーが押されたらJumpStateに遷移
            /// </summary>
            if (stateMgr.InputHandler.IsJumpKeyDown())
            {
                stateMgr.ChangeState(stateMgr.jumpState);
                return;
            }

            /// <summary>
            /// ダッシュに遷移する処理
            /// </summary>
            if (stateMgr.InputHandler.IsDashKeyDown())
            {
                if (stateMgr.InputHandler.IsMoveLeftKey())
                {
                    (stateMgr.dashState as Dash)?.DirectionSetter(false);
                    stateMgr.ChangeState(stateMgr.dashState);
                    return;
                }

                if (stateMgr.InputHandler.IsMoveRightKey())
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
            if (!stateMgr.InputHandler.IsMoveKey())
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
            if (stateMgr.InputHandler.IsMoveLeftKey())
            {
                if (stateMgr.ActionStatusChecker.IsWall(false))
                {
                    stateMgr.ChangeState(stateMgr.idleState);
                    return;
                }

                stateMgr.ActionHandler.Walk(false);
            }
            else if (stateMgr.InputHandler.IsMoveRightKey())
            {
                if (stateMgr.ActionStatusChecker.IsWall(true))
                {
                    stateMgr.ChangeState(stateMgr.idleState);
                    return;
                }

                stateMgr.ActionHandler.Walk(true);
            }
        }

        public void Exit(PlayerStateMgr stateMgr) { }
    }

    //Injectしています
    public class Dash : IState
    {
        PlayerDashTimeCtrl dashTimeCtrl;

        bool direction;
        PlayerAnimStateHandler animHandler;

        [Inject]
        private DashSparkFactory sparkFactory;
        [Inject]
        private PlayerDashKeepManager dashKeepManager;

        //インスタンスした時にdirectionを渡すようにしたい
        public Dash(PlayerAnimStateHandler animStateHandler)
        {
            this.animHandler = animStateHandler;
        }

        public void DirectionSetter(bool direction)
        {
            this.direction = direction;
        }

        public void Enter(PlayerStateMgr stateMgr)
        {
            stateMgr.ActionStatusChecker.SetPlayerDiresctionFromDashStateBigin(direction);

            sparkFactory.MakeEffect();

            dashTimeCtrl = stateMgr.gameObject.MyGetComponent_NullChker<PlayerDashTimeCtrl>();

            stateMgr.ActionHandler.Dash(direction);
            dashTimeCtrl.StartDashTimeCtrl();

            animHandler.ChangeAnimState(animHandler.dashState);

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
                if (!stateMgr.InputHandler.IsMoveKey())
                    stateMgr.ChangeState(stateMgr.idleState);
                else if (stateMgr.InputHandler.IsMoveLeftKey())
                    stateMgr.ChangeState(stateMgr.walkState);
                else if (stateMgr.InputHandler.IsMoveRightKey())
                    stateMgr.ChangeState(stateMgr.walkState);
                else
                {
                    //ここに到達しないが、万が一のために書いておく
                    Debug.LogError("DashStateにて例外が発生!すぐに確認してください");
                    stateMgr.ChangeState(stateMgr.idleState);
                }
                return;
            }

            if (stateMgr.ActionStatusChecker.IsFallingNow())
            {
                /// <summary>
                /// ダッシュしている最中に落下したら、ダッシュを維持したままFallに遷移
                /// </summary>
                dashKeepManager.KeepDashSpeed();

                stateMgr.ChangeState(stateMgr.fallState);
                return;
            }

            if (stateMgr.InputHandler.IsJumpKeyDown())
            {
                dashKeepManager.KeepDashSpeed();

                stateMgr.ChangeState(stateMgr.jumpState);
                return;
            }

            if (stateMgr.ActionStatusChecker.IsWall(direction))
            {
                stateMgr.ChangeState(stateMgr.idleState);
                return;
            }

            //ダッシュ中にダッシュキーを押されたら、ダッシュをし直す
            if (stateMgr.InputHandler.IsDashKeyDown())
            {
                dashTimeCtrl.StopDashTimeCtrl();

                /// <summary>
                /// 向きをコンストラクタで渡し、ダッシュのステートに遷移する
                /// </summary>
                if (stateMgr.InputHandler.IsMoveLeftKey())
                {
                    (stateMgr.dashState as Dash)?.DirectionSetter(false);
                    stateMgr.ChangeState(stateMgr.dashState);
                    return;
                }

                if (stateMgr.InputHandler.IsMoveRightKey())
                {
                    (stateMgr.dashState as Dash)?.DirectionSetter(true);
                    stateMgr.ChangeState(stateMgr.dashState);
                    return;
                }
            }
        }

        public void Exit(PlayerStateMgr stateMgr)
        {
            stateMgr.ActionHandler.Stop();
            dashTimeCtrl.StopDashTimeCtrl();
        }
    }

    public class Jump : IState, IWalker
    {
        //歩いているかどうかのプロパティです
        //Walkクラスのコメントを参照してください
        //IWalkerインターフェースを実装しているため、このプロパティを持っています
        public bool isWalkNow { get; set; }

        PlayerAnimStateHandler animHandler;

        public Jump(PlayerAnimStateHandler animStateHandler)
        {
            this.animHandler = animStateHandler;
        }

        public void Enter(PlayerStateMgr stateMgr)
        {
            isWalkNow = false;

            stateMgr.ActionHandler.Jump(stateMgr.PlayerStatus.JumpForce);

            animHandler.ChangeAnimState(animHandler.jumpState);

            PlayerAcitonSECtrl.OnPlaySE.OnNext(PlayerAcitonSECtrl.jumpSound);
        }

        public void Execute(PlayerStateMgr stateMgr)
        {
            if (stateMgr.ActionStatusChecker.IsGround())
            {
                /// <summary>
                /// 地面についていても上昇中ならジャンプ状態を維持
                /// </summary>
                if (stateMgr.ActionStatusChecker.isJumpingNow())
                    return;

                if (isWalkNow == false)
                    stateMgr.ChangeState(stateMgr.idleState);
                else
                    stateMgr.ChangeState(stateMgr.walkState);
            }

            /// <summary>
            /// 落下を始めたらFallに遷移
            /// </summary>
            if (stateMgr.ActionStatusChecker.IsFallingNow())
            {
                stateMgr.ChangeState(stateMgr.fallState);
                return;
            }

            if (stateMgr.InputHandler.IsJumpKeyDown())
            {
                if (
                    stateMgr.ActionStatusChecker.IsFarWall(false)
                    || stateMgr.ActionStatusChecker.IsFarWall(true)
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
            if (!stateMgr.InputHandler.IsMoveKey())
            {
                /// <summary>
                /// 一度しか停止は実行できないようにする
                /// </summary>
                if (isWalkNow)
                {
                    stateMgr.ActionHandler.Stop();
                    isWalkNow = false;
                }
                return;
            }

            /// <summary>
            /// 上の条件を回避したら、移動ができるようにする
            /// </summary>
            isWalkNow = true;

            if (stateMgr.InputHandler.IsMoveLeftKey())
            {
                stateMgr.ActionHandler.Walk(false);
                return;
            }

            if (stateMgr.InputHandler.IsMoveRightKey())
            {
                stateMgr.ActionHandler.Walk(true);
                return;
            }
        }

        public void Exit(PlayerStateMgr stateMgr) { }
    }

    public class Fall : IState, IWalker
    {
        //IWalkerインターフェースを実装しているため、このプロパティを持っています
        public bool isWalkNow { get; set; }

        PlayerAnimStateHandler animHandler;

        public Fall(PlayerAnimStateHandler animStateHandler)
        {
            this.animHandler = animStateHandler;
        }

        public void Enter(PlayerStateMgr stateMgr)
        {
            animHandler.ChangeAnimState(animHandler.fallState);

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
            if (!stateMgr.InputHandler.IsMoveKey())
            {
                stateMgr.ActionHandler.Stop();
                isWalkNow = false;
                return;
            }

            if (stateMgr.InputHandler.IsMoveLeftKey())
            {
                isWalkNow = stateMgr.ActionHandler.Walk(false);
                return;
            }

            if (stateMgr.InputHandler.IsMoveRightKey())
            {
                isWalkNow = stateMgr.ActionHandler.Walk(true);
                return;
            }
        }

        public void Execute(PlayerStateMgr stateMgr)
        {
            if (stateMgr.ActionStatusChecker.IsGround())
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

            if (stateMgr.InputHandler.IsMoveKey())
            {
                if (stateMgr.InputHandler.IsMoveLeftKey())
                {
                    if (stateMgr.ActionStatusChecker.IsFarWall(false) || stateMgr.ActionStatusChecker.IsWall(false))
                    {
                        stateMgr.ChangeState(stateMgr.wallFallState);
                        return;
                    }
                }
                else if (stateMgr.InputHandler.IsMoveRightKey())
                {
                    if (stateMgr.ActionStatusChecker.IsFarWall(true) || stateMgr.ActionStatusChecker.IsWall(true))
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

        PlayerAnimStateHandler animHandler;

        public WallFall(PlayerAnimStateHandler animStateHandler)
        {
            this.animHandler = animStateHandler;
        }

        public void Enter(PlayerStateMgr stateMgr)
        {
            /// <summary>
            /// 初期化
            /// </summary>
            IsWallFallExecutable = true;

            animHandler.ChangeAnimState(animHandler.wallFallState);

            /// <summary>
            /// 壁にぶつかっている方向を判定
            /// </summary>
            if (stateMgr.ActionStatusChecker.IsWall(true))
            {
                wall_facing_which = true;
            }
            else if (stateMgr.ActionStatusChecker.IsWall(false))
            {
                wall_facing_which = false;
            }
        }

        public void Execute(PlayerStateMgr stateMgr)
        {
            //地面についたらIdleに遷移
            if (stateMgr.ActionStatusChecker.IsGround())
            {
                stateMgr.ChangeState(stateMgr.idleState);
                return;
            }

            //Fallに遷移させるかどうかの判定の処理を書く
            if (wall_facing_which)
            {
                //もし壁にぶつからなくなったらFallに遷移
                if (!stateMgr.ActionStatusChecker.IsWall(true))
                {
                    stateMgr.ChangeState(stateMgr.fallState);
                    stateMgr.ActionHandler.StopY();
                    return;
                }

                //壁に向かって歩くのをやめたらFallに遷移
                if (!stateMgr.InputHandler.IsMoveRightKey())
                {
                    stateMgr.ChangeState(stateMgr.fallState);
                    stateMgr.ActionHandler.StopY();
                    return;
                }
            }
            else
            {
                //もし壁にぶつからなくなったらFallに遷移
                if (!stateMgr.ActionStatusChecker.IsWall(false))
                {
                    stateMgr.ChangeState(stateMgr.fallState);
                    stateMgr.ActionHandler.StopY();
                    return;
                }

                //壁に向かって歩くのをやめたらFallに遷移
                if (!stateMgr.InputHandler.IsMoveLeftKey())
                {
                    stateMgr.ChangeState(stateMgr.fallState);
                    stateMgr.ActionHandler.StopY();
                    return;
                }
            }

            //壁キック判定
            if (stateMgr.InputHandler.IsJumpKeyDown())
            {
                stateMgr.ChangeState(stateMgr.wallKick);
            }

            //どちらの移動キーも押されていない場合はFallに遷移
            if (!stateMgr.InputHandler.IsMoveKey())
            {
                stateMgr.ChangeState(stateMgr.fallState);
                return;
            }

            //↑をすべて回避したら壁ずりをする
            WallFalling(stateMgr);
        }

        private void WallFalling(PlayerStateMgr stateMgr)
        {
            if (!IsWallFallExecutable)
                return;

            stateMgr.ActionHandler.WallFall();
        }

        public void Exit(PlayerStateMgr stateMgr)
        {
            //ExitしたらfalseにしてWallFallの処理をできないようにする
            IsWallFallExecutable = false;
        }
    }

    //Injectしています
    public class WallKick : IState, IWalker
    {
        private WallKickFactory wallKickFactory;
        private PlayerDashKeepManager dashKeepManager;

        public bool isWalkNow { get; set; }

        private bool isOneTimeAbleTo_TurnOn_KeepDashSpeed = false;

        PlayerAnimStateHandler animHandler;

        public WallKick(PlayerAnimStateHandler animStateHandler)
        {
            this.animHandler = animStateHandler;
        }

        [Inject]
        public void MyInject(WallKickFactory wallKickFactory, PlayerDashKeepManager dashKeepManager)
        {
            this.wallKickFactory = wallKickFactory;
            this.dashKeepManager = dashKeepManager;
            Debug.LogWarning("WallKickFactory is injected!");
        }

        public void Enter(PlayerStateMgr stateMgr)
        {
            isWalkNow = false;
            isOneTimeAbleTo_TurnOn_KeepDashSpeed = false;

            animHandler.ChangeAnimState(animHandler.wallKickState);

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
            stateMgr.ActionHandler.Stop();
            stateMgr.ActionHandler.StopY();

            stateMgr.ActionHandler.Jump(stateMgr.PlayerStatus.JumpForce);

            PlayerAcitonSECtrl.OnPlaySE.OnNext(PlayerAcitonSECtrl.jumpSound);
        }

        public void Execute(PlayerStateMgr stateMgr)
        {
            if (stateMgr.ActionStatusChecker.IsFallingNow())
            {
                if (stateMgr.InputHandler.IsMoveLeftKey() && stateMgr.ActionStatusChecker.IsWall(false))
                {
                    animHandler.ChangeAnimState(animHandler.wallFallState);
                    stateMgr.ChangeState(stateMgr.wallFallState);
                    return;
                }

                if (stateMgr.InputHandler.IsMoveRightKey() && stateMgr.ActionStatusChecker.IsWall(true))
                {
                    animHandler.ChangeAnimState(animHandler.wallFallState);
                    stateMgr.ChangeState(stateMgr.wallFallState);
                    return;
                }


                stateMgr.ChangeState(stateMgr.fallState);
                return;
            }

            if (stateMgr.ActionStatusChecker.IsGround())
            {
                //上昇中ならジャンプ状態を維持
                if (stateMgr.ActionStatusChecker.isJumpingNow())
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
            if (stateMgr.InputHandler.IsJumpKeyDown())
            {
                if (stateMgr.InputHandler.IsMoveLeftKey() && stateMgr.ActionStatusChecker.IsWall(false))
                {
                    stateMgr.ChangeState(stateMgr.wallKick);
                    return;
                }

                if (stateMgr.InputHandler.IsMoveRightKey() && stateMgr.ActionStatusChecker.IsWall(true))
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
            if (!stateMgr.ActionStatusChecker.IsWall(true) && !stateMgr.ActionStatusChecker.IsWall(false))
            {
                if (stateMgr.InputHandler.IsDashKeyDown() || stateMgr.InputHandler.IsDashKey())
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
            if (!stateMgr.InputHandler.IsMoveKey())
            {
                /// <summary>
                /// 同時押しの判定を行っています。
                /// </summary>
                isWalkNow = false;
                stateMgr.ActionHandler.Stop();
                return;
            }

            if (stateMgr.InputHandler.IsMoveLeftKey())
            {
                stateMgr.ActionHandler.Walk(false);
                isWalkNow = true;
                return;
            }

            if (stateMgr.InputHandler.IsMoveRightKey())
            {
                stateMgr.ActionHandler.Walk(true);
                isWalkNow = true;
                return;
            }
        }

        public void Exit(PlayerStateMgr stateMgr) { }
    }

    // Injectしています
    public class DamageState : IState
    {
        public static event Action onPlayerDamageRecover;

        [Inject]
        private DamageTimeHandler damageTimeHandler;

        PlayerAnimStateHandler animHandler;
        ActionHandler ActionHandler;
        public DamageState(PlayerAnimStateHandler animHandler, ActionHandler ActionHandler)
        {
            this.animHandler = animHandler;
            this.ActionHandler = ActionHandler;
        }

        public void Enter(PlayerStateMgr stateMgr)
        {
            ActionHandler.Stop();
            ActionHandler.StopY();
            ActionHandler.Damage();
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
