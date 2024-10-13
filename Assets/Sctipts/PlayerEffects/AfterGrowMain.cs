using PlayerInfo;
using UnityEngine;
using UnityEngine.Pool;

public class AfterGrowMain : MonoBehaviour
{
    [SerializeField] private float AddPosY = 0.5f;

    ObjectPool<GameObject> pool;
    Transform playerTransform;
    SpriteRenderer spriteRenderer;
    PlayerStatus playerStatus;

    private void Awake()
    {
        spriteRenderer = this.gameObject.MyGetComponent_NullChker<SpriteRenderer>();
    }

    public void Init(ObjectPool<GameObject> pool, Transform transform , PlayerStatus playerStatus)
    {
        this.pool = pool;
        this.playerTransform = transform;
        this.playerStatus = playerStatus;
    }

    public void StartAnim_Movement()
    {
        spriteRenderer.flipX = !playerStatus.playerdirection;
        spriteRenderer.flipY = Random.Range(0, 2) == 0;

        Vector2 pos;
        pos = playerTransform.position;
        pos.y += Random.Range(-0.1f, AddPosY);
        this.transform.position = pos;
    }

    public void EndAnim()
    {
        pool.Release(this.gameObject);
    }
}
