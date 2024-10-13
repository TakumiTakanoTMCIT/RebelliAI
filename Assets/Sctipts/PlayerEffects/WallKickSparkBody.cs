using UnityEngine;
public class WallKickSparkBody : MonoBehaviour
{
    SpriteRenderer spriteRenderer;
    WallKickFactory wallKickFactory;
    Transform playerTransform;
    float YPosAdd = 0.5f;

    public void Init(WallKickFactory wallKickFactory, Transform playertransform, float YPosAdd)
    {
        this.wallKickFactory = wallKickFactory;
        playerTransform = playertransform;
        this.YPosAdd = YPosAdd;

        spriteRenderer = this.gameObject.MyGetComponent_NullChker<SpriteRenderer>();
    }

    public void StartMove(bool direction)
    {
        Vector2 pos = playerTransform.position;
        pos.y += YPosAdd;
        transform.position = pos;

        spriteRenderer.flipX = direction;
    }

    //アニメーションイベントです
    public void EndAnim()
    {
        wallKickFactory.ReleaseEffect(gameObject);
    }
}
