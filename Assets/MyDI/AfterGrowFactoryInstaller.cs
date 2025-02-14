using UnityEngine;
using Zenject;

public class AfterGrowFactoryInstaller : MonoInstaller
{
    [SerializeField] private GameObject afterGrowPrefab;
    public override void InstallBindings()
    {
        Container.BindFactory<AfterGrowMain, AfterGrowMain.Factory>()
            .FromComponentInNewPrefab(afterGrowPrefab)
            .AsTransient();
    }
}
