using UnityEngine;
using Zenject;
using LandingEffect;

public class LandingEffectInstaller : MonoInstaller
{
    [SerializeField] GroundChk groundChk;
    [SerializeField] Transform playerTransform;
    [SerializeField] LandingEffectBody landingEffectBody;

    public override void InstallBindings()
    {
        Container.Bind<GroundChk>()
            .FromInstance(groundChk)
            .AsSingle();

        Container.Bind<PoolHandler>()
            .AsSingle();

        Container.Bind<EventMediator>()
            .AsSingle();

        Container.Bind<LandingEffectHandler>()
            .AsSingle()
            .NonLazy();

        Container.Bind<PositionLogic>()
            .AsSingle();

        Container.Bind<AnimCtrl>()
            .AsTransient();

        Container.Bind<FlipLogic>()
            .AsTransient();

        Container.Bind<MoveLogic>()
            .AsTransient();

        Container.BindFactory<LandingEffectBody, LandingEffectBody.Factory>()
            .FromComponentInNewPrefab(landingEffectBody)
            .AsSingle();
    }
}
