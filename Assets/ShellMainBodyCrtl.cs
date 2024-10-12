using UnityEngine;
using UnityEngine.Pool;

public class ShellMainBodyCrtl : MonoBehaviour,IDestroyable
{
    Rigidbody2D rb;
    SpriteRenderer spriteRenderer;
    MameShellManager shellManager;

    private ObjectPool<GameObject> pool;

    bool direction;

    float speed;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    /// <summary>
    /// InitはShellManagerでCreateShellしたときに呼び出されます
    /// </summary>
    public void Init(MameShellManager shellManager, ObjectPool<GameObject> pool, float speed)
    {
        this.shellManager = shellManager;
        this.pool = pool;
        this.speed = speed;
    }

    /// <summary>
    /// GetShellのタイミングで呼ばれます
    /// </summary>
    public void SetDirection(bool direction)
    {
        this.direction = direction;
    }

    private void FixedUpdate()
    {
        if (direction) rb.velocity = new Vector2(speed, 0);
        else rb.velocity = new Vector2(-speed, 0);
    }

    private void Update()
    {
        if (!spriteRenderer.isVisible) pool.Release(this.gameObject);
    }

    public void DestroyShell()
    {
        pool.Release(this.gameObject);
    }
}
