using System;
using UnityEngine;
using UnityEngine.Pool;
using PlayerShot;

public class ShellMainBodyCrtl : MonoBehaviour
{
    Rigidbody2D rb;
    SpriteRenderer spriteRenderer;

    public event Action onDead;

    private ObjectPool<GameObject> pool;

    bool direction;

    [SerializeField] private float speed = 20.0f;

    bool isMoveable = true;

    bool isReleased = false;

    /// <summary>
    /// InitはAllShellManagerでCreateShellしたときに呼び出されます
    /// </summary>
    public void Init(ObjectPool<GameObject> pool)
    {
        rb = this.gameObject.MyGetComponent_NullChker<Rigidbody2D>();
        spriteRenderer = this.gameObject.MyGetComponent_NullChker<SpriteRenderer>();
        this.pool = pool;
    }

    /// <summary>
    /// GetShellのタイミングで呼ばれます
    /// </summary>
    public void GetShellAndSetDirection(bool direction, bool isDashExtraDamage)
    {
        this.direction = direction;
        spriteRenderer.flipX = !direction;
        gameObject.MyGetComponent_NullChker<DamageAbleFinder>().SetDamageAmount(isDashExtraDamage);
        isMoveable = true;
        isReleased = false;
        SoundEffectCtrl.OnPlayShotSE.OnNext(0);
    }

    private void FixedUpdate()
    {
        if (!isMoveable) return;

        if (direction) rb.velocity = new Vector2(speed, 0);
        else rb.velocity = new Vector2(-speed, 0);
    }

    private void OnBecameInvisible()
    {
        DestroyShell();
    }

    //アニメーションイベント
    //ダメージを与えるアニメーションが終わったら呼び出される
    public void DestroyShell()
    {
        if (isReleased) return;
        isReleased = true;
        onDead?.Invoke();
        pool.Release(gameObject);
    }

    public void StopMove()
    {
        isMoveable = false;
        rb.velocity = Vector2.zero;
    }

    public void Refrect()
    {
        direction = !direction;
        spriteRenderer.flipX = !spriteRenderer.flipX;
        StopMove();
        if (direction)
            rb.velocity = new Vector2(speed / 1.4f, speed / 1.4f);
        else
            rb.velocity = new Vector2(-speed / 1.4f, speed / 1.4f);
    }
}

public class MameAnimator
{
    ShellMainBodyCrtl mainbody;
    Animator animator;

    internal bool isExcuted = false;

    public MameAnimator(Animator animator)
    {
        this.animator = animator;
        mainbody = animator.gameObject.MyGetComponent_NullChker<ShellMainBodyCrtl>();
    }

    public void TakeDamage()
    {
        if (isExcuted) return;
        isExcuted = true;
        animator.SetTrigger("isTakeDamage");
        mainbody.StopMove();
    }

    public void RefrectShell()
    {
        if (isExcuted) return;
        isExcuted = true;
        mainbody.Refrect();
    }
}
