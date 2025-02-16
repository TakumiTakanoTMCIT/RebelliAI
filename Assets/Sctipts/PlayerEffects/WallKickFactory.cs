using ActionStatusChk;
using UnityEngine;
using UnityEngine.Pool;
using Zenject;

public class WallKickFactory : MonoBehaviour
{
    //Inject
    private IPlayerDirection playerDirection;

    [SerializeField] int maxCapacity = 10;

    [SerializeField] private GameObject player;
    [SerializeField] float YPosAdd = 0.5f;
    [SerializeField] private Transform parentTransform;
    [SerializeField] private ActionStatusChecker actionStatusChk;
    GameObject preafab;

    private ObjectPool<GameObject> pool;

    [Inject]
    public void Construct(IPlayerDirection playerDirection)
    {
        this.playerDirection = playerDirection;
    }

    private void Awake()
    {
        preafab = Resources.Load<GameObject>("WallKickSparkBody");

        pool = new ObjectPool<GameObject>(
            createFunc: CreateEffect,
            actionOnGet: OnGetEffect,
            actionOnRelease: OnReleaseEffect,
            actionOnDestroy: OnDestroyEffect,
            collectionCheck: true,
            defaultCapacity: maxCapacity
        );

        for (int i = 0; i < maxCapacity; i++)
        {
            var effect = CreateEffect();
            effect.transform.SetParent(parentTransform);
            effect.SetActive(false);
            pool.Release(effect);
        }
    }

    private GameObject CreateEffect()
    {
        var effect = Instantiate(preafab);
        effect.gameObject.MyGetComponent_NullChker<WallKickSparkBody>().Init(this, player.transform, YPosAdd);

        //Falseにするのは初期化が終わったあとでやれよ！！！きょうくんだ！
        effect.SetActive(false);
        return effect;
    }

    private void OnGetEffect(GameObject effect)
    {
        effect.SetActive(true);
    }

    private void OnReleaseEffect(GameObject effect)
    {
        effect.SetActive(false);
    }

    private void OnDestroyEffect(GameObject effect)
    {
        Destroy(effect);
    }

    public void ReleaseEffect(GameObject obj)
    {
        pool.Release(obj);
    }

    public void MakeEffect(Transform playertransform)
    {
        pool.Get().GetComponent<WallKickSparkBody>().StartMove(playerDirection.Direction.Value);
    }
}
