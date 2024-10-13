using System.Collections;
using System.Collections.Generic;
using PlayerInfo;
using UnityEngine;
using UnityEngine.Pool;

public class WallKickFactory : MonoBehaviour
{
    [SerializeField] int maxCapacity = 10;

    [SerializeField] private GameObject player;
    [SerializeField] float YPosAdd = 0.5f;
    GameObject preafab;
    PlayerStatus playerStatus;

    private ObjectPool<GameObject> pool;
    private void Awake()
    {
        pool = new ObjectPool<GameObject>(
            createFunc: CreateEffect,
            actionOnGet: OnGetEffect,
            actionOnRelease: OnReleaseEffect,
            actionOnDestroy: OnDestroyEffect,
            collectionCheck: true,
            defaultCapacity: maxCapacity
        );

        preafab = Resources.Load<GameObject>("WallKickSparkBody");

        playerStatus = player.MyGetComponent_NullChker<PlayerStatus>();
    }

    private GameObject CreateEffect()
    {
        var effect = Instantiate(preafab);
        effect.gameObject.MyGetComponent_NullChker<WallKickSparkBody>().Init(this,player.transform, YPosAdd);

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
        pool.Get().GetComponent<WallKickSparkBody>().StartMove(playerStatus.playerdirection);
    }
}
