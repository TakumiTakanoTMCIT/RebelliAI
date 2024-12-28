using System.ComponentModel;
using UnityEngine;
using Zenject;

public class PlayerStateInstaller : MonoInstaller
{
    [SerializeField] private DashSparkFactory dashSparkFactory;
    [SerializeField] private WallKickFactory wallKickFactory;
    [SerializeField] private DamageTimeHandler damageTimeHandler;
    [SerializeField] private PlayerDashKeepManager playerDashKeepManager;

    public override void InstallBindings()
    {
        Container.Bind<DashSparkFactory>().FromInstance(dashSparkFactory).AsSingle();
        Container.Bind<WallKickFactory>().FromInstance((WallKickFactory)wallKickFactory).AsSingle();
        Container.Bind<DamageTimeHandler>().FromInstance(damageTimeHandler).AsSingle();
        Container.Bind<PlayerDashKeepManager>().FromInstance(playerDashKeepManager).AsSingle();
    }
}
