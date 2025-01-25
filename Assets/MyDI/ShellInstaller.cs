using UnityEngine;
using Zenject;
using PlayerShot;
using LowChargeShot;

public class ShellInstaller : MonoInstaller
{
    [SerializeField] private ShellMoveStats lowerChargeStats;

    public override void InstallBindings()
    {
        Container.Bind<ShellMoveStats>()
            .WithId("LowerCharge")
            .FromInstance(lowerChargeStats)
            .AsSingle();

        Container.Bind<MoveCtrl>()
            .AsTransient();

        Container.Bind<VisualCtrl>()
            .AsTransient();

        Container.Bind<InitPositioner>()
            .AsTransient();

        Container.Bind<HitBoxCtrl>()
            .AsTransient();

        Container.Bind<StateCtrl>()
            .AsTransient();

        /*Container.Bind<IMovable>()
            .WithId("LowerCharge")
            .To<ShellMoveCtrlBase>()
            .AsSingle();*/
    }
}
