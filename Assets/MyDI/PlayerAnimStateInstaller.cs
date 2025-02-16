using UnityEngine;
using Zenject;

public class PlayerAnimStateInstaller : MonoInstaller
{
    [SerializeField] PlayerAnimStateHandler playerAnimStateHandler;
    [SerializeField] Animator playerAnimator;

    public override void InstallBindings()
    {
        Container.Bind<AnimatorCtrl>()
            .AsSingle()
            .WithArguments(playerAnimStateHandler, playerAnimator);

        //DamageStateの継承しているインターフェースをバインドするのにFromResolve()を使うため書いています。
        Container.Bind<DamageState>()
            .AsSingle()
            .WithArguments("isDamaging");

        Container.Bind<IPlayerAnimState>()
            .WithId("Damage")
            .To<DamageState>()
            .FromResolve();

        Container.Bind<IDamageStateSubject>()
            .To<DamageState>()
            .FromResolve();
    }
}
