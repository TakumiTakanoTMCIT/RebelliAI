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
    SpriteRenderer spriteRenderer;

    bool isStartedMove;

    private void Awake()
    {
        playerStatus = GameObject.Find("Player").MyGetComponent_NullChker<PlayerStatus>();
        rb = gameObject.MyGetComponent_NullChker<Rigidbody2D>();
        spriteRenderer = gameObject.MyGetComponent_NullChker<SpriteRenderer>();
        isStartedMove = false;
    }

    /// <summary>
    /// AnimationEventから呼び出される。アニメーションが終わったらシェルを動かす
    /// </summary>
    public void End_BiginingAnim()
    {
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
