using PlayerInfo;
using PlayerState;
using UnityEngine;

public class PlayerAnimStateHandler : MonoBehaviour
{
    Animator animator;
    PlayerStateMgr stateMgr;

    internal IPlayerAnimState idleState, walkState, jumpState, fallState, dashState, wallFallState;
    IPlayerAnimState currentState;

    AnimatorCtrl animatorCtrl;
    PlayerStatus playerStatus;

    SpriteRenderer spriteRenderer;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        stateMgr = GetComponent<PlayerStateMgr>();
        playerStatus = GetComponent<PlayerStatus>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        animatorCtrl = new AnimatorCtrl(animator);

        idleState = new IdleState(animatorCtrl, this, stateMgr);
        walkState = new WalkState(animatorCtrl, this, stateMgr);
        jumpState = new JumpState(animatorCtrl);
        fallState = new FallState(animatorCtrl);
        dashState = new DashState(animatorCtrl);
        wallFallState = new WallFallState(animatorCtrl);

        currentState = idleState;
    }

    private void Update()
    {
        if (playerStatus.playerdirection)
            spriteRenderer.flipX = false;
        else
            spriteRenderer.flipX = true;
    }

    public void ChangeAnimState(IPlayerAnimState newState)
    {
        currentState.Exit();
        currentState = newState;
        currentState.Enter();
    }

}

public class AnimatorCtrl
{
    Animator animator;
    public AnimatorCtrl(Animator animator)
    {
        this.animator = animator;
    }

    public void StartAnim(string name)
    {
        animator.SetBool(name, true);
    }

    public void StopAnim(string name)
    {
        animator.SetBool(name, false);
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
