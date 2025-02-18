using UnityEngine;
using Zenject;

public class GameSettingsInstaller : MonoInstaller
{
    [SerializeField] DisposableMgr _disposableMgr;
    [SerializeField] private PlayerStats _playerStats;
    public override void InstallBindings()
    {
        Container.Bind<PlayerStats>()
            .FromInstance(_playerStats)
            .AsSingle();

        Container.Bind<DisposableMgr>()
            .FromInstance(_disposableMgr)
            .AsSingle();
    }
}
