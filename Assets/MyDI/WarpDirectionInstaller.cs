using UnityEngine;
using Zenject;
using Warp;
using ObjectPoolFactory;

public class WarpDirectionInstaller : MonoInstaller
{
    [SerializeField] private WarpDirectionInfo warpDirectionInfo;

    public override void InstallBindings()
    {
        Container.Bind<WarpPool>()
            .AsSingle()
            .WithArguments(warpDirectionInfo.Prefab, warpDirectionInfo.MaxCapacity);//これがすべての引数ではないよ

        Container.BindFactory<WarpEffectBody, WarpEffectBody.Factory>()
            .FromComponentInNewPrefab(warpDirectionInfo.Prefab)
            .AsSingle();

        Container.Bind<WarpEffectBody>()
            .AsTransient();

        Container.Bind<int>()
            .WithId("WarpDirection")
            .FromInstance(warpDirectionInfo.MaxCapacity);

        Container.Bind<PoolHandler>()
            .AsSingle();

        Container.Bind<Timer>()
            .AsTransient();

        Container.Bind<Warp.AnimatorCtrl>()
            .AsTransient();

        Container.Bind<PositionSetter>()
            .AsTransient();
    }
}
