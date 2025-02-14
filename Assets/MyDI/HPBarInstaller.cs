using UnityEngine;
using Zenject;
using HPBar;

public class HPBarInstaller : MonoInstaller
{
    [SerializeField] GameObject baseObj, midPrefab, topObj, hpUnitPrefab;
    [SerializeField] HPBarHandler health;

    public override void InstallBindings()
    {
        Container.Bind<PlayerHP>()
            .AsSingle();

        Container.Bind<IPlayerHP>()
            .To<PlayerHP>()
            .FromResolve();

        Container.Bind<PlayerHPVisual>()
            .AsSingle();

        Container.Bind<GameObject>()
            .WithId("Base")
            .FromInstance(baseObj);

        Container.Bind<GameObject>()
            .WithId("Top")
            .FromInstance(topObj);

        Container.Bind<GameObject>()
            .WithId("Mid")
            .FromInstance(midPrefab);

        Container.Bind<GameObject>()
            .WithId("HPUnit")
            .FromInstance(hpUnitPrefab);

        Container.Bind<int>()
            .WithId("BarHeight")
            .FromInstance(12);

        Container.Bind<IHealth>()
            .FromInstance(health)
            .AsSingle();
    }
}
