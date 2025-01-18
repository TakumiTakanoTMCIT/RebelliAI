using UnityEngine;
using Zenject;
using Warp;
using ObjectPoolFactory;
using Enemy;

public class WarpDirectionInstaller : MonoInstaller
{
    [SerializeField] private WarpDirectionInfo warpDirectionInfo;
    [SerializeField] private FactoryInfo factoryManager;

    public override void InstallBindings()
    {
        Container.Bind<PoolReleaser>()
            .AsSingle();

        Container.Bind<WarpPool>()
            .AsSingle()
            .WithArguments(warpDirectionInfo.Prefab, warpDirectionInfo.MaxCapacity);//これがすべての引数ではないよ

        Container.BindFactory<WarpEffectBody, WarpEffectBody.Factory>()
            .FromComponentInNewPrefab(warpDirectionInfo.Prefab)
            .AsSingle();

        Container.Bind<DanboruPool>()
            .AsSingle()
            .WithArguments(
                factoryManager.danboruPrefab,
                factoryManager.danboruMaxCapacity
                );

        //EnemySpawnerについてのサブコンテナを登録します。今からやります

        Container.Bind<EnemySpawnPoser>()
            .FromComponentInHierarchy()
            .AsTransient();

        Container.BindFactory<EnemyBody, EnemyBodyFactory>()
            .FromComponentInNewPrefab(factoryManager.danboruPrefab)
            .AsSingle();

        //必要なものはここで登録
        Container.Bind<PositionSetter>()
            .AsTransient();

        Container.Bind<Warp.AnimatorCtrl>()
            .AsTransient();

        Container.Bind<Timer>()
            .AsTransient();

        Container.Bind<WarpEffectBody>()
            .FromSubContainerResolve()
            .ByNewContextPrefab(warpDirectionInfo.Prefab)
            .AsTransient();

        Container.Bind<int>()
            .WithId("WarpDirection")
            .FromInstance(warpDirectionInfo.MaxCapacity);

        Container.Bind<PoolHandler>()
            .AsSingle();
    }
}
