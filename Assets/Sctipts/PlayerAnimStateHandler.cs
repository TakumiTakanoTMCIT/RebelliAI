using System;
using PlayerInfo;
using PlayerState;
using UnityEngine;
using HPBar;
using UniRx;
using Door;
using ActionStatusChk;

public class PlayerAnimStateHandler : MonoBehaviour
{
    [SerializeField] ActionStatusChecker actionStatusChecker;
    [SerializeField] GameFlowManager gameFlowManager;
    [SerializeField] private DeathGlitchSparkFactory deathGlitchSparkFactory;
    [SerializeField] internal bool isDebugMode = false;
    internal Animator animator;
    PlayerStateMgr stateMgr;

    internal IPlayerAnimState idleState, walkState, jumpState, fallState, dashState, wallFallState, wallKickState, damageState, deathState, warpState, warpEscapeState, neutralIdleState;
    internal IPlayerAnimState currentState;

    AnimatorCtrl animatorCtrl;
    PlayerStatus playerStatus;

    SpriteRenderer spriteRenderer;

    bool isChangeableAnim = false;

    public static event Action onPlayerDeathAnimEnd;

    private Subject<Unit> onEnterDoor = new Subject<Unit>();
    public IObserver<Unit> OnEnterDoor => onEnterDoor;

    private Subject<Unit> onExitDoor = new Subject<Unit>();
    public IObserver<Unit> OnExitDoor => onExitDoor;

    private void Awake()
    {
        animator = gameObject.MyGetComponent_NullChker<Animator>();
        stateMgr = gameObject.MyGetComponent_NullChker<PlayerStateMgr>();
        playerStatus = gameObject.MyGetComponent_NullChker<PlayerStatus>();
        spriteRenderer = gameObject.MyGetComponent_NullChker<SpriteRenderer>();

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

        animatorCtrl = new AnimatorCtrl(this);

        idleState = new IdleState(animatorCtrl, this, stateMgr);
        walkState = new WalkState(animatorCtrl, this, stateMgr);
        jumpState = new JumpState(animatorCtrl);
        fallState = new FallState(animatorCtrl);
        dashState = new DashState(animatorCtrl);
        wallFallState = new WallFallState(animatorCtrl);
        wallKickState = new WallKickState(animatorCtrl);
        damageState = new DamageState(animatorCtrl, playerStatus, spriteRenderer, actionStatusChecker);
        deathState = new DeathState(animatorCtrl, this);
        warpState = new WarpState(animatorCtrl);
        neutralIdleState = new NeutralIdle(animatorCtrl);
        warpEscapeState = new WarpEscapeState(animatorCtrl);

        currentState = idleState;

        isChangeableAnim = true;

        Debug.Log("アニメーションステートハンドラが初期化されました");
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
        currentState = newState;
        currentState.Enter();
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
}

public class AnimatorCtrl
{
    Animator animator;
    PlayerAnimStateHandler _animStateHadnler;
    public AnimatorCtrl(PlayerAnimStateHandler animStateHandler)
    {
        _animStateHadnler = animStateHandler;

        this.animator = animStateHandler.gameObject.MyGetComponent_NullChker<Animator>();
    }

    public void StartAnim(string name)
    {
        if (_animStateHadnler.animator == null) _animStateHadnler.GetAnimator();
        _animStateHadnler.animator.SetBool(name, true);
    }

    public void StopAnim(string name)
    {
        if (_animStateHadnler.animator == null) _animStateHadnler.GetAnimator();
        _animStateHadnler.animator.SetBool(name, false);
    }
}

public interface IPlayerAnimState
{
    void Enter();
    void Exit();
}

public class IdleState : IPlayerAnimState
{
    PlayerAnimStateHandler animStateHandler;
    AnimatorCtrl animatorCtrl;
    PlayerStateMgr stateMgr;

    public IdleState(AnimatorCtrl animatorCtrl, PlayerAnimStateHandler stateHandler, PlayerStateMgr stateMgr)
    {
        this.animatorCtrl = animatorCtrl;
        this.animStateHandler = stateHandler;
        this.stateMgr = stateMgr;
    }

    public void Enter()
    {
        animatorCtrl.StartAnim("isIdle");
    }

    public void Exit()
    {
        animatorCtrl.StopAnim("isIdle");
    }
}

public class WalkState : IPlayerAnimState
{
    AnimatorCtrl AnimatorCtrl;
    PlayerAnimStateHandler stateHandler;
    PlayerStateMgr stateMgr;
    public WalkState(AnimatorCtrl animatorCtrl, PlayerAnimStateHandler stateHandler, PlayerStateMgr stateMgr)
    {
        this.AnimatorCtrl = animatorCtrl;
        this.stateHandler = stateHandler;
        this.stateMgr = stateMgr;
    }

    public void Enter()
    {
        AnimatorCtrl.StartAnim("isRun");
    }

    public void Exit()
    {
        AnimatorCtrl.StopAnim("isRun");
    }
}

public class JumpState : IPlayerAnimState
{
    AnimatorCtrl AnimatorCtrl;

