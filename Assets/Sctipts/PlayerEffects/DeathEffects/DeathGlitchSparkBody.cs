using UnityEngine;

public class DeathGlitchSparkBody : MonoBehaviour
{
    [SerializeField] private float speed = 10f;

    DeathGlitchSparkFactory factory;
    Rigidbody2D rb;
    Animator animator;
    private void Awake()
    {
        rb = gameObject.MyGetComponent_NullChker<Rigidbody2D>();
        animator = gameObject.MyGetComponent_NullChker<Animator>();
    }

    public void MyAwake(Vector3 pos, DeathGlitchSparkFactory factory)
    {
        //初期化
        transform.position = pos;
        this.factory = factory;

        //ランダムな方向に飛ばす
        int horizontal = Random.Range(-1, 2);
        int vertical = Random.Range(-1, 2);

        if (horizontal == 0 && vertical == 0)
        {
            horizontal = 1;
        }
        rb.AddForce(new Vector2(horizontal * speed, vertical * speed), ForceMode2D.Impulse);

        //ランダムな色にする
        int randomColor = Random.Range(0, 3);
        if (randomColor == 0)
        {
            animator.SetBool("isRed", true);
        }
        else if (randomColor == 1)
        {
            animator.SetBool("isBlue", true);
        }
        else
        {
            animator.SetBool("isBlack", true);
        }
    }

    public void OnBecameInvisible()
    {
        factory.ReturnObject(gameObject);
    }
}
