using UnityEngine;
using UniRx;
using Cysharp.Threading.Tasks;
using System;
using Zenject;
using ObjectPoolFactory;
using Warp;

public class WarpDirection : MonoBehaviour
{
    [SerializeField] private int MakeAmount = 20;
    [SerializeField] private float createIntervel = 0.1f, randomXRange = 1f, initialYPossition = 7f, showingTime = 1f;
    [SerializeField] private Transform playerTransform;

    [Inject]
    private WarpPool pool;

    private Subject<Unit> onCompletedWarpEffect = new Subject<Unit>();
    public IObservable<Unit> OnCompletedWarpEffect => onCompletedWarpEffect;

    public async UniTask StartWarpDirection()
    {
        //MakeAmount回エフェクトを上から下に生成（OnStageのとき）
        //OffStageのときにはプレイヤーのワープアニメーションが終わったらエフェクトを下から上に生成していく。

        for (int count = 0; count < MakeAmount; count++)
        {
            //インターバルだけ待機
            try
            {
                await UniTask.Delay(TimeSpan.FromSeconds(createIntervel));
            }
            catch (Exception e)
            {
                Debug.Log(e);
                return;
            }

            pool.GetObject().gameObject.MyGetComponent_NullChker<WarpEffectBody>().Init(
                pool,
                showingTime,
                playerTransform.position.x + UnityEngine.Random.Range(-randomXRange, randomXRange),
                UnityEngine.Random.Range(playerTransform.position.y, playerTransform.position.y + initialYPossition));
        }
        //すべて生成し終わったらプレイヤーのワープアニメーションが終わるまで待つ
        onCompletedWarpEffect.OnNext(Unit.Default);
    }
}
