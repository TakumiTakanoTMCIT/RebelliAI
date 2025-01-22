using UnityEngine;
using Zenject;
using PlayerState;
using KeyHandler;
using PlayerAction;

public class PlayerStateMgrInstaller : MonoInstaller
{
    [SerializeField] private InputHandler inputHandler;
    [SerializeField] private ActionStatusChk.ActionStatusChecker actionStatusChecker;
    [SerializeField] private PlayerAnimStateHandler animStateHandler;
    [SerializeField] private PlayerDashKeepManager dashKeepManager;

    public override void InstallBindings()
    {
        Container.Bind<PlayerStateData>()
            .AsSingle()
            .WithArguments(inputHandler, actionStatusChecker, animStateHandler, dashKeepManager);

        Container.Bind<IState>()
            .WithId("Idle")
            .To<PlayerState.Idle>()
            .AsSingle();

        Container.Bind<IState>()
            .WithId("Walk")
            .To<PlayerState.Walk>()
            .AsSingle();

        Container.Bind<IState>()
            .WithId("Dash")
            .To<PlayerState.Dash>()
            .AsSingle();

        Container.Bind<IState>()
            .WithId("Jump")
            .To<PlayerState.Jump>()
            .AsSingle();

        Container.Bind<IState>()
            .WithId("Fall")
            .To<PlayerState.Fall>()
            .AsSingle();

        Container.Bind<IState>()
            .WithId("WallFall")
            .To<PlayerState.WallFall>()
            .AsSingle();

        Container.Bind<IState>()
            .WithId("WallKick")
            .To<PlayerState.WallKick>()
            .AsSingle();

        Container.Bind<IState>()
            .WithId("Damage")
            .To<PlayerState.DamageState>()
            .AsSingle();

        Container.Bind<IState>()
            .WithId("Death")
            .To<PlayerState.DeathState>()
            .AsSingle();
    }
}
