using UnityEngine;
using UnityEngine.Pool;
using HPBar;
using ActionStatusChk;
using UniRx;
using Zenject;
using PlayerFlip;

public class AfterGrowMain : MonoBehaviour
{
    public class Factory : PlaceholderFactory<AfterGrowMain> { }

    [SerializeField]
    private float AddPosY = 0.5f;

    ObjectPool<GameObject> pool;
    Transform playerTransform;
    SpriteRenderer spriteRenderer;

    //Inject
    IDirection playerDirection;
    HPBar.EventMediator hpbarEventMediator;

    [Inject]
    public void Construct(IDirection direction1, HPBar.EventMediator eventMediator)
    {
        this.playerDirection = direction1;
        this.hpbarEventMediator = eventMediator;
    }

    private void Awake()
    {
        hpbarEventMediator.OnPlayerDamage.Subscribe(_ =>
        {
            EndAnim();
        })
        .AddTo(this);
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
}
