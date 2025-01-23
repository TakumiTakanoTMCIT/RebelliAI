using Zenject;
using UnityEngine;
using Muzzle;

public class MuzzleInstaller : MonoInstaller
{
    [SerializeField] Transform playerTrans;
    [SerializeField] MuzzulePositionManager muzzleManager;
    [SerializeField] MuzzlePositions muzzlePositions;

    public override void InstallBindings()
    {
        Container.Bind<MuzzulePositionManager>()
            .FromInstance(muzzleManager)
            .AsSingle();

        Container.Bind<GameObject>()
            .WithId("Muzzle")
            .FromInstance(muzzleManager.gameObject)
            .AsSingle();

        Container.Bind<MuzzlePositions>()
            .FromInstance(muzzlePositions)
            .AsSingle();

        Container.Bind<Transform>()
            .WithId("PlayerTrans")
            .FromInstance(playerTrans)
            .AsSingle();
    }
}
