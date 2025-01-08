using UnityEngine;
using Zenject;
using ObjectPoolFactory;
using Enemy;

public class FactoryInstaller : MonoInstaller
{
    [SerializeField] private FactoryInfo factoryManager;

    public override void InstallBindings()
    {

    }
}
