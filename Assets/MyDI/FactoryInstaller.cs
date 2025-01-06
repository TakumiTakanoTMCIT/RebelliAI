using UnityEngine;
using Zenject;
using ObjectPoolFactory;
using System;

public class FactoryInstaller : MonoInstaller
{
    [SerializeField] private FactoryInfo factoryManager;

    public override void InstallBindings()
    {
        Container.Bind<WarpPool>()
            .AsSingle()
            .WithArguments(factoryManager.warpPrefab, factoryManager.warpMaxCapacity);

        Container.Bind<EnemySpawnPoser>()
            .FromComponentInHierarchy()
            .AsTransient();

        Container.BindFactory<EnemyBody, EnemyBody.Factory>()
            .FromComponentInNewPrefab(factoryManager.danboruPrefab);

        Container.Bind<Lazy<EnemyBody.Factory>>()
            .FromMethod(ctx => new Lazy<EnemyBody.Factory>(() => ctx.Container.Resolve<EnemyBody.Factory>()))
            .AsSingle();

        Container.Bind<DanboruPool>()
            .AsSingle()
            .WithArguments<GameObject, int, Lazy<EnemyBody.Factory>>(
                factoryManager.danboruPrefab,
                factoryManager.danboruMaxCapacity,
                new Lazy<EnemyBody.Factory>(() => Container.Resolve<EnemyBody.Factory>())
                );
    }
}
