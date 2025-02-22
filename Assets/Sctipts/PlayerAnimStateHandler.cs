using System;
using Zenject;
using UnityEngine;
using HPBar;
using UniRx;
using PlayerAnimCtrl;

public class PlayerAnimStateHandler : MonoBehaviour
{
    private GameFlowManager gameFlowManager;
    [SerializeField] public bool isDebugMode = false;

    //Inject
    private LifeManager lifeManager;
    private IPlayerAnimState damageState;
    private AnimatorCtrl animatorCtrl;

    public IPlayerAnimState idleState, walkState, jumpState, onAirState, fallState, dashState, wallFallState, wallKickState, wallKickToFallState, deathState, warpState, warpEscapeState, neutralIdleState;

    PlayerShotAnimCtrl shotAnimCtrl;

    IPlayerAnimState currentState;
    bool isChangeableAnim = false;

    private Subject<Unit> onEnterDoor = new Subject<Unit>();
    public IObserver<Unit> OnEnterDoor => onEnterDoor;

    private Subject<Unit> onExitDoor = new Subject<Unit>();
    public IObserver<Unit> OnExitDoor => onExitDoor;

    private Subject<Unit> onPlayerDeathAnimEnd = new Subject<Unit>();
    public IObservable<Unit> OnPlayerDeathAnimEnd => onPlayerDeathAnimEnd;

    EventStreamer eventStreamer;
    PlayerState.EventMediator eventMediator;

    [Inject]
    public void Construct(LifeManager lifeManager, AnimatorCtrl animatorCtrl, [Inject(Id = "Damage")] IPlayerAnimState damageState, EventStreamer eventStreamer, PlayerState.EventMediator eventMediator)
    {
        this.lifeManager = lifeManager;
        this.animatorCtrl = animatorCtrl;
        this.damageState = damageState;
        this.eventStreamer = eventStreamer;
        this.eventMediator = eventMediator;
    }

    private void Awake()
    {
        shotAnimCtrl = gameObject.MyGetComponent_NullChker<PlayerShotAnimCtrl>();

        GameFlowManager.StartBattleAction.Subscribe(_ =>
        {
            currentState = idleState;
            idleState.Enter();
        })
        .AddTo(this);

        onEnterDoor.Subscribe(_ =>
        {
            isChangeableAnim = false;
        })
        .AddTo(this);

        onExitDoor.Subscribe(_ =>
        {
            isChangeableAnim = true;
        })
        .AddTo(this);

        eventStreamer.startBossDoorCutScene.Subscribe(_ =>
        {
            onEnterDoor.OnNext(Unit.Default);
        })
        .AddTo(this);

        eventStreamer.finishBossDoorCutScene.Subscribe(_ =>
        {
            onExitDoor.OnNext(Unit.Default);
        })
        .AddTo(this);

        //TODO : ZenjectでInjectする
        idleState = new IdleState(animatorCtrl, "isIdle");
        walkState = new WalkState(animatorCtrl, "isRun");
        jumpState = new JumpState(animatorCtrl, "isJump");
        onAirState = new OnAirState(animatorCtrl, "isJumpToFall");
        fallState = new FallState(animatorCtrl, "isFall");
        dashState = new DashState(animatorCtrl, "isDash");
        wallFallState = new WallFallState(animatorCtrl, "isWallFall");
        wallKickState = new WallKickState(animatorCtrl, "isWallKick");
        wallKickToFallState = new WallKickToFallState(animatorCtrl, "isWallJumpToFall");
        deathState = new DeathState(animatorCtrl, "isDeath");
        warpState = new WarpState(animatorCtrl, "isWarp");
        neutralIdleState = new NeutralIdle(animatorCtrl, "isNeutralIdle");
        warpEscapeState = new WarpEscapeState(animatorCtrl, "isWarpEscape");

        currentState = idleState;

        isChangeableAnim = true;

        lifeManager.OnPlayerDead.Subscribe(_ =>
        {
            OnDeath();
        })
        .AddTo(this);
    }

    public void OtherComponentGetter(GameFlowManager gameFlowManager)
    {
        this.gameFlowManager = gameFlowManager;
    }

    private void OnEnable()
    {
        HPBarHandler.onPlayerDamage += OnDamage;
    }

    private void OnDisable()
    {
        HPBarHandler.onPlayerDamage -= OnDamage;
    }

    public void ChangeAnimState(IPlayerAnimState newState)
    {
        if (!isChangeableAnim) return;
        if (currentState == newState) return;

        if (currentState != null) currentState.Exit();

        IPlayerAnimState prevState = currentState;
        currentState = newState;
        currentState.Enter();

        //他のステートからアイドルステートになったときにはショットのステートを終了する
        if (prevState != idleState && newState == idleState)
        {
            //ショットアニメーションを終了する
            shotAnimCtrl.EndShotAnim();
        }

        if (isDebugMode) Debug.Log($"Anim : {currentState}");
    }

    //アニメーションイベント
    public void OnFinishWarpIn()
    {
        GameFlowManager.onCompletedPlayerWarpIn.OnNext(Unit.Default);
    }

    public void OnFinishEscape()
    {
        Debug.Log("ワープエスケープアニメーション終了");
        gameFlowManager.OnCompletedEscapeAnim.OnNext(Unit.Default);
    }

    // アニメーションイベント
    public void EndAnimDeath()
    {
        onPlayerDeathAnimEnd.OnNext(Unit.Default);
    }

