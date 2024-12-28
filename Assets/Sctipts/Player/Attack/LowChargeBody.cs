using UnityEngine;
using PlayerShot;

class LowChargeBody : ShellBase
{
    [SerializeField] private int myLevel = 1;

    BoxCollider2D boxCollider2D;

    protected override void CustomAwake()
    {
        boxCollider2D = gameObject.MyGetComponent_NullChker<BoxCollider2D>();
    }

    protected override void CustomStart()
    {
        boxCollider2D.enabled = false;
        isStartedMove = false;
    }

    private void Update()
    {
        //移動を始めていなかったらプレイヤーの位置に合わせる
        if(!isStartedMove)
        {
            transform.position = playerTransform.position;
        }
    }

    public override void End_BiginingAnim()
    {
        boxCollider2D.enabled = true;
        isStartedMove = true;
        animatorCtrl.MoveAnim();
        MoveShell();
    }

    public override void DestroyShell()
    {
        Debug.LogError("LowChargeBody DestroyShell");
        Destroy(gameObject);
    }

    protected override void OnBecameInvisible()
    {
        DestroyShell();
    }

    public override void StopMove()
    {
        rb.velocity = Vector2.zero;
    }

    protected override void CustomMoveShell()
    {
        SoundEffectCtrl.OnPlayShotSE.OnNext(myLevel);
    }

    //インターフェース
    public override void TakeDamage()
    {
        StopMove();
    }

    public override void Refrect()
    {
        StopMove();
    }
}
