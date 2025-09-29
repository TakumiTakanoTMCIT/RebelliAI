using UnityEngine;
using KeyHandler;
using PlayerFlip;
using Zenject;

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

    [SerializeField]
    private Vector2 posOffset; //プレイヤーに対するスケボーの位置オフセット

    //Inject
    IDirection playerDirection;

    [Inject]
    public void Construct(IDirection direction)
    {
        this.playerDirection = direction;
    }

    //スケボーのモードを定義
    public enum Mode
    {
        Independent, //独立して動く
        isControlledByPlayer, //プレイヤーに合わせて動く
        holdByPlayer, //プレイヤーに掴まれている状態
        isBoardInactive // スケボーがしまわれている状態
    }
    private Mode currentMode;

    private void Start()
    {
        rb = playerObj.GetComponent<Rigidbody2D>();

        currentMode = Mode.isBoardInactive;

        skateBoard.SetActive(false);
    }

    public void Ride()
    {
        currentMode = Mode.isControlledByPlayer;
        skateBoard.SetActive(true);

        //プレイヤーの位置にスケボーを移動させる
        skateBoard.transform.position = playerObj.transform.position;
    }

    public void GetOff()
    {
        //乗っている状態から降りる
        currentMode = Mode.isBoardInactive;
        skateBoard.SetActive(false);
    }

    private void Update()
    {
        if (currentMode == Mode.isControlledByPlayer)
        {
            skateBoard.transform.position = playerObj.transform.position;
        }
    }

    public void HandleMove()
    {
        switch (currentMode)
        {
            case Mode.isControlledByPlayer:
                OnControlledByPlayer();
                break;

            case Mode.holdByPlayer:
                if (playerDirection.Direction.Value)
                {
                    skateBoard.transform.position = playerObj.transform.position + (Vector3)posOffset;
                }
                else
                {
                    skateBoard.transform.position = playerObj.transform.position + new Vector3(-posOffset.x, posOffset.y, 0);
                }
                break;

            case Mode.Independent:
                break;

            case Mode.isBoardInactive:

                //まだ処理を書いてないです。

                break;
            default:
                Debug.LogError("致命的なバグが発生しました。SkateBoardCtrlのcurrentModeが不正です");
                return;
        }
    }

    public void ChangeMode(Mode mode)
    {
        switch (mode)
        {
            case Mode.isControlledByPlayer:
                break;
            default:
                Debug.LogError("致命的なバグが発生しました。SkateBoardCtrlのChangeModeの引数が不正です");
                break;
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

    private void OnControlledByPlayer()
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
}
