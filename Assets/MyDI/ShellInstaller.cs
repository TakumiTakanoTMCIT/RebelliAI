using UnityEngine;
using Zenject;
using PlayerShot;
using LowChargeShot;

public class ShellInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
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

        Container.Bind<IAnimatable>()
            .WithId("LowCharge")
            .To<LowChargeShot.AnimCtrl>()
            .AsTransient();

        Container.Bind<IAnimatable>()
            .WithId("FullCharge")
            .To<FullChargeShotAnimCtrl>()
            .AsTransient();

        Container.Bind<FullCharge.MoveLogic>()
            .AsTransient();

        Container.BindFactory<FullCharge.MoveLogic, FullCharge.MoveLogic.Factory>()
            .AsSingle();

        Container.Bind<FullCharge.HitBoxCtrl>()
            .AsTransient();

        Container.Bind<FullCharge.SetInitialPositionLogic>()
            .AsTransient();

        Container.Bind<FullCharge.StateCtrl>()
            .AsTransient();

        Container.Bind<FullCharge.VisualCtrl>()
            .AsTransient();
    }
}
