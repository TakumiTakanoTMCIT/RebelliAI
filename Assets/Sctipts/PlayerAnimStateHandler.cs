using System;
using PlayerState;
using UnityEngine;
using HPBar;
using UniRx;
using ActionStatusChk;
using PlayerAnimCtrl;

public class PlayerAnimStateHandler : MonoBehaviour
{
    //向きに合わせて、スプライトを反転させるためのフィールド
    ActionStatusChecker actionStatusChecker;

    private GameFlowManager gameFlowManager;
    private DeathGlitchSparkFactory deathGlitchSparkFactory;
    [SerializeField] public bool isDebugMode = false;

    public IPlayerAnimState idleState, walkState, jumpState, fallState, dashState, wallFallState, wallKickState, damageState, deathState, warpState, warpEscapeState, neutralIdleState;

    PlayerShotAnimCtrl shotAnimCtrl;

    IPlayerAnimState currentState;
    Animator animator;
    AnimatorCtrl animatorCtrl;
    SpriteRenderer spriteRenderer;
    bool isChangeableAnim = false;

    public static event Action onPlayerDeathAnimEnd;

    private Subject<Unit> onEnterDoor = new Subject<Unit>();
    public IObserver<Unit> OnEnterDoor => onEnterDoor;

    private Subject<Unit> onExitDoor = new Subject<Unit>();
    public IObserver<Unit> OnExitDoor => onExitDoor;

    private void Awake()
    {
        actionStatusChecker = gameObject.MyGetComponent_NullChker<ActionStatusChecker>();
        animator = gameObject.MyGetComponent_NullChker<Animator>();
        spriteRenderer = gameObject.MyGetComponent_NullChker<SpriteRenderer>();
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

        animatorCtrl = new AnimatorCtrl(this, animator);

        idleState = new IdleState(animatorCtrl, "isIdle");
        walkState = new WalkState(animatorCtrl, "isRun");
        jumpState = new JumpState(animatorCtrl, "isJump");
        fallState = new FallState(animatorCtrl, "isFall");
        dashState = new DashState(animatorCtrl, "isDash");
        wallFallState = new WallFallState(animatorCtrl, "isWallFall");
        wallKickState = new WallKickState(animatorCtrl, "isWallKick");
        damageState = new DamageState(animatorCtrl, spriteRenderer, actionStatusChecker, "isDamaging");
        deathState = new DeathState(animatorCtrl, "isDeath");
        warpState = new WarpState(animatorCtrl, "isWarp");
        neutralIdleState = new NeutralIdle(animatorCtrl, "isNeutralIdle");
        warpEscapeState = new WarpEscapeState(animatorCtrl, "isWarpEscape");

        currentState = idleState;

        isChangeableAnim = true;
    }

    public void OtherComponentGetter(DeathGlitchSparkFactory deathGlitchSparkFactory, GameFlowManager gameFlowManager)
    {
        this.deathGlitchSparkFactory = deathGlitchSparkFactory;
        this.gameFlowManager = gameFlowManager;
    }

    private void OnEnable()
    {
        HPBarHandler.onPlayerDamage += OnDamage;
        HPBarHandler.onPlayerDeath += OnDeath;
    }

    private void OnDisable()
    {
        HPBarHandler.onPlayerDamage -= OnDamage;
        HPBarHandler.onPlayerDeath -= OnDeath;
    }

    private void Update()
    {
        if (currentState == damageState) return;

        if (actionStatusChecker.Direction)
            spriteRenderer.flipX = false;
        else
            spriteRenderer.flipX = true;
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
    }

    //アニメーションイベント
    public void OnFinishWarpIn()
    {
        Debug.Log("ワープあにめ終了");
        GameFlowManager.onCompletedPlayerWarpIn.OnNext(Unit.Default);
    }

    public void OnFinishEscape()
    {
        Debug.Log("ワープエスケープアニメーション終了");
        gameFlowManager.OnCompletedEscapeAnim.OnNext(Unit.Default);
    }

    // アニメーションイベント
    public async void EndAnimDeath()
    {
        onPlayerDeathAnimEnd?.Invoke();
        await deathGlitchSparkFactory.MakeDeathEffects();
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
        this.animBoolName = animBoolName;
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

public class DamageState : PlayerAnimStateBase
{
    SpriteRenderer spriteRenderer;
    ActionStatusChecker actionStatusChecker;
    public DamageState(AnimatorCtrl animatorCtrl, SpriteRenderer spriteRenderer, ActionStatusChecker actionStatusChecker, string animBoolName) : base(animatorCtrl, animBoolName)
    {
        this.animatorCtrl = animatorCtrl;
        this.spriteRenderer = spriteRenderer;
        this.actionStatusChecker = actionStatusChecker;
    }

    public override void Enter()
    {
        animatorCtrl.StartAnim(animBoolName);
        spriteRenderer.flipX = actionStatusChecker.Direction;
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
