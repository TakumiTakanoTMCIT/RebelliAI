using UnityEngine;
using UnityEngine.Pool;
using HPBar;
using ActionStatusChk;
using UniRx;
using Zenject;
using PlayerFlip;

public class AfterGrowMain : MonoBehaviour
{
    public class Factory : PlaceholderFactory<AfterGrowMain>
    {
    }

    [SerializeField]
    private float AddPosY = 0.5f;

    ObjectPool<GameObject> pool;
    Transform playerTransform;
    SpriteRenderer spriteRenderer;

    //Inject
    IDirection playerDirection;

    [Inject]
    public void Construct(IDirection direction1)
    {
        this.playerDirection = direction1;
    }

    private void OnEnable()
    {
        //イベントの登録
        HPBarHandler.onPlayerDamage += OnPlayerDeathAndEndAnim;
    }

    private void OnDisable()
    {
        //イベントの解除
        HPBarHandler.onPlayerDamage -= OnPlayerDeathAndEndAnim;
    }

    public void Init(ObjectPool<GameObject> pool, Transform transform)
    {
        this.pool = pool;
        this.playerTransform = transform;
    }

    public void StartAnim_Movement()
    {
        spriteRenderer = this.gameObject.MyGetComponent_NullChker<SpriteRenderer>();
        spriteRenderer.flipX = !playerDirection.Direction.Value;
        spriteRenderer.flipY = Random.Range(0, 2) == 0;

        Vector2 pos;
        pos = playerTransform.position;
        pos.y += Random.Range(-0.1f, AddPosY);
        this.transform.position = pos;
    }

    public void EndAnim()
    {
        if (!gameObject.activeSelf) return;
        pool.Release(this.gameObject);
    }

    //イベントハンドラー
    void OnPlayerDeathAndEndAnim()
    {
        EndAnim();
    }
}
