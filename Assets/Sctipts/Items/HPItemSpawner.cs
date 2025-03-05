using UnityEngine;
using UniRx;
using System;
using Zenject;

[RequireComponent(typeof(SpriteRenderer))]
public class HPItemSpawner : MonoBehaviour
{
    public class Factory : PlaceholderFactory<HPItemSpawner> { }

    //Inject
    private Item.HP.HPItemSpawner itemSpawner;

    private IReactiveProperty<bool> isVisible = new ReactiveProperty<bool>(false);
    private GameObject myHPInstance;

    SpriteRenderer spRenderer;

    [Inject]
    public void Construct(Item.HP.HPItemSpawner itemSpawner)
    {
        this.itemSpawner = itemSpawner;
    }

    private void Awake()
    {
        spRenderer = GetComponent<SpriteRenderer>();
        if (spRenderer == null)
        {
            Debug.LogError("SpriteRendererがアタッチされていません");
            return;
        }
    }

    private void OnEnable()
    {
        //画面内に入ったらアイテムを生成する。
        //DistinctUntilChanged()で連続して同じ値が流れてきた場合は無視する。
        //Updateで意味ないじゃん！と思っても大丈夫です。
        isVisible
            .DistinctUntilChanged()
            //.Where(_ => myHPInstance != null)//←アクティブならまだ画面内に写っているということ！もしReleaseしているならfalseになってるよ！
            .Where(isVisible => isVisible)
            .Subscribe(_ =>
                Spawn()
            )
            .AddTo(this);
    }

    private void Update()
    {
        isVisible.Value = spRenderer.isVisible;

        if (myHPInstance == null) return;
        if (!myHPInstance.activeSelf)
        {
            myHPInstance = null;
        }
    }

    private void Spawn()
    {
        myHPInstance = itemSpawner.DropHPItem(transform.position);
    }
}
