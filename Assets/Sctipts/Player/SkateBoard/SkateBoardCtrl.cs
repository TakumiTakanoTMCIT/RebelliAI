using UnityEngine;
using KeyHandler;

public class SkateBoardCtrl : MonoBehaviour
{
    [SerializeField]
    private GameObject skateBoard;

    [SerializeField]
    private InputHandler inputHandler;

    [SerializeField]
    private GameObject playerObj;
    Rigidbody2D rb;

    [SerializeField]
    private float moveSpeed = 5f, maxSpeed = 10f, speedDamping = 0.1f;

    [SerializeField]
    private float brakeForce = 10f; // ブレーキ強さ

    private bool isRidingSkateBoard = false;

    private void Start()
    {
        rb = playerObj.GetComponent<Rigidbody2D>();

        isRidingSkateBoard = false;

        skateBoard.SetActive(false);
    }

    public void Ride()
    {
        isRidingSkateBoard = true;
        skateBoard.SetActive(true);

        //プレイヤーの位置にスケボーを移動させる
        skateBoard.transform.position = playerObj.transform.position;
    }

    public void GetOff()
    {
        //乗っている状態から降りる
        isRidingSkateBoard = false;
        skateBoard.SetActive(false);
    }

    private void Update()
    {
        if (isRidingSkateBoard)
        {
            skateBoard.transform.position = playerObj.transform.position;
        }
    }

    public void HandleMove()
    {
        //ブレーキを押されてたらブレーキ処理
        if (inputHandler.IsBrakeSkateBoardKey())
        {
            Brake();
            return;
        }

        //同時押しは無効
        if (inputHandler.IsMoveLeftKey() && inputHandler.IsMoveRightKey())
        {
            Damping();
            return;
        }

        //なにも押していないなら徐々に速度を落とす
        if (!inputHandler.IsMoveKey())
        {
            Damping();
            return;
        }

        //すでに最大速度に達していたら何もしない
        if (rb.velocity.magnitude > maxSpeed)
        {
            Damping();
            return;
        }

        if (inputHandler.IsMoveLeftKey())
        {
            //左に移動
            rb.AddForce(Vector2.left * moveSpeed);
        }
        else if (inputHandler.IsMoveRightKey())
        {
            //右に移動
            rb.AddForce(Vector2.right * moveSpeed);
        }
    }

    private void Brake()
    {
        if (rb.velocity.magnitude > 0.1f) // ほぼ止まったら完全に止める
        {
            Vector2 oppositeForce = -rb.velocity.normalized * brakeForce;
            rb.AddForce(oppositeForce, ForceMode2D.Force);
        }
        else
        {
            rb.velocity = Vector2.zero; // 完全停止
        }
    }

    private void Damping()
    {
        //徐々に速度を落とす
        rb.velocity = Vector2.Lerp(rb.velocity, Vector2.zero, speedDamping);
    }
}