    //アニメーションイベント
    public void EndJumpToFall()
    {
        eventMediator.EndJumpToFallAnim.OnNext(Unit.Default);
    }

    //アニメーションイベント
    public void EndWallJumpToFall()
    {
        eventMediator.EndWallKickToFallAnim.OnNext(Unit.Default);
    }

    public void GetAnimator()
    {
        gameObject.MyGetComponent_NullChker<Animator>();
    }

    //イベントハンドラ
    void OnDamage()
    {
        ChangeAnimState(damageState);
    }

    //イベントハンドラ
    void OnDeath()
    {
        ChangeAnimState(deathState);
    }

    public bool WhatCurrentAnimState(IPlayerAnimState state)
    {
        if (currentState == state)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}

public class AnimatorCtrl
{
    Animator animator;
    PlayerAnimStateHandler _animStateHadnler;
    public AnimatorCtrl(PlayerAnimStateHandler animStateHandler, Animator animator)
    {
        this.animator = animator;
        _animStateHadnler = animStateHandler;

        this.animator = animStateHandler.gameObject.MyGetComponent_NullChker<Animator>();
    }

    public void StartAnim(string name)
    {
        if (animator == null) _animStateHadnler.GetAnimator();
        animator.SetBool(name, true);
    }

    public void StopAnim(string name)
    {
        if (animator == null) _animStateHadnler.GetAnimator();
        animator.SetBool(name, false);
    }
}

public interface IPlayerAnimState
{
    void Enter();
    void Exit();
}

public abstract class PlayerAnimStateBase : IPlayerAnimState
{
    protected string animBoolName;
    protected AnimatorCtrl animatorCtrl;

    public PlayerAnimStateBase(AnimatorCtrl animatorCtrl, string animBoolName)
    {
        this.animatorCtrl = animatorCtrl;


        if (animBoolName != null)
            this.animBoolName = animBoolName;
        else
            this.animBoolName = null;
    }

    public virtual void Enter()
    {
        animatorCtrl.StartAnim(animBoolName);
    }

    public virtual void Exit()
    {
        animatorCtrl.StopAnim(animBoolName);
    }
}

public class IdleState : PlayerAnimStateBase
{
    public IdleState(AnimatorCtrl animatorCtrl, string animBoolName) : base(animatorCtrl, animBoolName) { }
}

public class WalkState : PlayerAnimStateBase
{
    public WalkState(AnimatorCtrl animatorCtrl, string animBoolName) : base(animatorCtrl, animBoolName) { }
}

public class JumpState : PlayerAnimStateBase
{
    public JumpState(AnimatorCtrl animatorCtrl, string animBoolName) : base(animatorCtrl, animBoolName) { }
}

public class OnAirState : PlayerAnimStateBase
{
    public OnAirState(AnimatorCtrl animatorCtrl, string animBoolName) : base(animatorCtrl, animBoolName) { }
}

public class FallState : PlayerAnimStateBase
{
    public FallState(AnimatorCtrl animatorCtrl, string animBoolName) : base(animatorCtrl, animBoolName) { }
}

public class DashState : PlayerAnimStateBase
{
    public DashState(AnimatorCtrl animatorCtrl, string animBoolName) : base(animatorCtrl, animBoolName) { }
}

public class WallFallState : PlayerAnimStateBase
{
    public WallFallState(AnimatorCtrl animatorCtrl, string animBoolName) : base(animatorCtrl, animBoolName) { }
}

public class WallKickState : PlayerAnimStateBase
{
    public WallKickState(AnimatorCtrl animatorCtrl, string animBoolName) : base(animatorCtrl, animBoolName) { }
}

public class WallKickToFallState : PlayerAnimStateBase
{
    public WallKickToFallState(AnimatorCtrl animatorCtrl, string animBoolName) : base(animatorCtrl, animBoolName) { }
}

public interface IDamageStateSubject
{
    IObservable<Unit> OnEnterDamageAnim { get; }
}

public class DamageState : PlayerAnimStateBase, IDamageStateSubject
{
    private Subject<Unit> onEnterDamageAnim = new Subject<Unit>();
    public IObservable<Unit> OnEnterDamageAnim => onEnterDamageAnim;

    public DamageState(AnimatorCtrl animatorCtrl, string animBoolName) : base(animatorCtrl, animBoolName)
    {
        this.animatorCtrl = animatorCtrl;
    }

    public override void Enter()
    {
        animatorCtrl.StartAnim(animBoolName);
        onEnterDamageAnim.OnNext(Unit.Default);
    }
}

public class DeathState : PlayerAnimStateBase
{
    public DeathState(AnimatorCtrl animatorCtrl, string animBoolName) : base(animatorCtrl, animBoolName)
    {
        this.animatorCtrl = animatorCtrl;
    }
}

public class WarpState : PlayerAnimStateBase
{
    public WarpState(AnimatorCtrl animatorCtrl, string animBoolName) : base(animatorCtrl, animBoolName) { }
}

public class NeutralIdle : PlayerAnimStateBase
{
    public NeutralIdle(AnimatorCtrl animatorCtrl, string animBoolName) : base(animatorCtrl, animBoolName) { }
}

public class WarpEscapeState : PlayerAnimStateBase
{
    public WarpEscapeState(AnimatorCtrl animatorCtrl, string animBoolName) : base(animatorCtrl, animBoolName) { }
}
