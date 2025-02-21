using UnityEngine;
using Zenject;
using WallFallEffect;
using PlayerState;

public class WallFallEffectInstaller : MonoInstaller
{
    [SerializeField] private GameObject wallFallEffectPrefab;

    public override void InstallBindings()
    {
        Container.BindFactory<WallFallEffectBody, EffectFactory>()
            .FromComponentInNewPrefab(wallFallEffectPrefab)
            .AsSingle();

        Container.Bind<WallFallEffectPoolLogic>()
            .AsSingle()
            .NonLazy();

        //別にこれはTransientにする意味がないので、Singleでやりました。
        Container.Bind<RandomColorLogic>()
            .AsSingle()
            .NonLazy();

        Container.Bind<AnimLogic>()
            .AsTransient();

        Container.Bind<WallFallEffectHandler>()
            .AsSingle()
            .NonLazy();

        Container.Bind<EventMediator>()
            .AsSingle()
            .NonLazy();
    }
}
