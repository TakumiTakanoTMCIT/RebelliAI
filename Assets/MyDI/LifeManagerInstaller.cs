using UnityEngine;
using Zenject;

public class LifeManagerInstaller : MonoInstaller
{
    [SerializeField] private LifeManager lifeManager;
    public override void InstallBindings()
    {
        Container.Bind<LifeManager>()
            .FromInstance(lifeManager)
            .AsSingle();
    }
}
