using UnityEngine;
using Zenject;
using ObjectPoolFactory;
using System;
using Warp;

public class FactoryInstaller : MonoInstaller
{
    [SerializeField] private FactoryInfo factoryManager;

    public override void InstallBindings()
    {
        Container.Bind<WarpPool>()
            .AsSingle()
            .WithArguments(factoryManager.warpPrefab, factoryManager.warpMaxCapacity);

        Container.Bind<PoolHandler>()
            .AsSingle();

        Container.Bind<EnemySpawnPoser>()
            .FromComponentInHierarchy()
            .AsTransient();

        Container.Bind<DanboruPool>()
            .AsSingle()
            .WithArguments(
                factoryManager.danboruPrefab,
                factoryManager.danboruMaxCapacity
                );

        Container.Bind<IReturnable>()
            .To<DanboruPool>()
            .FromResolve();


        Container.BindFactory<EnemyBody, EnemyBodyFactory>()
            .FromComponentInNewPrefab(factoryManager.danboruPrefab)
            .AsSingle();
    }
}
