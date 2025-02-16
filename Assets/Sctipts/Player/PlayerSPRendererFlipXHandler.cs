using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using UniRx;

public class PlayerSPRendererFlipXHandler : MonoBehaviour
{
    [Inject]
    public void Construct([Inject(Id = "PlayerSPRenderer")] SpriteRenderer spriteRenderer, IPlayerDirection direction)
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
