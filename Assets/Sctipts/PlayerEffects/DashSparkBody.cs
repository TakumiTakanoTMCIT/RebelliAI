using UnityEngine;
using UnityEngine.Pool;

public class DashSparkBody : MonoBehaviour
{
    Transform playerTransform;
    SpriteRenderer spriteRenderer;
    //bool direction;

    ObjectPool<GameObject> pool;
    public void Init(ObjectPool<GameObject> pool, Transform playerTransform, bool direction)
    {
        spriteRenderer = this.gameObject.MyGetComponent_NullChker<SpriteRenderer>();

        this.pool = pool;
        this.playerTransform = playerTransform;
        spriteRenderer.flipX = direction;
        this.transform.position = playerTransform.position;
    }

    public void EndAnim()
    {
        pool.Release(gameObject);
    }
}
