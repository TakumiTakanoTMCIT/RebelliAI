using Door;
using UnityEngine;
using Zenject;

public class DoorInstaller : MonoInstaller
{
    [SerializeField] private DoorManager doorManager;
    public override void InstallBindings()
    {
        Container.Bind<DoorManager>()
            .FromInstance(doorManager)
            .AsSingle();

        Container.Bind<ColliderLogic>()
            .AsTransient();

        Container.Bind<TagLogic>()
            .AsTransient();

        Container.Bind<ParentSetter>()
            .AsTransient();
    }
}
