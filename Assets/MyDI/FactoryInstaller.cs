using UnityEngine;
using Zenject;
using ObjectPoolFactory;
using Warp;

public class FactoryInstaller : MonoInstaller
{
    [SerializeField] private FactoryInfo factoryManager;

    public override void InstallBindings()
    {
        Container.Bind<EnemySpawnPoser>()
            .FromComponentInHierarchy()
            .AsTransient();

        Container.Bind<DanboruPool>()
            .AsSingle()
            .WithArguments(
                factoryManager.danboruPrefab,
                factoryManager.danboruMaxCapacity
                );

        Container.BindFactory<EnemyBody, EnemyBodyFactory>()
            .FromComponentInNewPrefab(factoryManager.danboruPrefab)
            .AsSingle();
    }
}