    public JumpState(AnimatorCtrl animatorCtrl)
    {
        this.AnimatorCtrl = animatorCtrl;
    }

    public void Enter()
    {
        AnimatorCtrl.StartAnim("isJump");
    }

    public void Exit()
    {
        AnimatorCtrl.StopAnim("isJump");
    }
}

public class FallState : IPlayerAnimState
{
    AnimatorCtrl AnimatorCtrl;
    public FallState(AnimatorCtrl animatorCtrl)
    {
        this.AnimatorCtrl = animatorCtrl;
    }

    public void Enter()
    {
        AnimatorCtrl.StartAnim("isFall");
    }

    public void Exit()
    {
        AnimatorCtrl.StopAnim("isFall");
    }
}

public class DashState : IPlayerAnimState
{
    AnimatorCtrl AnimatorCtrl;
    public DashState(AnimatorCtrl animatorCtrl)
    {
        this.AnimatorCtrl = animatorCtrl;
    }

    public void Enter()
    {
        AnimatorCtrl.StartAnim("isDash");
    }

    public void Exit()
    {
        AnimatorCtrl.StopAnim("isDash");
    }
}

public class WallFallState : IPlayerAnimState
{
    AnimatorCtrl AnimatorCtrl;
    public WallFallState(AnimatorCtrl animatorCtrl)
    {
        this.AnimatorCtrl = animatorCtrl;
    }

    public void Enter()
    {
        AnimatorCtrl.StartAnim("isWallFall");
    }

    public void Exit()
    {
        AnimatorCtrl.StopAnim("isWallFall");
    }
}

public class WallKickState : IPlayerAnimState
{
    AnimatorCtrl AnimatorCtrl;
    public WallKickState(AnimatorCtrl animatorCtrl)
    {
        this.AnimatorCtrl = animatorCtrl;
    }

    public void Enter()
    {
        AnimatorCtrl.StartAnim("isWallKick");
    }

    public void Exit()
    {
        AnimatorCtrl.StopAnim("isWallKick");
    }
}

public class DamageState : IPlayerAnimState
{
    AnimatorCtrl animatorCtrl;
    PlayerStatus playerStatus;
    SpriteRenderer spriteRenderer;
    ActionStatusChecker actionStatusChecker;
    public DamageState(AnimatorCtrl animatorCtrl, PlayerStatus playerStatus, SpriteRenderer spriteRenderer, ActionStatusChecker actionStatusChecker)
    {
        this.animatorCtrl = animatorCtrl;
        this.playerStatus = playerStatus;
        this.spriteRenderer = spriteRenderer;
        this.actionStatusChecker = actionStatusChecker;
    }

    public void Enter()
    {
        animatorCtrl.StartAnim("isDamaging");

        spriteRenderer.flipX = actionStatusChecker.Direction;
    }

    public void Exit()
    {
        animatorCtrl.StopAnim("isDamaging");
    }
}

public class DeathState : IPlayerAnimState
{
    AnimatorCtrl animatorCtrl;
    PlayerAnimStateHandler animStateHandler;
    public DeathState(AnimatorCtrl animatorCtrl, PlayerAnimStateHandler animStateHandler)
    {
        this.animatorCtrl = animatorCtrl;
        this.animStateHandler = animStateHandler;
    }

    public void Enter()
    {
        if (animStateHandler.isDebugMode) Debug.Log("死亡アニメーションが再生されました");
        animatorCtrl.StartAnim("isDeath");
    }

    public void Exit()
    {
        animatorCtrl.StopAnim("isDeath");
    }
}

public class WarpState : IPlayerAnimState
{
    AnimatorCtrl animatorCtrl;
    public WarpState(AnimatorCtrl animatorCtrl)
    {
        this.animatorCtrl = animatorCtrl;
    }

    public void Enter()
    {
        Debug.Log("ワープアニメーションが再生されました");
        animatorCtrl.StartAnim("isWarp");
    }

    public void Exit()
    {
        animatorCtrl.StopAnim("isWarp");
    }
}

public class NeutralIdle : IPlayerAnimState
{
    AnimatorCtrl animatorCtrl;
    public NeutralIdle(AnimatorCtrl animatorCtrl)
    {
        this.animatorCtrl = animatorCtrl;
    }

    public void Enter()
    {
        Debug.Log("ニュートラルアニメーションが再生されました");
        animatorCtrl.StartAnim("isNeutralIdle");
    }

    public void Exit()
    {
        animatorCtrl.StopAnim("isNeutralIdle");
    }
}

public class WarpEscapeState : IPlayerAnimState
{
    AnimatorCtrl animatorCtrl;
    public WarpEscapeState(AnimatorCtrl animatorCtrl)
    {
        this.animatorCtrl = animatorCtrl;
    }

    public void Enter()
    {
        Debug.Log("ワープエスケープアニメーションが再生されました");
        animatorCtrl.StartAnim("isEscape");
    }

    public void Exit()
    {
        animatorCtrl.StopAnim("isEscape");
    }
}
