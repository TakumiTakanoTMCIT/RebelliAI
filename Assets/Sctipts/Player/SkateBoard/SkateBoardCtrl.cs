using UnityEngine;
using KeyHandler;
using PlayerFlip;
using Zenject;
using PlayerState;

public class SkateBoardCtrl : MonoBehaviour
{
    [SerializeField]
    private GameObject skateBoard;

    [SerializeField]
    private InputHandler inputHandler;

    [SerializeField]
    private PlayerStateMgr playerStateMgr;

    Rigidbody2D playerRb;

    [SerializeField]
    private float moveSpeed = 5f, maxSpeed = 10f, speedDamping = 0.1f;

    [SerializeField]
    private float brakeForce = 10f; // ブレーキ強さ

    [SerializeField]
    private Vector2 posOffset; //プレイヤーに対するスケボーの位置オフセット

    //Inject
    IDirection playerDirection;

    [Inject]
    public void Construct(IDirection direction)
    {
        this.playerDirection = direction;
    }

    private void Start()
    {
        playerRb = playerStateMgr.GetComponent<Rigidbody2D>();

        skateBoard.SetActive(false);
    }

    //PlayerStateMgrクラスでSkateBoardステートに入った瞬間に呼び出されます
    //つまり自前の初期化メソッド！
    public void Launch()
    {
        skateBoard.SetActive(true);
    }

    public void Ride()
    {
        //ブレーキを押されてたらブレーキ処理
        if (inputHandler.IsBrakeSkateBoardKey() && playerRb.velocity.x > 0.1f)
        {
            Brake();
            //プレイヤーの位置にスケボーを移動させる
            skateBoard.transform.position = playerStateMgr.transform.position;
            return;
        }

        //すでに最大速度に達していたら何もしない
        if (playerRb.velocity.magnitude > maxSpeed)
        {
            //プレイヤーの位置にスケボーを移動させる
            skateBoard.transform.position = playerStateMgr.transform.position;
            return;
        }

        //同時押しは無効
        if (inputHandler.IsMoveLeftKey() && inputHandler.IsMoveRightKey())
        {
            Damping();
            //プレイヤーの位置にスケボーを移動させる
            skateBoard.transform.position = playerStateMgr.transform.position;
            return;
        }

        //なにも押していないなら徐々に速度を落とす
        if (!inputHandler.IsMoveKey())
        {
            Damping();
            //プレイヤーの位置にスケボーを移動させる
            skateBoard.transform.position = playerStateMgr.transform.position;
            return;
        }

        if (inputHandler.IsMoveLeftKey())
        {
            //左に移動
            playerRb.AddForce(Vector2.left * moveSpeed);
        }
        else if (inputHandler.IsMoveRightKey())
        {
            //右に移動
            playerRb.AddForce(Vector2.right * moveSpeed);
        }

        //プレイヤーの位置にスケボーを移動させる
        skateBoard.transform.position = playerStateMgr.transform.position;
    }

    //PlayerStateMgeクラスからSkate状態を解除したExitの瞬間に呼び出されます。
    public void GetOff()
    {
        //乗っている状態から降りる
        skateBoard.SetActive(false);
    }

    private void Brake()
    {
        if (playerRb.velocity.magnitude > 0.1f) // ほぼ止まったら完全に止める
        {
            Vector2 oppositeForce = -playerRb.velocity.normalized * brakeForce;
            playerRb.AddForce(oppositeForce, ForceMode2D.Force);
        }
        else
        {
            playerRb.velocity = Vector2.zero; // 完全停止
        }
    }

    private void Damping()
    {
        //徐々に速度を落とす
        playerRb.velocity = Vector2.Lerp(playerRb.velocity, Vector2.zero, speedDamping);
    }
}
