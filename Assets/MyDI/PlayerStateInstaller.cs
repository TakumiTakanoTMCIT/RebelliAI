using ActionStatusChk;
using PlayerAction;
using UnityEngine;
using Zenject;

public class PlayerStateInstaller : MonoInstaller
{
    [SerializeField] private DashSparkFactory dashSparkFactory;
    [SerializeField] private WallKickFactory wallKickFactory;
    [SerializeField] private DamageTimeHandler damageTimeHandler;
    [SerializeField] private PlayerDashKeepManager playerDashKeepManager;

    [SerializeField] private Rigidbody2D playerRb;
    [SerializeField] private ActionStatusChecker actionStatusChecker;

    public override void InstallBindings()
    {
        Container.Bind<DashSparkFactory>().FromInstance(dashSparkFactory).AsSingle();
        Container.Bind<WallKickFactory>().FromInstance(wallKickFactory).AsSingle();
        Container.Bind<DamageTimeHandler>().FromInstance(damageTimeHandler).AsSingle();
        Container.Bind<PlayerDashKeepManager>().FromInstance(playerDashKeepManager).AsSingle();
        Container.Bind<ActionHandler>().AsSingle().WithArguments(playerRb, actionStatusChecker, playerDashKeepManager);
    }
}
