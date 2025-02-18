using UnityEngine;
using Zenject;

public class PlayerFlipXInstaller : MonoInstaller
{
    [SerializeField] SpriteRenderer spriteRenderer;

    public override void InstallBindings()
    {
        Container.Bind<SpriteRenderer>()
            .WithId("PlayerSPRenderer")
            .FromInstance(spriteRenderer)
            .AsSingle();

        Container.Bind<PlayerDirectionLogic>()
            .AsSingle();

        Container.Bind<IPlayerDirection>()
            .To<PlayerDirectionLogic>()
            .FromResolve();

        Container.Bind<IPlayerFlipXLogic>()
            .To<PlayerDirectionLogic>()
            .FromResolve();

        Container.Bind<PlayerFlipXHandler>()
            .AsSingle()
            .NonLazy();
    }
}
