using UnityEngine;
using Zenject;
using PlayerFlip;

public class PlayerFlipXInstaller : MonoInstaller
{
    [SerializeField] SpriteRenderer spriteRenderer;

    public override void InstallBindings()
    {
        Container.Bind<SpriteRenderer>()
            .WithId("PlayerSPRenderer")
            .FromInstance(spriteRenderer)
            .AsSingle();

        Container.Bind<Logic>()
            .AsSingle();

        Container.Bind<IDirection>()
            .To<Logic>()
            .FromResolve();

        Container.Bind<ILogic>()
            .To<Logic>()
            .FromResolve();

        Container.Bind<Handler>()
            .AsSingle()
            .NonLazy();
    }
}
