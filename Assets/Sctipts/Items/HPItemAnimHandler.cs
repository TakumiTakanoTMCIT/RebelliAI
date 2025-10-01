using UnityEngine;
using UniRx;
using Zenject;
using Item.HP;

public class HPItemAnimHandler : MonoBehaviour
{
    private SimpleAnimation animCtrl;
    private Rigidbody2D rb;

    // [Inject]
    private Item.HP.EventMediator eventMediator;
    private Item.HP.RbLogic rbLogic;

    private bool isPlaySpawnAnim = false;

    [Inject]
    public void Construct(Item.HP.EventMediator eventMediator, Item.HP.RbLogic rbLogic)
    {
        this.eventMediator = eventMediator;
        this.rbLogic = rbLogic;
    }

    private void Awake()
    {
        animCtrl = gameObject.MyGetComponent_NullChker<SimpleAnimation>();
        rb = gameObject.MyGetComponent_NullChker<Rigidbody2D>();
    }

    private void OnEnable()
    {
        isPlaySpawnAnim = false;

        animCtrl.Play("SpawnBlinking");
    }

    private void Update()
    {
        if (isPlaySpawnAnim) return;

        if (rbLogic.IsSlowlyJumping(rb, 5f))
        {
            isPlaySpawnAnim = true;
            animCtrl.Play("Spawn");
            //Debug.Log("Spawnアニメーションを再生します");
        }
    }

    //アニメーションが終わったら呼び出される
    public void EndAnim()
    {
        animCtrl.Play("Idling");
    }
}
