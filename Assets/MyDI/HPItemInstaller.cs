using Item.HP;
using UnityEngine;
using Zenject;

public class HPItemInstaller : MonoInstaller
{
    [SerializeField]
    GameObject hpItemPrefab, spawnerPrefab;

    [SerializeField]
    Transform hpItemParent;

    [SerializeField]
    HPItemInfo hpItemInfo;

    public override void InstallBindings()
    {
        Container.Bind<HPItemInfo>()
            .WithId("HPItem")
            .FromInstance(hpItemInfo)
            .AsSingle();

        Container.BindFactory<HPItem, HPItem.Factory>()
            .FromComponentInNewPrefab(hpItemPrefab)
            .AsSingle()
            .NonLazy();

        Container.Bind<AutoDisappearLogic>()
            .AsTransient();

        Container.Bind<HPItemSpawner>()
            .AsTransient();

        Container.Bind<Transform>()
            .WithId("HPItemParent")
            .FromInstance(hpItemParent)
            .AsCached();

        Container.Bind<ObjPoolLogic>()
            .AsSingle()
            .NonLazy();

        Container.Bind<ActLogic>()
            .AsSingle();

        Container.Bind<Item.HP.HPItemSpawner>()
            .AsSingle();

        Container.Bind<Item.HP.VisualLogic>()
            .AsSingle();

        Container.Bind<Item.HP.BlinkingHandler>()
            .AsSingle();

        Container.Bind<Item.HP.EventMediator>()
            .AsSingle();

        Container.Bind<Item.HP.RbLogic>()
            .AsTransient();

        // Spawnerに関するバインド
        Container.BindFactory<HPItemSpawner, HPItemSpawner.Factory>()
            .FromComponentInNewPrefab(spawnerPrefab)
            .AsSingle()
            .NonLazy();
    }
}
