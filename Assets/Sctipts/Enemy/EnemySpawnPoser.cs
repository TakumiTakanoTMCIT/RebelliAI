using UnityEngine;
using Zenject;
using Enemy;
using ObjectPoolFactory;
using UniRx;

public interface IEnemyPosController
{
    void GetSpawnHandler();
    void MakeInstance();
    void ResetInstance();
}

public class EnemySpawnPoser : MonoBehaviour, IEnemyPosController
{
    //Inject
    DanboruPool danboruPool;

    EnemyBody instance;
    ExplosionSpawner explosionSpawner;
    SpriteRenderer spriteRenderer;

    [Inject]
    public void Construct(DanboruPool danboruPool)
    {
        this.danboruPool = danboruPool;
        //Debug.Log("EnemySpawnPoser Constructed");
    }

    private void Awake()
    {
        GetSpawnHandler();
    }

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        //画面外に自身とインスタンスが見えない場合、インスタンスをReleaseする
        //毎フレーム監視
        Observable.EveryUpdate()
            //もし自身が見えない場合
            .Where(_ => spriteRenderer.isVisible == false)
            //もしインスタンスが見えない場合
            .Where(_ => instance?.IsVisible() == false)
            //インスタンスをReleaseする
            .Subscribe(_ =>
            {
                DieAndReleaseObj();
            })
            .AddTo(this);

        //画面内に自身が見える場合、インスタンスを生成する
        //毎フレーム監視
        Observable.EveryUpdate()
            //もし自身が見える場合
            .Where(_ => spriteRenderer.isVisible)
            //もしインスタンスがnullの場合
            .Where(_ => instance == null)
            //インスタンスを生成する
            .Subscribe(_ =>
            {
                MakeInstance();
            })
            .AddTo(this);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1, 0, 0, 0.5f);
        Gizmos.DrawCube(transform.position, new Vector3(1, 1, 1));
    }

    //インターフェース実装
    public void GetSpawnHandler()
    {
        explosionSpawner = GameObject.Find("ExplosionFactory").MyGetComponent_NullChker<ExplosionSpawner>();
    }

    //インターフェース実装
    public void MakeInstance()
    {
        Debug.Log("MakeInstance");
        instance = danboruPool.GetObject().GetComponent<EnemyBody>();
        instance.MyAwake(transform.position, transform, explosionSpawner);
    }

    private void DieAndReleaseObj()
    {
        instance.DieAndReleaseObj();
        instance = null;
    }

    //インターフェース実装
    //実装なし
    public void ResetInstance() { }
}
