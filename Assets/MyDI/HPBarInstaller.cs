using UnityEngine;
using Zenject;
using HPBar;
using UnityEngine.UI;
using System.Collections.Generic;

public class HPBarInstaller : MonoInstaller
{
    [SerializeField] GameObject baseObj, midPrefab, topObj, hpUnitPrefab;
    [SerializeField] HPBarHandler health;
    [SerializeField] HPBarInfo hpBarInfo;
    [SerializeField] Image baseImage, topImage;

    public override void InstallBindings()
    {
        Container.Bind<Image>()
            .WithId("Base")
            .FromInstance(baseImage);

        Container.Bind<Image>()
            .WithId("Top")
            .FromInstance(topImage);

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

        Container.Bind<HPBarInfo>()
            .FromInstance(hpBarInfo)
            .AsSingle();

        Container.Bind<HPBar.Base.BaseHandler>()
            .AsSingle()
            .NonLazy();

        Container.Bind<HPBar.Base.RandomSpriteSetter>()
            .AsSingle();

        Container.Bind<HPBar.Base.RandomLogic>()
            .AsSingle();

        Container.Bind<HPBar.Base.VisualLogic>()
            .AsSingle();

        Container.Bind<HPBar.EventMediator>()
            .AsSingle();

        //Topに関する処理

        Container.Bind<HPBar.Top.Handler>()
            .AsSingle()
            .NonLazy();

        Container.Bind<HPBar.Top.RandomLogic>()
            .AsSingle();

        Container.Bind<HPBar.Top.VisualLogic>()
            .AsSingle();

        //Midに関する処理

        Container.Bind<HPBar.Mids>()
            .AsSingle();

        Container.Bind<HPBar.Mid.DivideLogic>()
            .AsSingle()
            .NonLazy();

        Container.Bind<HPBar.Mid.VisualLogic>()
            .AsSingle();

        Container.Bind<HPBar.Mid.RandomLogic>()
            .AsSingle();

        Container.Bind<HPBar.Mid.GroupSpriteSetter>()
            .AsSingle();

        Container.Bind<HPBar.Mid.RandomDividePoint>()
            .AsSingle();
    }
}
