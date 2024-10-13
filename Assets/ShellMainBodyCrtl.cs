using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.Video;

public class ShellMainBodyCrtl : MonoBehaviour, IDestroyable
{
    Rigidbody2D rb;
    SpriteRenderer spriteRenderer;

    private ObjectPool<GameObject> pool;

    bool direction;

    [SerializeField] private float speed = 20.0f;

    bool isMoveable = true;

    /// <summary>
    /// InitはShellManagerでCreateShellしたときに呼び出されます
    /// </summary>
    public void Init(ObjectPool<GameObject> pool)
    {
        rb = this.gameObject.MyGetComponent_NullChker<Rigidbody2D>();
        spriteRenderer = this.gameObject.MyGetComponent_NullChker<SpriteRenderer>();

        this.pool = pool;
    }

    private void OnEnable()
    {
        isMoveable = true;
    }

    /// <summary>
    /// GetShellのタイミングで呼ばれます
    /// </summary>
    public void SetDirection(bool direction)
    {
        this.direction = direction;
        spriteRenderer.flipX = !direction;
    }

    private void FixedUpdate()
    {
        if (!isMoveable) return;

        if (direction) rb.velocity = new Vector2(speed, 0);
        else rb.velocity = new Vector2(-speed, 0);
    }

    private void Update()
    {
        if (!spriteRenderer.isVisible) DestroyShell();
    }

    public void DestroyShell()
    {
        pool.Release(this.gameObject);
    }

    public void StopMove()
    {
        isMoveable = false;
        rb.velocity = Vector2.zero;
    }
}

public class MameAnimator
{
    ShellMainBodyCrtl mainbody;
    Animator animator;
    public MameAnimator(Animator animator)
    {
        this.animator = animator;
        mainbody = animator.gameObject.MyGetComponent_NullChker<ShellMainBodyCrtl>();
    }

    public void TakeDamage()
    {
        animator.SetTrigger("isTakeDamage");
        mainbody.StopMove();
        Debug.Log("SetTrigger");
    }

    public void RefrectShell()
    {
        //追記予定です。
    }
}
