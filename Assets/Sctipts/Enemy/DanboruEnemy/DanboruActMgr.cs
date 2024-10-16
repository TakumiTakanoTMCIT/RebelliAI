using UnityEngine;

public class DanboruActMgr : MonoBehaviour
{
    Rigidbody2D rb;
    private void Awake()
    {
        rb = gameObject.MyGetComponent_NullChker<Rigidbody2D>();
    }

    public bool IsJumping(float jumpForceThreshold)
    {
        return rb.velocity.y > -jumpForceThreshold;
    }

    public void Jump(float jumpForce)
    {
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
    }
}
