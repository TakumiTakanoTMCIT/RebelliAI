using UnityEngine;

public class DanboruActMgr : MonoBehaviour
{
    [SerializeField] float fallThreshold = 0.1f;
    Rigidbody2D rb;
    private void Awake()
    {
        rb = gameObject.MyGetComponent_NullChker<Rigidbody2D>();
    }

    public bool IsJumping()
    {
        return rb.velocity.y > 0;
    }

    public bool IsFalling()
    {
        return rb.velocity.y < -fallThreshold;
    }

    public void Jump(float jumpForce)
    {
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
    }
}
