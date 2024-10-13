using ActionStatusChk;
using PlayerInfo;
using UnityEngine;

public interface IDestroyable
{
    void DestroyShell();
}

public class ChargedShellBodyCtrl : MonoBehaviour, IDestroyable
{
    [SerializeField] private float speed = 5.0f;

    PlayerStatus playerStatus;
    Rigidbody2D rb;
    SpriteRenderer spriteRenderer;

    private void Awake()
    {
        GameObject player = GameObject.Find("Player");
        playerStatus = player.GetComponent<PlayerStatus>();
        rb = this.GetComponent<Rigidbody2D>();
        spriteRenderer = this.GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        MoveShell();
    }

    private void MoveShell()
    {
        if (playerStatus.playerdirection)
        {
            rb.velocity = new Vector2(speed, 0);
        }
        else
        {
            rb.velocity = new Vector2(-speed, 0);
        }

        spriteRenderer.flipX = !playerStatus.playerdirection;
    }

    private void Update()
    {
        if (spriteRenderer == null)
        {
            MoveShell();
        }

        if (!spriteRenderer.isVisible) DestroyShell();
    }

    public void DestroyShell()
    {
        Destroy(this.gameObject);
    }

    public void StopMove()
    {
        rb.velocity = Vector2.zero;
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
}
