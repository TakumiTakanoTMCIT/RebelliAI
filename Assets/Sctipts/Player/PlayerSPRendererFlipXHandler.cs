using UnityEngine;
using Zenject;
using UniRx;
using PlayerFlip;

public class PlayerSPRendererFlipXHandler : MonoBehaviour
{
    [Inject]
    public void Construct([Inject(Id = "PlayerSPRenderer")] SpriteRenderer spriteRenderer, IDirection direction)
    {
        //directionが変更されたら、spriteRendererの向きを変更する
        direction.Direction.Subscribe(direction =>
        {
            //なぜ!directionなのかというと、UnityではflipXがtrueのときに反転するためです
            //つまり、trueのときに右を向かせるには、flipXをfalseにする必要があります
            spriteRenderer.flipX = !direction;
        })
        .AddTo(this);
    }
}
