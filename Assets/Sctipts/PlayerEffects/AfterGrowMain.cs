using PlayerInfo;
using UnityEngine;
using UnityEngine.Pool;
using HPBar;
using ActionStatusChk;

public class AfterGrowMain : MonoBehaviour
{
    [SerializeField] private float AddPosY = 0.5f;

    ActionStatusChecker actionStatusChecker;
    ObjectPool<GameObject> pool;
    Transform playerTransform;
    SpriteRenderer spriteRenderer;

    private void OnEnable()
    {
        //イベントの登録
        HPBarHandler.onPlayerDeath += OnPlayerDeathAndEndAnim;
        HPBarHandler.onPlayerDamage += OnPlayerDeathAndEndAnim;
    }

    private void OnDisable()
    {
        //イベントの解除
        HPBarHandler.onPlayerDeath -= OnPlayerDeathAndEndAnim;
        HPBarHandler.onPlayerDamage -= OnPlayerDeathAndEndAnim;
    }

    public void Init(ObjectPool<GameObject> pool, Transform transform, ActionStatusChecker actionStatusChecker)
    {
        this.pool = pool;
        this.playerTransform = transform;
        this.actionStatusChecker = actionStatusChecker;
    }

    public void StartAnim_Movement()
    {
        spriteRenderer = this.gameObject.MyGetComponent_NullChker<SpriteRenderer>();
        spriteRenderer.flipX = !actionStatusChecker.Direction;
        spriteRenderer.flipY = Random.Range(0, 2) == 0;

        Vector2 pos;
        pos = playerTransform.position;
        pos.y += Random.Range(-0.1f, AddPosY);
        this.transform.position = pos;
    }

    public void EndAnim()
    {
        //if (!gameObject.activeSelf) return;
        pool.Release(this.gameObject);
    }

    //イベントハンドラー
    void OnPlayerDeathAndEndAnim()
    {
        Debug.Log("OnPlayerDeathAndEndAnim");
        EndAnim();
    }
}
