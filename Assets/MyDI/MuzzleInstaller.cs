using Zenject;
using UnityEngine;
using Muzzle;

public class MuzzleInstaller : MonoInstaller
{
    [SerializeField] MuzzulePositionManager muzzleManager;

    public override void InstallBindings()
    {
        Container.Bind<MuzzulePositionManager>()
            .FromInstance(muzzleManager)
            .AsSingle();

        Container.Bind<GameObject>()
            .WithId("Muzzle")
            .FromInstance(muzzleManager.gameObject)
            .AsSingle();
    }
}
