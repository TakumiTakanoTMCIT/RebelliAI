using UnityEngine;

public class DanboruAnimStateMgr : MonoBehaviour
{
    Animator animator;
    AnimHandler animHandler;
    DanboruActMgr actMgr;
    bool isExucutable;

    [SerializeField] float idlingTime = 5f, chargeTime = 3f;
    [SerializeField] internal float jumpForce = 10f, jumpForceThreshold = 0.1f;
    internal IEnemyAnimState currentState, IdleState, ChargeState, ReleaseState;
    private void Awake()
    {
        isExucutable = false;
        animator = gameObject.MyGetComponent_NullChker<Animator>();
        actMgr = gameObject.MyGetComponent_NullChker<DanboruActMgr>();

        animHandler = new AnimHandler(animator);

        IdleState = new Idle(this, animHandler, idlingTime);
        ChargeState = new Charge(this, animHandler, chargeTime);
        ReleaseState = new Release(this, animHandler, actMgr);
    }

    //Bodyから呼び出される。SetActive(true)されたときに呼び出される
    public void MyAwake()
    {
        isExucutable = true;

        currentState = IdleState;
        currentState.OnEnter();
    }

    private void FixedUpdate()
    {
        if (!isExucutable) return;
        currentState.OnFixedUpdate();
    }

    private void LateUpdate()
    {
        if (!isExucutable) return;
        currentState.OnLateUpdate();
    }

    public void ReadyToJump()
    {
        actMgr.Jump(jumpForce);
    }

    public void ChangeState(IEnemyAnimState nextState)
    {
        isExucutable = false;

        currentState.OnExit();
        currentState = nextState;
        currentState.OnEnter();

        isExucutable = true;
    }
}

public class AnimHandler
{
    Animator animator;
    public AnimHandler(Animator animator)
    {
        this.animator = animator;
    }

    public void SetBool(string animName, bool value)
    {
        animator.SetBool(animName, value);
    }

    public void SetTrigger(string animName)
    {
        animator.SetTrigger(animName);
    }
}

public interface IEnemyAnimState
{
    void OnEnter();
    void OnFixedUpdate();
    void OnLateUpdate();
    void OnExit();
}

public class Idle : IEnemyAnimState
{
    float countDownTime;

    internal float idlingTime, hereIdlingTime;

    AnimHandler animHandler;
    DanboruAnimStateMgr animStateMgr;
    public Idle(DanboruAnimStateMgr animStateMgr, AnimHandler animHandler, float idlingTime)
    {
        this.idlingTime = idlingTime;
        this.animHandler = animHandler;
        this.animStateMgr = animStateMgr;
    }

    public void OnEnter()
    {
        animHandler.SetBool("isIdle", true);
        hereIdlingTime = UnityEngine.Random.Range(1, idlingTime);
        countDownTime = 0;
    }

    public void OnFixedUpdate()
    {
        countDownTime += Time.deltaTime;
    }

    public void OnLateUpdate()
    {
        if (countDownTime >= hereIdlingTime)
        {
            animStateMgr.ChangeState(animStateMgr.ChargeState);
        }
    }

    public void OnExit()
    {
        animHandler.SetBool("isIdle", false);
    }
}

public class Charge : IEnemyAnimState
{
    float coutDownTime;
    AnimHandler animHandler;
    DanboruAnimStateMgr animStateMgr;
    float chargeTime;
    public Charge(DanboruAnimStateMgr animStateMgr, AnimHandler animHandler, float chargeTime)
    {
        this.animStateMgr = animStateMgr;
        this.animHandler = animHandler;
        this.chargeTime = chargeTime;
    }

    public void OnEnter()
    {
        animHandler.SetBool("isCharge", true);
        coutDownTime = 0;
    }

    public void OnFixedUpdate()
    {
        coutDownTime += Time.deltaTime;
    }

    public void OnLateUpdate()
    {
        if (coutDownTime >= chargeTime)
        {
            animStateMgr.ChangeState(animStateMgr.ReleaseState);
        }
    }

    public void OnExit()
    {
        animHandler.SetBool("isCharge", false);
    }
}

public class Release : IEnemyAnimState
{
    DanboruGroundChk groundChk;
    AnimHandler animHandler;
    DanboruAnimStateMgr animStateMgr;
    DanboruActMgr actMgr;
    public Release(DanboruAnimStateMgr animStateMgr, AnimHandler animHandler, DanboruActMgr actMgr)
    {
        this.actMgr = actMgr;
        groundChk = animStateMgr.transform.GetChild(0).gameObject.MyGetComponent_NullChker<DanboruGroundChk>();
        this.animStateMgr = animStateMgr;
        this.animHandler = animHandler;
    }

    public void OnEnter()
    {
        animHandler.SetTrigger("OnRelease");
    }

    public void OnFixedUpdate()
    {
        if (!actMgr.IsFalling()) return;
        if (groundChk.IsGround())
        {
            animStateMgr.ChangeState(animStateMgr.IdleState);
        }
    }

    public void OnLateUpdate()
    {

    }

    public void OnExit()
    {

    }
}
