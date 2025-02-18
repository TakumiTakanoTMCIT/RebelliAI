using UnityEngine;
using Zenject;

public class EventStreamerInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        Container.Bind<EventStreamer>()
            .AsSingle();
    }
}
