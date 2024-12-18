using UniRx;
using PlayerInfo;
using UnityEngine;

public interface IDestroyable
{
    void DestroyShell();
}

public class ChargedShellBodyCtrl : MonoBehaviour, IDestroyable
{
    [SerializeField] private int myLevel = 1;
    [SerializeField] private float speed = 5.0f;

    PlayerStatus playerStatus;
    Rigidbody2D rb;
    BoxCollider2D boxCollider2D;
    SpriteRenderer spriteRenderer;
    ChargedShellAnimatorCtrl animatorCtrl;

    bool isStartedMove;

    private void Awake()
    {
        playerStatus = GameObject.Find("Player").MyGetComponent_NullChker<PlayerStatus>();
        boxCollider2D = gameObject.MyGetComponent_NullChker<BoxCollider2D>();
        rb = gameObject.MyGetComponent_NullChker<Rigidbody2D>();
        spriteRenderer = gameObject.MyGetComponent_NullChker<SpriteRenderer>();
        isStartedMove = false;
    }

    public void Init(ChargedShellAnimatorCtrl animatorCtrl)
    {
        this.animatorCtrl = animatorCtrl;
    }

    private void Start()
    {
        animatorCtrl.StartAnim();
        boxCollider2D.enabled = false;
        isStartedMove = false;
    }

    private void Update()
    {
        //スタートアニメーション中はプレイヤーの銃口に追従する
        if(isStartedMove) return;
        transform.position = playerStatus.transform.position;
    }

    /// <summary>
    /// AnimationEventから呼び出される。アニメーションが終わったらシェルを動かす
    /// </summary>
    public void End_BiginingAnim()
    {
        animatorCtrl.MoveAnim();
        boxCollider2D.enabled = true;
        MoveShell();
    }

    private void MoveShell()
    {
        gameObject.MyGetComponent_NullChker<ChargedShellDamageAbleFinder>().IsExtraDamage(playerStatus.IsDashNow());

        if (playerStatus.playerdirection)
        {
            rb.velocity = new Vector2(speed, 0);
        }
        else
        {
            rb.velocity = new Vector2(-speed, 0);
        }

        spriteRenderer.flipX = !playerStatus.playerdirection;
        isStartedMove = true;
        SoundEffectCtrl.OnPlayShotSE.OnNext(myLevel);
    }

    private void OnBecameInvisible()
    {
        if (!isStartedMove) return;
        DestroyShell();
    }

    public void DestroyShell()
    {
        Destroy(this.gameObject);
    }

    public void StopMove()
    {
        rb.velocity = Vector2.zero;
    }

    //アニメーションイベント
    void FinishRefrectAnim()
    {
        DestroyShell();
    }
}

public class ChargedShellAnimatorCtrl
{
    private Animator animator;
    ChargedShellBodyCtrl bodyCtrl;

    public ChargedShellAnimatorCtrl(Animator animator, ChargedShellBodyCtrl bodyCtrl)
    {
        this.animator = animator;
        this.bodyCtrl = bodyCtrl;
    }

    public void StartAnim()
    {
        animator.SetTrigger("onReset");
    }

    public void MoveAnim()
    {
        animator.SetTrigger("onFinishBigin");
    }

    public void TakeDamage()
    {
        //Debug.Log("TakeDamage。チャージシェルのアニメーションを再生します");
        bodyCtrl.StopMove();
        animator.SetTrigger("isHit");
    }

    public void RefrectShell()
    {
        //Debug.Log("RefrectShell。チャージシェルのアニメーションを再生します");
        bodyCtrl.StopMove();
        animator.SetTrigger("isRefrect");
    }
}
